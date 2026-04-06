using DoAn_API.Entities.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_API.Entities
{
    public class Tip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int VoteCount { get; set; } = 0;
        public int SaveCount { get; set; } = 0;
        // Khóa ngoại liên kết với người dùng
        [Required]
        public string UserId { get; set; }
        public string? AuthorName { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public PostStatus Status { get; set; } = PostStatus.Pending;
    }
}
