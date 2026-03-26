using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class UserSkill
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userid")]
    public string UserId { get; set; }

    [BsonElement("skillid")]
    public string SkillId { get; set; }

    [BsonElement("level")]
    public string Level { get; set; } = "Beginner";
}