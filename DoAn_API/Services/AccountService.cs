using DoAn_API.DTOs;
using DoAn_API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DoAn_API.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IUploadService _uploadService;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IUploadService uploadService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _uploadService = uploadService;
        }

        public async Task<(bool Succeeded, string Message, IEnumerable<string>? Errors)> RegisterAsync(AuthDTOs.RegisterDto model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return (false, "Tên đăng nhập đã tồn tại!", null);

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return (false, "Đăng ký thất bại!", result.Errors.Select(e => e.Description));

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            await _userManager.AddToRoleAsync(user, "User");

            return (true, "Đăng ký tài khoản thành công!", null);
        }

        public async Task<(bool Succeeded, int StatusCode, string Message, AuthDTOs.AuthResponseDto? Data)> LoginAsync(AuthDTOs.LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            // 1. Kiểm tra User tồn tại
            if (user == null)
                return (false, StatusCodes.Status401Unauthorized, "Sai tên đăng nhập hoặc mật khẩu!", null);

            // 2. Kiểm tra tài khoản có đang bị khóa không TRƯỚC KHI check mật khẩu
            if (await _userManager.IsLockedOutAsync(user))
            {
                return (false, StatusCodes.Status403Forbidden, "Tài khoản của bạn đã bị tạm khóa do vi phạm chính sách hoặc nhập sai quá nhiều lần!", null);
            }

            // 3. Kiểm tra mật khẩu
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // Ghi nhận 1 lần đăng nhập sai (Để kích hoạt tính năng tự động khóa của Identity)
                await _userManager.AccessFailedAsync(user);
                
                if (await _userManager.IsLockedOutAsync(user))
                    return (false, StatusCodes.Status403Forbidden, "Tài khoản của bạn đã bị khóa do nhập sai mật khẩu quá nhiều lần!", null);

                return (false, StatusCodes.Status401Unauthorized, "Sai tên đăng nhập hoặc mật khẩu!", null);
            }

            // 4. Nếu mật khẩu đúng, reset bộ đếm số lần đăng nhập sai
            await _userManager.ResetAccessFailedCountAsync(user);

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

            return (true, StatusCodes.Status200OK, "Đăng nhập thành công!", new AuthDTOs.AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                Username = user.UserName,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Roles = userRoles
            });
        }

        public async Task<(bool Succeeded, int StatusCode, string Message, IEnumerable<string>? Errors)> ChangePasswordAsync(string userId, AuthDTOs.ChangePasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, StatusCodes.Status404NotFound, "Không tìm thấy tài khoản.", null);

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            
            if (!result.Succeeded)
                return (false, StatusCodes.Status400BadRequest, "Đổi mật khẩu thất bại!", result.Errors.Select(e => e.Description));

            return (true, StatusCodes.Status200OK, "Đổi mật khẩu thành công!", null);
        }

        public async Task<(bool Succeeded, int StatusCode, string Message, IEnumerable<string>? Errors)> UpdateProfileAsync(string userId, AuthDTOs.UpdateProfileDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, StatusCodes.Status404NotFound, "Không tìm thấy tài khoản.", null);

            if (!string.IsNullOrEmpty(user.AvatarUrl) && user.AvatarUrl != model.AvatarUrl)
            {
                _uploadService.DeleteImage(user.AvatarUrl);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.AvatarUrl = model.AvatarUrl;

            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
                return (false, StatusCodes.Status400BadRequest, "Cập nhật thông tin thất bại!", result.Errors.Select(e => e.Description));

            return (true, StatusCodes.Status200OK, "Cập nhật thông tin cá nhân thành công!", null);
        }

        public async Task<(bool Succeeded, int StatusCode, string Message, AuthDTOs.UserProfileDto? Data)> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, StatusCodes.Status404NotFound, "Không tìm thấy tài khoản.", null);

            return (true, StatusCodes.Status200OK, "Thành công!", new AuthDTOs.UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl
            });
        }
    }
}