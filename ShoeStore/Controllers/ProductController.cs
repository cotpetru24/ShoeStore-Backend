using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.Product;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductController(ProductService injectedService)
        {
            _service = injectedService;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetProductsResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductsAsync([FromQuery] GetProductsRequest request)
        {
            var products = await _service.GetProductsAsync(request);
            return Ok(products);
        }


        [HttpGet("featured")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetProductsResponseDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFeaturedProductsAsync()
        {
            var products = await _service.GetFeaturedProductsAsync();
            return Ok(products);
        }


        [HttpGet("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetSingleProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetSingleProductResponseDto>?> GetProductByIdAsync(int productId)
        {
            var product = await _service.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound(new { message = $"Product with ID {productId} not found" });

            return Ok(product);
        }
    }
}
