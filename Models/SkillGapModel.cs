using System.Text.Json.Serialization;

namespace API.Models;

public class SkillGapModel
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("skillId")]
    public string SkillId { get; set; }

    [JsonPropertyName("completionPercentage")]
    public double CompletionPercentage { get; set; }

    [JsonPropertyName("skillGapPercentage")]
    public double SkillGapPercentage { get; set; }

    [JsonPropertyName("isMastered")]
    public bool IsMastered { get; set; }
}