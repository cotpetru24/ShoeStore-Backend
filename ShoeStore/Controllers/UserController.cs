using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.User;
using ShoeStore.Services;
using System.Security.Claims;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserService _userService;

        public UserController(
            UserManager<IdentityUser> injectedUserManager, 
            RoleManager<IdentityRole> inctedRoleManager,
            UserService userService)
        {
            _roleManager = inctedRoleManager;
            _userManager = injectedUserManager;
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfileAsync()
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(userProfile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUserProfileAsync([FromBody] UpdateUserProfileRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var success = await _userService.UpdateUserProfileAsync(userId, request);
            if (!success)
            {
                return BadRequest(new { message = "Failed to update user profile" });
            }

            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var success = await _userService.ChangePasswordAsync(userId, request);
            if (!success)
            {
                return BadRequest(new { message = "Failed to change password. Please check your current password." });
            }

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStatsAsync()
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token" });
            }

            var stats = await _userService.GetUserStatsAsync(userId);
            return Ok(stats);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = _userManager.Users.Select(u => new
            {
                u.Id,
                u.Email,
                u.EmailConfirmed,
                u.LockoutEnd,
                u.LockoutEnabled,
                u.AccessFailedCount
            }).ToList();

            return Ok(users);
        }
    }
}
