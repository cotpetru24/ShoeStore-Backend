using System.Net;
using Xunit;

namespace ShoeStore.Tests.IntegrationTests
{
    public class ProductControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ProductControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }


        [Fact]
        public async Task GetProducts_ReturnsSuccessStatusCode()
        {
            var response = await _client.GetAsync("/api/product?PageNumber=1&PageSize=10");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        [Fact]
        public async Task GetProducts_WithInvalidPaging_ReturnsSuccessWithDefaults()
        {
            var response = await _client.GetAsync("/api/product?PageNumber=0&PageSize=10");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


        [Fact]
        public async Task GetProductById_WithInvalidId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/product/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Fact]
        public async Task GetProducts_ValidatesPageSize()
        {
            var response = await _client.GetAsync("/api/product?PageNumber=1&PageSize=200");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
