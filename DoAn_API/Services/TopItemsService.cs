using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace DoAn_API.Services
{
    public interface ITopItemsService
    {
        Task<List<TopRecipeDto>> GetTopRecipesAsync(int count);
        Task<List<TopTipDto>> GetTopTipsAsync(int count);
    }

    public class TopItemsService : ITopItemsService
    {
        private readonly ApplicationDbContext _context;

        public TopItemsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TopRecipeDto>> GetTopRecipesAsync(int count)
        {
            return await _context.Recipes
                .Where(r => r.Status == PostStatus.Approved)
                .Include(r => r.User)
                .OrderByDescending(r => r.Id)
                .Take(count)
                .Select(r => new TopRecipeDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    CookTime = r.CookTime,
                    TotalCalories = r.TotalCalories,
                    VoteCount = r.VoteCount,
                    AuthorName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "Đầu bếp gia đình"
                })
                .ToListAsync();
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
                    AuthorName = t.User != null ? t.User.FullName : "Đầu bếp gia đình"
                })
                .ToListAsync();
        }
    }
}
