namespace DoAn_API.Entities
{
    public class AiIngredientResponse
    {
        public string IngredientName { get; set; }
        public double Amount { get; set; }
        public string Unit { get; set; } = "gram";
    }
}
