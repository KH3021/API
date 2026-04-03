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
            // 🔄 UPDATE existing notification
            await _mongo.UpdateNotificationTime(existing.Id);
        }
        else
        {
            // ➕ CREATE new notification
            var nid = await _mongo.GenerateNotificationId();

            var notification = new Notification
            {
                Nid = nid,
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            await _mongo.AddNotification(notification);
        }
    }
}