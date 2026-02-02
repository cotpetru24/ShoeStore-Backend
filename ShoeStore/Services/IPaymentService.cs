using ShoeStore.Dto.Payment;
using Stripe;

namespace ShoeStore.Services
{
    public interface IPaymentService
    {
        public Task<CreatePaymentIntentResponseDto> CreatePaymentIntent(CreatePaymentIntentRequestDto request);
        public Task<PaymentIntent> StorePaymentDetails(string paymentIntentId);
        public Task<bool> RefundPayment(string paymentIntentId);
        public Task<PaymentIntent> GetPaymentIntentFromStripe(string paymentIntentId);

    }
}
