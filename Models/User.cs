using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userid")]
    public string? UserId { get; set; }

    [BsonElement("fullname")]
    public string FullName { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("password")]
    public string Password { get; set; }

    [BsonElement("role")]
    public string Role { get; set; } = "Client";

    [BsonElement("createddate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [BsonElement("lastlogin")]
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
}