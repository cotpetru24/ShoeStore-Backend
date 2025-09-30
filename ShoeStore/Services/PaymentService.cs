﻿using Microsoft.Extensions.Options;
using ShoeStore.Dto.Payment;
using Stripe;
using ShoeStore.DataContext.PostgreSQL.Models;

namespace ShoeStore.Services
{
    public class PaymentService
    {
        private readonly PaymentIntentService _paymentIntentService;
        private readonly PaymentIntentCreateOptions _paymentIntentCreateOptions;
        private readonly string _stripeSecretKey;
        private readonly ShoeStoreContext _dbContext = new ShoeStoreContext();


        public PaymentService(ShoeStoreContext dbContext)
        {
            _dbContext = dbContext;
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

            //var paymentMethod = paymentIntent.PaymentMethod;
            //var latestCharge = paymentIntent.LatestCharge as Charge;

            //var paymentToStore = new Payment()
            //{
            //    OrderId = storePaymentDto.OrderId,
            //    PaymentIntentId = paymentIntent.Id,
            //    Amount = paymentIntent.Amount / 100m,
            //    Currency = paymentIntent.Currency,
            //    TransactionId = paymentIntent.Id,
            //    PaymentStatusId = paymentIntent.Status == "succeeded" ? 1 : 2,
            //    ProcessedAt = DateTime.Now,
            //    CreatedAt = DateTime.Now,
            //    UpdatedAt = DateTime.Now,
            //    CustomerId = paymentIntent.CustomerId,
            //    CardBrand = paymentMethod?.Card?.Brand,
            //    CardLast4 = paymentMethod?.Card?.Last4,
            //    BillingName = paymentMethod?.BillingDetails?.Name,
            //    BillingEmail = paymentMethod?.BillingDetails?.Email,
            //    ReceiptUrl = latestCharge?.ReceiptUrl
            //};

            //_dbContext.Payments.Add(paymentToStore);
            //await _dbContext.SaveChangesAsync();
            var paymentMethod = paymentIntent.PaymentMethod;

            // Map Stripe type to your DB PaymentMethod.Id
            int? paymentMethodId = null;
            if (paymentMethod?.Type == "card")
            {
                paymentMethodId = _dbContext.PaymentMethods
                    .Where(pm => pm.Code == "card")
                    .Select(pm => pm.Id)
                    .FirstOrDefault();
            }
            else if (paymentMethod?.Type == "paypal")
            {
                paymentMethodId = _dbContext.PaymentMethods
                    .Where(pm => pm.Code == "paypal")
                    .Select(pm => pm.Id)
                    .FirstOrDefault();
            }
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
                OrderId = storePaymentDto.OrderId,
                PaymentIntentId = paymentIntent.Id,
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency,
                TransactionId = paymentIntent.Id,
                PaymentStatusId = statusId,
                PaymentMethodId = paymentMethodId,
                ProcessedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CustomerId = paymentIntent.CustomerId,
                CardBrand = paymentMethod?.Card?.Brand,
                CardLast4 = paymentMethod?.Card?.Last4,
                BillingName = $"{userDetails?.FirstName} {userDetails?.LastName}",
                BillingEmail = userEmail,
                ReceiptUrl = latestCharge?.ReceiptUrl
            };

            _dbContext.Payments.Add(paymentToStore);
            await _dbContext.SaveChangesAsync();


            return paymentIntent;
        }


        public async Task<bool?> RefundPayment(int orderId)
        {
            var exitingPayment = _dbContext.Payments.FirstOrDefault(p => p.OrderId == orderId && p.PaymentStatusId == 3);
           
            if (exitingPayment == null) return null;

            exitingPayment.PaymentStatusId = 6;
            exitingPayment.UpdatedAt = DateTime.Now;

            int rowsAffected = await _dbContext.SaveChangesAsync();
            if (rowsAffected != 1) return false;

            return true;
        }

    }
}