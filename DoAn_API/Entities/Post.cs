using DoAn_API.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_API.Entities
{
    public abstract class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int VoteCount { get; set; } = 0;
        public int SaveCount { get; set; } = 0;

        [Required]
        public string UserId { get; set; }
        public string? AuthorName { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public PostStatus Status { get; set; } = PostStatus.Pending;

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<UserActivity> Activities { get; set; }
    }
}