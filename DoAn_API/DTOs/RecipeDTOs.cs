﻿namespace DoAn_API.DTOs
{
    public class RecipeDTOs
    {
        public class CreateRecipeRequestDto
        {
            public string Title { get; set; }
            public string? Description { get; set; }
            public int CookTime { get; set; }
            public int TotalCalories { get; set; }
            public int Difficulty { get; set; }
            public string? ImageUrl { get; set; }
            public string? AuthorName { get; set; }

            public List<int> CategoryIds { get; set; } = new List<int>();
            public List<IngredientDto> Ingredients { get; set; } = new List<IngredientDto>();
            public List<StepDto> Steps { get; set; } = new List<StepDto>();
        }

        public class IngredientDto
        {
            public int IngredientId { get; set; }
            public Double Amount { get; set; }
            public string Unit { get; set; }
        }

        public class StepDto
        {
            public int StepNumber { get; set; }
            public string Instruction { get; set; }
            public string? ImageUrl { get; set; } 
        }
    }
}
