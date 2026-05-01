﻿﻿﻿namespace DoAn_API.DTOs
{
    public class AuthDTOs
    {
        public class RegisterDto
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string FullName { get; set; }
        }

        public class LoginDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class ChangePasswordDto
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
        }

        public class UpdateProfileDto
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string? AvatarUrl { get; set; }
        }

        public class UserProfileDto
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public string? AvatarUrl { get; set; }
        }

        public class AuthResponseDto
        {
            public string Token { get; set; }
            public string Username { get; set; }
            public List<string> Roles { get; set; }
            public string? AvatarUrl { get; set; }
            public DateTime Expiration { get; set; }
        }
    }
}
