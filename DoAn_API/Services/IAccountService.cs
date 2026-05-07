using DoAn_API.DTOs;

namespace DoAn_API.Services
{
    public interface IAccountService
    {
        Task<(bool Succeeded, string Message, IEnumerable<string>? Errors)> RegisterAsync(AuthDTOs.RegisterDto model);
        Task<(bool Succeeded, int StatusCode, string Message, AuthDTOs.AuthResponseDto? Data)> LoginAsync(AuthDTOs.LoginDto model);
        Task<(bool Succeeded, int StatusCode, string Message, IEnumerable<string>? Errors)> ChangePasswordAsync(string userId, AuthDTOs.ChangePasswordDto model);
        Task<(bool Succeeded, int StatusCode, string Message, IEnumerable<string>? Errors)> UpdateProfileAsync(string userId, AuthDTOs.UpdateProfileDto model);
        Task<(bool Succeeded, int StatusCode, string Message, AuthDTOs.UserProfileDto? Data)> GetProfileAsync(string userId);
    }
}