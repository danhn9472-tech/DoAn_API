﻿using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using DoAn_API.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly INotificationService _notificationService;

        public AdminService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IMemoryCache cache, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<AdminDTOs.PendingReportDto>> GetPendingCommentReportsAsync()
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
                    CommentAuthorAvatarUrl = r.Comment.User.AvatarUrl,
                    AuthorId = r.Comment.UserId,
                    ReportedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task ResolveCommentReportAsync(int reportId, bool deleteComment, bool banUser = false)
        {
            var report = await _context.CommentReports
                .Include(r => r.Comment)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null) throw new System.Collections.Generic.KeyNotFoundException("Báo cáo không tồn tại");

            var authorId = report.Comment?.UserId;

            if (deleteComment && report.Comment != null) _context.Comments.Remove(report.Comment);
            
            report.Status = deleteComment ? ReportStatus.Resolved : ReportStatus.Dismissed;

            // Thực hiện khóa User nếu được yêu cầu
            if (banUser && !string.IsNullOrEmpty(authorId))
            {
                var user = await _userManager.FindByIdAsync(authorId);
                if (user != null)
                {
                    await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.MaxValue);
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AdminDTOs.PendingPostReportDto>> GetPendingPostReportsAsync()
        {
            return await _context.PostReports
                .Where(r => r.Status == ReportStatus.Pending)
                .Include(r => r.User)
                .Include(r => r.Recipe).ThenInclude(rc => rc.User)
                .Include(r => r.Tip).ThenInclude(t => t.User)
                .Select(r => new AdminDTOs.PendingPostReportDto
                {
                    ReportId = r.Id,
                    ReporterName = r.User.UserName,
                    Reason = r.Reason,
                    PostTitle = r.RecipeId != null ? r.Recipe.Title : r.Tip.Title,
                    PostType = r.RecipeId != null ? "Recipe" : "Tip",
                    PostAuthor = r.RecipeId != null ? r.Recipe.User.UserName : r.Tip.User.UserName,
                    PostAuthorAvatarUrl = r.RecipeId != null ? r.Recipe.User.AvatarUrl : r.Tip.User.AvatarUrl,
                    AuthorId = r.RecipeId != null ? r.Recipe.UserId : r.Tip.UserId,
                    ReportedAt = r.CreatedAt,
                    RecipeId = r.RecipeId,
                    TipId = r.TipId
                })
                .ToListAsync();
        }

        public async Task ResolvePostReportAsync(int reportId, bool deletePost, bool banUser = false)
        {
            var report = await _context.PostReports
                .Include(r => r.Recipe).ThenInclude(rc => rc.Comments)
                .Include(r => r.Tip).ThenInclude(t => t.Comments)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null) throw new System.Collections.Generic.KeyNotFoundException("Báo cáo không tồn tại.");

            var authorId = report.RecipeId != null ? report.Recipe?.UserId : report.Tip?.UserId;

            if (deletePost)
            {
                int postId = report.RecipeId ?? report.TipId ?? 0;
                if (postId > 0)
                {
                    var activities = _context.UserActivities.Where(ua => ua.PostId == postId);
                    _context.UserActivities.RemoveRange(activities);
                }

                if (report.RecipeId.HasValue && report.Recipe != null)
                {
                    if (report.Recipe.Comments != null && report.Recipe.Comments.Any())
                        _context.Comments.RemoveRange(report.Recipe.Comments);
                    _context.Recipes.Remove(report.Recipe);
                }
                else if (report.TipId.HasValue && report.Tip != null)
                {
                    if (report.Tip.Comments != null && report.Tip.Comments.Any())
                        _context.Comments.RemoveRange(report.Tip.Comments);
                    _context.Tips.Remove(report.Tip);
                }
            }
            
            report.Status = deletePost ? ReportStatus.Resolved : ReportStatus.Dismissed;

            // Thực hiện khóa User nếu được yêu cầu
            if (banUser && !string.IsNullOrEmpty(authorId))
            {
                var user = await _userManager.FindByIdAsync(authorId);
                if (user != null)
                {
                    await _userManager.SetLockoutEndDateAsync(user, System.DateTimeOffset.MaxValue);
                    await _userManager.UpdateSecurityStampAsync(user);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<DashboardStatDto> GetStatisticsAsync()
        {
            return new DashboardStatDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalRecipes = await _context.Recipes.CountAsync(),
                TotalTips = await _context.Tips.CountAsync(),
                PendingPosts = await _context.Recipes.CountAsync(r => r.Status == PostStatus.Pending)
                             + await _context.Tips.CountAsync(t => t.Status == PostStatus.Pending)
            };
        }

        public async Task<IEnumerable<AdminDTOs.UserDto>> GetUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDtos = new List<AdminDTOs.UserDto>();

            foreach (var user in users)
            {
                userDtos.Add(new AdminDTOs.UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    AvatarUrl = user.AvatarUrl,
                    IsLockedOut = await _userManager.IsLockedOutAsync(user),
                    LockoutEnd = user.LockoutEnd,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }
            return userDtos;
        }

        public async Task ToggleUserLockoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Không tìm thấy người dùng.");

            if (await _userManager.IsLockedOutAsync(user))
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            await _userManager.UpdateSecurityStampAsync(user);
        }

        public async Task<IEnumerable<PendingPostDto>> GetPendingPostsAsync()
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
                    ImageUrl = r.ImageUrl,
                    AuthorAvatarUrl = r.User !=null ? r.User.AvatarUrl : null
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
                    ImageUrl = t.ImageUrl,
                    AuthorAvatarUrl = t.User != null ? t.User.AvatarUrl : null
                }).ToListAsync();

            return recipes.Concat(tips).OrderByDescending(p => p.CreatedAt);
        }

        public async Task ApprovePostAsync(int id, string type, int newStatus)
        {
            if (type == "Recipe")
            {
                var recipe = await _context.Recipes.FindAsync(id);
                if (recipe == null) throw new KeyNotFoundException("Không tìm thấy công thức.");
                recipe.Status = (PostStatus)newStatus;

                if ((PostStatus)newStatus == PostStatus.Approved)
                {
                    await _notificationService.SendNotificationAsync(recipe.UserId, $"Tuyệt vời! Công thức '{recipe.Title}' của bạn đã được duyệt.", "Approval", recipe.Id);
                }
            }
            else
            {
                var tip = await _context.Tips.FindAsync(id);
                if (tip == null) throw new KeyNotFoundException("Không tìm thấy mẹo vặt.");
                tip.Status = (PostStatus)newStatus;

                if ((PostStatus)newStatus == PostStatus.Approved)
                {
                    await _notificationService.SendNotificationAsync(tip.UserId, $"Bài viết '{tip.Title}' của bạn đã được duyệt.", "Approval", tip.Id);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<CategoryDTOs.CategoryTreeDto> CreateCategoryAsync(CategoryDTOs.CategoryTreeDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Type = dto.Type ?? "Recipe", 
                ParentId = dto.ParentId == 0 ? null : dto.ParentId 
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            
            // Xóa cache cũ để hệ thống tự load lại Cây danh mục mới
            _cache.Remove("CategoryTree");

            dto.Id = category.Id;
            return dto;
        }

        public async Task<CategoryDTOs.CategoryTreeDto> UpdateCategoryAsync(int id, CategoryDTOs.CategoryTreeDto dto)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) throw new KeyNotFoundException("Không tìm thấy danh mục.");

            existing.Name = dto.Name;
            existing.Type = dto.Type ?? "Recipe";
            existing.ParentId = dto.ParentId == 0 ? null : dto.ParentId;

            await _context.SaveChangesAsync();
            
            // Xóa cache cũ để hệ thống tự load lại Cây danh mục mới
            _cache.Remove("CategoryTree");

            return dto;
        }
    }
}