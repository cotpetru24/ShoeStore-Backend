using System.ComponentModel.DataAnnotations;

namespace ShoeStore.Dto.Address
{
    public class CreateAddressRequestDto
    {
        [Required(ErrorMessage = "Address line 1 is required")]
        public string AddressLine1 { get; set; } = null!;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "Postcode is required")]
        public string Postcode { get; set; } = null!;

        public string Country { get; set; } = "United Kingdom";
    }
}
