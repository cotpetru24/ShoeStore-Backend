using Microsoft.AspNetCore.Mvc;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }


        [HttpPost("createPaymentIntent")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] long amount)
        {
            try
            {
                var paymentIntent = await _paymentService.CreatePaymentIntent(amount);
                return Ok(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
