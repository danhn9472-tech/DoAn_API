﻿﻿﻿using DoAn_API.Data;
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

        // -------UNIFIED VOTE ENDPOINT (recipe/tip)-------
        [HttpPost("vote/{itemType}/{itemId}")]
        public async Task<IActionResult> ToggleVote(string itemType, int itemId)
        {
            var validTypes = new[] { "recipe", "tip" };
            if (!validTypes.Contains(itemType.ToLower()))
            {
                return BadRequest(new { message = "Loại item không hợp lệ. Dùng 'recipe' hoặc 'tip'" });
            }

            return await VoteItemInternal(itemType.ToLower(), itemId);
        }

        private async Task<IActionResult> VoteItemInternal(string itemType, int itemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (itemType == "recipe")
            {
                var recipe = await _context.Recipes.FindAsync(itemId);
                if (recipe == null) return NotFound(new { message = "Công thức không tồn tại" });

                var activity = await _context.UserActivities
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.RecipeId == itemId);

                if (activity == null)
                {
                    _context.UserActivities.Add(new UserActivity { UserId = userId, RecipeId = itemId, IsVoted = true });
                    recipe.VoteCount++;
                }
                else
                {
                    activity.IsVoted = !activity.IsVoted;
                    recipe.VoteCount = activity.IsVoted ? recipe.VoteCount + 1 : recipe.VoteCount - 1;
                }

                await _context.SaveChangesAsync();
                return Ok(new { count = recipe.VoteCount, status = activity?.IsVoted ?? true });
            }
            else if (itemType == "tip")
            {
                var tip = await _context.Tips.FindAsync(itemId);
                if (tip == null) return NotFound(new { message = "Tip không tồn tại" });

                var activity = await _context.UserActivities
                    .FirstOrDefaultAsync(a => a.TipId == itemId && a.UserId == userId);

                if (activity == null)
                {
                    activity = new UserActivity { TipId = itemId, UserId = userId, IsVoted = true };
                    _context.UserActivities.Add(activity);
                    tip.VoteCount++;
                }
                else
                {
                    activity.IsVoted = !activity.IsVoted;
                    tip.VoteCount = activity.IsVoted ? tip.VoteCount + 1 : tip.VoteCount - 1;
                }

                await _context.SaveChangesAsync();
                return Ok(new { count = tip.VoteCount, status = activity?.IsVoted ?? true });
            }

            return BadRequest();
        }

        // -------BÌNH LUẬN-------
        [HttpPost("comment")]
        public async Task<IActionResult> PostComment([FromBody] CommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var post = await _context.Posts.FindAsync(dto.PostId);
            if (post == null) return NotFound(new { message = "Bài viết không tồn tại." });

            var comment = new Comment
            {
                Content = dto.Content,
                UserId = userId,
                PostId = dto.PostId,
                CreatedAt = DateTime.UtcNow 
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = comment.Id,
                content = comment.Content,
                createdAt = comment.CreatedAt
            });
        }

        //------COI BÌNH LUẬN CỦA CÔNG THỨC----
        [AllowAnonymous]
        [HttpGet("recipe/{recipeId}/comments")]
        public async Task<IActionResult> GetRecipeComments(int recipeId)
        {
            return await GetCommentsInternal("recipe", recipeId);
        }

        // ------COI BÌNH LUẬN------
        [AllowAnonymous]
        [HttpGet("comments/{itemType}/{itemId}")]
        public async Task<IActionResult> GetComments(string itemType, int itemId)
        {
            var validTypes = new[] { "recipe", "tip" };
            if (!validTypes.Contains(itemType.ToLower()))
            {
                return BadRequest(new { message = "Loại item không hợp lệ. Dùng 'recipe' hoặc 'tip'" });
            }

            return await GetCommentsInternal(itemType.ToLower(), itemId);
        }
        //-------UNIFIED GET COMMENTS ENDPOINT (recipe/tip)-------
        private async Task<IActionResult> GetCommentsInternal(string itemType, int itemId)
        {
            var comments = await _context.Comments
                .Where(c => itemType == "recipe" 
                    ? c.RecipeId == itemId 
                    : c.TipId == itemId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new {
                    id = c.Id,
                    content = c.Content,
                    createdAt = c.CreatedAt,
                    authorName = c.User != null ? (c.User.FullName ?? c.User.UserName) : "Thành viên NutriCook"
                })
                .ToListAsync();

            return Ok(comments);
        }

        // -------UNIFIED SAVE ENDPOINT (recipe/tip)-------
        [HttpPost("save/{itemType}/{itemId}")]
        public async Task<IActionResult> ToggleSave(string itemType, int itemId)
        {
            var validTypes = new[] { "recipe", "tip" };
            if (!validTypes.Contains(itemType.ToLower()))
            {
                return BadRequest(new { message = "Loại item không hợp lệ. Dùng 'recipe' hoặc 'tip'" });
            }

            return await SaveItemInternal(itemType.ToLower(), itemId);
        }

        private async Task<IActionResult> SaveItemInternal(string itemType, int itemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (itemType == "recipe")
            {
                var recipe = await _context.Recipes.FindAsync(itemId);
                if (recipe == null) return NotFound(new { message = "Công thức không tồn tại" });

                var activity = await _context.UserActivities
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.RecipeId == itemId);

                if (activity == null)
                {
                    _context.UserActivities.Add(new UserActivity { UserId = userId, RecipeId = itemId, IsSaved = true });
                    recipe.SaveCount++;
                }
                else
                {
                    activity.IsSaved = !activity.IsSaved;
                    recipe.SaveCount = activity.IsSaved ? recipe.SaveCount + 1 : recipe.SaveCount - 1;
                }

                await _context.SaveChangesAsync();
                return Ok(new { count = recipe.SaveCount, status = activity?.IsSaved ?? true });
            }
            else if (itemType == "tip")
            {
                var tip = await _context.Tips.FindAsync(itemId);
                if (tip == null) return NotFound(new { message = "Tip không tồn tại" });

                var activity = await _context.UserActivities
                    .FirstOrDefaultAsync(a => a.TipId == itemId && a.UserId == userId);

                if (activity == null)
                {
                    activity = new UserActivity { TipId = itemId, UserId = userId, IsSaved = true };
                    _context.UserActivities.Add(activity);
                    tip.SaveCount++;
                }
                else
                {
                    activity.IsSaved = !activity.IsSaved;
                    tip.SaveCount = activity.IsSaved ? tip.SaveCount + 1 : tip.SaveCount - 1;
                }

                await _context.SaveChangesAsync();
                return Ok(new { count = tip.SaveCount, status = activity?.IsSaved ?? true });
            }

            return BadRequest();
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
