using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Product;
using ShoeStore.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        [ProducesResponseType(200, Type = typeof(GetProductsResposeDto))]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetProductsAsync([FromQuery] GetProductsRequest request)
        {
            try
            {
                var products = await _service.GetProductsAsync(request);

                return Ok(products);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("featured")]
        [ProducesResponseType(200, Type = typeof(GetProductsResposeDto))]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetfeaturedProductsAsync()
        {
            try
            {
                var products = await _service.GetfeaturedProductsAsync();

                return Ok(products);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{productId}")]
        [ProducesResponseType(200, Type = typeof(ProductDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetSingleProductResponseDto>?> GetProductByIdAsync(int productId)
        {
            try
            {
                var product = await _service.GetProductByIdAsync(productId);

                if (product == null) return NotFound(productId);

                return Ok(product);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
