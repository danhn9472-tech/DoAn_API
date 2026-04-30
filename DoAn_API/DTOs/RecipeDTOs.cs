﻿﻿﻿namespace DoAn_API.DTOs
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

        public class PaginatedRecipeResponseDto
        {
            public IEnumerable<RecipeListItemDto> Data { get; set; }
            public int CurrentPage { get; set; }
            public int PageSize { get; set; }
            public int TotalItems { get; set; }
            public int TotalPages { get; set; }
        }

        public class RecipeListItemDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string? Description { get; set; }
            public string? ImageUrl { get; set; }
            public int CookTime { get; set; }
            public DoAn_API.Entities.Enums.DifficultyLevel Difficulty { get; set; }
            public double TotalCalories { get; set; }
            public int VoteCount { get; set; }
            public int SaveCount { get; set; }
            public string UserId { get; set; }
            public DoAn_API.Entities.Enums.PostStatus Status { get; set; }
            public string? AuthorName { get; set; }
            public List<CategoryDto> Categories { get; set; }
        }

        public class CategoryDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class RecipeDetailDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string? Description { get; set; }
            public string? ImageUrl { get; set; }
            public int CookTime { get; set; }
            public double TotalCalories { get; set; }
            public double TotalProtein { get; set; }
            public double TotalFat { get; set; }
            public double TotalCarbs { get; set; }
            public DoAn_API.Entities.Enums.DifficultyLevel Difficulty { get; set; }
            public string? AuthorName { get; set; }
            public string UserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public int VoteCount { get; set; }
            public int SaveCount { get; set; }
            public List<CategoryDto> Categories { get; set; }
            public List<RecipeIngredientDetailDto> Ingredients { get; set; }
            public List<RecipeStepDetailDto> Steps { get; set; }
        }

        public class RecipeIngredientDetailDto
        {
            public string IngredientName { get; set; }
            public double Amount { get; set; }
            public string Unit { get; set; }
        }

        public class RecipeStepDetailDto
        {
            public int StepOrder { get; set; }
            public string Content { get; set; }
            public string? ImageUrl { get; set; }
        }
    }
}
