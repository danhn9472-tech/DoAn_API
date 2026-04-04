using DoAn_API.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_API.Entities
{
    public class CommentReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Reason { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int CommentId { get; set; }
        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; }
    }
}
