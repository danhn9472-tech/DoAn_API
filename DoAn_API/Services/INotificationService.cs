using DoAn_API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string message, string type, int? referenceId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
    }
}