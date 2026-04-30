using DoAn_API.DTOs;
using DoAn_API.Entities.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DoAn_API.Services
{
    public interface IRecipeService
    {
        Task<RecipeDTOs.PaginatedRecipeResponseDto> GetRecipesAsync(int page, int pageSize);
        Task<RecipeDTOs.RecipeDetailDto> GetRecipeByIdAsync(int id);
        Task<IEnumerable<PendingPostDto>> GetPendingRecipesAsync();
        Task<IEnumerable<RecipeDTOs.RecipeListItemDto>> FilterByCategoriesAsync(List<int> categoryIds);
        Task<int> CreateRecipeAsync(RecipeDTOs.CreateRecipeRequestDto dto, string userId);
        Task UpdateRecipeAsync(int id, UpdateRecipeDto dto, string userId, bool isAdmin);
        Task DeleteRecipeAsync(int id, string userId, bool isAdmin);
        Task ChangeStatusAsync(int id, PostStatus newStatus);
    }
}