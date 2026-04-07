namespace DoAn_API.DTOs
{
    public class UpdateRecipeDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public int CookTime { get; set; }
        public int Difficulty { get; set; }
        public List<int> CategoryIds { get; set; } = new();
        public string? ImageUrl { get; set; }

        public List<UpdateIngredientDto> Ingredients { get; set; } = new();
        public List<UpdateStepDto> Steps { get; set; } = new();

        public class UpdateIngredientDto
        {
            public string IngredientName { get; set; }
            public double Amount { get; set; }
            public string Unit { get; set; }
        }

        public class UpdateStepDto
        {
            public string Instruction { get; set; }
            public string? ImageUrl { get; set; }
        }
    }
}