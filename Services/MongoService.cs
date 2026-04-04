using API.Models;
using MongoDB.Driver;

namespace API.Services;

public class MongoService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Skill> _skills;
    private readonly IMongoCollection<Result> _results;
    private readonly IMongoCollection<UserSkill> _userSkills;
    private readonly IMongoCollection<Notification> _notifications;

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

        _users = database.GetCollection<User>("Users");
        _skills = database.GetCollection<Skill>("Skills");
        _results = database.GetCollection<Result>("Results");
        _userSkills = database.GetCollection<UserSkill>("UserSkills");
        _notifications = database.GetCollection<Notification>("Notifications");
    }

    // USER ID GENERATOR

    public async Task<string> GenerateUserId()
    {
        var lastUser = await _users
            .Find(_ => true)
            .SortByDescending(u => u.UserId)
            .FirstOrDefaultAsync();

        if (lastUser == null || string.IsNullOrEmpty(lastUser.UserId))
            return "U001";

        int numberPart = int.Parse(lastUser.UserId.Substring(1));
        return $"U{numberPart + 1:D3}";
    }

    // USERS

    public async Task CreateUser(User user)
    {
        await _users.InsertOneAsync(user);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByCustomId(string userId)
    {
        return await _users.Find(u => u.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<List<User>> GetAllUsers()
    {
        return await _users.Find(_ => true).ToListAsync();
    }

    public async Task<bool> DeleteUser(string userId)
    {
        var result = await _users.DeleteOneAsync(u => u.UserId == userId);
        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdateUserSafe(string userId, UpdateUserDto dto)
    {
        var existingUser = await _users
            .Find(u => u.UserId == userId)
            .FirstOrDefaultAsync();

        if (existingUser == null)
            return false;

        existingUser.FullName = dto.FullName;
        existingUser.Email = dto.Email;

        if (!string.IsNullOrEmpty(dto.Password))
        {
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        await _users.ReplaceOneAsync(u => u.UserId == userId, existingUser);

        return true;
    }

    // SKILLS

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
        return await _skills.Find(s => s.SkillId == skillId).FirstOrDefaultAsync();
    }

    public async Task DeleteSkill(string skillId)
    {
        await _skills.DeleteOneAsync(s => s.SkillId == skillId);
    }

    // RESULTS

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

    // NEW METHOD (VERY IMPORTANT FOR ADMIN DASHBOARD)
    public async Task<List<Result>> GetAllResults()
    {
        return await _results.Find(_ => true).ToListAsync();
    }

    public async Task<List<Result>> GetResultsByUserAndSkill(string userId, string skillId)
    {
        return await _results
            .Find(r => r.UserId == userId && r.SkillId == skillId)
            .ToListAsync();
    }

    // USER SKILLS

    public async Task<string> AddUserSkill(UserSkill userSkill)
    {
        var exists = await _userSkills
            .Find(x => x.UserId == userSkill.UserId && x.SkillId == userSkill.SkillId)
            .FirstOrDefaultAsync();

        if (exists != null)
            return "Skill already added";

        await _userSkills.InsertOneAsync(userSkill);
        return "Skill added";
    }

    public async Task<List<UserSkill>> GetAllUserSkills()
    {
        return await _userSkills.Find(_ => true).ToListAsync();
    }

    public async Task<List<UserSkill>> GetUserSkills(string userId)
    {
        return await _userSkills
            .Find(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> DeleteUserSkill(string userId, string skillId)
    {
        var result = await _userSkills
            .DeleteOneAsync(x => x.UserId == userId && x.SkillId == skillId);

        return result.DeletedCount > 0;
    }

    public async Task<List<Result>> GetAllResultsAgg()
    {
        return await _results
            .Find(_ => true)  
            .ToListAsync();
    }

    // NOTIFICATIONS

    public IMongoCollection<Notification> Notifications => _notifications;

    public async Task AddNotification(Notification notification)
    {
        await _notifications.InsertOneAsync(notification);
    }

    public async Task<string> GenerateNotificationId()
    {
        var last = await _notifications
            .Find(_ => true)
            .SortByDescending(n => n.Nid)
            .FirstOrDefaultAsync();

        if (last == null || string.IsNullOrEmpty(last.Nid))
            return "N001";

        int num = int.Parse(last.Nid.Substring(1));
        return $"N{num + 1:D3}";
    }

    public async Task<List<Notification>> GetUserNotifications(string userId)
    {
        return await _notifications
            .Find(n => n.UserId == userId)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notification?> GetUserNotificationByMessage(string userId, string message)
    {
        return await _notifications
            .Find(n => n.UserId == userId && n.Message == message)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateNotificationTime(string id)
    {
        var update = Builders<Notification>.Update
            .Set(n => n.CreatedAt, DateTime.UtcNow)
            .Set(n => n.IsRead, false);

        await _notifications.UpdateOneAsync(n => n.Id == id, update);
    }

    public async Task<bool> MarkAsReadByNid(string nid)
    {
        var update = Builders<Notification>.Update.Set(x => x.IsRead, true);

        var result = await _notifications.UpdateOneAsync(
            x => x.Nid == nid,
            update
        );

        return result.ModifiedCount > 0;
    }

    public async Task MarkAllAsRead(string userId)
    {
        var update = Builders<Notification>.Update.Set(x => x.IsRead, true);

        await _notifications.UpdateManyAsync(
            x => x.UserId == userId,
            update
        );
    }
}