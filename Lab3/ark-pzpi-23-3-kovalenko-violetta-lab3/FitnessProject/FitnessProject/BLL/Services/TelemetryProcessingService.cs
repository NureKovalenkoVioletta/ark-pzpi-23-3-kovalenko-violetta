using FitnessProject.BLL.DTO.Telemetry;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.Services.Helpers;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Data;
using FitnessProject.Entities;
using FitnessProject.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitnessProject.BLL.Services;

public class TelemetryProcessingService : ITelemetryProcessingService
{
    private readonly ITelemetrySampleRepository _telemetrySampleRepository;
    private readonly ISleepRecordRepository _sleepRecordRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly ApplicationDbContext _context;

    public TelemetryProcessingService(
        ITelemetrySampleRepository telemetrySampleRepository,
        ISleepRecordRepository sleepRecordRepository,
        IDeviceRepository deviceRepository,
        ApplicationDbContext context)
    {
        _telemetrySampleRepository = telemetrySampleRepository;
        _sleepRecordRepository = sleepRecordRepository;
        _deviceRepository = deviceRepository;
        _context = context;
    }

    public async Task ProcessTelemetryAsync(TelemetryReceiveDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var device = await _deviceRepository.GetByIdAsync(dto.DeviceId);
            if (device == null)
            {
                throw new ArgumentException($"Device with ID {dto.DeviceId} not found");
            }

            device.LastSeen = DateTime.UtcNow;
            await _deviceRepository.UpdateWithoutSaveAsync(device);

            if (IsSleepData(dto))
            {
                await ProcessSleepDataAsync(dto, false);
            }
            else
            {
                await ProcessTelemetrySampleAsync(dto, false);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ProcessBatchAsync(TelemetryReceiveBatchDto batchDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            foreach (var item in batchDto.Items)
            {
                var device = await _deviceRepository.GetByIdAsync(item.DeviceId);
                if (device == null)
                {
                    continue;
                }

                device.LastSeen = DateTime.UtcNow;
                await _deviceRepository.UpdateWithoutSaveAsync(device);

                if (IsSleepData(item))
                {
                    await ProcessSleepDataAsync(item, false);
                }
                else
                {
                    await ProcessTelemetrySampleAsync(item, false);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private bool IsSleepData(TelemetryReceiveDto dto)
    {
        return dto.Metadata != null && 
               (dto.Metadata.ContainsKey("TotalSleepMinutes") || 
                dto.Metadata.ContainsKey("DeepSleepMinutes") ||
                dto.Metadata.ContainsKey("LightSleepMinutes"));
    }

    private async Task ProcessTelemetrySampleAsync(TelemetryReceiveDto dto, bool saveChanges = true)
    {
        var (isValid, normalizedValue, errorMessage) = TelemetryNormalizer.NormalizeAndValidate(
            dto.TelemetryType, 
            dto.Value);

        if (!isValid)
        {
            throw new ArgumentException(
                $"Telemetry data validation failed. {errorMessage} " +
                $"DeviceId: {dto.DeviceId}, Timestamp: {dto.Timestamp}, Type: {dto.TelemetryType}");
        }

        var existing = await CheckDuplicateTelemetrySampleAsync(
            dto.DeviceId, 
            dto.Timestamp, 
            dto.TelemetryType);

        if (existing != null)
        {
            existing.TelemetryValue = normalizedValue;
            if (saveChanges)
            {
                await _telemetrySampleRepository.UpdateAsync(existing);
            }
            else
            {
                await _telemetrySampleRepository.UpdateWithoutSaveAsync(existing);
            }
            return;
        }

        var telemetrySample = new TelemetrySample
        {
            DeviceId = dto.DeviceId,
            Timestamp = dto.Timestamp,
            TelemetryType = dto.TelemetryType,
            TelemetryValue = normalizedValue
        };

        if (saveChanges)
        {
            await _telemetrySampleRepository.AddAsync(telemetrySample);
        }
        else
        {
            await _telemetrySampleRepository.AddWithoutSaveAsync(telemetrySample);
        }
    }

    private async Task ProcessSleepDataAsync(TelemetryReceiveDto dto, bool saveChanges = true)
    {
        if (dto.Metadata == null)
        {
            throw new ArgumentException("Sleep data requires metadata");
        }

        var (isValid, errorMessage) = TelemetryNormalizer.ValidateSleepData(dto.Metadata);
        if (!isValid)
        {
            throw new ArgumentException(
                $"Sleep data validation failed. {errorMessage} " +
                $"DeviceId: {dto.DeviceId}, Timestamp: {dto.Timestamp}");
        }

        var date = dto.Timestamp.Date;
        var totalSleepMinutes = TelemetryNormalizer.NormalizeSleepMinutes(
            dto.Metadata.GetValueOrDefault("TotalSleepMinutes"));
        var deepSleepMinutes = TelemetryNormalizer.NormalizeSleepMinutes(
            dto.Metadata.GetValueOrDefault("DeepSleepMinutes"));
        var lightSleepMinutes = TelemetryNormalizer.NormalizeSleepMinutes(
            dto.Metadata.GetValueOrDefault("LightSleepMinutes"));
        var awakeMinutes = TelemetryNormalizer.NormalizeSleepMinutes(
            dto.Metadata.GetValueOrDefault("AwakeMinutes"));
        var sleepQuality = TelemetryNormalizer.NormalizeSleepQuality(
            dto.Metadata.GetValueOrDefault("SleepQuality"));

        DateTime? startTime = null;
        if (dto.Metadata.TryGetValue("StartTime", out var startTimeObj) && startTimeObj != null)
        {
            if (DateTime.TryParse(startTimeObj.ToString(), out var parsedStartTime))
            {
                startTime = parsedStartTime;
            }
        }

        DateTime? endTime = null;
        if (dto.Metadata.TryGetValue("EndTime", out var endTimeObj) && endTimeObj != null)
        {
            if (DateTime.TryParse(endTimeObj.ToString(), out var parsedEndTime))
            {
                endTime = parsedEndTime;
            }
        }

        var existing = await CheckDuplicateSleepRecordAsync(dto.DeviceId, date);

        if (existing != null)
        {
            existing.TotalSleepMinutes = totalSleepMinutes;
            existing.DeepSleepMinutes = deepSleepMinutes;
            existing.LightSleepMinutes = lightSleepMinutes;
            existing.AwakeMinutes = awakeMinutes;
            existing.SleepQuality = sleepQuality;
            existing.StartTime = startTime;
            existing.EndTime = endTime;
            if (saveChanges)
            {
                await _sleepRecordRepository.UpdateAsync(existing);
            }
            else
            {
                await _sleepRecordRepository.UpdateWithoutSaveAsync(existing);
            }
            return;
        }

        var sleepRecord = new SleepRecord
        {
            DeviceId = dto.DeviceId,
            Date = date,
            TotalSleepMinutes = totalSleepMinutes,
            DeepSleepMinutes = deepSleepMinutes,
            LightSleepMinutes = lightSleepMinutes,
            AwakeMinutes = awakeMinutes,
            SleepQuality = sleepQuality,
            StartTime = startTime,
            EndTime = endTime
        };

        if (saveChanges)
        {
            await _sleepRecordRepository.AddAsync(sleepRecord);
        }
        else
        {
            await _sleepRecordRepository.AddWithoutSaveAsync(sleepRecord);
        }
    }

    private async Task<TelemetrySample?> CheckDuplicateTelemetrySampleAsync(
        int deviceId, 
        DateTime timestamp, 
        TelemetryType telemetryType)
    {
        var existing = await _telemetrySampleRepository.FindAsync(t =>
            t.DeviceId == deviceId &&
            t.Timestamp == timestamp &&
            t.TelemetryType == telemetryType);

        return existing.FirstOrDefault();
    }

    private async Task<SleepRecord?> CheckDuplicateSleepRecordAsync(
        int deviceId, 
        DateTime date)
    {
        var existing = await _sleepRecordRepository.FindAsync(s =>
            s.DeviceId == deviceId &&
            s.Date.Date == date.Date);

        return existing.FirstOrDefault();
    }
}

