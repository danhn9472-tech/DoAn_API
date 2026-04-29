﻿using DoAn_API.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DoAn_API.Entities
{
    public class Recipe : Post
    {
        public string? Description { get; set; }
        public int CookTime { get; set; }
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalFat { get; set; }
        public double TotalCarbs { get; set; }
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; }
        public virtual ICollection<RecipeStep> RecipeSteps { get; set; }
        public virtual ICollection<RecipeCategory> RecipeCategories { get; set; }
    }
}
