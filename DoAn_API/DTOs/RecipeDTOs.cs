namespace DoAn_API.DTOs
{
    public class RecipeDTOs
    {
        public class CreateRecipeDto
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int CookTime { get; set; }
            public List<IngredientItemDto> Ingredients { get; set; }
            public List<string> StepDescriptions { get; set; }
        }

        public class IngredientItemDto
        {
            public string Name { get; set; }
            public double Amount { get; set; }
            public string Unit { get; set; }
        }
    }
}
