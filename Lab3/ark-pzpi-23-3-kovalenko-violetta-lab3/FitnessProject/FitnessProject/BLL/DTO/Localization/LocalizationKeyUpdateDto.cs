namespace FitnessProject.BLL.DTO.Localization;

public class LocalizationKeyUpdateDto
{
    public string Key { get; set; } = string.Empty;
    public string Culture { get; set; } = string.Empty; // uk or en
    public string Value { get; set; } = string.Empty;
}

