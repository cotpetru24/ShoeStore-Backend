using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.User;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
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
                return NotFound(new { message = "User not found" });

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


        //[Authorize(Roles = "Administrator")]
        //[HttpGet("users")]
        //public async Task<IActionResult> GetAllUsersAsync()
        //{
        //    var users = await _userService.GetAllUsersAsync();
        //    return Ok(users);
        //}
    }
}
