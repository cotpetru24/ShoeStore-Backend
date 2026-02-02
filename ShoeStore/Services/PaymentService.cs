using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto;
using ShoeStore.Dto.Payment;
using Stripe;

namespace ShoeStore.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ShoeStoreContext _dbContext;

        public PaymentService(ShoeStoreContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<CreatePaymentIntentResponseDto> CreatePaymentIntent(CreatePaymentIntentRequestDto request)
        {
            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = request.Amount,
                Currency = "gbp",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var response = await service.CreateAsync(options);

            var intent = new CreatePaymentIntentResponseDto()
            {
                ClientSecret = response.ClientSecret,
            };

            return intent;
        }


        public async Task<PaymentIntent> StorePaymentDetails(string paymentIntentId)
        {

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(
                paymentIntentId,
                new PaymentIntentGetOptions
                {
                    Expand = new List<string> { "latest_charge" }
                }
            );

            var latestCharge = paymentIntent.LatestCharge as Charge;

            var paymentMethod = latestCharge?.PaymentMethodDetails;

            var paymentMethodCode = paymentMethod?.Type switch
            {
                "card" => "card",
                "paypal" => "paypal",
                _ => throw new InvalidOperationException(
                    $"Unsupported payment method type: {paymentMethod?.Type}")
            };

            var paymentMethodId = _dbContext.PaymentMethods
                .Where(pm => pm.Code == paymentMethodCode)
                .Select(pm => pm.Id)
                .Single();


            var statusId = paymentIntent.Status switch
            {
                "succeeded" => (int)PaymentStatusEnum.Paid,
                "processing" => (int)PaymentStatusEnum.Authorised,
                "requires_capture" => (int)PaymentStatusEnum.Authorised,
                "requires_payment_method" => (int)PaymentStatusEnum.Failed,
                "requires_confirmation" => (int)PaymentStatusEnum.Pending,
                "requires_action" => (int)PaymentStatusEnum.Pending,
                "canceled" => (int)PaymentStatusEnum.Failed,

                _ => (int)PaymentStatusEnum.Pending
            };

            var paymentToStore = new Payment()
            {
                PaymentIntentId = paymentIntent.Id,
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency,
                TransactionId = paymentIntent.Id,
                PaymentStatus = statusId,
                PaymentMethodId = paymentMethodId,
                ProcessedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CardBrand = paymentMethod?.Card?.Brand,
                CardLast4 = paymentMethod?.Card?.Last4,
                ReceiptUrl = latestCharge?.ReceiptUrl
            };

            _dbContext.Payments.Add(paymentToStore);
            await _dbContext.SaveChangesAsync();

            return paymentIntent;
        }


        public async Task<bool> RefundPayment(string paymentIntentId)
        {
            var refundService = new Stripe.RefundService();
            var refund = await refundService.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId
            });

            var existingPayment = _dbContext.Payments
                .FirstOrDefault(p => p.PaymentIntentId == paymentIntentId);

            if (existingPayment != null)
            {
                existingPayment.PaymentStatus = (int)PaymentStatusEnum.Refunded;
                existingPayment.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            return refund.Status == "succeeded";
        }


        public async Task<PaymentIntent> GetPaymentIntentFromStripe(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            return paymentIntent;
        }
    }
}