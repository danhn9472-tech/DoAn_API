using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAn_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InteractionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public InteractionController(ApplicationDbContext context) => _context = context;

        // VOTE (LIKE) CÔNG THỨC
        [HttpPost("vote-recipe/{id}")]
        public async Task<IActionResult> VoteRecipe(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) return NotFound();

            var activity = await _context.UserActivities
                .FirstOrDefaultAsync(x => x.UserId == userId && x.RecipeId == id);

            if (activity == null)
            {
                _context.UserActivities.Add(new UserActivity { UserId = userId, RecipeId = id, IsVoted = true });
                recipe.VoteCount++;
            }
            else
            {
                activity.IsVoted = !activity.IsVoted; // Toggle Like/Unlike
                recipe.VoteCount = activity.IsVoted ? recipe.VoteCount + 1 : recipe.VoteCount - 1;
            }

            await _context.SaveChangesAsync();
            return Ok(new { count = recipe.VoteCount, status = activity?.IsVoted ?? true });
        }

        // BÌNH LUẬN
        [HttpPost("comment")]
        public async Task<IActionResult> PostComment([FromBody] CommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comment = new Comment
            {
                Content = dto.Content,
                UserId = userId,
                RecipeId = dto.RecipeId,
                TipId = dto.TipId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }
        [Authorize]
        [HttpGet("saved-recipes")]
        public async Task<IActionResult> GetSavedRecipes()
        {
            // 1. Lấy Id của người dùng 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2. Truy vấn các "Save" của user này
            var savedRecipes = await _context.UserActivities
                .Where(ua => ua.UserId == userId && ua.IsSaved == true && ua.RecipeId != null)
                .Include(ua => ua.Recipe) // Kết nối để lấy thông tin món ăn
                    .ThenInclude(r => r.RecipeIngredients) // (Tùy chọn) Lấy thêm nguyên liệu
                .Select(ua => ua.Recipe) // Chỉ lấy đối tượng Recipe trả về
                .ToListAsync();

            return Ok(savedRecipes);
        }
    }
}
