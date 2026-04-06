namespace DoAn_API.DTOs
{
    public class TipDTOs
    {
        public class CreateTipDto
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public string? ImageUrl { get; set; }
            public string? AuthorName { get; set; }
        }
    }
}
