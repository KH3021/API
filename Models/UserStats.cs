using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class UserStats
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userid")]
    public string UserId { get; set; }

    [BsonElement("points")]
    public int Points { get; set; } = 0;

    [BsonElement("level")]
    public int Level { get; set; } = 1;

    [BsonElement("completedtasks")]
    public int CompletedTasks { get; set; } = 0;
}