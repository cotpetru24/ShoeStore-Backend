using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Configuration;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



namespace ShoeStore.Services
{
    public class AuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ShoeStoreContext _context;
        private readonly JwtSettings _jwtSettings;


        public AuthService(
            UserManager<IdentityUser> injectedUserManager, 
            RoleManager<IdentityRole> inctedRoleManager,
            ShoeStoreContext injectedContext,
            IOptions<JwtSettings> injectedJwtSettings)
        {
            _roleManager = inctedRoleManager;
            _userManager = injectedUserManager;
            _context = injectedContext;
            _jwtSettings = injectedJwtSettings.Value;
        }


        public async Task<string?> LoginAsync(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null) return null;

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedAccessException();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return token;
        }


        public async Task<string?> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null) throw new InvalidOperationException("User already exists.");

            var newUser = new IdentityUser()
            {
                UserName = request.Email,
                Email = request.Email,

            };

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user: {errors}");
            }

            if (!string.IsNullOrEmpty(request.Role))
            {
                if (!await _roleManager.RoleExistsAsync(request.Role))
                {
                    var roleResult =await _roleManager.CreateAsync(new IdentityRole(request.Role));
                    if ((!roleResult.Succeeded))
                    {
                        var roleErrors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create role : {roleErrors}");
                    }
                }

                await _userManager.AddToRoleAsync(newUser, request.Role);
            }

            var roles = await _userManager.GetRolesAsync(newUser);
            var token = GenerateJwtToken(newUser, roles);

            return token;

        }


        private string GenerateJwtToken(IdentityUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
