﻿﻿﻿using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using DoAn_API.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DoAn_API.DTOs.CategoryDTOs;

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
        // ------DUYỆT RECIPE VÀ TIP------
        [HttpGet("pending-posts")]
        public async Task<IActionResult> GetPendingPosts()
        {
            var recipes = await _context.Recipes
                .Where(r => r.Status == PostStatus.Pending)
                .Select(r => new PendingPostDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    AuthorName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "Ẩn danh",
                    CreatedAt = r.CreatedAt,
                    Type = "Recipe",
                    ImageUrl = r.ImageUrl
                }).ToListAsync();

            var tips = await _context.Tips
                .Where(t => t.Status == PostStatus.Pending)
                .Select(t => new PendingPostDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    AuthorName = t.User != null ? (t.User.FullName ?? t.User.UserName) : "Ẩn danh",
                    CreatedAt = t.CreatedAt,
                    Type = "Tip",
                    ImageUrl = t.ImageUrl
                }).ToListAsync();

            return Ok(recipes.Concat(tips).OrderByDescending(p => p.CreatedAt));
        }

        [HttpPost("approve-post")]
        public async Task<IActionResult> ApprovePost(int id, string type, int newStatus)
        {
            if (type == "Recipe")
            {
                var recipe = await _context.Recipes.FindAsync(id);
                if (recipe == null) return NotFound();
                recipe.Status = (PostStatus)newStatus;
            }
            else
            {
                var tip = await _context.Tips.FindAsync(id);
                if (tip == null) return NotFound();
                tip.Status = (PostStatus)newStatus;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật trạng thái thành công!" });
        }
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryTreeDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Type = dto.Type ?? "Recipe", 
                ParentId = dto.ParentId == 0 ? null : dto.ParentId 
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        // [PUT] api/Admin/categories/{id}
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryTreeDto dto)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = dto.Name;
            existing.Type = dto.Type ?? "Recipe";
            existing.ParentId = dto.ParentId == 0 ? null : dto.ParentId;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }
    }
}
