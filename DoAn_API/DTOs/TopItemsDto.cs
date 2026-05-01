namespace DoAn_API.DTOs
{
    public class TopRecipeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int CookTime { get; set; }
        public double TotalCalories { get; set; }
        public int VoteCount { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorAvatarUrl { get; set; }
    }

    public class TopTipDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VoteCount { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorAvatarUrl { get; set; }
    }
}
