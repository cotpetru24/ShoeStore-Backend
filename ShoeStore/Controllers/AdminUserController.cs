using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.Admin;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Administrator")]
    public class AdminUserController : ControllerBase
    {
        private readonly AdminUserService _adminUserService;

        public AdminUserController(AdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminUsersListDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdminUsersListDto>> GetUsersAsync([FromQuery] GetAdminUsersRequestDto request)
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

            var users = await _adminUserService.GetUsersAsync(request);
            return Ok(users);
        }


        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminUserDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdminUserDto>> GetUserByIdAsync(string userId)
        {
            var user = await _adminUserService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }


        [HttpPut("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserAsync(string userId, [FromBody] AdminUpdateUserRequestDto request)
        {
            var success = await _adminUserService.UpdateUserAsync(userId, request);
            if (!success)
                return NotFound(new { message = "User not found or update failed" });

            return Ok(new { message = "User updated successfully" });
        }


        [HttpDelete("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUserAsync(string userId)
        {
            var success = await _adminUserService.DeleteUserAsync(userId);
            if (!success)
                return NotFound(new { message = "User not found" });

            return Ok(new { message = "User deleted successfully" });
        }


        [HttpPut("{userId}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserPasswordAsync(string userId, [FromBody] AdminUpdateUserPasswordRequestDto request)
        {
            var success = await _adminUserService.UpdateUserPasswordAsync(userId, request);
            if (!success)
                return NotFound(new { message = "User not found or password update failed" });

            return Ok(new { message = "User password updated successfully" });
        }


        [HttpGet("{userId}/orders")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminOrdersListDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserOrdersAsync(string userId, [FromQuery] GetAdminUserOrdersRequestDto request)
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;
            request.UserId = userId;

            var orders = await _adminUserService.GetUserOrdersAsync(request);
            return Ok(orders);
        }
    }
}
