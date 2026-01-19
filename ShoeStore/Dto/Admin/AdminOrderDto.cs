using ShoeStore.Dto.Order;

namespace ShoeStore.Dto.Admin
{
    public class AdminOrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? OrderStatusName { get; set; }
        public string? OrderStatusCode { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public AdminShippingAddressDto? ShippingAddress { get; set; }
        public AdminBillingAddressDto? BillingAddress { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public AdminPaymentDto Payment { get; set; } = new AdminPaymentDto();
    }


    public class AdminOrderListDto
    {
        public List<AdminOrderDto> Orders { get; set; } = new List<AdminOrderDto>();
        public int TotalQueryCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public AdminOrdersStatsDto AdminOrdersStats { get; set; }
    }


    public class AdminOrdersStatsDto
    {
        public int TotalOrdersCount { get; set; }
        public int TotalPendingOrdersCount { get; set; }
        public int TotalProcessingOrdersCount { get; set; }
        public int TotalDeliveredOrdersCount { get; set; }
    }


    public class AdminShippingAddressDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }


    public class AdminBillingAddressDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }


    public class AdminPaymentBriefDto
    {
        public int Id { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? PaymentStatusName { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }


    public class AdminPaymentDto
    {
        public int Id { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? CardBrand { get; set; }
        public string? CardLast4 { get; set; }
        public string? BillingName { get; set; }
        public string? BillingEmail { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReceiptUrl { get; set; }
    }


    public class UpdateOrderStatusRequestDto
    {
        public int OrderStatusId { get; set; }
        public string? Notes { get; set; }
    }


    public class GetAdminOrdersRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public AdminOrderSortBy SortBy { get; set; } = AdminOrderSortBy.DateCreated;
        public AdminOrderSortDirection SortDirection { get; set; } = AdminOrderSortDirection.Descending;
    }


    public enum AdminOrderSortBy
    {
        DateCreated,
        Total,
    }


    public enum AdminOrderSortDirection
    {
        Ascending,
        Descending,
    }

}
