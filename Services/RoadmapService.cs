using API.Models;
using System.Text;
using System.Text.Json;

namespace API.Services;

public class RoadmapService
{
    private readonly MongoService _mongo;
    private readonly IConfiguration _config;

    public RoadmapService(MongoService mongo, IConfiguration config)
    {
        _mongo = mongo;
        _config = config;
    }

    public async Task<string?> GenerateRoadmap(string userId, string skillId)
    {
        // 🔹 FETCH DATA
        var user = await _mongo.GetUserByCustomId(userId);
        var skill = await _mongo.GetSkillById(skillId);
        var results = await _mongo.GetResultsByUserAndSkill(userId, skillId);

        if (user == null || skill == null || results.Count == 0)
            return null;

        var latest = results.OrderByDescending(r => r.Date).First();
        var avgScore = results.Average(r => r.Percentage);
        var attempts = results.Count;

        string level = latest.Percentage < 40 ? "Beginner"
                      : latest.Percentage < 70 ? "Intermediate"
                      : "Advanced";

        // 🔹 WEAK AREAS
        var weakQuestions = latest.Answers
            .Where(a => a.SelectedAnswer != a.CorrectAnswer)
            .Select(a => a.Question)
            .Take(5)
            .ToList();

        // 🔥 PROMPT (VERY IMPORTANT)
        var prompt = $@"
Create a detailed (1000–1500 words) personalized learning roadmap.

User:
- Name: {user.FullName}
- UserId: {user.UserId}
- Skill: {skill.SkillName}
- Attempts: {attempts}
- Latest Score: {latest.Percentage}%
- Average Score: {avgScore:F2}%
- Level: {level}

Weak Areas:
{string.Join(", ", weakQuestions)}

Requirements:
- Analyze user mistakes deeply
- Explain weak areas clearly
- Provide step-by-step roadmap

Include:
1. Skill gap analysis
2. Daily & weekly plan
3. Practice strategy
4. 3 projects
5. Resources (YouTube, Docs, Platforms)
6. Timeline
7. Mistake fixing strategy

Make it SPECIFIC to this user (not generic).
Use headings and structured format.
";

        // 🔥 SAME MODEL AS TEST CONTROLLER
        var requestBody = new
        {
            model = "llama-3.1-8b-instant",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var apiKey = _config["Groq:ApiKey"];

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await client.PostAsync(
            "https://api.groq.com/openai/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);

        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content;
    }
}