namespace ShoeStore.Dto.Order
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string AddressLine1 { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Postcode { get; set; } = null!;
        public string Country { get; set; } = null!;
    }
}
