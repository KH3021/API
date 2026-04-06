using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class StudyPlan
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userid")]
    public string UserId { get; set; }

    [BsonElement("content")]
    public string Content { get; set; }

    [BsonElement("createdat")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}