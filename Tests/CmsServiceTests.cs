using Xunit;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Services;

namespace ShoeStore.Tests
{
    public class CmsServiceTests
    {
        private ShoeStoreContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ShoeStoreContext>()
                .UseInMemoryDatabase(databaseName: $"CmsTestDb_{Guid.NewGuid()}")
                .Options;
            return new ShoeStoreContext(options);
        }

        [Fact]
        public async Task GetCmsNavAndFooterAsync_ShouldReturnActiveProfileData_WhenActiveProfileExists()
        {
            var context = CreateContext();
            var cmsService = new CmsService(context);

            var activeProfile = new CmsProfile
            {
                Id = 1,
                Name = "Active Profile",
                SiteName = "Test Store",
                IsActive = true,
                NavbarBg = "#000000",
                NavbarText = "#ffffff",
                NavbarLink = "#cccccc",
                FooterBg = "#111111",
                FooterText = "#eeeeee",
                FooterLink = "#dddddd"
            };

            var inactiveProfile = new CmsProfile
            {
                Id = 2,
                Name = "Inactive Profile",
                SiteName = "Inactive Store",
                IsActive = false,
                NavbarBg = "#ffffff",
                NavbarText = "#000000",
                NavbarLink = "#000000",
                FooterBg = "#ffffff",
                FooterText = "#000000",
                FooterLink = "#000000"
            };

            context.CmsProfiles.Add(activeProfile);
            context.CmsProfiles.Add(inactiveProfile);
            await context.SaveChangesAsync();

            var result = await cmsService.GetCmsNavAndFooterAsync();

            Assert.NotNull(result);
            Assert.Equal("Test Store", result.WebsiteName);
            Assert.Equal("#000000", result.NavbarBgColor);
            Assert.Equal("#ffffff", result.NavbarTextColor);
        }
    }
}
