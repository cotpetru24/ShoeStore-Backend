﻿namespace ShoeStore.Dto.Auth
{
    public class RegisterRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
