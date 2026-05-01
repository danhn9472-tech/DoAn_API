﻿﻿﻿﻿﻿﻿﻿using DoAn_API.Data;
using DoAn_API.DTOs;
using DoAn_API.Entities;
using DoAn_API.Entities.Enums;
using DoAn_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly ITopItemsService _topItemsService;
        private readonly IRecipeService _recipeService;

        public RecipesController(ITopItemsService topItemsService, IRecipeService recipeService)
        {
            _topItemsService = topItemsService;
            _recipeService = recipeService;
        }

        //--------GET TẤT CẢ CÔNG THỨC ĐÃ DUYỆT--------
        [HttpGet]
        public async Task<IActionResult> GetRecipes([FromQuery] int page = 1, [FromQuery] int pageSize = 16)
        {
            var result = await _recipeService.GetRecipesAsync(page, pageSize);
            return Ok(result);
        }

        //-------GET REIPE THEO ID--------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecipe(int id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);

            if (recipe == null)
            {
                return NotFound(new { message = "Không tìm thấy công thức này." });
            }

            return Ok(recipe);
        }
        //-----TẠO CÔNG THỨC MỚI-----
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] RecipeDTOs.CreateRecipeRequestDto dto)
        {
            if (dto == null) return BadRequest("Dữ liệu không hợp lệ.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            int newRecipeId = await _recipeService.CreateRecipeAsync(dto, userId);
            return Ok(new { message = "Tạo công thức thành công, hệ thống đã tự động tính toán dinh dưỡng!", recipeId = newRecipeId });
        }
        //-----SỬA CÔNG THỨC-----
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutRecipe(int id, [FromBody] UpdateRecipeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            await _recipeService.UpdateRecipeAsync(id, dto, userId, isAdmin);
            return Ok(new { message = "Cập nhật thành công!", id = id });
        }


        //-----XÓA CÔNG THỨC-----
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            await _recipeService.DeleteRecipeAsync(id, userId, isAdmin);
            return Ok(new { message = "Xóa công thức thành công." });
        }
        //-------LẤY RA NHỮNG CÔNG THỨC MỚI NHẤT ĐÃ DUYỆT, SỐ LƯỢNG DO CLIENT YÊU CẦU--------
        [HttpGet("top/{count}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetTopRecipes(int count)
        {
            var recipes = await _topItemsService.GetTopRecipesAsync(count);
            return Ok(recipes);
        }

        //-------LẤY RA NHỮNG CÔNG THỨC ĐÃ DUYỆT THEO DANH MỤC (DANH MỤC ĐƯỢC TRUYỀN VÀO DƯỚI DẠNG LIST ID)--------
        [HttpGet("filter-by-categories")]
        public async Task<IActionResult> FilterByCategories([FromQuery] List<int> categoryIds)
        {
            var results = await _recipeService.FilterByCategoriesAsync(categoryIds);
            return Ok(results);
        }
    }
}