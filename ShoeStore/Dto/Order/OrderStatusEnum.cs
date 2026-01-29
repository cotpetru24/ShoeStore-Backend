namespace ShoeStore.Dto.Order
{
    // NOTE:
    // - Currently, Processing is set after verifying the PaymentIntent status with Stripe
    //   during the backend order placement flow.
    // - For production, this SHOULD !!!! be set only via a Stripe webhook
    //   (payment_intent.succeeded) to handle async completion and recovery cases.
    // - Cancelled and Returned are terminal states of an order.
    // - PaymentFailed means no money was captured.
    public enum OrderStatusEnum
    {
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        PaymentFailed = 6,
        Returned = 7,
    }
}
