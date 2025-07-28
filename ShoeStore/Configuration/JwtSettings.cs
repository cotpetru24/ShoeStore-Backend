namespace ShoeStore.Configuration
{
    public class JwtSettings
    {
        //---------------- to which folder should i add this file ---------------
        public string Secret { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int ExpiryMinutes { get; set; }
    }

}
