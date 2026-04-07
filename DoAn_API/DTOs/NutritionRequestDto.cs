namespace DoAn_API.DTOs
{
    public class NutritionRequestDto
    {
        public List<IngredientItemDto> Ingredients { get; set; }
    }

    public class IngredientItemDto
    {
        public string IngredientName { get; set; }
        public double Amount { get; set; }
        public string Unit { get; set; }
    }
}
