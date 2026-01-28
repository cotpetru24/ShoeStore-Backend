namespace ShoeStore.Dto.Order
{
    // NOTE:
    // - Processing can ONLY be set by payment webhook
    // - Cancelled and Returned are terminal
    // - PaymentFailed means no money was captured
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
