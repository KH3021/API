using API.Models;
using MongoDB.Driver;

namespace API.Services;

public class MongoService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Skill> _skills;
    private readonly IMongoCollection<Result> _results;

    public MongoService(IConfiguration config)
    {
        var connection = config["MongoDB:ConnectionString"]
                      ?? Environment.GetEnvironmentVariable("MONGO_CONNECTION");

        if (string.IsNullOrEmpty(connection))
        {
            throw new Exception("MongoDB connection string is missing ❌");
        }

        var client = new MongoClient(connection);
        var database = client.GetDatabase("AuthDB");

        // 🔥 EXACT COLLECTION NAMES
        _users = database.GetCollection<User>("Users");
        _skills = database.GetCollection<Skill>("Skills");
        _results = database.GetCollection<Result>("Results");
    }

    // ================= USER ID GENERATOR =================

    public async Task<string> GenerateUserId()
    {
        var lastUser = await _users
            .Find(_ => true)
            .SortByDescending(u => u.UserId)
            .FirstOrDefaultAsync();

        if (lastUser == null || string.IsNullOrEmpty(lastUser.UserId))
            return "U001";

        int numberPart = int.Parse(lastUser.UserId.Substring(1));
        int newNumber = numberPart + 1;

        return $"U{newNumber:D3}";
    }

    // ================= USERS =================

    public async Task CreateUser(User user)
    {
        await _users.InsertOneAsync(user);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByCustomId(string userId)
    {
        return await _users
            .Find(u => u.UserId == userId)
            .FirstOrDefaultAsync();
    }

    // ================= SKILLS =================

    public async Task<List<Skill>> GetAllSkills()
    {
        return await _skills.Find(_ => true).ToListAsync();
    }

    public async Task AddSkill(Skill skill)
    {
        await _skills.InsertOneAsync(skill);
    }

    public async Task<Skill?> GetSkillById(string skillId)
    {
        return await _skills
            .Find(s => s.SkillId == skillId)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteSkill(string skillId)
    {
        await _skills.DeleteOneAsync(s => s.SkillId == skillId);
    }

    // ================= RESULTS =================

    public async Task SaveResult(Result result)
    {
        await _results.InsertOneAsync(result);
    }

    public async Task<List<Result>> GetResultsByUser(string userId)
    {
        return await _results
            .Find(r => r.UserId == userId)
            .SortByDescending(r => r.Date)
            .ToListAsync();
    }
}