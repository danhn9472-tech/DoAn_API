using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn_API.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } 

        [Required]
        public string Type { get; set; } 
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; }
        public virtual ICollection<RecipeCategory> RecipeCategories { get; set; }
    }
}