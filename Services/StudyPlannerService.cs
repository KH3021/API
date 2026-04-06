using API.Models;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace API.Services;

public class StudyPlannerService
{
    private readonly MongoService _mongo;
    private readonly IConfiguration _config;

    public StudyPlannerService(MongoService mongo, IConfiguration config)
    {
        _mongo = mongo;
        _config = config;
    }

    public async Task<string> GeneratePlan(string userId)
    {
        var user = await _mongo.GetUserByCustomId(userId);
        if (user == null)
            throw new Exception("User not found");

        var results = await _mongo.GetResultsByUser(userId);

        string prompt;

        // 🔰 FIRST TIME USER (NO RESULTS)
        if (results.Count == 0)
        {
            var skills = await _mongo.GetAllSkills();
            var skillNames = skills.Select(s => s.SkillName).ToList();

            prompt = $@"
                        User: {user.FullName}

                        Skills Available:
                        {string.Join(", ", skillNames)}

                        Task:
                        Create a COMPLETE 7-day beginner study plan.

                        For EACH DAY include:
                        - Topic
                        - What to learn
                        - Practice task
                        - 2 YouTube links:
                           • 1 English video
                           • 1 Hindi video
                        - 1 Website/article to read

                        Format strictly like:

                        Day 1:
                        Topic:
                        Learn:
                        Practice:
                        YouTube (English):
                        YouTube (Hindi):
                        Website:

                        Day 2:
                        ...

                        Make it simple and useful.
                        ";
        }
        else
        {
            // 🎯 USER HAS RESULTS → MASTERY PLAN
            var grouped = results.GroupBy(r => r.SkillId);
            var analysis = new StringBuilder();

            foreach (var group in grouped)
            {
                var skill = await _mongo.GetSkillById(group.Key);
                var avg = group.Average(r => r.Percentage);

                analysis.AppendLine($"{skill?.SkillName}: {avg:F1}%");
            }

            prompt = $@"
                        User: {user.FullName}

                        Performance Analysis:
                        {analysis}

                        Task:
                        Create a 7-day ADVANCED mastery study plan.

                        For EACH DAY include:
                        - Focus area (weak skill)
                        - What to improve
                        - Practice task
                        - 2 YouTube links:
                           • 1 English video
                           • 1 Hindi video
                        - 1 Website/article for deep learning

                        Format strictly like:

                        Day 1:
                        Focus:
                        Improve:
                        Practice:
                        YouTube (English):
                        YouTube (Hindi):
                        Website:

                        Day 2:
                        ...

                        Make it practical and improvement-focused.
                        ";
        }

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
            throw new Exception("API Key missing");

        var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await client.PostAsync(
            "https://api.groq.com/openai/v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"AI Error: {json}");

        using var doc = JsonDocument.Parse(json);

        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrEmpty(content))
            throw new Exception("Empty AI response");

        return content.Replace("\\n", "\n");
    }
}