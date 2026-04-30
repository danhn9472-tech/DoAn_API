﻿namespace DoAn_API.DTOs
{
    public class MyPostsDto
    {
        public List<MyRecipeItemDto> Recipes { get; set; } = new();
        public List<MyTipItemDto> Tips { get; set; } = new();
    }

    public class UserProfileDetailDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int TotalRecipes { get; set; }
        public int TotalTips { get; set; }
        public List<MyRecipeItemDto> Recipes { get; set; } = new();
        public List<MyTipItemDto> Tips { get; set; } = new();
    }

    public class MyRecipeItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int CookTime { get; set; }
        public double TotalCalories { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VoteCount { get; set; }
        public int SaveCount { get; set; }
    }

    public class MyTipItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VoteCount { get; set; }
        public int SaveCount { get; set; }
    }
}
