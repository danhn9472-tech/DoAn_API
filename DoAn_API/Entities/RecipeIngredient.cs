using System.ComponentModel.DataAnnotations.Schema;

﻿namespace DoAn_API.Entities
{
    public class RecipeIngredient
    {
        public int Id { get; set; }
        
        public int RecipeId { get; set; }
        public virtual Recipe Recipe { get; set; }

        public int IngredientId { get; set; } 
        [ForeignKey("IngredientId")]
        public virtual IngredientNutrition Ingredient { get; set; }

        public double Amount { get; set; }
        public string Unit { get; set; } 
    }
}
