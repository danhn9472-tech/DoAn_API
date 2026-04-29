using DoAn_API.Data;
using DoAn_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DoAn_API.Entities;

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
                .Include(a => a.Post)
                .ToListAsync();

            var result = new SavedItemsDto();

            result.SavedRecipes = savedActivities
                .Where(a => a.Post is Recipe)
                .Select(a => (Recipe)a.Post)
                .Select(r => new SavedRecipeDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    ImageUrl = r.ImageUrl,
                    CookTime = r.CookTime,
                    TotalCalories = r.TotalCalories,
                    AuthorName = r.AuthorName ?? "Ẩn danh"
                }).ToList();

            result.SavedTips = savedActivities
                .Where(a => a.Post is Tip)
                .Select(a => (Tip)a.Post)
                .Select(t => new SavedTipDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    ImageUrl = t.ImageUrl,
                    AuthorName = t.AuthorName ?? "Ẩn danh",
                    CreatedAt = t.CreatedAt
                }).ToList();

            return Ok(result);
        }

        [HttpGet("my-posts")]
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