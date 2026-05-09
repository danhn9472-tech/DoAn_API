using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public class SearchService : ISearchService
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SearchResultDto> SearchAsync(string keyword)
        {
            var result = new SearchResultDto();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return result; // Trả về đối tượng có 2 danh sách rỗng
            }

            keyword = keyword.Trim().ToLower();

            // 1. Tìm kiếm trong bảng Recipes theo Title
            result.Recipes = await _context.Recipes
                .Where(r => r.Status == PostStatus.Approved && !r.IsDeleted)
                .Where(r => r.Title.ToLower().Contains(keyword))
                .Include(r => r.User)
                .Include(r => r.RecipeCategories).ThenInclude(rc => rc.Category)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RecipeDTOs.RecipeListItemDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    CookTime = r.CookTime,
                    Difficulty = r.Difficulty,
                    TotalCalories = r.TotalCalories,
                    VoteCount = r.VoteCount,
                    SaveCount = r.SaveCount,
                    UserId = r.UserId,
                    Status = r.Status,
                    AuthorName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "Ẩn danh",
                    AuthorAvatarUrl = r.User != null ? r.User.AvatarUrl : null,
                    Categories = r.RecipeCategories.Select(rc => new RecipeDTOs.CategoryDto
                    {
                        Id = rc.Category.Id,
                        Name = rc.Category.Name
                    }).ToList()
                })
                .ToListAsync();

            // 2. Tìm kiếm trong bảng Tips theo Title
            result.Tips = await _context.Tips
                .Where(t => t.Status == PostStatus.Approved && !t.IsDeleted)
                .Where(t => t.Title.ToLower().Contains(keyword))
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
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
                    AuthorName = t.User != null ? (t.User.FullName ?? t.User.UserName) : "Ẩn danh",
                    AuthorAvatarUrl = t.User != null ? t.User.AvatarUrl : null
                })
                .ToListAsync();

            return result;
        }
    }
}