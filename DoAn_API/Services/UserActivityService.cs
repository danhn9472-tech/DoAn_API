using DoAn_API.Data;
using DoAn_API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public class UserActivityService : IUserActivityService
    {
        private readonly ApplicationDbContext _context;

        public UserActivityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SavedItemsDto> GetMyRecipeBookAsync(string userId)
        {
            var result = new SavedItemsDto();

            result.SavedRecipes = await _context.Recipes
                .Where(r => r.Activities.Any(a => a.UserId == userId && a.IsSaved))
                .Select(r => new SavedRecipeDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    ImageUrl = r.ImageUrl,
                    CookTime = r.CookTime,
                    TotalCalories = r.TotalCalories,
                    AuthorName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "Ẩn danh"
                })
                .ToListAsync();

            result.SavedTips = await _context.Tips
                .Where(t => t.Activities.Any(a => a.UserId == userId && a.IsSaved))
                .Select(t => new SavedTipDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    ImageUrl = t.ImageUrl,
                    AuthorName = t.User != null ? (t.User.FullName ?? t.User.UserName) : "Ẩn danh",
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return result;
        }

        public async Task<MyPostsDto> GetMyPostsAsync(string userId)
        {
            var recipes = await _context.Recipes
                .Where(r => r.UserId == userId)
                .Select(r => new MyRecipeItemDto
                {
                    Id = r.Id, Title = r.Title, Description = r.Description, ImageUrl = r.ImageUrl,
                    CookTime = r.CookTime, TotalCalories = r.TotalCalories, Status = (int)r.Status,
                    CreatedAt = r.CreatedAt, VoteCount = r.VoteCount, SaveCount = r.SaveCount
                }).ToListAsync();

            var tips = await _context.Tips
                .Where(t => t.UserId == userId)
                .Select(t => new MyTipItemDto
                {
                    Id = t.Id, Title = t.Title, Content = t.Content, ImageUrl = t.ImageUrl,
                    Status = (int)t.Status, CreatedAt = t.CreatedAt, VoteCount = t.VoteCount,
                    SaveCount = t.SaveCount
                }).ToListAsync();

            return new MyPostsDto { Recipes = recipes, Tips = tips };
        }
    }
}