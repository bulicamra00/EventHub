namespace EventHub.Application.Features.Notifications.Queries.GetMyNotifications;

public class NotificationDTO
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}