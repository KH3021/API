namespace API.Models;

public class SkillGapModel
{
    public string UserId { get; set; }
    public string SkillId { get; set; }

    public int BeginnerPerfect { get; set; }
    public int MidPerfect { get; set; }
    public int ExpertPerfect { get; set; }

    public double CompletionPercentage { get; set; }
    public double SkillGapPercentage { get; set; }

    public bool IsMastered { get; set; }
}