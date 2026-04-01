using System.ComponentModel.DataAnnotations;

namespace DoAn_API.Entities
{
    public class IngredientNutrition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } 

        public double Calories { get; set; } 
        public double Protein { get; set; }  
        public double Fat { get; set; }      
        public double Carbs { get; set; }   
    }
}
