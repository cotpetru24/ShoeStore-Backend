namespace ShoeStore.Configuration
{
    public class StripeSettings
    {
        public string SecretKey { get; set; } = default!;

        public string PublishableKey { get; set; } = default!;
    }
}
