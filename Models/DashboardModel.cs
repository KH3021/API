namespace API.Models;

public class DashboardModel
{
    public string UserId { get; set; }

    public int TotalSkills { get; set; }
    public int TotalTests { get; set; }

    public double AveragePercentage { get; set; }

    public List<object> SkillProgress { get; set; }
}