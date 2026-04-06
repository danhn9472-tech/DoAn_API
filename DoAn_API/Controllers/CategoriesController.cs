using DoAn_API.Data;
using DoAn_API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static DoAn_API.DTOs.CategoryDTOs;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CategoriesController(ApplicationDbContext context) => _context = context;

        [HttpGet("tree")]
        public async Task<IActionResult> GetCategoryTree()
        {
            var allCategories = await _context.Categories.ToListAsync();

            var parentIds = allCategories
                .Where(c => c.ParentId != null)
                .Select(c => c.ParentId)
                .Distinct()
                .ToList();

            var leafCategories = allCategories
                .Where(c => !parentIds.Contains(c.Id))
                .ToList();

            var categoryTree = leafCategories
                .GroupBy(leaf => leaf.ParentId)
                .Select(group =>
                {
                    var parent = allCategories.FirstOrDefault(p => p.Id == group.Key);

                    var grandParent = parent?.ParentId != null
                        ? allCategories.FirstOrDefault(p => p.Id == parent.ParentId)
                        : null;
                    string groupLabel = grandParent != null
                        ? $"{grandParent.Name} - {parent?.Name}"
                        : (parent?.Name ?? "Mục khác");

                    return new CategoryDTOs.CategoryTreeDto
                    {
                        Id = parent?.Id ?? 0,
                        Name = groupLabel, 
                        Type = parent?.Type ?? "Unknown",

                        SubCategories = group.Select(leaf => new CategoryDTOs.CategoryTreeDto
                        {
                            Id = leaf.Id,      
                            Name = leaf.Name,  
                            Type = leaf.Type
                        }).ToList()
                    };
                })
                .OrderBy(c => c.Name) 
                .ToList();

            return Ok(categoryTree);
        }
    }
}