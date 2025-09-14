using Microsoft.Extensions.Options;
using Stripe;

namespace ShoeStore.Services
{
    public class PaymentService
    {
        private readonly PaymentIntentService _paymentIntentService;
        private readonly PaymentIntentCreateOptions _paymentIntentCreateOptions;
        private readonly string _stripeSecretKey;


        public PaymentService()
        {
        }

        public async Task<PaymentIntent> CreatePaymentIntent(long amount)
        {

            var service = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = "gbp",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            return await service.CreateAsync(options);
        }
    }
}