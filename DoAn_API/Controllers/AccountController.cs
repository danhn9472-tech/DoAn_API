﻿using DoAn_API.Data;
﻿﻿﻿using DoAn_API.DTOs;
using DoAn_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // 1. ĐĂNG KÝ TÀI KHOẢN
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthDTOs.RegisterDto model)
        {
            var result = await _accountService.RegisterAsync(model);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message });
        }

        // 2. ĐĂNG NHẬP & CẤP TOKEN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDTOs.LoginDto model)
        {
            var result = await _accountService.LoginAsync(model);

            if (!result.Succeeded)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            return Ok(result.Data);
        }

        // 3. ĐỔI MẬT KHẨU
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] AuthDTOs.ChangePasswordDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _accountService.ChangePasswordAsync(userId, model);
            
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message });
        }

        // 4. CẬP NHẬT THÔNG TIN CÁ NHÂN
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] AuthDTOs.UpdateProfileDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _accountService.UpdateProfileAsync(userId, model);
            
            if (!result.Succeeded)
                return StatusCode(result.StatusCode, new { message = result.Message, errors = result.Errors });

            return Ok(new { message = result.Message });
        }

        // 5. LẤY THÔNG TIN CÁ NHÂN
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _accountService.GetProfileAsync(userId);
            if (!result.Succeeded) return StatusCode(result.StatusCode, new { message = result.Message });

            return Ok(result.Data);
        }
    }
}
