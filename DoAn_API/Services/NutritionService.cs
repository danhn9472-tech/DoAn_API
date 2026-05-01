using DoAn_API.Data;
using DoAn_API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DoAn_API.Services
{
    public class NutritionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public NutritionService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task CalculateTotalNutritionAsync(Recipe recipe)
        {
            if (recipe.RecipeIngredients == null || !recipe.RecipeIngredients.Any()) return;

            double totalCalories = 0, totalProtein = 0, totalFat = 0, totalCarbs = 0;

            // Kiểm tra xem dữ liệu đã có trong Cache chưa
            if (!_cache.TryGetValue("AllIngredientsNutrition", out List<IngredientNutrition> allNutritions))
            {
                // Nếu chưa có (Lần gọi đầu tiên hoặc Cache đã hết hạn), lấy từ DB
                allNutritions = await _context.IngredientNutritions.ToListAsync();
                
                // Lưu vào Cache, thiết lập thời gian sống là 24 giờ
                _cache.Set("AllIngredientsNutrition", allNutritions, TimeSpan.FromHours(24));
            }

            foreach (var item in recipe.RecipeIngredients)
            {
                var nutrition = allNutritions.FirstOrDefault(n => n.Name.Equals(item.Ingredient.Name, StringComparison.OrdinalIgnoreCase));

                if (nutrition == null)
                {
                    nutrition = allNutritions.FirstOrDefault(n =>
                        item.Ingredient.Name.ToLower().Contains(n.Name.ToLower()) ||
                        n.Name.ToLower().Contains(item.Ingredient.Name.ToLower()));
                }

                if (nutrition != null)
                {
                    double amountInGrams = ConvertToGrams(item.Amount, item.Unit);

                    double ratio = amountInGrams / 100.0;

                    totalCalories += ratio * nutrition.Calories;
                    totalProtein += ratio * nutrition.Protein;
                    totalFat += ratio * nutrition.Fat;
                    totalCarbs += ratio * nutrition.Carbs;
                }
            }

            recipe.TotalCalories = Math.Round(totalCalories, 2);
            recipe.TotalProtein = Math.Round(totalProtein, 2);
            recipe.TotalFat = Math.Round(totalFat, 2);
            recipe.TotalCarbs = Math.Round(totalCarbs, 2);
        }


        private double ConvertToGrams(double amount, string unit)
        {
            if (string.IsNullOrWhiteSpace(unit)) return amount;

            string u = unit.ToLower().Trim();

            switch (u)
            {
                case "kg":
                case "kilogram":
                    return amount * 1000.0;

                case "lít":
                case "l":
                    return amount * 1000.0;

                case "ml":
                    return amount * 1.0;

                case "muỗng canh":
                case "tbsp":
                    return amount * 15.0;

                case "lá":
                    return amount * 2.0;

                case "cọng":
                    return amount * 10.0;

                case "chén":
                    return amount * 150.0;

                case "muỗng cà phê":
                case "tsp":
                    return amount * 5.0;

                case "quả":
                    return amount * 55.0;

                case "gram":
                case "g":
                case "gr":
                default:
                    return amount;
            }
        }
    }
}