using API.Models;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

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

    public async Task<string?> GenerateRoadmap(string userId)
    {
        //  FETCH USER
        var user = await _mongo.GetUserByCustomId(userId);
        if (user == null)
            return null;

        //  GET RESULTS
        var results = await _mongo.GetResultsByUser(userId);

        string prompt;

        if (results.Count == 0)
        {
            var skills = await _mongo.GetAllSkills();

            var skillNames = skills
                .Select(s => s.SkillName)
                .ToList();

            prompt = $@"
User Name: {user.FullName}

Skills:
{string.Join(", ", skillNames)}

Task:
Write a beginner-friendly 500-word guide to improve skills using SkillBridge.

Include:
- How to start learning
- Importance of consistency
- Practice strategy
- Motivation tips
";
        }
        else
        {
            var grouped = results.GroupBy(r => r.SkillId);
            var analysisBuilder = new StringBuilder();

            foreach (var group in grouped)
            {
                var skillData = await _mongo.GetSkillById(group.Key);
                var skillName = skillData?.SkillName ?? "Unknown";

                var attempts = group.Count();
                var avgScore = group.Average(r => r.Percentage);

                analysisBuilder.AppendLine(
                    $"Skill: {skillName}, Attempts: {attempts}, Avg: {avgScore:F1}%"
                );
            }

            prompt = $@"
User: {user.FullName}

Performance:
{analysisBuilder}

Task:
Create a personalized roadmap (max 700 words) including:
- Strengths & weaknesses
- Improvement plan
- Practice strategy
";
        }

        // LIMIT PROMPT SIZE
        if (prompt.Length > 4000)
            prompt = prompt.Substring(0, 4000);

        var requestBody = new
        {
            model = "llama-3.1-8b-instant",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var apiKey = _config["Groq:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("API Key not found");

        var client = new HttpClient();

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.PostAsync(
            "https://api.groq.com/openai/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"AI Error: {response.StatusCode} | {json}");

        using var doc = JsonDocument.Parse(json);

        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrEmpty(content))
            throw new Exception("Empty response from AI ");

        // OPTIONAL: Fix formatting
        content = content.Replace("\\n", "\n");

        return content;
    }
}