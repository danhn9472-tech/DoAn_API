﻿﻿﻿﻿﻿namespace DoAn_API.DTOs
{
    public class SavedItemsDto
    {
        public List<SavedRecipeDto> SavedRecipes { get; set; } = new List<SavedRecipeDto>();
        public List<SavedTipDto> SavedTips { get; set; } = new List<SavedTipDto>();
    }

    public class SavedRecipeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Slug { get; set; }
        public string? ImageUrl { get; set; }
        public int CookTime { get; set; }
        public DoAn_API.Entities.Enums.DifficultyLevel Difficulty { get; set; }
        public double TotalCalories { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorAvatarUrl { get; set; }
        public string UserId { get; set; }
    }

    public class SavedTipDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Slug { get; set; }
        public string? ImageUrl { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorAvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}