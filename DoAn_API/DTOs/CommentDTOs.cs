namespace DoAn_API.DTOs
{
    public class CommentDto
    {
        public string Content { get; set; }
        public int? RecipeId { get; set; }
        public int? TipId { get; set; }
    }
}
