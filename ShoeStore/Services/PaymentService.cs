using Microsoft.Extensions.Options;
using ShoeStore.Dto.Payment;
using Stripe;
using ShoeStore.DataContext.PostgreSQL.Models;

namespace ShoeStore.Services
{
    public class PaymentService
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

        public async Task<PaymentIntent> StorePaymentDetails(StorePaymentDto storePaymentDto, string userId, string userEmail)
        {
            var userDetails = _dbContext.UserDetails.FirstOrDefault(u => u.AspNetUserId == userId);

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(
                storePaymentDto.PaymentIntentId,
                new PaymentIntentGetOptions
                {
                    Expand = new List<string> { "payment_method", "latest_charge" }
                }
            );

            var paymentMethod = paymentIntent.PaymentMethod;

            // Map Stripe type to your DB PaymentMethod.Id
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

            // add more mappings if needed

            var latestCharge = paymentIntent.LatestCharge as Charge;

            var statusId = paymentIntent.Status switch
            {
                "succeeded" => 3,
                "processing" => 2,
                "requires_capture" => 2,
                "canceled" => 5,
                "requires_payment_method" => 1,
                "requires_confirmation" => 1,
                "requires_action" => 1,
                _ => 1
            };

            if (latestCharge?.AmountRefunded > 0)
                statusId = latestCharge.AmountRefunded >= latestCharge.Amount ? 6 : 7;

            var paymentToStore = new Payment()
            {
                PaymentIntentId = paymentIntent.Id,
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency,
                TransactionId = paymentIntent.Id,
                PaymentStatus = statusId,
                PaymentMethodId = paymentMethodId,
                ProcessedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CardBrand = paymentMethod?.Card?.Brand,
                CardLast4 = paymentMethod?.Card?.Last4,
                ReceiptUrl = latestCharge?.ReceiptUrl
            };

            _dbContext.Payments.Add(paymentToStore);
            await _dbContext.SaveChangesAsync();

            return paymentIntent;
        }

        public async Task<bool?> RefundPayment(int paymentId)
        {
            var existingPayment = _dbContext.Payments.FirstOrDefault(p => p.Id == paymentId);

            if (existingPayment == null) return null;

            existingPayment.PaymentStatus = 6;
            existingPayment.UpdatedAt = DateTime.Now;

            await _dbContext.SaveChangesAsync();
            //int rowsAffected = await _dbContext.SaveChangesAsync();
            //if (rowsAffected != 1) return false;

            return true;
        }

    }
}