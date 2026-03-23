using MongoDB.Driver;
using API.Models;
using Microsoft.Extensions.Configuration;

namespace API.Services;

public class MongoService
{
    private readonly IMongoCollection<User> _users;

    public MongoService(IConfiguration config)
    {
        // 🔥 FIX: Support BOTH local + Railway
        var connection = config["MongoDB:ConnectionString"]
                      ?? Environment.GetEnvironmentVariable("MONGO_CONNECTION");

        if (string.IsNullOrEmpty(connection))
        {
            throw new Exception("MongoDB connection string is missing ❌");
        }

        var client = new MongoClient(connection);
        var database = client.GetDatabase("AuthDB");
        _users = database.GetCollection<User>("Users");
    }

    // ✅ Create user
    public async Task CreateUser(User user)
    {
        await _users.InsertOneAsync(user);
    }

    // ❌ (not used anymore if hashing)
    public async Task<User?> GetUser(string email, string password)
    {
        return await _users
            .Find(u => u.Email == email && u.Password == password)
            .FirstOrDefaultAsync();
    }

    // ✅ Get by email (used for login)
    public async Task<User?> GetUserByEmail(string email)
    {
        return await _users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();
    }
}