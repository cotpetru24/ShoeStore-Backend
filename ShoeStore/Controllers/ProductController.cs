using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ShoeStoreContext _context;

        public ProductController(ShoeStoreContext injectedContext)
        {
            _context = injectedContext;
        }



        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Product>))]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetCustomersAsync(int page = 1, int pageSize = 30, string? country = null)
        {
            try
            {
                IEnumerable<Product> customers = await _context.Products.ToListAsync();
                return Ok(customers);
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
