using API.Models;

namespace API.Services;

public class ProgressService
{
    private readonly MongoService _mongo;

    public ProgressService(MongoService mongo)
    {
        _mongo = mongo;
    }

    public async Task<ProgressModel> CalculateProgress(string userId, string skillId, int score)
    {
        string level;

        if (score < 40)
            level = "Beginner";
        else if (score < 70)
            level = "Intermediate";
        else
            level = "Expert";

        var result = new Result
        {
            UserId = userId,
            SkillId = skillId,
            Score = score,
            Level = level,
            Date = DateTime.UtcNow
        };

        await _mongo.SaveResult(result);

        return new ProgressModel
        {
            UserId = userId,
            SkillId = skillId,
            Score = score,
            Level = level
        };
    }
}