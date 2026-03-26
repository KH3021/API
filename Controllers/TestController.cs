using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly MongoService _mongo;

    public TestController(IConfiguration config, MongoService mongo)
    {
        _config = config;
        _mongo = mongo;
    }

    // ================= GENERATE TEST =================

    [HttpGet("generate/{skill}/{level}/{set}")]
    public async Task<IActionResult> GenerateTest(string skill, string level, int set)
    {
        var apiKey = _config["Groq:ApiKey"];

        // 🔥 UPDATED PROMPT (STRICT JSON)
        var prompt = $"Generate 5 UNIQUE {level} level MCQ questions for {skill}. " +
             $"This is question set number {set}. Do NOT repeat questions from other sets. " +
             $"Each set must be different. " +
             $"Each question MUST have exactly 4 options only. Not more, not less. " +
             $"Include code-based questions. " +
             $"Return ONLY valid JSON array. No explanation, no markdown. " +
             $"Format strictly: " +
             $"[{{\"questionText\":\"\",\"options\":[\"A\",\"B\",\"C\",\"D\"],\"answer\":\"A\"}}]";
        var requestBody = new
        {
            model = "llama-3.1-8b-instant",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

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

        // 🔥 CLEAN RESPONSE (STRONG VERSION)
        content = content.Trim()
                         .Replace("```json", "")
                         .Replace("```", "")
                         .Replace("\\n", " ")
                         .Replace("\\\"", "\"");

        // 🔥 REMOVE INVALID ESCAPE CHARACTERS (KEY FIX)
        content = System.Text.RegularExpressions.Regex.Replace(content, @"\\(?![""\\/bfnrtu])", "");

        // 🔥 REMOVE BAD SINGLE QUOTES
        content = content.Replace("\\'", "'");

        // 🔥 Extract JSON array safely
        int start = content.IndexOf('[');
        int end = content.LastIndexOf(']');

        if (start != -1 && end != -1)
        {
            content = content.Substring(start, end - start + 1);
        }

        // 🔥 FIX BROKEN JSON STRUCTURE
        content = content.Replace("}{", "},{");

        // 🔥 REMOVE TRAILING GARBAGE
        content = content.TrimEnd(',', ';');

        // 🔥 TRY PARSING
        try
        {
            List<Question> questions;

            // Case 1: Wrapped JSON string
            if (content.StartsWith("\""))
            {
                content = JsonSerializer.Deserialize<string>(content);
            }

            // Case 2: Fix broken separators
            content = content.Replace("}{", "},{");

            // 🔥 Parse into JsonDocument first (SAFE MODE)
            using var jsonDoc = JsonDocument.Parse(content);

            var list = new List<Question>();

            foreach (var item in jsonDoc.RootElement.EnumerateArray())
            {
                var options = item.GetProperty("options")
                                  .EnumerateArray()
                                  .Select(x => x.GetString())
                                  .Where(x => !string.IsNullOrEmpty(x))
                                  .Take(4) // 🔥 LIMIT TO 4 OPTIONS (KEY FIX)
                                  .ToList();

                list.Add(new Question
                {
                    QuestionText = item.GetProperty("questionText").GetString(),
                    Options = options,
                    Answer = item.GetProperty("answer").GetString()
                });
            }

            return Ok(list);
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                error = "Parsing failed",
                message = ex.Message,
                raw = content
            });
        }
    }

    // ================= SUBMIT TEST =================

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitTest([FromBody] SubmitRequest request)
    {
        // 🔥 Validate user
        var user = await _mongo.GetUserByCustomId(request.UserId);

        if (user == null)
            return BadRequest("User not found");

        int score = 0;
        int total = request.Answers.Count;

        foreach (var q in request.Answers)
        {
            if (q.SelectedAnswer == q.CorrectAnswer)
                score++;
        }

        double percentage = (double)score / total * 100;

        string resultText = percentage < 40 ? "Fail"
                            : percentage < 75 ? "Average"
                            : "Excellent";

        var result = new Result
        {
            UserId = request.UserId,
            SkillName = request.SkillName,
            Level = request.Level,
            Score = score,
            Total = total,
            Percentage = percentage,
            ResultText = resultText
        };

        await _mongo.SaveResult(result);

        return Ok(result);
    }

    // ================= USER RESULTS =================

    [HttpGet("results/{userId}")]
    public async Task<IActionResult> GetResults(string userId)
    {
        var results = await _mongo.GetResultsByUser(userId);
        return Ok(results);
    }
}