using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
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

        // Lấy toàn bộ danh sách bài viết Tip (Công khai)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tip>>> GetTips()
        {
            return await _context.Tips
                .Include(t => t.User) // Để hiển thị tên tác giả
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
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
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.Tips.Add(tip);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng bài viết thành công!", tipId = tip.Id });
        }

        // Xóa bài viết (Chỉ chủ bài viết hoặc Admin)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTip(int id)
        {
            var tip = await _context.Tips.FindAsync(id);
            if (tip == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tip.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Tips.Remove(tip);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa bài viết." });
        }
    }
}
