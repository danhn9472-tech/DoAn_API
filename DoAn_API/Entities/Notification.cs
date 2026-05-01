using System;

namespace DoAn_API.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } // ID của người sẽ NHẬN thông báo
        public string Message { get; set; } // Nội dung thông báo
        public string Type { get; set; } // Loại thông báo: "Approval", "Comment", "Vote"
        public int? ReferenceId { get; set; } // ID bài viết liên quan để khi User bấm vào thì chuyển hướng
        public bool IsRead { get; set; } = false; // Đã đọc chưa?
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}