using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using DoAn_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NutritionController : ControllerBase
    {
        private readonly NutritionService _nutritionService;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public NutritionController(NutritionService nutritionService, ApplicationDbContext context, IConfiguration configuration)
        {
            _nutritionService = nutritionService;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] List<IngredientItemDto> ingredients)
        {
            if (ingredients == null || !ingredients.Any()) return BadRequest();

            var tempRecipe = new Recipe
            {
                RecipeIngredients = ingredients.Select(i => new RecipeIngredient
                {
                    IngredientName = i.IngredientName,
                    Amount = i.Amount,
                    Unit = i.Unit
                }).ToList()
            };

            await _nutritionService.CalculateTotalNutritionAsync(tempRecipe);

            return Ok(new
            {
                calories = tempRecipe.TotalCalories,
                protein = tempRecipe.TotalProtein,
                fat = tempRecipe.TotalFat,
                carbs = tempRecipe.TotalCarbs
            });
        }

        [HttpGet("search-ingredients")]
        public async Task<IActionResult> SearchIngredients(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Ok(new List<string>());

            var suggestions = await _context.IngredientNutritions
                .Where(i => i.Name.ToLower().Contains(term.ToLower()))
                .Select(i => i.Name)
                .Take(10)
                .ToListAsync();

            return Ok(suggestions);
        }
    }
}
