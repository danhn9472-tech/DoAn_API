using DoAn_API.Entities;
namespace DoAn_API.Services
{
    public class NutritionService
    {
        private readonly Dictionary<string, (double Cal, double Pro, double Fat, double Carb)> _ingredientLibrary = new()
        {
            { "Ức gà", (165, 31, 3.6, 0) },
            { "Trứng", (155, 13, 11, 1.1) },
            { "Thịt bò", (250, 26, 15, 0) },
            { "Gạo", (130, 2.7, 0.3, 28) },
            { "Sữa tươi", (42, 3.4, 1, 5) }
        };

        public void Calc(Recipe recipe) 
        {
            double totalCalories = 0;
            double totalProtein = 0;
            double totalFat = 0;
            double totalCarbs = 0;

            if (recipe.RecipeIngredients != null)
            {
                foreach (var item in recipe.RecipeIngredients)
                {
                    // Tìm kiếm gần đúng tên nguyên liệu trong thư viện
                    var nutrition = _ingredientLibrary.FirstOrDefault(x => 
                        item.IngredientName.Contains(x.Key, StringComparison.OrdinalIgnoreCase)); 
                    if (nutrition.Key != null)
                    {
                        // Tính toán dựa trên đơn vị 'g' hoặc 'ml' (giả định 1ml = 1g cho đơn giản) [cite: 55, 56]
                        double ratio = item.Amount / 100.0; 

                        totalCalories += ratio * nutrition.Value.Cal;
                        totalProtein += ratio * nutrition.Value.Pro;
                        totalFat += ratio * nutrition.Value.Fat;
                        totalCarbs += ratio * nutrition.Value.Carb;
                    }
                }
            }

            recipe.TotalCalories = Math.Round(totalCalories, 2);
            recipe.TotalProtein = Math.Round(totalProtein, 2);
            recipe.TotalFat = Math.Round(totalFat, 2);
            recipe.TotalCarbs = Math.Round(totalCarbs, 2);
        }
    }
}
