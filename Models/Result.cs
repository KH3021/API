using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class Result
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userid")]
    public string UserId { get; set; }

    [BsonElement("skillid")]
    public string SkillId { get; set; }

    [BsonElement("level")]
    public string Level { get; set; }

    [BsonElement("score")]
    public int Score { get; set; }

    [BsonElement("total")]
    public int Total { get; set; }

    [BsonElement("percentage")]
    public double Percentage { get; set; }

    [BsonElement("resulttext")]
    public string ResultText { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    // IMPORTANT: STORE USER ANSWERS
    [BsonElement("answers")]
    public List<AnswerModel> Answers { get; set; } = new();
}