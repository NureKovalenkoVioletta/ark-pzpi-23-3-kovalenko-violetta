using FitnessProject.BLL.DTO.Localization;
using FitnessProject.BLL.Services.Interfaces;
using System.Xml.Linq;
using System.Resources.Extensions;


namespace FitnessProject.BLL.Services;

public class LocalizationAdminService : ILocalizationAdminService
{
    private readonly string _resourcesPath;
    private readonly string[] _allowedCultures = new[] { "uk", "en" };

    public LocalizationAdminService(IWebHostEnvironment env)
    {
        _resourcesPath = Path.Combine(env.ContentRootPath, "Resources");
    }

    // Повертає всі ключі та значення локалізації для вказаної культури
    public Task<Dictionary<string, string>> GetKeysAsync(string culture)
    {
        EnsureCulture(culture);
        var path = ResolveResxPath(culture);
        var dict = ReadResx(path);
        return Task.FromResult(dict);
    }

    // Повертає список ключів, що відсутні у порівняльній культурі відносно базової
    public Task<IEnumerable<string>> GetMissingKeysAsync(string baseCulture, string compareCulture)
    {
        EnsureCulture(baseCulture);
        EnsureCulture(compareCulture);

        var baseKeys = ReadResx(ResolveResxPath(baseCulture)).Keys.ToHashSet();
        var compareKeys = ReadResx(ResolveResxPath(compareCulture)).Keys.ToHashSet();

        var missing = baseKeys.Except(compareKeys);
        return Task.FromResult<IEnumerable<string>>(missing);
    }

    // Оновлює або створює конкретний ключ локалізації для заданої культури
    public Task UpdateKeyAsync(LocalizationKeyUpdateDto dto)
    {
        EnsureCulture(dto.Culture);
        var path = ResolveResxPath(dto.Culture);
        var data = ReadResx(path);
        data[dto.Key] = dto.Value;
        WriteResx(path, data);
        return Task.CompletedTask;
    }

    // Експортує всі ключі для uk та en у єдину DTO
    public async Task<LocalizationExportDto> ExportAsync()
    {
        var uk = await GetKeysAsync("uk");
        var en = await GetKeysAsync("en");
        return new LocalizationExportDto { Uk = uk, En = en };
    }

    // Імпортує ключі для uk/en, перезаписуючи відповідні .resx файли
    public async Task ImportAsync(LocalizationImportDto dto)
    {
        if (dto.Uk != null)
        {
            var pathUk = ResolveResxPath("uk");
            WriteResx(pathUk, dto.Uk);
        }

        if (dto.En != null)
        {
            var pathEn = ResolveResxPath("en");
            WriteResx(pathEn, dto.En);
        }
    }

    // Визначає шлях до .resx файлу для заданої культури
    private string ResolveResxPath(string culture)
    {
        var suffix = culture switch
        {
            "uk" => "Shared.uk.resx",
            "en" => "Shared.en.resx",
            _ => throw new ArgumentException("Unsupported culture", nameof(culture))
        };
        return Path.Combine(_resourcesPath, suffix);
    }

    // Зчитує .resx у словник ключ → значення
    private static Dictionary<string, string> ReadResx(string path)
    {
        var dict = new Dictionary<string, string>();
        if (!File.Exists(path))
        {
            return dict;
        }

        var doc = XDocument.Load(path);
        foreach (var data in doc.Descendants("data"))
        {
            var nameAttr = data.Attribute("name");
            var valueElem = data.Element("value");
            if (nameAttr != null && valueElem != null)
            {
                dict[nameAttr.Value] = valueElem.Value;
            }
        }

        return dict;
    }

    // Записує словник ключів у .resx файл, створюючи директорію за потреби
    private static void WriteResx(string path, Dictionary<string, string> data)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("root",
                new XElement("resheader",
                    new XAttribute("name", "resmimetype"),
                    new XElement("value", "text/microsoft-resx")),
                new XElement("resheader",
                    new XAttribute("name", "version"),
                    new XElement("value", "2.0")),
                new XElement("resheader",
                    new XAttribute("name", "reader"),
                    new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),
                new XElement("resheader",
                    new XAttribute("name", "writer"),
                    new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),
                data.OrderBy(k => k.Key).Select(kvp =>
                    new XElement("data",
                        new XAttribute("name", kvp.Key),
                        new XAttribute(XNamespace.Xml + "space", "preserve"),
                        new XElement("value", kvp.Value)))
            ));

        doc.Save(path);
    }

    // Перевіряє та нормалізує культуру, дозволяючи лише uk/en
    private void EnsureCulture(string culture)
    {
        var shortCulture = culture.ToLowerInvariant() switch
        {
            var c when c.StartsWith("uk") || c.StartsWith("ua") => "uk",
            var c when c.StartsWith("en") => "en",
            _ => culture.ToLowerInvariant()
        };
        if (!_allowedCultures.Contains(shortCulture))
        {
            throw new ArgumentException("Unsupported culture", nameof(culture));
        }
    }
}

