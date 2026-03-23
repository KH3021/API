using MongoDB.Driver;
using API.Models;
using Microsoft.Extensions.Configuration;

namespace API.Services;

public class MongoService
{
    private readonly IMongoCollection<User> _users;

    public MongoService(IConfiguration config)
    {
        // ✅ Get connection string from appsettings.json
        var connection = config["MongoDB:ConnectionString"];

        var client = new MongoClient(connection);
        var database = client.GetDatabase("AuthDB");
        _users = database.GetCollection<User>("Users");
    }

    // ✅ Create user
    public async Task CreateUser(User user)
    {
        await _users.InsertOneAsync(user);
    }

    // ✅ Get user for login
    public async Task<User?> GetUser(string email, string password)
    {
        return await _users
            .Find(u => u.Email == email && u.Password == password)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _users
            .Find(u => u.Email == email)
            .FirstOrDefaultAsync();
    }
}