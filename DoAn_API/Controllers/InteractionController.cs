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

        // -------VOTE (LIKE) CÔNG THỨC-------
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

        // -------BÌNH LUẬN-------
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
        [HttpGet("recipe/{recipeId}/comments")]
        public async Task<IActionResult> GetRecipeComments(int recipeId)
        {
            var comments = await _context.Comments
                .Where(c => c.RecipeId == recipeId)
                .Include(c => c.User) 
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new {
                    id = c.Id,
                    content = c.Content,
                    authorName = c.User != null ? (c.User.FullName ?? c.User.UserName) : "Thành viên NutriCook"
                })
                .ToListAsync();

            return Ok(comments);
        }


        // -------LƯU CÔNG THỨC-------
        [Authorize]
        [HttpGet("saved-recipes")]
        public async Task<IActionResult> GetSavedRecipes()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var savedRecipes = await _context.UserActivities
                .Where(ua => ua.UserId == userId && ua.IsSaved == true && ua.RecipeId != null)
                .Include(ua => ua.Recipe) 
                    .ThenInclude(r => r.RecipeIngredients) 
                .Select(ua => ua.Recipe) 
                .ToListAsync();

            return Ok(savedRecipes);
        }

        // GỬI BÁO CÁO BÌNH LUẬN
        [Authorize]
        [HttpPost("comment/{commentId}/report")]
        public async Task<IActionResult> ReportComment(int commentId, [FromBody] ReportDTOs.CreateReportDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kiểm tra bình luận có tồn tại không
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null) return NotFound(new { message = "Bình luận không tồn tại." });

            // Kiểm tra xem user này đã báo cáo bình luận này chưa (chống spam báo cáo)
            var existingReport = await _context.CommentReports
                .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);

            if (existingReport != null)
                return BadRequest(new { message = "Bạn đã báo cáo bình luận này rồi." });

            var report = new CommentReport
            {
                CommentId = commentId,
                UserId = userId,
                Reason = dto.Reason
            };

            _context.CommentReports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cảm ơn bạn đã báo cáo. Chúng tôi sẽ xem xét sớm nhất." });
        }
    }
}
