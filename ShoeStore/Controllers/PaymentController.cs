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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreatePaymentIntentResponseDto>> CreatePaymentIntent([FromBody] CreatePaymentIntentRequestDto request)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var intent = await _paymentService.CreatePaymentIntent(request);
            return Ok(intent);
        }


        [HttpPost("storePaymentDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StorePaymentDetails([FromBody] StorePaymentDto storePaymentDto)
        {
            var userId = User.FindFirst("Id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            var userEmail = User.FindFirst("Email")?.Value;
            var paymentIntent = await _paymentService.StorePaymentDetails(storePaymentDto.PaymentIntentId);

            return Ok();
        }


        //[HttpPut("RefundPayment")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> RefundPayment([FromBody] int orderId)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var userId = User.FindFirst("Id")?.Value;
        //    if (string.IsNullOrEmpty(userId))
        //        return Unauthorized(new { message = "User not authenticated" });

        //    var refundResult = await _paymentService.RefundPayment(orderId);
        //    if (refundResult == null)
        //        return NotFound(new { message = "Payment intent not found or could not be refunded." });

        //    if (refundResult == false)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to refund." });
        //    }

        //    return Ok(new { message = "Payment refunded successfully." });
        //}
    }
}
