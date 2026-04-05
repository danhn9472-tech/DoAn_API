namespace DoAn_API.DTOs
{
    public class CategoryDTOs
    {
        public class CategoryTreeDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public List<CategoryTreeDto> SubCategories { get; set; } = new List<CategoryTreeDto>();
        }
    }
}
