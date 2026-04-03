using API.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace API.Services;

public class AnalysisService
{
    private readonly MongoService _mongo;
    private readonly RoadmapService _roadmap;
    private readonly NotificationService _notification;

    public AnalysisService(
        MongoService mongo,
        RoadmapService roadmap,
        NotificationService notification)
    {
        _mongo = mongo;
        _roadmap = roadmap;
        _notification = notification;
    }

    public async Task<byte[]> GenerateAnalysis(string userId)
    {
        // 🔹 Get User
        var user = await _mongo.GetUserByCustomId(userId);
        if (user == null)
            throw new Exception("User not found");

        // 🔹 Get Skills
        var userSkills = await _mongo.GetUserSkills(userId);
        var skills = new List<string>();

        foreach (var us in userSkills)
        {
            var skill = await _mongo.GetSkillById(us.SkillId);
            if (skill != null)
            {
                string level = string.IsNullOrEmpty(us.Level) ? "Beginner" : us.Level;
                skills.Add($"{skill.SkillName} - {level}");
            }
        }

        // 🔹 Get Performance
        var results = await _mongo.GetResultsByUser(userId);
        var performance = new List<string>();

        foreach (var r in results)
        {
            var skill = await _mongo.GetSkillById(r.SkillId);
            string skillName = skill != null ? skill.SkillName : r.SkillId;

            double score = Math.Round(r.Percentage, 1);
            performance.Add($"{skillName} - Score: {score}%");
        }

        // 🔹 Get Roadmap (AI with fallback)
        string roadmapText = "";
        try
        {
            var aiResult = await _roadmap.GenerateRoadmap(userId);

            roadmapText = string.IsNullOrEmpty(aiResult)
                ? "• Improve weak skills\n• Practice daily\n• Build projects"
                : aiResult;
        }
        catch
        {
            roadmapText = "• Improve weak skills\n• Practice daily\n• Build projects";
        }

        // 🔹 Clean AI Text
        var cleanText = CleanAIText(roadmapText);
        var lines = cleanText.Split('\n');

        // 🔹 Create PDF
        using var stream = new MemoryStream();
        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var doc = new Document(pdf);

        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        // 🔹 Title
        doc.Add(new Paragraph("SKILL ANALYSIS REPORT")
            .SetFont(boldFont)
            .SetFontSize(20));

        // 🔹 User Info
        doc.Add(new Paragraph($"Name: {user.FullName}").SetFont(normalFont));
        doc.Add(new Paragraph($"Email: {user.Email}").SetFont(normalFont));

        // 🔹 Objective
        doc.Add(new Paragraph("\nObjective:").SetFont(boldFont));
        doc.Add(new Paragraph("Aspiring developer with strong interest in learning new technologies.")
            .SetFont(normalFont));

        // 🔹 Skills
        doc.Add(new Paragraph("\nSkills:").SetFont(boldFont));

        if (skills.Count == 0)
        {
            doc.Add(new Paragraph("No skills added yet").SetFont(normalFont));
        }
        else
        {
            foreach (var s in skills)
            {
                doc.Add(new Paragraph($"• {s}").SetFont(normalFont));
            }
        }

        // 🔹 Total Skills
        doc.Add(new Paragraph($"\nTotal Skills: {skills.Count}")
            .SetFont(boldFont));

        // 🔹 Performance
        doc.Add(new Paragraph("\nPerformance:").SetFont(boldFont));

        if (performance.Count == 0)
        {
            doc.Add(new Paragraph("No test results available").SetFont(normalFont));
        }
        else
        {
            foreach (var p in performance)
            {
                doc.Add(new Paragraph($"• {p}").SetFont(normalFont));
            }
        }

        // 🔹 Separator
        doc.Add(new Paragraph("\n--------------------------------\n")
            .SetFont(normalFont));

        // 🔥 Roadmap Section
        doc.Add(new Paragraph("Recommended Learning Roadmap:")
            .SetFont(boldFont));

        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var text = line.Trim();

                if (text.Contains("Strength") ||
                    text.Contains("Weakness") ||
                    text.Contains("Improvement") ||
                    text.Contains("Practice") ||
                    text.Contains("Conclusion") ||
                    text.Contains("Introduction"))
                {
                    doc.Add(new Paragraph("\n" + text)
                        .SetFont(boldFont)
                        .SetFontSize(12));
                }
                else
                {
                    doc.Add(new Paragraph(text)
                        .SetFont(normalFont));
                }
            }
        }

        // 🔹 Date
        doc.Add(new Paragraph($"\nGenerated on: {DateTime.Now:dd MMM yyyy}")
            .SetFont(normalFont));

        doc.Close();

        // 🔔 Notification Trigger
        await _notification.SendOrUpdateNotification(
            userId,
            "Your skill analysis report is ready 📊"
        );

        return stream.ToArray();
    }

    // 🔥 Markdown Cleaner
    private string CleanAIText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text
            .Replace("**", "")
            .Replace("*", "")
            .Replace("#", "")
            .Replace("\r", "")
            .Trim();
    }
}