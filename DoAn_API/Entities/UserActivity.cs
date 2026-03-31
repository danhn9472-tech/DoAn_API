using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_API.Entities
{
    public class UserActivity
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public int? RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public virtual Recipe? Recipe { get; set; }

        public int? TipId { get; set; }

        [ForeignKey("TipId")]
        public virtual Tip? Tip { get; set; }

        public bool IsVoted { get; set; }
        public bool IsSaved { get; set; }
    }
}
