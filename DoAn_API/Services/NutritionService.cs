using DoAn_API.Data;
using DoAn_API.Entities;
using Microsoft.EntityFrameworkCore;
namespace DoAn_API.Services
{
    public class NutritionService
    {
        private readonly ApplicationDbContext _context;

        public NutritionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CalculateTotalNutritionAsync(Recipe recipe)
        {
            double totalCalories = 0;
            double totalProtein = 0;
            double totalFat = 0;
            double totalCarbs = 0;

            if (recipe.RecipeIngredients != null)
            {
                foreach (var item in recipe.RecipeIngredients)
                {
                    // Dò tìm nguyên liệu trong Database dựa trên tên người dùng nhập
                    var nutrition = await _context.IngredientNutritions
                        .FirstOrDefaultAsync(n => item.IngredientName.ToLower().Contains(n.Name.ToLower())
                                               || n.Name.ToLower().Contains(item.IngredientName.ToLower()));

                    if (nutrition != null)
                    {
                        // Tính toán tỷ lệ dựa trên khối lượng 
                        double ratio = item.Amount / 100.0;

                        totalCalories += ratio * nutrition.Calories;
                        totalProtein += ratio * nutrition.Protein;
                        totalFat += ratio * nutrition.Fat;
                        totalCarbs += ratio * nutrition.Carbs;
                    }
                }
            }

            // Lưu kết quả cuối cùng vào Recipe 
            recipe.TotalCalories = Math.Round(totalCalories, 2); 
            recipe.TotalProtein = Math.Round(totalProtein, 2); 
            recipe.TotalFat = Math.Round(totalFat, 2);
            recipe.TotalCarbs = Math.Round(totalCarbs, 2);
        }
    }
}
