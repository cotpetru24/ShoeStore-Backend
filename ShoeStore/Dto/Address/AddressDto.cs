namespace ShoeStore.Dto.Address
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string AddressLine1 { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Postcode { get; set; } = null!;
        public string Country { get; set; } = null!;
    }
}
