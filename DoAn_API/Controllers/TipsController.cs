﻿using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using DoAn_API.Entities.Enums;
using DoAn_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITopItemsService _topItemsService;

        public TipsController(ApplicationDbContext context, ITopItemsService topItemsService)
        {
            _context = context;
            _topItemsService = topItemsService;
        }

        //-------LẤY DANH SÁCH BÀI VIẾT-------
        [HttpGet]
        public async Task<IActionResult> GetTips()
        {
            var tips = await _context.Tips
                .Where(t => t.Status == PostStatus.Approved) 
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    Id = t.Id,
                    Title = t.Title,
                    Content = t.Content,
                    ImageUrl = t.ImageUrl,
                    CreatedAt = t.CreatedAt,
                    VoteCount = t.VoteCount,
                    SaveCount = t.SaveCount,
                    UserId = t.UserId,
                    Status = t.Status,
                    AuthorName = t.User != null ? t.User.FullName : "Đầu bếp gia đình"
                })
                .ToListAsync();

            return Ok(tips);
        }
        //-------LẤY THÔNG TIN CHI TIẾT BÀI VIẾT THEO ID-------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTipById(int id)
        {
            var tip = await _context.Tips.FindAsync(id);

            if (tip == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết" });
            }

            return Ok(tip);
        }
        //-------LẤY DANH SÁCH BÀI VIẾT MỚI NHẤT-------
        [HttpGet("top/{count}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopTips(int count)
        {
            var tips = await _topItemsService.GetTopTipsAsync(count);
            return Ok(tips);
        }
        //-------TẠO MỚI BÀI VIẾT-------
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Tip>> PostTip([FromBody] TipDTOs.CreateTipDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var tip = new Tip
            {
                Title = dto.Title,
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                AuthorName = dto.AuthorName,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.Tips.Add(tip);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng bài viết thành công!", tipId = tip.Id });
        }
        //-------CẬP NHẬT BÀI VIẾT THEO ID-------
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTip(int id, [FromBody] TipDTOs.CreateTipDto dto)
        {
            var tip = await _context.Tips.FindAsync(id);
            if (tip == null) return NotFound(new { message = "Không tìm thấy bài viết." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tip.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            tip.Title = dto.Title;
            tip.Content = dto.Content;
            tip.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật bài viết thành công!", tipId = tip.Id });
        }
        //-------XÓA BÀI VIẾT THEO ID-------
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTip(int id)
        {
            var tip = await _context.Tips.Include(t => t.Comments).FirstOrDefaultAsync(t => t.Id == id);
            if (tip == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tip.UserId != userId && !User.IsInRole("Admin")) return Forbid();

            var activities = _context.UserActivities.Where(ua => ua.PostId == id);
            _context.UserActivities.RemoveRange(activities);

            if (tip.Comments != null && tip.Comments.Any())
            {
                _context.Comments.RemoveRange(tip.Comments);
            }

            _context.Tips.Remove(tip);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa bài viết." });
        }
        //-------LẤY DANH SÁCH BÀI VIẾT CỦA NGƯỜI DÙNG ĐANG ĐĂNG NHẬP-------
        [Authorize]
        [HttpGet("my-tips")]
        public async Task<IActionResult> GetMyTips()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var myTips = await _context.Tips
                .Where(t => t.UserId == userId)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new
                {
                    Id = t.Id,
                    Title = t.Title,
                    Content = t.Content,
                    ImageUrl = t.ImageUrl,
                    CreatedAt = t.CreatedAt,
                    VoteCount = t.VoteCount,
                    SaveCount = t.SaveCount,
                    UserId = t.UserId,
                    Status = t.Status,
                    AuthorName = t.User != null ? t.User.FullName : "Đầu bếp ẩn danh"
                })
                .ToListAsync();

            return Ok(myTips);
        }
        //-------THÊM HOẶC BỎ VOTE CHO BÀI VIẾT THEO ID------- [DEPRECATED - Use /api/Interaction/vote/tip/{id}]
        [HttpPost("{id}/vote")]
        [Authorize] 
        public async Task<IActionResult> ToggleVoteOld(int id)
        {
            // ⚠️ DEPRECATED: Redirect to new unified endpoint
            return await ToggleVoteInternal(id);
        }

        private async Task<IActionResult> ToggleVoteInternal(int id)
        {
            var tip = await _context.Tips.FindAsync(id);
            if (tip == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var activity = await _context.UserActivities
                .FirstOrDefaultAsync(a => a.PostId == id && a.UserId == userId);

            if (activity == null)
            {
                activity = new UserActivity { PostId = id, UserId = userId, IsVoted = true };
                _context.UserActivities.Add(activity);
                tip.VoteCount++;
            }
            else
            {
                activity.IsVoted = !activity.IsVoted;
                tip.VoteCount += activity.IsVoted ? 1 : -1;
            }

            await _context.SaveChangesAsync();

            return Ok(new { newCount = tip.VoteCount });
        }

        //-------THÊM HOẶC BỎ LƯU BÀI VIẾT THEO ID------- 
        [HttpPost("{id}/save")]
        [Authorize]
        public async Task<IActionResult> ToggleSaveOld(int id)
        {
            return await ToggleSaveInternal(id);
        }

        private async Task<IActionResult> ToggleSaveInternal(int id)
        {
            var tip = await _context.Tips.FindAsync(id);
            if (tip == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var activity = await _context.UserActivities
                .FirstOrDefaultAsync(a => a.PostId == id && a.UserId == userId);

            if (activity == null)
            {
                activity = new UserActivity { PostId = id, UserId = userId, IsSaved = true };
                _context.UserActivities.Add(activity);
                tip.SaveCount++;
            }
            else
            {
                activity.IsSaved = !activity.IsSaved;
                tip.SaveCount += activity.IsSaved ? 1 : -1;
            }

            await _context.SaveChangesAsync();

            return Ok(new { count = tip.SaveCount, status = activity?.IsSaved ?? true });
        }
    }
}
