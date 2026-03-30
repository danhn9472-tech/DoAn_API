namespace DoAn_API.Entities
{
    public class RecipeIngredient
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string IngredientName { get; set; }
        public double Amount { get; set; }
        public string Unit { get; set; } 
        public virtual Recipe Recipe { get; set; }
    }
}
