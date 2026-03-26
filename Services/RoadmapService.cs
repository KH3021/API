using API.Models;

namespace API.Services;

public class RoadmapService
{
    private readonly MongoService _mongo;

    public RoadmapService(MongoService mongo)
    {
        _mongo = mongo;
    }

    public async Task<RoadmapModel?> GenerateRoadmap(string userId, string skillId, string level)
    {
        var skill = await _mongo.GetSkillById(skillId);

        if (skill == null)
            return null;

        var steps = new List<string>();

        if (level == "Beginner")
        {
            steps.Add("Learn basics of " + skill.SkillName);
            steps.Add("Practice simple problems");
            steps.Add("Build small projects");
        }
        else if (level == "Intermediate")
        {
            steps.Add("Revise core concepts of " + skill.SkillName);
            steps.Add("Solve real-world problems");
            steps.Add("Build intermediate projects");
        }
        else
        {
            steps.Add("Master advanced topics of " + skill.SkillName);
            steps.Add("Work on large-scale projects");
            steps.Add("Contribute to open source");
        }

        return new RoadmapModel
        {
            UserId = userId,
            SkillId = skill.SkillId,
            SkillName = skill.SkillName,
            Steps = steps
        };
    }
}