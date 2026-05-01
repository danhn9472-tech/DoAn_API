﻿﻿﻿﻿﻿﻿﻿﻿﻿using DoAn_API.Data;
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
    public class TipsController : ControllerBase
    {
        private readonly ITipService _tipService;

        public TipsController(ITipService tipService)
        {
            _tipService = tipService;
        }

        //-------LẤY DANH SÁCH BÀI VIẾT-------
        [HttpGet]
        public async Task<IActionResult> GetTips([FromQuery] int page = 1, [FromQuery] int pageSize = 16)
        {
            var result = await _tipService.GetTipsAsync(page, pageSize);
            return Ok(result);
        }

        //-------LẤY THÔNG TIN CHI TIẾT BÀI VIẾT THEO ID-------
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTipById(int id)
        {
            var tip = await _tipService.GetTipByIdAsync(id);

            if (tip == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết" });
            }

            return Ok(tip);
        }
        //-------LẤY DANH SÁCH BÀI VIẾT MỚI NHẤT-------
        [HttpGet("top/{count}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopTips(int count)
        {
            var tips = await _tipService.GetTopTipsAsync(count);
            return Ok(tips);
        }
        //-------TẠO MỚI BÀI VIẾT-------
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostTip([FromBody] TipDTOs.CreateTipDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            int tipId = await _tipService.CreateTipAsync(dto, userId);
            return Ok(new { message = "Đăng bài viết thành công!", tipId = tipId });
        }

        //-------CẬP NHẬT BÀI VIẾT THEO ID-------
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTip(int id, [FromBody] TipDTOs.CreateTipDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            await _tipService.UpdateTipAsync(id, dto, userId, isAdmin);
            return Ok(new { message = "Cập nhật bài viết thành công!", tipId = id });
        }

        //-------XÓA BÀI VIẾT THEO ID-------
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTip(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            await _tipService.DeleteTipAsync(id, userId, isAdmin);
            return Ok(new { message = "Đã xóa bài viết." });
        }
    }
}
