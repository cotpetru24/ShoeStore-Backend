using Xunit;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Services;
using ShoeStore.Dto.Order;
using Moq;
using AutoMapper;

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

        private PaymentService CreatePaymentService(ShoeStoreContext context)
        {
            return new PaymentService(context);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldThrowArgumentException_WhenRequestedQuantityExceedsStock()
        {
            var context = CreateContext();
            var mapper = Mock.Of<IMapper>();
            var paymentService = CreatePaymentService(context);
            var orderService = new OrderService(context, mapper, paymentService);

            var brand = new Brand { Id = 1, Name = "Nike" };
            var audience = new Audience { Id = 1, Code = "men", DisplayName = "Men" };
            var product = new Product
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
            var shippingAddress = new ShippingAddress
            {
                Id = 1,
                UserId = "user1",
                AddressLine1 = "123 Test St",
                City = "Test City",
                County = "Test County",
                Postcode = "T123ST",
                Country = "UK"
            };
            var orderStatus = new OrderStatus { Id = 1, Code = "processing", DisplayName = "Processing" };

            context.Brands.Add(brand);
            context.Audiences.Add(audience);
            context.Products.Add(product);
            context.ShippingAddresses.Add(shippingAddress);
            context.OrderStatuses.Add(orderStatus);
            await context.SaveChangesAsync();

            var request = new PlaceOrderRequestDto
            {
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
        public async Task PlaceOrderAsync_ShouldDecreaseStock_WhenOrderIsPlaced()
        {
            var context = CreateContext();
            var mapper = Mock.Of<IMapper>();
            var paymentService = CreatePaymentService(context);
            var orderService = new OrderService(context, mapper, paymentService);

            var brand = new Brand { Id = 1, Name = "Nike" };
            var audience = new Audience { Id = 1, Code = "men", DisplayName = "Men" };
            var productSize = new ProductSize { Id = 1, UkSize = 9, Stock = 10, Barcode = "BAR123", Sku = "1-9-BAR123" };
            var product = new Product
            {
                Id = 1,
                Name = "Test Shoe",
                Price = 100,
                BrandId = 1,
                AudienceId = 1,
                ProductSizes = new List<ProductSize> { productSize }
            };
            var shippingAddress = new ShippingAddress
            {
                Id = 1,
                UserId = "user1",
                AddressLine1 = "123 Test St",
                City = "Test City",
                County = "Test County",
                Postcode = "T123ST",
                Country = "UK"
            };
            var orderStatus = new OrderStatus { Id = 1, Code = "processing", DisplayName = "Processing" };

            context.Brands.Add(brand);
            context.Audiences.Add(audience);
            context.Products.Add(product);
            context.ShippingAddresses.Add(shippingAddress);
            context.OrderStatuses.Add(orderStatus);
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

            await orderService.PlaceOrderAsync(request, "user1");

            await context.Entry(productSize).ReloadAsync();
            Assert.Equal(7, productSize.Stock);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldCreateOrderWithCorrectTotal_WhenOrderIsPlaced()
        {
            var context = CreateContext();
            var mapper = Mock.Of<IMapper>();
            var paymentService = CreatePaymentService(context);
            var orderService = new OrderService(context, mapper, paymentService);

            var brand = new Brand { Id = 1, Name = "Nike" };
            var audience = new Audience { Id = 1, Code = "men", DisplayName = "Men" };
            var product = new Product
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
            var shippingAddress = new ShippingAddress
            {
                Id = 1,
                UserId = "user1",
                AddressLine1 = "123 Test St",
                City = "Test City",
                County = "Test County",
                Postcode = "T123ST",
                Country = "UK"
            };
            var orderStatus = new OrderStatus { Id = 1, Code = "processing", DisplayName = "Processing" };

            context.Brands.Add(brand);
            context.Audiences.Add(audience);
            context.Products.Add(product);
            context.ShippingAddresses.Add(shippingAddress);
            context.OrderStatuses.Add(orderStatus);
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

            var response = await orderService.PlaceOrderAsync(request, "user1");

            var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == response.OrderId);
            Assert.NotNull(order);
            Assert.Equal(195m, order.Total);
        }
    }
}
