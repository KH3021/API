namespace API.Models;

public class AnalysisModel
{
    public string FullName { get; set; }
    public string Email { get; set; }

    public List<string> Skills { get; set; } = new();

    public int TotalSkills { get; set; }

    public List<string> Performance { get; set; } = new();

    public string? Roadmap { get; set; }

    public DateTime GeneratedOn { get; set; } = DateTime.Now;
}