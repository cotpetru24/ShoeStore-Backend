using Microsoft.EntityFrameworkCore;
using Moq;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto;
using ShoeStore.Dto.Order;
using ShoeStore.Services;
using Stripe;
using Xunit;

namespace ShoeStore.Tests
{
    public class OrderServiceTests
    {
        private ShoeStoreContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ShoeStoreContext>()
                .UseInMemoryDatabase(databaseName: $"OrderTestDb_{Guid.NewGuid()}")
                .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new ShoeStoreContext(options);
        }


        [Fact]
        public async Task PlaceOrderAsync_ShouldThrowArgumentException_WhenRequestedQuantityExceedsStock()
        {
            var context = CreateContext();
            var mockPayment = new Mock<IPaymentService>();

            mockPayment.Setup(x => x.GetPaymentIntentFromStripe(It.IsAny<string>()))
                    .ReturnsAsync(new PaymentIntent { Id = "paymentTest", Status = "succeeded" });
            mockPayment.Setup(x => x.StorePaymentDetails(It.IsAny<string>()))
                .ReturnsAsync((string id) =>
                {
                    context.Payments.Add(new Payment
                    {
                        PaymentIntentId = id,
                        PaymentMethodId = 1,
                        PaymentStatus = (int)PaymentStatusEnum.Paid,
                        Amount = 200m,
                        Currency = "gbp",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    context.SaveChanges();
                    return new PaymentIntent { Id = id, Status = "succeeded" };
                });

            var orderService = new OrderService(context, mockPayment.Object);

            var brand = new Brand { Id = 1, Name = "Nike" };
            var audience = new Audience { Id = 1, Code = "men", DisplayName = "Men" };

            var product = new DataContext.PostgreSQL.Models.Product
            {
                Id = 1,
                Name = "Test Shoe",
                Price = 100,
                BrandId = 1,
                AudienceId = 1,
                ProductSizes = new List<ProductSize>
                {
                    new ProductSize { Id = 1, UkSize = 9, Stock = 5, Barcode = "BAR123", Sku = "1-9-BAR123" }
                }
            };

            var shippingAddress = new UserAddress
            {
                Id = 1,
                UserId = "user1",
                AddressLine1 = "123 Test St",
                City = "Test City",
                Postcode = "T123ST",
                Country = "UK"
            };

            context.Brands.Add(brand);
            context.Audiences.Add(audience);
            context.Products.Add(product);
            context.UserAddresses.Add(shippingAddress);
            await context.SaveChangesAsync();

            var paymentMethod = new DataContext.PostgreSQL.Models.PaymentMethod()
            {
                Id = 1,
                Code = "card",
                DisplayName = "Card",
                Provider = "stripe",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.PaymentMethods.Add(paymentMethod);

            var request = new PlaceOrderRequestDto
            {
                PaymentIntentId = "paymentTest",
                OrderItems = new List<OrderItemRequestDto>
                {
                    new OrderItemRequestDto { ProductId = 1, ProductSizeBarcode = "BAR123", Quantity = 10 }
                },
                ShippingAddressId = 1,
                BillingAddressSameAsShipping = true,
                ShippingCost = 0,
                Discount = 0
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                orderService.PlaceOrderAsync(request, "user1"));

            Assert.Contains("Insufficient stock", exception.Message);
        }


        [Fact]
        public async Task CreateOrderAsync_ShouldDecreaseStock_WhenOrderIsCreated()
        {
            var context = CreateContext();
            var mockPayment = new Mock<IPaymentService>();

            mockPayment.Setup(x => x.GetPaymentIntentFromStripe(It.IsAny<string>()))
                .ReturnsAsync(new PaymentIntent { Id = "paymentTest", Status = "succeeded" });
            mockPayment.Setup(x => x.StorePaymentDetails(It.IsAny<string>()))
                .ReturnsAsync((string id) =>
                {
                    context.Payments.Add(new Payment
                    {
                        PaymentIntentId = id,
                        PaymentMethodId = 1,
                        PaymentStatus = (int)PaymentStatusEnum.Paid,
                        Amount = 200m,
                        Currency = "gbp",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    context.SaveChanges();
                    return new PaymentIntent { Id = id, Status = "succeeded" };
                });

            var orderService = new OrderService(context, mockPayment.Object);

            var brand = new Brand { Id = 1, Name = "Nike" };
            var audience = new Audience { Id = 1, Code = "men", DisplayName = "Men" };
            var productSize = new ProductSize { Id = 1, UkSize = 9, Stock = 10, Barcode = "BAR123", Sku = "1-9-BAR123" };

            var product = new DataContext.PostgreSQL.Models.Product
            {
                Id = 1,
                Name = "Test Shoe",
                Price = 100,
                BrandId = 1,
                AudienceId = 1,
                ProductSizes = new List<ProductSize> { productSize }
            };

            var shippingAddress = new UserAddress
            {
                Id = 1,
                UserId = "user1",
                AddressLine1 = "123 Test St",
                City = "Test City",
                Postcode = "T123ST",
                Country = "UK"
            };

            context.Brands.Add(brand);
            context.Audiences.Add(audience);
            context.Products.Add(product);
            context.UserAddresses.Add(shippingAddress);
            await context.SaveChangesAsync();

            var paymentMethod = new DataContext.PostgreSQL.Models.PaymentMethod()
            {
                Id = 1,
                Code = "card",
                DisplayName = "Card",
                Provider = "stripe",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.PaymentMethods.Add(paymentMethod);

            var payment = new Payment
            {
                PaymentIntentId = "paymentTest",
                PaymentMethodId = 1,
                PaymentStatus = (int)PaymentStatusEnum.Paid,
                Amount = 200m,
                Currency = "gbp",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            var request = new PlaceOrderRequestDto
            {
                OrderItems = new List<OrderItemRequestDto>
                {
                    new OrderItemRequestDto { ProductId = 1, ProductSizeBarcode = "BAR123", Quantity = 3 }
                },
                ShippingAddressId = 1,
                BillingAddressSameAsShipping = true,
                ShippingCost = 0,
                Discount = 0
            };

            await orderService.CreateOrderAsync(request, "user1", "paymentTest");

            await context.Entry(productSize).ReloadAsync();
            Assert.Equal(7, productSize.Stock);
        }


        [Fact]
        public async Task PlaceOrderAsync_ShouldCreateOrderWithCorrectTotal_WhenOrderIsPlaced()
        {
            var context = CreateContext();
            var mockPaymentService = new Mock<IPaymentService>();

            mockPaymentService.Setup(x => x.GetPaymentIntentFromStripe(It.IsAny<string>()))
                    .ReturnsAsync(new PaymentIntent { Id = "paymentTest", Status = "succeeded" });
            mockPaymentService.Setup(x => x.StorePaymentDetails(It.IsAny<string>()))
                .ReturnsAsync((string id) =>
                {
                    context.Payments.Add(new Payment
                    {
                        PaymentIntentId = id,
                        PaymentMethodId = 1,
                        PaymentStatus = (int)PaymentStatusEnum.Paid,
                        Amount = 200m,
                        Currency = "gbp",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    context.SaveChanges();
                    return new PaymentIntent { Id = id, Status = "succeeded" };
                });

            var orderService = new OrderService(context, mockPaymentService.Object);

            var brand = new Brand { Id = 1, Name = "Nike" };
            var audience = new Audience { Id = 1, Code = "men", DisplayName = "Men" };

            var product = new DataContext.PostgreSQL.Models.Product
            {
                Id = 1,
                Name = "Test Shoe",
                Price = 100,
                BrandId = 1,
                AudienceId = 1,
                ProductSizes = new List<ProductSize>
                {
                    new ProductSize { Id = 1, UkSize = 9, Stock = 10, Barcode = "BAR123", Sku = "1-9-BAR123" }
                }
            };

            var shippingAddress = new UserAddress
            {
                Id = 1,
                UserId = "user1",
                AddressLine1 = "123 Test St",
                City = "Test City",
                Postcode = "T123ST",
                Country = "UK"
            };

            context.Brands.Add(brand);
            context.Audiences.Add(audience);
            context.Products.Add(product);
            context.UserAddresses.Add(shippingAddress);
            await context.SaveChangesAsync();

            var paymentMethod = new DataContext.PostgreSQL.Models.PaymentMethod
            {
                Id = 1,
                Code = "card",
                DisplayName = "Card",
                Provider = "stripe",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var payment = new Payment
            {
                PaymentIntentId = "paymentTest",
                PaymentMethodId = 1,
                PaymentStatus = (int)PaymentStatusEnum.Paid,
                Amount = 200m,
                Currency = "gbp",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.PaymentMethods.Add(paymentMethod);
            context.Payments.Add(payment);
            await context.SaveChangesAsync();

            var request = new PlaceOrderRequestDto
            {
                OrderItems = new List<OrderItemRequestDto>
                {
                    new OrderItemRequestDto { ProductId = 1, ProductSizeBarcode = "BAR123", Quantity = 2 }
                },
                ShippingAddressId = 1,
                BillingAddressSameAsShipping = true,
                ShippingCost = 5,
                Discount = 10
            };

            var response = await orderService.CreateOrderAsync(request, "user1", "paymentTest");

            var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == response.OrderId);
            Assert.NotNull(order);
            Assert.Equal(195m, order.Total);
        }
    }
}
