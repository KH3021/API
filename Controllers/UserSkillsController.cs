using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserSkillsController : ControllerBase
{
    private readonly MongoService _mongo;
    private readonly NotificationService _notification;

    public UserSkillsController(MongoService mongo, NotificationService notification)
    {
        _mongo = mongo;
        _notification = notification;
    }

    // Add skill to user (with validation)
    [HttpPost("add")]
    public async Task<IActionResult> AddSkill(UserSkill userSkill)
    {
        // Check if user exists
        var user = await _mongo.GetUserByCustomId(userSkill.UserId);
        if (user == null)
            return BadRequest("User not found");

        // Check if skill exists
        var skill = await _mongo.GetSkillById(userSkill.SkillId);
        if (skill == null)
            return BadRequest("Invalid SkillId");

        var result = await _mongo.AddUserSkill(userSkill);

        // 🔔 SEND NOTIFICATION
        if (result == "Skill added")
        {
            await _notification.SendOrUpdateNotification(
                userSkill.UserId,
                $"New skill '{skill.SkillName}' added 🚀"
            );
        }

        return Ok(result);
    }

    // Get all skills of user
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetSkills(string userId)
    {
        var skills = await _mongo.GetUserSkills(userId);
        return Ok(skills);
    }

    // Delete skill
    [HttpDelete("{userId}/{skillId}")]
    public async Task<IActionResult> DeleteSkill(string userId, string skillId)
    {
        var success = await _mongo.DeleteUserSkill(userId, skillId);

        if (!success)
            return NotFound("Skill not found");

        return Ok("Skill removed");
    }
}