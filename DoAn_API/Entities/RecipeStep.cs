using System.ComponentModel.DataAnnotations;

namespace DoAn_API.Entities
{
    public class RecipeStep
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int StepOrder { get; set; }
        [Required]
        public string Content { get; set; }
        public string? ImageUrl { get; set; }

        public virtual Recipe Recipe { get; set; }
    }
}
