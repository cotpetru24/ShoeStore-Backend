using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.Admin;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        #region Dashboard

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStatsAsync()
        {
            try
            {
                var stats = await _adminService.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching dashboard stats", error = ex.Message });
            }
        }

        #endregion

        #region Users

        [HttpGet("users")]
        public async Task<IActionResult> GetUsersAsync([FromQuery] GetAdminUsersRequestDto request)
        {
            try
            {
                if (request.PageNumber < 1) request.PageNumber = 1;
                if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

                //var users = await _adminService.GetUsersAsync(pageNumber, pageSize, searchTerm);
                var users = await _adminService.GetUsersAsync(request);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching users", error = ex.Message });
            }
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _adminService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching user", error = ex.Message });
            }
        }

        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUserAsync(string userId, [FromBody] UpdateUserRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _adminService.UpdateUserAsync(userId, request);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to update user" });
                }

                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user", error = ex.Message });
            }
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _adminService.CreateUserAsync(request);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to create user" });
                }

                return Ok(new { message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating user", error = ex.Message });
            }
        }

        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUserAsync(string userId)
        {
            try
            {
                var success = await _adminService.DeleteUserAsync(userId);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to delete user" });
                }

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting user", error = ex.Message });
            }
        }

        [HttpPut("users/{userId}/password")]
        public async Task<IActionResult> UpdateUserPasswordAsync(string userId, [FromBody] UpdateUserPasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _adminService.UpdateUserPasswordAsync(userId, request);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to update user password" });
                }

                return Ok(new { message = "User password updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user password", error = ex.Message });
            }
        }

        [HttpGet("users/{userId}/orders")]
        public async Task<IActionResult> GetUserOrdersAsync(string userId, [FromQuery] GetUserOrdersRequestDto request)
        {
            try
            {
                if (request.PageNumber < 1) request.PageNumber = 1;
                if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;
                request.UserId = userId;

                var orders = await _adminService.GetUserOrdersAsync(request);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching user orders", error = ex.Message });
            }
        }

        #endregion

        #region Orders

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrdersAsync([FromQuery] GetAdminOrdersRequestDto request)
        {
            try
            {
                if (request.PageNumber < 1) request.PageNumber = 1;
                if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

                var orders = await _adminService.GetOrdersAsync(request);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching orders", error = ex.Message });
            }
        }

        [HttpGet("orders/{orderId}")]
        public async Task<IActionResult> GetOrderByIdAsync(int orderId)
        {
            try
            {
                var order = await _adminService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching order", error = ex.Message });
            }
        }

        [HttpPut("orders/{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatusAsync(int orderId, [FromBody] UpdateOrderStatusRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _adminService.UpdateOrderStatusAsync(orderId, request);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to update order status" });
                }

                return Ok(new { message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating order status", error = ex.Message });
            }
        }

        #endregion

        #region Products

        [HttpGet("products")]
        public async Task<IActionResult> GetProductsAsync([FromQuery] GetProductsRequestDto request)
        {
            try
            {
                if (request.PageNumber < 1) request.PageNumber = 1;
                if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

                var products = await _adminService.GetProductsAsync(request);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching products", error = ex.Message });
            }
        }

        [HttpGet("products/{productId}")]
        public async Task<IActionResult> GetProductByIdAsync(int productId)
        {
            try
            {
                var product = await _adminService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new { message = "Product not found" });
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching product", error = ex.Message });
            }
        }

        [HttpPost("products")]
        public async Task<IActionResult> CreateProductAsync([FromBody] CreateProductRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _adminService.CreateProductAsync(request);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to create product" });
                }

                return Ok(new { message = "Product created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating product", error = ex.Message });
            }
        }

        [HttpPut("products/{productId}")]
        public async Task<IActionResult> UpdateProductAsync(int productId, [FromBody] UpdateProductRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _adminService.UpdateProductAsync(productId, request);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to update product" });
                }

                return Ok(new { message = "Product updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating product", error = ex.Message });
            }
        }

        [HttpDelete("products/{productId}")]
        public async Task<IActionResult> DeleteProductAsync(int productId)
        {
            try
            {
                var success = await _adminService.DeleteProductAsync(productId);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to delete product" });
                }

                return Ok(new { message = "Product deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting product", error = ex.Message });
            }
        }

        #endregion
    }
}
