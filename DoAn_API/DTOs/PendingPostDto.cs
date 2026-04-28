namespace DoAn_API.DTOs
{
    public class PendingPostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } 
        public string? ImageUrl { get; set; }
    }
}
