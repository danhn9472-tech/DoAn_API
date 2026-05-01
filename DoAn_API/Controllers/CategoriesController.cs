﻿using DoAn_API.Data;
using DoAn_API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static DoAn_API.DTOs.CategoryDTOs;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public CategoriesController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetCategoryTree()
        {
            // 1. Kiểm tra xem cây danh mục đã có trong Cache chưa
            if (_cache.TryGetValue("CategoryTree", out List<CategoryDTOs.CategoryTreeDto> cachedTree))
            {
                return Ok(cachedTree); // Trả về luôn từ RAM, bỏ qua mọi logic bên dưới
            }

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

            // 2. Lưu kết quả vào Cache, set thời gian sống là 24 giờ
            _cache.Set("CategoryTree", categoryTree, TimeSpan.FromHours(24));

            return Ok(categoryTree);
        }
    }
}