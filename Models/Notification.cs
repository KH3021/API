using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("nid")]
    public string Nid { get; set; }

    [BsonElement("userid")]
    public string UserId { get; set; }

    [BsonElement("message")]
    public string Message { get; set; }

    [BsonElement("isread")]
    public bool IsRead { get; set; } = false;

    [BsonElement("createdat")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}