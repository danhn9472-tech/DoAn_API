using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using DoAn_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly NutritionService _nutritionService;

        public RecipesController(ApplicationDbContext context, NutritionService nutritionService)
        {
            _context = context;
            _nutritionService = nutritionService;
        }

        // GET: api/Recipes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipes()
        {
            return await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .Include(r => r.RecipeSteps)
                .OrderByDescending(r => r.Id)
                .ToListAsync();
        }

        // GET: api/Recipes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recipe>> GetRecipe(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .Include(r => r.RecipeSteps)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound(new { message = "Không tìm thấy công thức này." });
            }

            return recipe;
        }

        // POST: api/Recipes
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Recipe>> PostRecipe([FromBody] RecipeDTOs.CreateRecipeDto dto)
        {
            // Lấy UserId từ JWT Token của người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Chuyển đổi từ DTO sang Entity
            var recipe = new Recipe
            {
                Title = dto.Title,
                Description = dto.Description,
                CookTime = dto.CookTime,
                UserId = userId,

                // Map danh sách nguyên liệu
                RecipeIngredients = dto.Ingredients.Select(i => new RecipeIngredient
                {
                    IngredientName = i.Name,
                    Amount = i.Amount,
                    Unit = i.Unit
                }).ToList(),

                // Map danh sách các bước nấu (tự động đánh số thứ tự)
                RecipeSteps = dto.StepDescriptions.Select((content, index) => new RecipeStep
                {
                    Content = content,
                    StepOrder = index + 1
                }).ToList()
            };

            // Gọi Service tính toán dinh dưỡng (Protein, Calo...)
            _nutritionService.Calc(recipe);

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipe);
        }

        // DELETE: api/Recipes/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Comments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (recipe.UserId != userId && !User.IsInRole("Admin")) return Forbid();

            // Xóa tương tác và bình luận trước 
            var activities = _context.UserActivities.Where(ua => ua.RecipeId == id);
            _context.UserActivities.RemoveRange(activities);

            if (recipe.Comments != null && recipe.Comments.Any())
            {
                _context.Comments.RemoveRange(recipe.Comments);
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa công thức thành công." });
        }
    }
}
