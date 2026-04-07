using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("reports/pending")]
        public async Task<IActionResult> GetPendingReports()
        {
            var reports = await _context.CommentReports
                .Where(r => r.Status == Entities.Enums.ReportStatus.Pending)
                .Include(r => r.User) 
                .Include(r => r.Comment) 
                    .ThenInclude(c => c.User) 
                .Select(r => new {
                    ReportId = r.Id,
                    ReporterName = r.User.UserName,
                    Reason = r.Reason,
                    CommentContent = r.Comment.Content,
                    CommentAuthor = r.Comment.User.UserName,
                    ReportedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reports);
        }

        [HttpPost("reports/{reportId}/resolve")]
        public async Task<IActionResult> ResolveReport(int reportId, [FromQuery] bool deleteComment)
        {
            var report = await _context.CommentReports
                .Include(r => r.Comment)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null) return NotFound();

            if (deleteComment && report.Comment != null)
            {
                _context.Comments.Remove(report.Comment);
                report.Status = Entities.Enums.ReportStatus.Resolved;
            }
            else
            {
                report.Status = Entities.Enums.ReportStatus.Dismissed; // Bỏ qua nếu bình luận không vi phạm
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xử lý báo cáo." });
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = new DashboardStatDto
            {
                TotalUsers = await _context.Users.CountAsync(),

                TotalRecipes = await _context.Recipes.CountAsync(),

                TotalTips = await _context.Tips.CountAsync(),

                PendingPosts = await _context.Recipes.CountAsync(r => r.Status == PostStatus.Pending)
                             + await _context.Tips.CountAsync(t => t.Status == PostStatus.Pending)
            };

            return Ok(stats);
        }
    }
}
