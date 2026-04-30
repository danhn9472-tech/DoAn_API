using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using DoAn_API.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public class TipService : ITipService
    {
        private readonly ApplicationDbContext _context;

        public TipService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TipDTOs.PaginatedTipResponseDto> GetTipsAsync(int page, int pageSize)
        {
            var query = _context.Tips.Where(t => t.Status == PostStatus.Approved);

            var totalItems = await query.CountAsync();

            var tips = await query
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TipDTOs.TipResponseDto
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
                    AuthorName = t.User != null ? (t.User.FullName ?? t.User.UserName) : "Đầu bếp gia đình"
                })
                .ToListAsync();

            return new TipDTOs.PaginatedTipResponseDto
            {
                Data = tips,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        public async Task<TipDTOs.TipResponseDto> GetTipByIdAsync(int id)
        {
            var tip = await _context.Tips.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
            if (tip == null) return null;

            return new TipDTOs.TipResponseDto
            {
                Id = tip.Id, Title = tip.Title, Content = tip.Content, ImageUrl = tip.ImageUrl,
                CreatedAt = tip.CreatedAt, VoteCount = tip.VoteCount, SaveCount = tip.SaveCount,
                UserId = tip.UserId, Status = tip.Status,
                AuthorName = tip.User != null ? (tip.User.FullName ?? tip.User.UserName) : "Đầu bếp gia đình"
            };
        }

        public async Task<List<TopTipDto>> GetTopTipsAsync(int count)
        {
            return await _context.Tips
                .Where(t => t.Status == PostStatus.Approved)
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .Select(t => new TopTipDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Content = t.Content,
                    ImageUrl = t.ImageUrl,
                    CreatedAt = t.CreatedAt,
                    VoteCount = t.VoteCount,
                    AuthorName = t.User != null ? (t.User.FullName ?? t.User.UserName) : "Đầu bếp gia đình"
                })
                .ToListAsync();
        }

        public async Task<int> CreateTipAsync(TipDTOs.CreateTipDto dto, string userId)
        {
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

            return tip.Id;
        }

        public async Task UpdateTipAsync(int id, TipDTOs.CreateTipDto dto, string userId, bool isAdmin)
        {
            var tip = await _context.Tips.FindAsync(id);
            if (tip == null) throw new KeyNotFoundException("Không tìm thấy bài viết.");
            if (tip.UserId != userId && !isAdmin) throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa bài viết này.");

            tip.Title = dto.Title;
            tip.Content = dto.Content;
            tip.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTipAsync(int id, string userId, bool isAdmin)
        {
            var tip = await _context.Tips.Include(t => t.Comments).FirstOrDefaultAsync(t => t.Id == id);
            if (tip == null) throw new KeyNotFoundException("Không tìm thấy bài viết.");
            if (tip.UserId != userId && !isAdmin) throw new UnauthorizedAccessException("Bạn không có quyền xóa bài viết này.");

            var activities = _context.UserActivities.Where(ua => ua.PostId == id);
            _context.UserActivities.RemoveRange(activities);

            if (tip.Comments != null && tip.Comments.Any())
            {
                _context.Comments.RemoveRange(tip.Comments);
            }

            _context.Tips.Remove(tip);
            await _context.SaveChangesAsync();
        }
    }
}