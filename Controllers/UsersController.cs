using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly MongoService _mongo;

    public UsersController(MongoService mongo)
    {
        _mongo = mongo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _mongo.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser(string userId)
    {
        var user = await _mongo.GetUserByCustomId(userId);

        if (user == null)
            return NotFound("User not found");

        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _mongo.GetUserByEmail(email);

        if (user == null)
            return NotFound("User not found");

        return Ok(user);
    }

    // 🔥 UPDATED SAFE PUT
    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, UpdateUserDto dto)
    {
        var success = await _mongo.UpdateUserSafe(userId, dto);

        if (!success)
            return NotFound("User not found");

        return Ok("User updated successfully");
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var success = await _mongo.DeleteUser(userId);

        if (!success)
            return NotFound("User not found");

        return Ok("User deleted successfully");
    }
}