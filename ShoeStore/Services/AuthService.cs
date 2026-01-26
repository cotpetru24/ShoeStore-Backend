using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShoeStore.Configuration;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace ShoeStore.Services
{
    public class AuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ShoeStoreContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IConfiguration _configuration;
        private readonly string _adminRole = "Administrator";
        private readonly string _customerRole = "Customer";

        public AuthService(
            UserManager<IdentityUser> injectedUserManager,
            RoleManager<IdentityRole> inctedRoleManager,
            ShoeStoreContext injectedContext,
            IOptions<JwtSettings> injectedJwtSettings,
            IConfiguration configuration)
        {
            _roleManager = inctedRoleManager;
            _userManager = injectedUserManager;
            _context = injectedContext;
            _jwtSettings = injectedJwtSettings.Value;
            _configuration = configuration;
        }

        public async Task SeedAdminAccount()
        {
            if (!await _roleManager.RoleExistsAsync(_adminRole))
                await _roleManager.CreateAsync(new IdentityRole(_adminRole));

            if (!await _roleManager.RoleExistsAsync(_customerRole))
                await _roleManager.CreateAsync(new IdentityRole(_customerRole));

            var adminEmail = _configuration["AdminAccount:Email"];
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                IdentityUser user = new IdentityUser()
                {
                    UserName = _configuration["AdminAccount:Email"],
                    Email = _configuration["AdminAccount:Email"],
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, _configuration["AdminAccount:Password"]);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join("; ", result.Errors.Select(e => e.Description))
                    );
                }


                await _userManager.AddToRoleAsync(user, _adminRole);

                UserDetail newUserDetails = new UserDetail()
                {
                    FirstName = "FirstName",
                    LastName = "LastName",
                    AspNetUserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UserDetails.AddAsync(newUserDetails);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<string?> LoginAsync(LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null) return null;

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedAccessException();
            }

            var token = await GenerateJwtToken(user);

            return token;
        }


        public async Task<string> RegisterAsync(RegisterRequestDto request)
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
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }


            await _userManager.AddToRoleAsync(newUser, _customerRole);

            UserDetail newUserDetails = new UserDetail()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                AspNetUserId = newUser.Id
            };

            await _context.UserDetails.AddAsync(newUserDetails);
            await _context.SaveChangesAsync();

            var token = await GenerateJwtToken(newUser);

            return token;

        }


        private async Task<string> GenerateJwtToken(IdentityUser user)
        {

            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            var userDetails = await _context.UserDetails
                .Where(u => u.AspNetUserId == user.Id)
                .FirstOrDefaultAsync();

            var claims = new List<Claim>(userClaims);

            foreach (var role in userRoles)
            {
                claims.Add(new Claim("role", role));
            }

            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));
            claims.Add(new Claim("Id", user.Id));
            claims.Add(new Claim("Email", user.Email));
            claims.Add(new Claim("FirstName", userDetails.FirstName));


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
