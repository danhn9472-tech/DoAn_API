using DoAn_API.Data;
using DoAn_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserActivityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserActivityController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-recipe-book")]
        public async Task<IActionResult> GetMyRecipeBook()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var savedActivities = await _context.UserActivities
                .Where(a => a.UserId == userId && a.IsSaved)
                .Include(a => a.Recipe)
                .Include(a => a.Tip)
                .ToListAsync();

            var result = new SavedItemsDto();

            result.SavedRecipes = savedActivities
                .Where(a => a.RecipeId != null && a.Recipe != null)
                .Select(a => new SavedRecipeDto
                {
                    Id = a.Recipe.Id,
                    Title = a.Recipe.Title,
                    ImageUrl = a.Recipe.ImageUrl,
                    CookTime = a.Recipe.CookTime,
                    TotalCalories = a.Recipe.TotalCalories,
                    AuthorName = a.Recipe.AuthorName ?? "Ẩn danh"
                }).ToList();

            result.SavedTips = savedActivities
                .Where(a => a.TipId != null && a.Tip != null)
                .Select(a => new SavedTipDto
                {
                    Id = a.Tip.Id,
                    Title = a.Tip.Title,
                    ImageUrl = a.Tip.ImageUrl,
                    AuthorName = a.Tip.AuthorName ?? "Ẩn danh",
                    CreatedAt = a.Tip.CreatedAt
                }).ToList();

            return Ok(result);
        }

        [HttpGet("my-posts")]
        [Authorize]
        public async Task<IActionResult> GetMyPosts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var recipes = await _context.Recipes
                .Where(r => r.UserId == userId)
                .Select(r => new MyPostItemDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    ImageUrl = r.ImageUrl,
                    Status = (int)r.Status,
                    CreatedAt = r.CreatedAt,
                    VoteCount = r.VoteCount,
                    SaveCount = r.SaveCount
                }).ToListAsync();

            var tips = await _context.Tips
                .Where(t => t.UserId == userId)
                .Select(t => new MyPostItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    ImageUrl = t.ImageUrl,
                    Status = (int)t.Status,
                    CreatedAt = t.CreatedAt,
                    VoteCount = t.VoteCount,
                    SaveCount = t.SaveCount
                }).ToListAsync();

            return Ok(new MyPostsDto { Recipes = recipes, Tips = tips });
        }
    }
}