﻿using DoAn_API.DTOs;
using DoAn_API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static DoAn_API.DTOs.AuthDTOs;

namespace DoAn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // 1. ĐĂNG KÝ TÀI KHOẢN
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthDTOs.RegisterDto model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại!" });

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = "Đăng ký thất bại!", errors = result.Errors });

            // Kiểm tra và tạo Role "User" nếu chưa có
            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Gán quyền User mặc định
            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }

        // 2. ĐĂNG NHẬP & CẤP TOKEN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDTOs.LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // Kiểm tra xem người dùng có đang bị khóa tài khoản (Ban) hay không
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Tài khoản của bạn đã bị khóa do vi phạm chính sách của hệ thống!" });
                }

                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    username = user.UserName,
                    fullName = user.FullName,
                    roles = userRoles
                });
            }
            return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu!" });
        }

        // 3. ĐỔI MẬT KHẨU
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] AuthDTOs.ChangePasswordDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản." });

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            
            if (!result.Succeeded)
                return BadRequest(new { message = "Đổi mật khẩu thất bại!", errors = result.Errors });

            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        // 4. CẬP NHẬT THÔNG TIN CÁ NHÂN
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] AuthDTOs.UpdateProfileDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản." });

            user.FullName = model.FullName;
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
                return BadRequest(new { message = "Cập nhật thông tin thất bại!", errors = result.Errors });

            return Ok(new { message = "Cập nhật thông tin cá nhân thành công!" });
        }

        // 5. LẤY THÔNG TIN CÁ NHÂN
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { message = "Không tìm thấy tài khoản." });

            return Ok(new AuthDTOs.UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FullName = user.FullName
            });
        }
    }
}
