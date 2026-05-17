using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task<SearchResultDto> SearchAsync(string keyword, int page, int pageSize)
        {
            var result = new SearchResultDto 
            { 
                CurrentPage = page, 
                PageSize = pageSize 
            };

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return result;
            }

            keyword = keyword.Trim().ToLower();

            var recipeQuery = _context.Recipes
                .Where(r => r.Status == PostStatus.Approved && !r.IsDeleted)
                .Where(r => r.Title.Contains(keyword));

            result.TotalRecipes = await recipeQuery.CountAsync();

            result.Recipes = await recipeQuery
                .Include(r => r.User)
                .Include(r => r.RecipeCategories).ThenInclude(rc => rc.Category)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RecipeDTOs.RecipeListItemDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Slug = r.Slug,
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

            var tipQuery = _context.Tips
                .Where(t => t.Status == PostStatus.Approved && !t.IsDeleted)
                .Where(t => t.Title.Contains(keyword));

            result.TotalTips = await tipQuery.CountAsync();

            result.Tips = await tipQuery
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TipDTOs.TipResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Slug = t.Slug,
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

            var maxItems = Math.Max(result.TotalRecipes, result.TotalTips);
            result.TotalPages = (int)Math.Ceiling(maxItems / (double)pageSize);

            return result;
        }
    }
}