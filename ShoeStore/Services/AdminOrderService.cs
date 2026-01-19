using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Admin;
using ShoeStore.Dto.Order;

namespace ShoeStore.Services
{
    public class AdminOrderService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ShoeStoreContext _context;

        public AdminOrderService(
            UserManager<IdentityUser> userManager,
            ShoeStoreContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<AdminOrderListDto> GetOrdersAsync(GetAdminOrdersRequestDto request)
        {
            var query = _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(ps => ps.ProductSize)
                    .ThenInclude(p => p.Product)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.PaymentStatus)
                .Include(o => o.UserDetail)
                    .ThenInclude(u => u.AspNetUser)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();

                query = query.Where(o =>
                    o.Id.ToString().Contains(term) ||
                    (o.UserDetail != null && (
                        ((o.UserDetail.FirstName + " " + o.UserDetail.LastName).ToLower().Contains(term)) ||
                        (o.UserDetail.AspNetUser != null &&
                         o.UserDetail.AspNetUser.Email.ToLower().Contains(term))
                    ))
                );
            }

            if (!string.IsNullOrEmpty(request.StatusFilter))
            {
                query = query.Where(o => o.OrderStatus!.Code == request.StatusFilter);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= request.FromDate);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= request.ToDate);
            }

            // Apply sorting
            var sortBy = request.SortBy;
            var sortDir = request.SortDirection;

            IQueryable<Order> sortedQuery = query;

            switch (sortBy)
            {
                case AdminOrderSortBy.Total:
                    sortedQuery = sortDir == AdminOrderSortDirection.Ascending
                        ? query.OrderBy(o => o.Total)
                        : query.OrderByDescending(o => o.Total);
                    break;

                case AdminOrderSortBy.DateCreated:
                    sortedQuery = sortDir == AdminOrderSortDirection.Ascending
                        ? query.OrderBy(o => o.CreatedAt)
                        : query.OrderByDescending(o => o.CreatedAt);
                    break;

                default:
                    sortedQuery = sortDir == AdminOrderSortDirection.Ascending
                        ? query.OrderBy(o => o.CreatedAt)
                        : query.OrderByDescending(o => o.CreatedAt);
                    break;
            }

            query = sortedQuery;

            var totalQueryCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalQueryCount / request.PageSize);

            var orders = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var adminOrders = new List<AdminOrderDto>();

            // Populate order information
            foreach (var order in orders)
            {
                var user = await _userManager.FindByIdAsync(order.UserId!);
                var userDetail = await _context.UserDetails
                    .FirstOrDefaultAsync(ud => ud.AspNetUserId == order.UserId);

                var orderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductSize.ProductId,
                    ProductName = oi.ProductName ?? "Unknown Product",
                    Quantity = oi.Quantity,
                    ProductPrice = oi.ProductPrice,
                }).ToList();

                var payment = order.Payments.Select(p => new AdminPaymentDto
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod?.DisplayName ?? "Unknown",
                    PaymentStatus = p.PaymentStatus?.DisplayName ?? "Unknown",
                    Amount = p.Amount,
                    TransactionId = p.TransactionId,
                    CreatedAt = p.CreatedAt
                }).FirstOrDefault();

                adminOrders.Add(new AdminOrderDto
                {
                    Id = order.Id,
                    UserId = order.UserId!,
                    UserEmail = user?.Email ?? "",
                    UserName = userDetail != null ? $"{userDetail.FirstName} {userDetail.LastName}".Trim() : "",
                    OrderStatusName = order.OrderStatus?.DisplayName,
                    OrderStatusCode = order.OrderStatus?.Code,
                    Subtotal = order.Subtotal,
                    ShippingCost = order.ShippingCost,
                    Discount = order.Discount,
                    Total = order.Total,
                    Notes = order.Notes,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt,
                    ShippingAddress = order.ShippingAddress != null ? new AdminShippingAddressDto
                    {
                        Id = order.ShippingAddress.Id,
                        FirstName = "",
                        LastName = "",
                        AddressLine1 = order.ShippingAddress.AddressLine1,
                        AddressLine2 = "",
                        City = order.ShippingAddress.City,
                        State = order.ShippingAddress.County,
                        PostalCode = order.ShippingAddress.Postcode,
                        Country = order.ShippingAddress.Country,
                        PhoneNumber = ""
                    } : null,
                    BillingAddress = order.BillingAddress != null ? new AdminBillingAddressDto
                    {
                        Id = order.BillingAddress.Id,
                        FirstName = "",
                        LastName = "",
                        AddressLine1 = order.BillingAddress.AddressLine1,
                        AddressLine2 = "",
                        City = order.BillingAddress.City,
                        State = order.BillingAddress.County,
                        PostalCode = order.BillingAddress.Postcode,
                        Country = order.BillingAddress.Country,
                        PhoneNumber = ""
                    } : null,
                    OrderItems = orderItems,
                    Payment = payment
                });
            }

            var totalOrdersCount = await _context.Orders.CountAsync();
            var totalPendingOrdersCount = await _context.Orders
                .Where(o => o.OrderStatus!.Id == 1)
                .CountAsync();
            var totalProcessingOrdersCount = await _context.Orders
                .Where(o => o.OrderStatus!.Id == 2)
                .CountAsync();
            var totalDeliveredOrdersCount = await _context.Orders
                .Where(o => o.OrderStatus!.Id == 4)
                .CountAsync();

            var stats = new AdminOrdersStatsDto()
            {
                TotalOrdersCount = totalOrdersCount,
                TotalPendingOrdersCount = totalPendingOrdersCount,
                TotalProcessingOrdersCount = totalProcessingOrdersCount,
                TotalDeliveredOrdersCount = totalDeliveredOrdersCount
            };

            return new AdminOrderListDto
            {
                AdminOrdersStats = stats,
                Orders = adminOrders,
                TotalQueryCount = totalQueryCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<AdminOrderDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderStatus)
                          .Include(o => o.ShippingAddress)
                          .Include(o => o.UserDetail)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                    .ThenInclude(ps => ps.Product)
                        .ThenInclude(p => p.Brand)

                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                    .ThenInclude(ps => ps.Product)
                        //.ThenInclude(p => p.ProductImages)
                        .ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary))

                .Include(o => o.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(o => o.Payments)
                    .ThenInclude(s => s.PaymentStatus)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            var user = await _userManager.FindByIdAsync(order.UserId!);
            var userDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.AspNetUserId == order.UserId);

            return new AdminOrderDto
            {
                Id = order.Id,
                UserId = order.UserId!,
                UserEmail = user?.Email ?? "",
                UserName = userDetail != null ? $"{userDetail.FirstName} {userDetail.LastName}".Trim() : "",
                OrderStatusName = order.OrderStatus.DisplayName,
                OrderStatusCode = order.OrderStatus.Code,
                Subtotal = order.Subtotal,
                ShippingCost = order.ShippingCost,
                Discount = order.Discount,
                Total = order.Total,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                ShippingAddress = order.ShippingAddress != null ? new AdminShippingAddressDto
                {
                    Id = order.ShippingAddress.Id,
                    FirstName = order.UserDetail.FirstName,
                    LastName = order.UserDetail.LastName,
                    AddressLine1 = order.ShippingAddress.AddressLine1,
                    AddressLine2 = "",
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.County,
                    PostalCode = order.ShippingAddress.Postcode,
                    Country = order.ShippingAddress.Country,
                    PhoneNumber = ""
                } : null,
                BillingAddress = order.BillingAddress != null ? new AdminBillingAddressDto
                {
                    Id = order.BillingAddress.Id,
                    FirstName = order.UserDetail.FirstName,
                    LastName = order.UserDetail.LastName,
                    AddressLine1 = order.BillingAddress.AddressLine1,
                    AddressLine2 = "",
                    City = order.BillingAddress.City,
                    State = order.BillingAddress.County,
                    PostalCode = order.BillingAddress.Postcode,
                    Country = order.BillingAddress.Country,
                    PhoneNumber = ""
                } : null,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductSize.ProductId,
                    ProductName = oi.ProductName ?? "Unknown Product",
                    Quantity = oi.Quantity,
                    ProductPrice = oi.ProductPrice,
                    BrandName = oi.ProductSize.Product.Brand.Name,
                    Barcode = oi.ProductSize.Barcode,
                    MainImage = oi.ProductSize.Product.ProductImages
                        .Select(pi => pi.ImagePath)
                        .FirstOrDefault(),
                    //MainImage = oi.ProductSize.Product.ProductImages
                    //    .Where(pi => pi.ProductId == oi.ProductSize.Product.Id && pi.IsPrimary)
                    //    .Select(pi => pi.ImagePath)
                    //    .FirstOrDefault(),
                    Size = oi.ProductSize.UkSize.ToString()
                }).ToList(),
                Payment = order.Payments.Select(p => new AdminPaymentDto
                {
                    Id = p.Id,
                    PaymentStatus = p.PaymentStatus?.DisplayName ?? "Unknown",
                    Amount = p.Amount,
                    TransactionId = p.TransactionId,
                    CreatedAt = p.CreatedAt,
                    Currency = p.Currency,
                    CardBrand = p.CardBrand,
                    CardLast4 = p.CardLast4,
                    BillingName = p.BillingName,
                    BillingEmail = p.BillingEmail,
                    PaymentMethod = p.PaymentMethod.DisplayName,
                    ReceiptUrl = p.ReceiptUrl
                }).FirstOrDefault()
            };
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequestDto request)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.OrderStatusId = request.OrderStatusId;
            order.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(request.Notes))
            {
                order.Notes = $"{order.Notes}. Note added on {DateTime.UtcNow.ToString()} => {request.Notes}";
            }

            var rowsAffected = await _context.SaveChangesAsync();
            return rowsAffected == 1;
        }
    }
}
