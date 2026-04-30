using DoAn_API.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_API.Entities
{
    public class PostReport
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

        public int? RecipeId { get; set; }
        [ForeignKey("RecipeId")]
        public virtual Recipe? Recipe { get; set; }

        public int? TipId { get; set; }
        [ForeignKey("TipId")]
        public virtual Tip? Tip { get; set; }
    }
}