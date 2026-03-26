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
        var prompt = $"Generate 5 {level} level MCQ questions for {skill}. " +
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

        // 🔥 CLEAN JSON (IMPROVED)
        content = content.Trim()
                         .Replace("```json", "")
                         .Replace("```", "")
                         .Replace("\\n", "")
                         .Replace("\\\"", "\"");

        // 🔥 Extract only JSON array
        int start = content.IndexOf('[');
        int end = content.LastIndexOf(']');

        if (start != -1 && end != -1)
        {
            content = content.Substring(start, end - start + 1);
        }

        try
        {
            List<Question> questions;

            // 🔥 First attempt (normal JSON)
            try
            {
                questions = JsonSerializer.Deserialize<List<Question>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                // 🔥 If failed → it is STRING JSON → deserialize twice
                var innerJson = JsonSerializer.Deserialize<string>(content);

                questions = JsonSerializer.Deserialize<List<Question>>(innerJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return Ok(questions);
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