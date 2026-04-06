using API.Models;
using MongoDB.Driver;

namespace API.Services;

public class GamificationService
{
    private readonly MongoService _mongo;

    public GamificationService(MongoService mongo)
    {
        _mongo = mongo;
    }

    // ✅ Get or Create Stats
    public async Task<UserStats> GetOrCreate(string userId)
    {
        var stats = await _mongo.UserStats
            .Find(x => x.UserId == userId)
            .FirstOrDefaultAsync();

        if (stats == null)
        {
            stats = new UserStats
            {
                UserId = userId
            };

            await _mongo.UserStats.InsertOneAsync(stats);
        }

        return stats;
    }

    // ✅ Add simple points
    public async Task AddPoints(string userId, int points)
    {
        var stats = await GetOrCreate(userId);

        stats.Points += points;
        stats.Level = stats.Points / 100 + 1;

        await _mongo.UserStats.ReplaceOneAsync(x => x.UserId == userId, stats);
    }

    // ✅ Add points from quiz result
    public async Task AddQuizPoints(Result result)
    {
        var stats = await GetOrCreate(result.UserId);

        stats.Points += result.Score;

        if (result.Percentage >= 80)
            stats.Points += 20;

        stats.Level = stats.Points / 100 + 1;

        await _mongo.UserStats.ReplaceOneAsync(x => x.UserId == result.UserId, stats);
    }

    // ✅ Task completion reward
    public async Task CompleteTask(string userId)
    {
        var stats = await GetOrCreate(userId);

        stats.Points += 50;
        stats.CompletedTasks += 1;
        stats.Level = stats.Points / 100 + 1;

        await _mongo.UserStats.ReplaceOneAsync(x => x.UserId == userId, stats);
    }
}