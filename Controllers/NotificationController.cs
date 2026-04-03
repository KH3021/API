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

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetNotifications(string userId)
    {
        var data = await _mongo.GetUserNotifications(userId);
        return Ok(data);
    }

    [HttpPost("read/{nid}")]
    public async Task<IActionResult> MarkAsRead(string nid)
    {
        await _mongo.MarkAsReadByNid(nid);
        return Ok("Marked as read");
    }
}