using DoAn_API.Data;
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

        [HttpGet("ingredients-tree")]
        public async Task<IActionResult> GetIngredientsTree()
        {
            // Chỉ lấy những danh mục GỐC (ParentId == null) có Type là Ingredient
            var parentCategories = await _context.Categories
                .Where(c => c.Type == "Ingredient" && c.ParentId == null)
                .Include(c => c.SubCategories) // Kéo theo các mục con của nó
                .ToListAsync();

            // Ánh xạ sang DTO
            var result = parentCategories.Select(parent => new CategoryTreeDto
            {
                Id = parent.Id,
                Name = parent.Name,
                Type = parent.Type,
                SubCategories = parent.SubCategories.Select(child => new CategoryTreeDto
                {
                    Id = child.Id,
                    Name = child.Name,
                    Type = child.Type
                }).ToList()
            }).ToList();

            return Ok(result);
        }
    }
}