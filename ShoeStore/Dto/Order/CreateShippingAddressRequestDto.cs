using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Dto.Order
{
    public class CreateShippingAddressRequestDto
    {
        [Required(ErrorMessage = "Address line 1 is required")]
        public string AddressLine1 { get; set; } = null!;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "County is required")]
        public string County { get; set; } = null!;

        [Required(ErrorMessage = "Postcode is required")]
        public string Postcode { get; set; } = null!;

        public string Country { get; set; } = "United Kingdom";
    }
}
