using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Services;

namespace ShoeStore.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ShoeStoreContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Remove any existing ShoeStoreContext registration
                var contextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(ShoeStoreContext));
                if (contextDescriptor != null)
                {
                    services.Remove(contextDescriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<ShoeStoreContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                    options.EnableSensitiveDataLogging();
                });
            });

            // Set environment to Testing to skip seeding operations
            builder.UseEnvironment("Testing");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            // Skip seeding operations in test environment
            // The seeding will be handled by the test setup if needed

            return host;
        }
    }
}
