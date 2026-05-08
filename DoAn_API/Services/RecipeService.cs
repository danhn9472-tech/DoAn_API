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
    public class RecipeService : IRecipeService
    {
        private readonly ApplicationDbContext _context;
        private readonly NutritionService _nutritionService;
        private readonly IUploadService _uploadService;
        private readonly IPostDeletionService _postDeletionService;

        public RecipeService(ApplicationDbContext context, NutritionService nutritionService, IUploadService uploadService, IPostDeletionService postDeletionService)
        {
            _context = context;
            _nutritionService = nutritionService;
            _uploadService = uploadService;
            _postDeletionService = postDeletionService;
        }

        public async Task<RecipeDTOs.PaginatedRecipeResponseDto> GetRecipesAsync(int page, int pageSize)
        {
            var query = _context.Recipes
                .Where(r => r.Status == PostStatus.Approved && !r.IsDeleted);

            var totalItems = await query.CountAsync();

            var recipes = await query
                .Include(r => r.User)
                .Include(r => r.RecipeCategories)
                    .ThenInclude(rc => rc.Category)
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                    AuthorName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "Đầu bếp gia đình",
                    AuthorAvatarUrl = r.User != null ? r.User.AvatarUrl : null,
                    Categories = r.RecipeCategories.Select(rc => new RecipeDTOs.CategoryDto
                    {
                        Id = rc.Category.Id,
                        Name = rc.Category.Name
                    }).ToList()
                })
                .ToListAsync();

            return new RecipeDTOs.PaginatedRecipeResponseDto
            {
                Data = recipes,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        public async Task<RecipeDTOs.RecipeDetailDto> GetRecipeByIdAsync(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients).ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeSteps)
                .Include(r => r.User)
                .Include(r => r.RecipeCategories).ThenInclude(rc => rc.Category)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (recipe == null) return null;

            return new RecipeDTOs.RecipeDetailDto
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl,
                CookTime = recipe.CookTime,
                TotalCalories = recipe.TotalCalories,
                TotalProtein = recipe.TotalProtein,
                TotalFat = recipe.TotalFat,
                TotalCarbs = recipe.TotalCarbs,
                Difficulty = recipe.Difficulty,
                AuthorName = recipe.User != null ? (recipe.User.FullName ?? recipe.User.UserName) : "Đầu bếp gia đình",
                AuthorAvatarUrl = recipe.User != null ? recipe.User.AvatarUrl : null,
                UserId = recipe.UserId,
                CreatedAt = recipe.CreatedAt,
                VoteCount = recipe.VoteCount,
                SaveCount = recipe.SaveCount,
                Categories = recipe.RecipeCategories.Select(rc => new RecipeDTOs.CategoryDto { Id = rc.CategoryId, Name = rc.Category.Name }).ToList(),
                Ingredients = recipe.RecipeIngredients.Select(ri => new RecipeDTOs.RecipeIngredientDetailDto { IngredientId = ri.IngredientId, IngredientName = ri.Ingredient.Name, Amount = ri.Amount, Unit = ri.Unit }).ToList(),
                Steps = recipe.RecipeSteps.OrderBy(s => s.StepOrder).Select(s => new RecipeDTOs.RecipeStepDetailDto { StepOrder = s.StepOrder, Content = s.Content, ImageUrl = s.ImageUrl }).ToList()
            };
        }

        public async Task<List<RecipeDTOs.TopRecipeDto>> GetTopRecipesAsync(int count)
        {
            return await _context.Recipes
                .Where(r => r.Status == PostStatus.Approved && !r.IsDeleted)
                .Include(r => r.User)
                .OrderByDescending(r => r.Id)
                .Take(count)
                .Select(r => new RecipeDTOs.TopRecipeDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    CookTime = r.CookTime,
                    Difficulty = r.Difficulty,
                    TotalCalories = r.TotalCalories,
                    VoteCount = r.VoteCount,
                    AuthorName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "Đầu bếp gia đình",
                    AuthorAvatarUrl = r.User != null ? r.User.AvatarUrl : null,
                    UserId = r.UserId
                })
                .ToListAsync();
        }

        public async Task<int> CreateRecipeAsync(RecipeDTOs.CreateRecipeRequestDto dto, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var recipe = new Recipe
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    CookTime = dto.CookTime,
                    Difficulty = (DifficultyLevel)dto.Difficulty,
                    ImageUrl = dto.ImageUrl,
                    Status = PostStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,

                    RecipeCategories = dto.CategoryIds != null
                        ? dto.CategoryIds.Select(cId => new RecipeCategory { CategoryId = cId }).ToList()
                        : new List<RecipeCategory>(),

                    RecipeIngredients = dto.Ingredients != null
                        ? dto.Ingredients.Select(i => new RecipeIngredient { IngredientId = i.IngredientId, Amount = i.Amount, Unit = i.Unit }).ToList()
                        : new List<RecipeIngredient>(),

                    RecipeSteps = dto.Steps != null
                        ? dto.Steps.Select((s, index) => new RecipeStep { StepOrder = s.StepOrder > 0 ? s.StepOrder : index + 1, Content = s.Content, ImageUrl = s.ImageUrl }).ToList()
                        : new List<RecipeStep>()
                };

                await _nutritionService.CalculateTotalNutritionAsync(recipe);
                _context.Recipes.Add(recipe);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return recipe.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateRecipeAsync(int id, RecipeDTOs.UpdateRecipeDto dto, string userId, bool isAdmin)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .Include(r => r.RecipeSteps)
                .Include(r => r.RecipeCategories)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) throw new KeyNotFoundException("Không tìm thấy công thức.");
            if (recipe.UserId != userId && !isAdmin) throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa công thức này.");

            recipe.Title = dto.Title;
            recipe.Description = dto.Description;
            recipe.CookTime = dto.CookTime;
            recipe.Difficulty = (DifficultyLevel)dto.Difficulty;
            
            // Kiểm tra: Nếu có ảnh mới gửi lên và khác với ảnh hiện tại -> Xóa ảnh cũ
            if (recipe.ImageUrl != dto.ImageUrl)
            {
                if (!string.IsNullOrEmpty(recipe.ImageUrl))
                {
                    _uploadService.DeleteImage(recipe.ImageUrl);
                }
                recipe.ImageUrl = dto.ImageUrl;
            }

            // Lấy danh sách các ảnh bước thực hiện MỚI được gửi lên
            var newStepImages = dto.Steps.Where(s => !string.IsNullOrEmpty(s.ImageUrl)).Select(s => s.ImageUrl).ToList();
            // So sánh để xóa các ảnh bước thực hiện CŨ không còn sử dụng nữa
            foreach (var oldStep in recipe.RecipeSteps)
            {
                if (!string.IsNullOrEmpty(oldStep.ImageUrl) && !newStepImages.Contains(oldStep.ImageUrl))
                {
                    _uploadService.DeleteImage(oldStep.ImageUrl);
                }
            }

            _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);
            _context.RecipeSteps.RemoveRange(recipe.RecipeSteps);
            recipe.RecipeCategories.Clear();

            foreach (var catId in dto.CategoryIds)
            {
                recipe.RecipeCategories.Add(new RecipeCategory { CategoryId = catId });
            }

            recipe.RecipeIngredients = dto.Ingredients.Select(i => new RecipeIngredient
            {
                IngredientId = i.IngredientId,
                Amount = i.Amount,
                Unit = i.Unit
            }).ToList();
            await _nutritionService.CalculateTotalNutritionAsync(recipe);

            int stepNum = 1;
            recipe.RecipeSteps = dto.Steps.Select(s => new RecipeStep
            {
                StepOrder = s.StepOrder > 0 ? s.StepOrder : stepNum++,
                Content = s.Content,
                ImageUrl = s.ImageUrl
            }).ToList();

            recipe.Status = PostStatus.Pending;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecipeAsync(int id, string userId, bool isAdmin)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Comments)
                .Include(r => r.RecipeSteps)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null) throw new KeyNotFoundException("Không tìm thấy công thức.");
            if (recipe.UserId != userId && !isAdmin) throw new UnauthorizedAccessException("Bạn không có quyền xóa công thức này.");

            _postDeletionService.QueueFullPostDeletion(recipe);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RecipeDTOs.RecipeListItemDto>> FilterByCategoriesAsync(List<int> categoryIds)
        {
            var query = _context.Recipes
                .Where(r => r.Status == PostStatus.Approved && !r.IsDeleted)
                .AsQueryable();

            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(r => r.RecipeCategories.Any(rc => categoryIds.Contains(rc.CategoryId)));
            }

            return await query
                .Include(r => r.User)
                .Include(r => r.RecipeCategories)
                    .ThenInclude(rc => rc.Category)
                .OrderByDescending(r => r.Id)
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
                    AuthorName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "Đầu bếp gia đình",
                    AuthorAvatarUrl = r.User != null ? r.User.AvatarUrl : null,
                    Categories = r.RecipeCategories.Select(rc => new RecipeDTOs.CategoryDto {
                        Id = rc.Category.Id,
                        Name = rc.Category.Name
                    }).ToList()
                }).ToListAsync();
        }
    }
}