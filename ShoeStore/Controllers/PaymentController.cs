using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Dto.Payment;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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


                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");



                var paymentIntent = await _paymentService.CreatePaymentIntent(amount);
                return Ok(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("storePaymentDetails")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> StorePaymentDetails([FromBody] StorePaymentDto storePaymentDto)
        {
            try
            {

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var userEmail = User.FindFirst("Email")?.Value;

                var paymentIntent = await _paymentService.StorePaymentDetails(storePaymentDto, userId, userEmail);

                //here return the paymentobjuct or just the paymnet id
                return Ok();
                //return Ok(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("RefundPayment")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RefundPayment([FromBody] int orderId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var userId = User.FindFirst("Id")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var refundResult = await _paymentService.RefundPayment(orderId);
                if (refundResult == null)
                {
                    return NotFound("Payment intent not found or could not be refunded.");
                }
                if (refundResult == false)
                {
                    return StatusCode(500, "Failed to refund.");
                }
                return Ok("Payment refunded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }


    }
}
