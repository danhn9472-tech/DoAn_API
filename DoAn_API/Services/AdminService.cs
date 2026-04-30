using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;

        public AdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdminDTOs.PendingReportDto>> GetPendingReportsAsync()
        {
            return await _context.CommentReports
                .Where(r => r.Status == ReportStatus.Pending)
                .Include(r => r.User)
                .Include(r => r.Comment).ThenInclude(c => c.User)
                .Select(r => new AdminDTOs.PendingReportDto
                {
                    ReportId = r.Id,
                    ReporterName = r.User.UserName,
                    Reason = r.Reason,
                    CommentContent = r.Comment.Content,
                    CommentAuthor = r.Comment.User.UserName,
                    ReportedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task ResolveReportAsync(int reportId, bool deleteComment)
        {
            var report = await _context.CommentReports
                .Include(r => r.Comment)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null) throw new System.Collections.Generic.KeyNotFoundException("Báo cáo không tồn tại");

            if (deleteComment && report.Comment != null) _context.Comments.Remove(report.Comment);
            
            report.Status = deleteComment ? ReportStatus.Resolved : ReportStatus.Dismissed;
            await _context.SaveChangesAsync();
        }
    }
}