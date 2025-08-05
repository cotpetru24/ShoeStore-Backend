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

                //here return create and return product DTO
                return Ok(products);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
