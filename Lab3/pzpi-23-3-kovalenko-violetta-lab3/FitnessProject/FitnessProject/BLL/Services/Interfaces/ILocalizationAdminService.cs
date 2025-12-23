using FitnessProject.BLL.DTO.Localization;

namespace FitnessProject.BLL.Services.Interfaces;

public interface ILocalizationAdminService
{
    Task<Dictionary<string, string>> GetKeysAsync(string culture);
    Task<IEnumerable<string>> GetMissingKeysAsync(string baseCulture, string compareCulture);
    Task UpdateKeyAsync(LocalizationKeyUpdateDto dto);
    Task<LocalizationExportDto> ExportAsync();
    Task ImportAsync(LocalizationImportDto dto);
}

