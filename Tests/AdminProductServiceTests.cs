using Xunit;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Services;
using ShoeStore.Dto.Admin;
using ShoeStore.Dto.Product;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace ShoeStore.Tests
{
    public class AdminProductServiceTests
    {
        private ShoeStoreContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ShoeStoreContext>()
                .UseInMemoryDatabase(databaseName: $"AdminTestDb_{Guid.NewGuid()}")
                .Options;
            return new ShoeStoreContext(options);
        }

        [Fact]
        public async Task CreateProductAsync_ShouldGenerateCorrectSku_WhenProductSizeIsCreated()
        {
            var context = CreateContext();
            var adminProductService = new AdminProductService(context);

            var brand = new Brand { Id = 5, Name = "Nike" };
            var audience = new Audience { Id = 1, Code = "men", DisplayName = "Men" };

            context.Brands.Add(brand);
            context.Audiences.Add(audience);
            await context.SaveChangesAsync();

            var productDto = new AdminCreateProductRequestDto
            {
                Name = "Test Product",
                Description = "Description",
                Price = 100,
                BrandId = 5,
                AudienceId = 1,
                ProductSizes = new List<AdminCreateProductSizeRequestDto>
                {
                    new AdminCreateProductSizeRequestDto
                    {
                        Size = 42,
                        Stock = 10,
                        Barcode = "1234567890123"
                    }}
            };

            var createdProduct = await adminProductService.CreateProductAsync(productDto);

            Assert.NotNull(createdProduct);
            Assert.NotNull(createdProduct.ProductSizes);
            Assert.Single(createdProduct.ProductSizes);
            Assert.Equal("5-42-1234567890123", createdProduct.ProductSizes.First().Sku);
        }
    }
}
