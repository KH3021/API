namespace API.Models;

public class RoadmapModel
{
    public string UserId { get; set; }
    public string SkillId { get; set; }
    public string SkillName { get; set; }
    public List<string> Steps { get; set; }
}