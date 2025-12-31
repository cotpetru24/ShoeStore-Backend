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
    public class AdminServiceTests
    {
        private ShoeStoreContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ShoeStoreContext>()
                .UseInMemoryDatabase(databaseName: $"AdminTestDb_{Guid.NewGuid()}")
                .Options;
            return new ShoeStoreContext(options);
        }

        private UserManager<IdentityUser> CreateUserManager(ShoeStoreContext context)
        {
            var userStore = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<IdentityUser>(context);
            var options = Options.Create(new IdentityOptions());
            var passwordHasher = new PasswordHasher<IdentityUser>();
            var userValidators = new List<IUserValidator<IdentityUser>>();
            var passwordValidators = new List<IPasswordValidator<IdentityUser>>();
            var keyNormalizer = new UpperInvariantLookupNormalizer();
            var errors = new IdentityErrorDescriber();
            var logger = Mock.Of<ILogger<UserManager<IdentityUser>>>();
            var serviceProvider = Mock.Of<IServiceProvider>();

            return new UserManager<IdentityUser>(
                userStore,
                options,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                errors,
                serviceProvider,
                logger);
        }

        private RoleManager<IdentityRole> CreateRoleManager(ShoeStoreContext context)
        {
            var roleStore = new Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<IdentityRole>(context);
            var roleValidators = new List<IRoleValidator<IdentityRole>>();
            var keyNormalizer = new UpperInvariantLookupNormalizer();
            var errors = new IdentityErrorDescriber();
            var logger = Mock.Of<ILogger<RoleManager<IdentityRole>>>();

            return new RoleManager<IdentityRole>(
                roleStore,
                roleValidators,
                keyNormalizer,
                errors,
                logger);
        }

        [Fact]
        public async Task CreateProductAsync_ShouldGenerateCorrectSku_WhenProductSizeIsCreated()
        {
            var context = CreateContext();
            var userManager = CreateUserManager(context);
            var roleManager = CreateRoleManager(context);
            var adminService = new AdminService(userManager, roleManager, context);

            var brand = new Brand { Id = 5, Name = "Nike" };
            var audience = new Audience { Id = 1, Code = "men", DisplayName = "Men" };

            context.Brands.Add(brand);
            context.Audiences.Add(audience);
            await context.SaveChangesAsync();

            var productDto = new AdminProductDto
            {
                Name = "Test Product",
                Description = "Description",
                Price = 100,
                BrandId = 5,
                AudienceId = 1,
                ProductSizes = new List<ProductSizeDto>
                {
                    new ProductSizeDto { Size = 42, Stock = 10, Barcode = "1234567890123" }
                }
            };

            var createdProduct = await adminService.CreateProductAsync(productDto);

            Assert.NotNull(createdProduct);
            Assert.NotNull(createdProduct.ProductSizes);
            Assert.Single(createdProduct.ProductSizes);
            Assert.Equal("5-42-1234567890123", createdProduct.ProductSizes.First().Sku);
        }
    }
}
