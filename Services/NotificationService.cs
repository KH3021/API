using API.Models;

namespace API.Services;

public class NotificationService
{
    private readonly MongoService _mongo;

    public NotificationService(MongoService mongo)
    {
        _mongo = mongo;
    }

    public async Task SendOrUpdateNotification(string userId, string message)
    {
        var existing = await _mongo.GetUserNotificationByMessage(userId, message);

        if (existing != null)
        {
            // 🔥 FORCE UPDATE (IMPORTANT FIX)
            existing.IsRead = false; // ✅ reset so frontend treats it as NEW
            existing.CreatedAt = DateTime.UtcNow; // ✅ update timestamp

            await _mongo.UpdateNotification(existing); // 🔥 must update full object
        }
        else
        {
            // ➕ CREATE NEW
            var nid = await _mongo.GenerateNotificationId();

            var notification = new Notification
            {
                Nid = nid,
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _mongo.AddNotification(notification);
        }
    }
}