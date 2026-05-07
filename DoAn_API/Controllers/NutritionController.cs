using DoAn_API.DTOs;
using DoAn_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NutritionController : ControllerBase
    {
        private readonly NutritionService _nutritionService;

        public NutritionController(NutritionService nutritionService)
        {
            _nutritionService = nutritionService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] List<IngredientItemDto> ingredients)
        {
            var result = await _nutritionService.CalculateNutritionFromIngredientsAsync(ingredients);
            
            if (result == null) return BadRequest();
            
            return Ok(result);
        }

        [HttpGet("search-ingredients")]
        public async Task<IActionResult> SearchIngredients(string term)
        {
            var suggestions = await _nutritionService.SearchIngredientsAsync(term);
            return Ok(suggestions);
        }
    }
}
