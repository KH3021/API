using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly MongoService _mongo;

    public NotificationController(MongoService mongo)
    {
        _mongo = mongo;
    }

    // 🔹 GET ALL NOTIFICATIONS FOR USER
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetNotifications(string userId)
    {
        var data = await _mongo.GetUserNotifications(userId);

        if (data == null || !data.Any())
            return Ok(new List<object>()); // return empty list instead of null

        return Ok(data);
    }

    // 🔹 MARK SINGLE NOTIFICATION AS READ
    [HttpPost("read/{nid}")]
    public async Task<IActionResult> MarkAsRead(string nid)
    {
        var success = await _mongo.MarkAsReadByNid(nid);

        if (!success)
            return NotFound("Notification not found");

        return Ok("Marked as read");
    }

    // 🔹 MARK ALL AS READ (OPTIONAL BUT VERY USEFUL)
    [HttpPost("read-all/{userId}")]
    public async Task<IActionResult> MarkAllAsRead(string userId)
    {
        await _mongo.MarkAllAsRead(userId);
        return Ok("All notifications marked as read");
    }
}