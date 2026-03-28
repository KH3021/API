using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MongoService _mongo;

    public AuthController(MongoService mongo)
    {
        _mongo = mongo;
    }

    // ================= REGISTER =================

    [HttpPost("register")]
    public async Task<IActionResult> Register(User user)
    {
        if (string.IsNullOrEmpty(user.FullName) ||
            string.IsNullOrEmpty(user.Email) ||
            string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("All fields required");
        }

        var existingUser = await _mongo.GetUserByEmail(user.Email);

        if (existingUser != null)
        {
            return BadRequest("User already exists ❌");
        }

        // 🔥 AUTO GENERATE USER ID
        user.UserId = await _mongo.GenerateUserId();

        // 🔐 HASH PASSWORD
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        await _mongo.CreateUser(user);

        return Ok(new
        {
            message = "User registered successfully",
            user.UserId,
            user.Email,
            user.FullName,
            user.Role
        });
    }

    // ================= LOGIN =================

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        var user = await _mongo.GetUserByEmail(model.Email);

        if (user == null)
            return Unauthorized("Invalid email or password");

        bool isValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);

        if (!isValid)
            return Unauthorized("Invalid email or password");

        return Ok(new
        {
            userId = user.UserId,
            email = user.Email,
            fullName = user.FullName,
            role = user.Role   // 🔥 IMPORTANT
        });
    }

    // ================= GET USER =================

    [HttpGet("user/{email}")]
    public async Task<IActionResult> GetUser(string email)
    {
        var user = await _mongo.GetUserByEmail(email);

        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.UserId,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedDate
        });
    }
}