using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.Models;

[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly MongoService _mongo;

    public SkillsController(MongoService mongo)
    {
        _mongo = mongo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSkills()
    {
        return Ok(await _mongo.GetAllSkills());
    }

    [HttpPost]
    public async Task<IActionResult> AddSkill(Skill skill)
    {
        skill.Id = null;
        await _mongo.AddSkill(skill);
        return Ok("Skill Added");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSkill(string id)
    {
        var skill = await _mongo.GetSkillById(id);
        if (skill == null) return NotFound();
        return Ok(skill);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSkill(string id)
    {
        await _mongo.DeleteSkill(id);
        return Ok("Deleted");
    }
}