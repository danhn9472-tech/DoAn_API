using DoAn_API.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DoAn_API.Entities
{
    public class Recipe
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int CookTime { get; set; }
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalFat { get; set; }
        public double TotalCarbs { get; set; }
        public int VoteCount { get; set; } = 0;
        public int SaveCount { get; set; } = 0;

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; }
        public virtual ICollection<RecipeStep> RecipeSteps { get; set; }
        public PostStatus Status { get; set; } = PostStatus.Pending;
        public virtual ICollection<RecipeCategory> RecipeCategories { get; set; }
    }
}
