using DoAn_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoAn_API.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace DoAn_API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController: ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context) => _context = context;

        // Lấy danh sách các báo cáo đang chờ xử lý
        [HttpGet("reports/pending")]
        public async Task<IActionResult> GetPendingReports()
        {
            var reports = await _context.CommentReports
                .Where(r => r.Status == Entities.Enums.ReportStatus.Pending)
                .Include(r => r.User) // Người báo cáo
                .Include(r => r.Comment) // Bình luận bị báo cáo
                    .ThenInclude(c => c.User) // Tác giả của bình luận
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

        // Xử lý báo cáo (Ví dụ: Xóa bình luận và đánh dấu Resolved)
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
    }
}
