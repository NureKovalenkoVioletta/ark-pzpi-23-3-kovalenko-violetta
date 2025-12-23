namespace FitnessProject.BLL.DTO.Admin;

public class AdminOverviewDto
{
    public string BuildVersion { get; set; } = string.Empty;
    public string Commit { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Connection { get; set; } = string.Empty;
    public string LastMigration { get; set; } = string.Empty;
    public string Uptime { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
}

