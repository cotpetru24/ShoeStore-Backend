//--------------- delete this file ----------------











//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using ShoeStore.Dto.Admin;
//using ShoeStore.Services;

//namespace ShoeStore.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize(Roles = "Administrator")]
//    [Obsolete("This controller is deprecated. Use AdminDashboardController, AdminUserController, AdminOrderController, and AdminProductController instead.")]
//    public class AdminController : ControllerBase
//    {
//        private readonly AdminService _adminService;

//        public AdminController(AdminService adminService)
//        {
//            _adminService = adminService;
//        }

//        #region Dashboard

//        [HttpGet("dashboard")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminDashboardDto))]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<AdminDashboardDto>> GetDashboardStatsAsync()
//        {
//            var stats = await _adminService.GetDashboardStatsAsync();
//            return Ok(stats);
//        }

//        #endregion

//        #region Users

//        [HttpGet("users")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminUsersListDto))]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<AdminUsersListDto>> GetUsersAsync([FromQuery] GetAdminUsersRequestDto request)
//        {
//            if (request.PageNumber < 1) request.PageNumber = 1;
//            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

//            var users = await _adminService.GetUsersAsync(request);
//            return Ok(users);
//        }

//        [HttpGet("users/{userId}")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminUserDto))]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<ActionResult<AdminUserDto>> GetUserByIdAsync(string userId)
//        {
//            var user = await _adminService.GetUserByIdAsync(userId);
//            if (user == null)
//                return NotFound(new { message = "User not found" });

//            return Ok(user);
//        }

//        [HttpPut("users/{userId}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> UpdateUserAsync(string userId, [FromBody] UpdateUserRequestDto request)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var success = await _adminService.UpdateUserAsync(userId, request);
//            if (!success)
//            {
//                return NotFound(new { message = "User not found or update failed" });
//            }

//            return Ok(new { message = "User updated successfully" });
//        }

//        [HttpPost("users")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserRequestDto request)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var success = await _adminService.CreateUserAsync(request);
//            if (!success)
//            {
//                return BadRequest(new { message = "Failed to create user" });
//            }

//            return Ok(new { message = "User created successfully" });
//        }

//        [HttpDelete("users/{userId}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> DeleteUserAsync(string userId)
//        {
//            var success = await _adminService.DeleteUserAsync(userId);
//            if (!success)
//            {
//                return NotFound(new { message = "User not found" });
//            }

//            return Ok(new { message = "User deleted successfully" });
//        }

//        [HttpPut("users/{userId}/password")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> UpdateUserPasswordAsync(string userId, [FromBody] UpdateUserPasswordRequestDto request)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var success = await _adminService.UpdateUserPasswordAsync(userId, request);
//            if (!success)
//            {
//                return NotFound(new { message = "User not found or password update failed" });
//            }

//            return Ok(new { message = "User password updated successfully" });
//        }

//        [HttpGet("users/{userId}/orders")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminOrderListDto))]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> GetUserOrdersAsync(string userId, [FromQuery] GetUserOrdersRequestDto request)
//        {
//            if (request.PageNumber < 1) request.PageNumber = 1;
//            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;
//            request.UserId = userId;

//            var orders = await _adminService.GetUserOrdersAsync(request);
//            return Ok(orders);
//        }

//        #endregion

//        #region Orders

//        [HttpGet("orders")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminOrderListDto))]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> GetOrdersAsync([FromQuery] GetAdminOrdersRequestDto request)
//        {
//            if (request.PageNumber < 1) request.PageNumber = 1;
//            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

//            var orders = await _adminService.GetOrdersAsync(request);
//            return Ok(orders);
//        }

//        [HttpGet("orders/{orderId}")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminOrderDto))]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> GetOrderByIdAsync(int orderId)
//        {
//            var order = await _adminService.GetOrderByIdAsync(orderId);
//            if (order == null)
//                return NotFound(new { message = "Order not found" });

//            return Ok(order);
//        }

//        [HttpPut("orders/{orderId}/status")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> UpdateOrderStatusAsync(int orderId, [FromBody] UpdateOrderStatusRequestDto request)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var success = await _adminService.UpdateOrderStatusAsync(orderId, request);
//            if (!success)
//            {
//                return NotFound(new { message = "Order not found or update failed" });
//            }

//            return Ok(new { message = "Order status updated successfully" });
//        }

//        #endregion

//        #region Products

//        [HttpGet("products")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminProductListDto))]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> GetProductsAsync([FromQuery] GetProductsRequestDto request)
//        {
//            if (request.PageNumber < 1) request.PageNumber = 1;
//            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

//            var products = await _adminService.GetProductsAsync(request);
//            return Ok(products);
//        }

//        [HttpGet("products/brands")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AdminBrandDto>))]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> GetProductBrandsAsync()
//        {
//            var brands = await _adminService.GetProductBrandsAsync();
//            return Ok(brands);
//        }

//        [HttpGet("products/audience")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AdminAudienceDto>))]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> GetProductAudienceAsync()
//        {
//            var audience = await _adminService.GetProductAudienceAsync();
//            return Ok(audience);
//        }

//        [HttpGet("products/{productId}")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminProductDto))]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> GetProductByIdAsync(int productId)
//        {
//            var product = await _adminService.GetProductByIdAsync(productId);
//            if (product == null)
//                return NotFound(new { message = "Product not found" });

//            return Ok(product);
//        }

//        [HttpPost("products")]
//        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminProductDto))]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> CreateProductAsync([FromBody] AdminProductDto productToAdd)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var response = await _adminService.CreateProductAsync(productToAdd);
//            if (response == null)
//                return BadRequest(new { message = "Failed to create the product" });

//            return Ok(response);
//        }

//        [HttpPut("products/{productId}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status400BadRequest)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> UpdateProductAsync(int productId, [FromBody] AdminProductDto productToUpdate)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var success = await _adminService.UpdateProductAsync(productId, productToUpdate);
//            if (!success)
//            {
//                return NotFound(new { message = "Product not found or update failed" });
//            }

//            return Ok(new { message = "Product updated successfully" });
//        }

//        [HttpDelete("products/{productId}")]
//        [ProducesResponseType(StatusCodes.Status200OK)]
//        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//        [ProducesResponseType(StatusCodes.Status404NotFound)]
//        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//        public async Task<IActionResult> DeleteProductAsync(int productId)
//        {
//            var success = await _adminService.DeleteProductAsync(productId);
//            if (!success)
//            {
//                return NotFound(new { message = "Product not found" });
//            }

//            return Ok(new { message = "Product deleted successfully" });
//        }

//        #endregion
//    }
//}
