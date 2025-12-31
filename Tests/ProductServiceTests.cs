using Xunit;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Services;

namespace ShoeStore.Tests
{
    public class ProductServiceTests
    {
        private ShoeStoreContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ShoeStoreContext>()
                .UseInMemoryDatabase(databaseName: $"ProductTestDb_{Guid.NewGuid()}")
                .Options;
            return new ShoeStoreContext(options);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldCalculateCorrectTotalStock_WhenMultipleSizesExist()
        {
            var context = CreateContext();
            var productService = new ProductService(context);

            var brand = new Brand { Id = 1, Name = "Nike" };
            var audience = new Audience { Id = 1, Code = "men", DisplayName = "Men" };
            var product = new Product
            {
                Id = 1,
                Name = "Air Max",
                Description = "Comfortable shoes",
                Price = 100,
                BrandId = 1,
                AudienceId = 1,
                ProductSizes = new List<ProductSize>
                {
                    new ProductSize { Id = 1, UkSize = 9, Stock = 10, Barcode = "BAR1", Sku = "1-9-BAR1" },
                    new ProductSize { Id = 2, UkSize = 10, Stock = 5, Barcode = "BAR2", Sku = "1-10-BAR2" },
                    new ProductSize { Id = 3, UkSize = 11, Stock = 15, Barcode = "BAR3", Sku = "1-11-BAR3" }
                }
            };

            context.Brands.Add(brand);
            context.Audiences.Add(audience);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var result = await productService.GetProductByIdAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result.Product);
            Assert.Equal(30, result.Product.TotalStock);
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnZeroStock_WhenNoSizesExist()
        {
            var context = CreateContext();
            var productService = new ProductService(context);

            var brand = new Brand { Id = 1, Name = "Nike" };
            var audience = new Audience { Id = 1, Code = "men", DisplayName = "Men" };
            var product = new Product
            {
                Id = 1,
                Name = "Out of Stock Shoe",
                Description = "No stock",
                Price = 50,
                BrandId = 1,
                AudienceId = 1,
                ProductSizes = new List<ProductSize>()
            };

            context.Brands.Add(brand);
            context.Audiences.Add(audience);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var result = await productService.GetProductByIdAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result.Product);
            Assert.Equal(0, result.Product.TotalStock);
        }
    }
}
