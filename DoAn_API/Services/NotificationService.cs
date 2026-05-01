using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SendNotificationAsync(string userId, string message, string type, int? referenceId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                ReferenceId = referenceId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20) 
                .Select(n => new NotificationDto
                {
                    Id = n.Id, Message = n.Message, Type = n.Type, 
                    ReferenceId = n.ReferenceId, IsRead = n.IsRead, CreatedAt = n.CreatedAt
                }).ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}