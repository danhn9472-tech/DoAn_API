namespace DoAn_API.DTOs
{
    public class MyPostsDto
    {
        public List<MyPostItemDto> Recipes { get; set; } = new();
        public List<MyPostItemDto> Tips { get; set; } = new();
    }

    public class MyPostItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VoteCount { get; set; }
        public int SaveCount { get; set; }
    }
}
