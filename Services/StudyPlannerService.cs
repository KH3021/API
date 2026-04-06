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

        // 🔰 NO RESULTS → BEGINNER PLAN
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

Each day must include:
- Topic
- What to learn
- Practice task
- 2 YouTube links (English + Hindi)
- 1 Website

Format:

Day 1:
Topic:
Learn:
Practice:
YouTube (English):
YouTube (Hindi):
Website:

Day 2:
...

Make it simple and beginner friendly.
";
        }
        else
        {
            // 🔥 STEP 1: ANALYZE ALL RESULTS
            var grouped = results
                .GroupBy(r => r.SkillId)
                .Select(g => new
                {
                    SkillId = g.Key,
                    Attempts = g.Count(),
                    Avg = g.Average(x => x.Percentage)
                })
                .ToList();

            var analysis = new StringBuilder();
            var weakSkills = new StringBuilder();

            // 🔥 STEP 2: SORT (WEAKEST FIRST)
            var sorted = grouped.OrderBy(x => x.Avg).ToList();

            foreach (var g in sorted)
            {
                var skill = await _mongo.GetSkillById(g.SkillId);
                var skillName = skill?.SkillName ?? "Unknown";

                string level;

                if (g.Avg < 40)
                    level = "Weak";
                else if (g.Avg < 70)
                    level = "Average";
                else
                    level = "Strong";

                analysis.AppendLine(
                    $"{skillName} | Attempts: {g.Attempts} | Avg: {g.Avg:F1}% | Level: {level}"
                );

                // 🔥 collect weak skills (top 3)
                if (g.Avg < 70 && weakSkills.ToString().Split('\n').Length <= 3)
                {
                    weakSkills.AppendLine(skillName);
                }
            }

            // 🔥 STEP 3: STRONG PROMPT
            prompt = $@"
User: {user.FullName}

Full Performance Analysis:
{analysis}

Important:
Focus FIRST on weak and average skills:
{weakSkills}

Task:
Create a 7-day ADVANCED study plan.

Rules:
- Start from weakest skills
- Gradually improve to stronger skills
- Include daily improvement tasks
- Include practice
- Include:
   • 1 YouTube English
   • 1 YouTube Hindi
   • 1 Website

STRICT FORMAT:

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