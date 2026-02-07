using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ShoeStore.Dto
{
    public enum AdminUsersSortByEnum
    {
        DateCreated = 1,
        Name = 2,
    }

    public enum AdminProductsSortByEnum
    {
        DateCreated = 1,
        Name = 2,
        Stock = 3
    }


    public enum ProductSortByEnum
    {
        Name = 1,
        Price = 2,
        Brand = 3,
    }


    public enum OrdersSortByEnum
    {
        Date = 1,
        Total = 2
    }


    public enum SortDirectionEnum
    {
        Ascending = 1,
        Descending = 2,
    }


    public enum UserRoleEnum
    {
        Administrator = 1,
        Customer = 2,
    }


    public enum ProductStatusEmun
    {
        Active = 1,
        Inactive = 2,
    }


    public enum AdminProductStockStatusEnum
    {
        [Display(Name = "Low Stock")]
        LowStock = 1,

        [Display(Name = "High Stock")]
        HighStock = 2,

        [Display(Name = "In Stock")]
        InStock = 3,

        [Display(Name = "Out of Stock")]
        OutOfStock = 4
    }


    public enum PaymentStatusEnum
    {
        Pending = 1,
        Authorised = 3,
        Failed = 4,
        Refunded = 6,
        Paid = 12
    }


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
