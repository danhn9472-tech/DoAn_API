using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using DoAn_API.Entities.Enums;
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
        public async Task<IActionResult> GetRecipes()
        {
            var recipes = await _context.Recipes
                .Where(r => r.Status == PostStatus.Approved) 
                .Include(r => r.User) 
                .OrderByDescending(r => r.Id)
                .Select(r => new
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    CookTime = r.CookTime,
                    TotalCalories = r.TotalCalories,
                    VoteCount = r.VoteCount,
                    SaveCount = r.SaveCount,
                    UserId = r.UserId,
                    Status = r.Status,
                    AuthorName = r.User != null ? r.User.FullName : "Đầu bếp gia đình"
                })
                .ToListAsync();

            return Ok(recipes);
        }

        // GET: api/Recipes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recipe>> GetRecipe(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .Include(r => r.RecipeSteps)
                .Include(r => r.User)
                .Include(r => r.RecipeCategories)
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
        public async Task<IActionResult> CreateRecipe([FromBody] RecipeDTOs.CreateRecipeRequestDto dto)
        {
            if (dto == null) return BadRequest("Dữ liệu không hợp lệ.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var recipe = new Recipe
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    CookTime = dto.CookTime,
                    Difficulty = (DifficultyLevel)dto.Difficulty,
                    ImageUrl = dto.ImageUrl,
                    AuthorName = dto.AuthorName,
                    Status = PostStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,

                    RecipeCategories = dto.CategoryIds != null
                        ? dto.CategoryIds.Select(cId => new RecipeCategory { CategoryId = cId }).ToList()
                        : new List<RecipeCategory>(),

                    RecipeIngredients = dto.Ingredients != null
                        ? dto.Ingredients.Select(i => new RecipeIngredient
                        {
                            IngredientName = i.IngredientName,
                            Amount = i.Amount,
                            Unit = i.Unit
                        }).ToList()
                        : new List<RecipeIngredient>(),

                    RecipeSteps = dto.Steps != null
                        ? dto.Steps.Select((s, index) => new RecipeStep
                        {
                            StepOrder = index + 1,
                            Content = s.Instruction
                        }).ToList()
                        : new List<RecipeStep>()
                };

                await _nutritionService.CalculateTotalNutritionAsync(recipe);

                // BƯỚC 3: LƯU TOÀN BỘ VÀO DATABASE
                _context.Recipes.Add(recipe);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Tạo công thức thành công, hệ thống đã tự động tính toán dinh dưỡng!", recipeId = recipe.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi máy chủ: " + ex.Message);
            }
        }

        // DELETE: api/Recipes
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Comments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Chỉ chủ sở hữu công thức/Admin mới có quyền xóa
            if (recipe.UserId != userId && !isAdmin)
            {
                return Forbid();
            }

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

        [HttpPost("analyze-nutrition")]
        public async Task<IActionResult> AnalyzeNutrition([FromBody] AnalyzeRequestDTOs dto)
        {
            if (dto.Ingredients == null || !dto.Ingredients.Any())
            {
                return BadRequest(new { message = "Danh sách nguyên liệu không được để trống." });
            }

            var tempRecipe = new Recipe
            {
                RecipeIngredients = dto.Ingredients.Select(i => new RecipeIngredient
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

        [HttpGet("~/api/Nutrition/ingredients")]
        public async Task<IActionResult> GetAllIngredientNames()
        {
            var names = await _context.IngredientNutritions
                                      .Select(n => n.Name)
                                      .OrderBy(n => n)
                                      .ToListAsync();
            return Ok(names);
        }

        [Authorize]
        [HttpGet("my-recipes")]
        public async Task<IActionResult> GetMyRecipes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var myRecipes = await _context.Recipes
                .Where(r => r.UserId == userId)
                .Include(r => r.User)
                .OrderByDescending(r => r.Id)
                .Select(r => new
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    CookTime = r.CookTime,
                    TotalCalories = r.TotalCalories,
                    VoteCount = r.VoteCount,
                    SaveCount = r.SaveCount,
                    UserId = r.UserId,
                    Status = r.Status,
                    AuthorName = r.User != null ? r.User.FullName : "Đầu bếp gia đình"
                })
                .ToListAsync();

            return Ok(myRecipes);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetPendingRecipes()
        {
            var pendingRecipes = await _context.Recipes
                .Where(r => r.Status == PostStatus.Pending)
                .Include(r => r.User)
                .OrderBy(r => r.Id)
                .ToListAsync();

            return Ok(pendingRecipes);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/change-status")]
        public async Task<IActionResult> ChangeRecipeStatus(int id, [FromBody] PostStatus newStatus)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound(new { message = "Không tìm thấy công thức." });

            recipe.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã cập nhật trạng thái bài viết." });
        }

        [HttpGet("filter-by-categories")]
        public async Task<IActionResult> FilterByCategories([FromQuery] List<int> categoryIds)
        {
            var query = _context.Recipes
                .Where(r => r.Status == PostStatus.Approved)
                .AsQueryable();
            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(r => r.RecipeCategories.Any(rc => categoryIds.Contains(rc.CategoryId)));
            }

            var results = await query
                .Include(r => r.User)
                .OrderByDescending(r => r.Id)
                .Select(r => new
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    CookTime = r.CookTime,
                    TotalCalories = r.TotalCalories,
                    VoteCount = r.VoteCount,
                    SaveCount = r.SaveCount,
                    AuthorName = r.User != null ? r.User.FullName : "Đầu bếp gia đình"
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}