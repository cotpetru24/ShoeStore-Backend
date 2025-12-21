using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Dto.Order
{
    public class BillingAddressDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string AddressLine1 { get; set; } = null!;
        public string City { get; set; } = null!;
        public string County { get; set; } = null!;
        public string Postcode { get; set; } = null!;
        public string Country { get; set; } = null!;
    }
}













