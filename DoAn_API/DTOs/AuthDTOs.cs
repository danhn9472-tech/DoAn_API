namespace DoAn_API.DTOs
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

        public class AuthResponseDto
        {
            public string Token { get; set; }
            public string Username { get; set; }
            public List<string> Roles { get; set; }
            public DateTime Expiration { get; set; }
        }
    }
}
