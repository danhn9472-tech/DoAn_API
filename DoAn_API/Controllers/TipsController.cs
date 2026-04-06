using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using DoAn_API.Entities.Enums;
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

        public TipsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Tips
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

        // Tạo bài viết Tip mới (Yêu cầu đăng nhập)
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

        // Chỉnh sửa bài viết (Chỉ chủ bài viết hoặc Admin)
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

        // Xóa bài viết (Chỉ chủ bài viết hoặc Admin)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTip(int id)
        {
            var tip = await _context.Tips.Include(t => t.Comments).FirstOrDefaultAsync(t => t.Id == id);
            if (tip == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tip.UserId != userId && !User.IsInRole("Admin")) return Forbid();

            var activities = _context.UserActivities.Where(ua => ua.TipId == id);
            _context.UserActivities.RemoveRange(activities);

            if (tip.Comments != null && tip.Comments.Any())
            {
                _context.Comments.RemoveRange(tip.Comments);
            }

            _context.Tips.Remove(tip);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa bài viết." });
        }

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
        [HttpPost("{id}/vote")]
        [Authorize] 
        public async Task<IActionResult> ToggleVote(int id)
        {
            var tip = await _context.Tips.FindAsync(id);
            if (tip == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var activity = await _context.UserActivities
                .FirstOrDefaultAsync(a => a.TipId == id && a.UserId == userId);

            if (activity == null)
            {
                activity = new UserActivity { TipId = id, UserId = userId, IsVoted = true };
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
        [HttpPost("{id}/save")]
        [Authorize]
        public async Task<IActionResult> ToggleSave(int id)
        {
            var tip = await _context.Tips.FindAsync(id);
            if (tip == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var activity = await _context.UserActivities
                .FirstOrDefaultAsync(a => a.TipId == id && a.UserId == userId);

            if (activity == null)
            {
                activity = new UserActivity { TipId = id, UserId = userId, IsSaved = true };
                _context.UserActivities.Add(activity);
                tip.SaveCount++;
            }
            else
            {
                activity.IsSaved = !activity.IsSaved;
                tip.SaveCount += activity.IsSaved ? 1 : -1;
            }

            await _context.SaveChangesAsync();

            return Ok(new { newCount = tip.SaveCount });
        }
    }
}
