using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public InteractionService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<ToggleResultDto> ToggleVoteAsync(string itemType, int itemId, string userId)
        {
            string postOwnerId = null;
            string postTitle = null;

            if (itemType == "recipe")
            {
                var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == itemId && !r.IsDeleted);
                if (recipe == null) throw new KeyNotFoundException("Công thức không tồn tại hoặc đã bị xóa");
                postOwnerId = recipe.UserId; postTitle = recipe.Title;
            }
            else if (itemType == "tip")
            {
                var tip = await _context.Tips.FirstOrDefaultAsync(t => t.Id == itemId && !t.IsDeleted);
                if (tip == null) throw new KeyNotFoundException("Tip không tồn tại hoặc đã bị xóa");
                postOwnerId = tip.UserId; postTitle = tip.Title;
            }

            var activity = await _context.UserActivities
                .FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == itemId);

            int increment = 0;
            if (activity == null)
            {
                activity = new UserActivity { UserId = userId, PostId = itemId, IsVoted = true };
                _context.UserActivities.Add(activity);
                increment = 1;
                
                // Gửi thông báo (chỉ gửi nếu người Like KHÔNG PHẢI là tác giả)
                if (postOwnerId != userId)
                {
                    await _notificationService.SendNotificationAsync(postOwnerId, $"Có người vừa thích bài viết '{postTitle}' của bạn.", "Vote", itemId);
                }
            }
            else
            {
                activity.IsVoted = !activity.IsVoted;
                increment = activity.IsVoted ? 1 : -1;
            }

            await _context.SaveChangesAsync();

            int newCount = 0;
            if (increment != 0)
            {
                if (itemType == "recipe")
                {
                    await _context.Recipes.Where(r => r.Id == itemId).ExecuteUpdateAsync(s => s.SetProperty(r => r.VoteCount, r => r.VoteCount + increment));
                    newCount = await _context.Recipes.Where(r => r.Id == itemId).Select(r => r.VoteCount).FirstOrDefaultAsync();
                }
                else if (itemType == "tip")
                {
                    await _context.Tips.Where(t => t.Id == itemId).ExecuteUpdateAsync(s => s.SetProperty(t => t.VoteCount, t => t.VoteCount + increment));
                    newCount = await _context.Tips.Where(t => t.Id == itemId).Select(t => t.VoteCount).FirstOrDefaultAsync();
                }
            }
            else
            {
                newCount = itemType == "recipe" 
                    ? await _context.Recipes.Where(r => r.Id == itemId).Select(r => r.VoteCount).FirstOrDefaultAsync()
                    : await _context.Tips.Where(t => t.Id == itemId).Select(t => t.VoteCount).FirstOrDefaultAsync();
            }

            return new ToggleResultDto { Count = newCount, Status = activity.IsVoted };
        }

        public async Task<CommentResponseDto> PostCommentAsync(CommentDto dto, string userId)
        {
            var comment = new Comment
            {
                Content = dto.Content,
                UserId = userId,
                PostId = dto.PostId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == dto.PostId && !p.IsDeleted);
            
            if (post != null && post.UserId != userId)
            {
                await _notificationService.SendNotificationAsync(post.UserId, $"Bài viết '{post.Title}' của bạn vừa có bình luận mới.", "Comment", post.Id);
            }

            var user = await _context.Users.FindAsync(userId);

            return new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId,
                AuthorName = user != null ? (user.FullName ?? user.UserName) : "Thành viên NutriCook",
                AuthorAvatarUrl = user != null ? user.AvatarUrl : null
            };
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsAsync(int itemId)
        {
            return await _context.Comments
                .Where(c => c.PostId == itemId && !c.IsDeleted)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CommentResponseDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    AuthorName = c.User != null ? (c.User.FullName ?? c.User.UserName) : "Thành viên NutriCook",
                    AuthorAvatarUrl = c.User != null ? c.User.AvatarUrl : null,
                    UserId = c.UserId
                })
                .ToListAsync();
        }

        public async Task<ToggleResultDto> ToggleSaveAsync(string itemType, int itemId, string userId)
        {
            if (itemType == "recipe")
            {
                if (!await _context.Recipes.AnyAsync(r => r.Id == itemId && !r.IsDeleted)) throw new KeyNotFoundException("Công thức không tồn tại hoặc đã bị xóa");
            }
            else if (itemType == "tip")
            {
                if (!await _context.Tips.AnyAsync(t => t.Id == itemId && !t.IsDeleted)) throw new KeyNotFoundException("Tip không tồn tại hoặc đã bị xóa");
            }

            var activity = await _context.UserActivities.FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == itemId);

            int increment = 0;
            if (activity == null)
            {
                activity = new UserActivity { UserId = userId, PostId = itemId, IsSaved = true };
                _context.UserActivities.Add(activity);
                increment = 1;
            }
            else
            {
                activity.IsSaved = !activity.IsSaved;
                increment = activity.IsSaved ? 1 : -1;
            }

            await _context.SaveChangesAsync();
            
            int newCount = itemType == "recipe"
                ? await _context.Recipes.Where(r => r.Id == itemId).Select(r => r.SaveCount).FirstOrDefaultAsync()
                : await _context.Tips.Where(t => t.Id == itemId).Select(t => t.SaveCount).FirstOrDefaultAsync();

            if (increment != 0)
            {
                if (itemType == "recipe")
                {
                    await _context.Recipes.Where(r => r.Id == itemId).ExecuteUpdateAsync(s => s.SetProperty(r => r.SaveCount, r => r.SaveCount + increment));
                    newCount += increment;
                }
                else if (itemType == "tip")
                {
                    await _context.Tips.Where(t => t.Id == itemId).ExecuteUpdateAsync(s => s.SetProperty(t => t.SaveCount, t => t.SaveCount + increment));
                    newCount += increment;
                }
            }

            return new ToggleResultDto { Count = newCount, Status = activity.IsSaved };
        }

        public async Task ReportCommentAsync(int commentId, string reason, string userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);
            if (comment == null) throw new KeyNotFoundException("Bình luận không tồn tại hoặc đã bị xóa.");

            var existingReport = await _context.CommentReports
                .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);

            if (existingReport != null)
                throw new InvalidOperationException("Bạn đã báo cáo bình luận này rồi.");

            var report = new CommentReport
            {
                CommentId = commentId,
                UserId = userId,
                Reason = reason
            };

            _context.CommentReports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task ReportPostAsync(string itemType, int itemId, string reason, string userId)
        {
            int? recipeId = itemType == "recipe" ? itemId : null;
            int? tipId = itemType == "tip" ? itemId : null;

            if (recipeId.HasValue && !await _context.Recipes.AnyAsync(r => r.Id == recipeId && !r.IsDeleted))
                throw new KeyNotFoundException("Công thức không tồn tại hoặc đã bị xóa.");
            if (tipId.HasValue && !await _context.Tips.AnyAsync(t => t.Id == tipId && !t.IsDeleted))
                throw new KeyNotFoundException("Mẹo vặt không tồn tại hoặc đã bị xóa.");

            var existingReport = await _context.PostReports
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RecipeId == recipeId && r.TipId == tipId);

            if (existingReport != null)
                throw new InvalidOperationException("Bạn đã báo cáo bài viết này rồi.");

            var report = new PostReport { UserId = userId, Reason = reason, RecipeId = recipeId, TipId = tipId };

            _context.PostReports.Add(report);
            await _context.SaveChangesAsync();
        }

        public async Task<(bool IsVoted, bool IsSaved)> CheckStatusAsync(string itemType, int itemId, string userId)
        {
            var activity = await _context.UserActivities
                .FirstOrDefaultAsync(x => x.UserId == userId && x.PostId == itemId);
            return activity == null ? (false, false) : (activity.IsVoted, activity.IsSaved);
        }
    }
}