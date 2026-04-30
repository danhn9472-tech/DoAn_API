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

        public RecipeService(ApplicationDbContext context, NutritionService nutritionService)
        {
            _context = context;
            _nutritionService = nutritionService;
        }

        public async Task<RecipeDTOs.PaginatedRecipeResponseDto> GetRecipesAsync(int page, int pageSize)
        {
            var query = _context.Recipes
                .Where(r => r.Status == PostStatus.Approved);

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
                .FirstOrDefaultAsync(r => r.Id == id);

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
                UserId = recipe.UserId,
                CreatedAt = recipe.CreatedAt,
                VoteCount = recipe.VoteCount,
                SaveCount = recipe.SaveCount,
                Categories = recipe.RecipeCategories.Select(rc => new RecipeDTOs.CategoryDto { Id = rc.CategoryId, Name = rc.Category.Name }).ToList(),
                Ingredients = recipe.RecipeIngredients.Select(ri => new RecipeDTOs.RecipeIngredientDetailDto { IngredientName = ri.Ingredient.Name, Amount = ri.Amount, Unit = ri.Unit }).ToList(),
                Steps = recipe.RecipeSteps.OrderBy(s => s.StepOrder).Select(s => new RecipeDTOs.RecipeStepDetailDto { StepOrder = s.StepOrder, Content = s.Content, ImageUrl = s.ImageUrl }).ToList()
            };
        }

        public async Task<IEnumerable<PendingPostDto>> GetPendingRecipesAsync()
        {
            return await _context.Recipes
                .Where(r => r.Status == PostStatus.Pending)
                .Include(r => r.User)
                .OrderBy(r => r.Id)
                .Select(r => new PendingPostDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    AuthorName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "Ẩn danh",
                    CreatedAt = r.CreatedAt,
                    Type = "Recipe",
                    ImageUrl = r.ImageUrl
                }).ToListAsync();
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
                        ? dto.Steps.Select((s, index) => new RecipeStep { StepOrder = index + 1, Content = s.Instruction, ImageUrl = s.ImageUrl }).ToList()
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

        public async Task UpdateRecipeAsync(int id, UpdateRecipeDto dto, string userId, bool isAdmin)
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
            if (!string.IsNullOrEmpty(dto.ImageUrl)) recipe.ImageUrl = dto.ImageUrl;

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
                StepOrder = stepNum++,
                Content = s.Instruction,
                ImageUrl = s.ImageUrl
            }).ToList();

            recipe.Status = PostStatus.Pending;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecipeAsync(int id, string userId, bool isAdmin)
        {
            var recipe = await _context.Recipes.Include(r => r.Comments).FirstOrDefaultAsync(r => r.Id == id);
            if (recipe == null) throw new KeyNotFoundException("Không tìm thấy công thức.");
            if (recipe.UserId != userId && !isAdmin) throw new UnauthorizedAccessException("Bạn không có quyền xóa công thức này.");

            var activities = _context.UserActivities.Where(ua => ua.PostId == id);
            _context.UserActivities.RemoveRange(activities);
            if (recipe.Comments != null && recipe.Comments.Any()) _context.Comments.RemoveRange(recipe.Comments);

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeStatusAsync(int id, PostStatus newStatus)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe == null) throw new KeyNotFoundException("Không tìm thấy công thức.");

            recipe.Status = newStatus;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RecipeDTOs.RecipeListItemDto>> FilterByCategoriesAsync(List<int> categoryIds)
        {
            var query = _context.Recipes
                .Where(r => r.Status == PostStatus.Approved)
                .AsQueryable();

            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(r => r.RecipeCategories.Any(rc => categoryIds.Contains(rc.CategoryId)));
            }

            return await query
                .Include(r => r.User)
                .OrderByDescending(r => r.Id)
                .Select(r => new RecipeDTOs.RecipeListItemDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    CookTime = r.CookTime,
                    TotalCalories = r.TotalCalories,
                    VoteCount = r.VoteCount,
                    SaveCount = r.SaveCount,
                    AuthorName = r.User != null ? (r.User.FullName ?? r.User.UserName) : "Đầu bếp gia đình"
                }).ToListAsync();
        }
    }
}