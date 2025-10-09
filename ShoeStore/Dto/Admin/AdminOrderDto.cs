using ShoeStore.Dto.Order;
using System;
using System.Collections.Generic;

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
        public List<AdminPaymentDto> Payments { get; set; } = new List<AdminPaymentDto>();
    }

    public class AdminOrderListDto
    {
        public List<AdminOrderDto> Orders { get; set; } = new List<AdminOrderDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    //public class AdminOrderItemDto
    //{
    //    public int Id { get; set; }
    //    public int ProductId { get; set; }
    //    public string ProductName { get; set; } = null!;
    //    public string? ProductImagePath { get; set; }
    //    public int Quantity { get; set; }
    //    public decimal UnitPrice { get; set; }
    //    public decimal TotalPrice { get; set; }
    //}

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

    public class AdminPaymentDto
    {
        public int Id { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? PaymentStatusName { get; set; }
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? CreatedAt { get; set; }
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
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortDirection { get; set; } = "desc";
    }
}
