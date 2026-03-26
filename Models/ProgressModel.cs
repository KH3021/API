namespace API.Models;

public class ProgressModel
{
    public string UserId { get; set; }
    public string SkillId { get; set; }
    public int Score { get; set; }
    public string? Level { get; set; }
}