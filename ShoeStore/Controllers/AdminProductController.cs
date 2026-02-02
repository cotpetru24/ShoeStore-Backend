using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.Admin;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "Administrator")]
    public class AdminProductController : ControllerBase
    {
        private readonly AdminProductService _adminProductService;

        public AdminProductController(AdminProductService adminProductService)
        {
            _adminProductService = adminProductService;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminProductListDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdminProductListDto>> GetProductsAsync([FromQuery] GetAdminProductsRequestDto request)
        {
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 10;

            var products = await _adminProductService.GetProductsAsync(request);
            return Ok(products);
        }


        [HttpGet("brands")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AdminBrandDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AdminBrandDto>>> GetProductBrandsAsync()
        {
            var brands = await _adminProductService.GetProductBrandsAsync();
            return Ok(brands);
        }


        [HttpGet("audience")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AdminAudienceDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AdminAudienceDto>>> GetProductAudienceAsync()
        {
            var audience = await _adminProductService.GetProductAudienceAsync();
            return Ok(audience);
        }


        [HttpGet("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminProductDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdminProductDto>> GetProductByIdAsync(int productId)
        {
            var product = await _adminProductService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdminProductDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AdminProductDto>> CreateProductAsync([FromBody] AdminProductDto productToAdd)
        {
            var response = await _adminProductService.CreateProductAsync(productToAdd);
            if (response == null)
                return BadRequest(new { message = "Failed to create the product" });

            return Ok(response);
        }


        [HttpPut("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProductAsync(int productId, [FromBody] AdminProductDto productToUpdate)
        {
            var success = await _adminProductService.UpdateProductAsync(productId, productToUpdate);
            if (!success)
                return NotFound(new { message = "Product not found or update failed" });

            return Ok(new { message = "Product updated successfully" });
        }


        [HttpDelete("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProductAsync(int productId)
        {
            var success = await _adminProductService.DeleteProductAsync(productId);
            if (!success)
                return NotFound(new { message = "Product not found" });

            return Ok(new { message = "Product deleted successfully" });
        }
    }
}
