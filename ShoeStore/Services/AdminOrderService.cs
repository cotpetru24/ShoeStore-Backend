using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto;
using ShoeStore.Dto.Address;
using ShoeStore.Dto.Admin;
using ShoeStore.Dto.Order;
using Order = ShoeStore.DataContext.PostgreSQL.Models.Order;

namespace ShoeStore.Services
{
    public class AdminOrderService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ShoeStoreContext _context;
        private readonly IPaymentService _paymentService;

        public AdminOrderService(UserManager<IdentityUser> userManager, ShoeStoreContext context, IPaymentService paymentService)
        {
            _userManager = userManager;
            _context = context;
            _paymentService = paymentService;
        }


        public async Task<AdminOrdersListDto> GetOrdersAsync(GetAdminOrdersRequestDto request)
        {
            var query = _context.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(ps => ps.ProductSize)
                    .ThenInclude(p => p.Product)
                .Include(o => o.Payment)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(o => o.UserDetail)
                    .ThenInclude(u => u.AspNetUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var term = $"%{request.SearchTerm}%";

                query = query.Where(o =>
                    EF.Functions.ILike(o.Id.ToString(), term) ||
                    (o.UserDetail != null && (
                        EF.Functions.ILike(o.UserDetail.FirstName + " " + o.UserDetail.LastName, term) ||
                        (o.UserDetail.AspNetUser != null &&
                         EF.Functions.ILike(o.UserDetail.AspNetUser.Email, term))
                    ))
                );
            }

            if (request.StatusFilter != null)
            {
                query = query.Where(o => (OrderStatusEnum)o.OrderStatus == request.StatusFilter);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= request.FromDate);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= request.ToDate);
            }

            var sortBy = request.SortBy;
            var sortDir = request.SortDirection;

            IQueryable<Order> sortedQuery = query;

            switch (sortBy)
            {
                case OrdersSortByEnum.Total:
                    sortedQuery = sortDir == SortDirectionEnum.Ascending
                        ? query.OrderBy(o => o.Total)
                        : query.OrderByDescending(o => o.Total);
                    break;

                case OrdersSortByEnum.Date:
                    sortedQuery = sortDir == SortDirectionEnum.Ascending
                        ? query.OrderBy(o => o.CreatedAt)
                        : query.OrderByDescending(o => o.CreatedAt);
                    break;

                default:
                    sortedQuery = sortDir == SortDirectionEnum.Ascending
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

            foreach (var order in orders)
            {
                var user = await _userManager.FindByIdAsync(order.UserId!);
                var userDetail = await _context.UserDetails
                    .FirstOrDefaultAsync(ud => ud.AspNetUserId == order.UserId);

                var orderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductSize.ProductId,
                    ProductName = oi.ProductSize.Product.Name,
                    Quantity = oi.Quantity,
                    ProductPrice = oi.UnitPrice,
                }).ToList();

                var payment = new AdminPaymentDto
                {
                    Id = order.Payment.Id,
                    PaymentMethod = order.Payment.PaymentMethod.DisplayName,
                    PaymentStatus = (PaymentStatusEnum)order.Payment.PaymentStatus,
                    Amount = order.Payment.Amount,
                    TransactionId = order.Payment.TransactionId,
                    CreatedAt = order.Payment.CreatedAt
                };

                adminOrders.Add(new AdminOrderDto
                {
                    Id = order.Id,
                    UserId = order.UserId!,
                    UserEmail = user?.Email ?? "",
                    UserName = userDetail != null ? $"{userDetail.FirstName} {userDetail.LastName}".Trim() : "",
                    OrderStatus = (OrderStatusEnum)order.OrderStatus,
                    Subtotal = order.Subtotal,
                    ShippingCost = order.ShippingCost,
                    Discount = order.Discount,
                    Total = order.Total,
                    Notes = order.Notes,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt,
                    ShippingAddress = order.ShippingAddress != null ? new AddressDto
                    {
                        Id = order.ShippingAddress.Id,
                        AddressLine1 = order.ShippingAddress.AddressLine1,
                        City = order.ShippingAddress.City,
                        Postcode = order.ShippingAddress.Postcode,
                        Country = order.ShippingAddress.Country,
                    } : null,
                    BillingAddress = order.BillingAddress != null ? new AddressDto
                    {
                        Id = order.BillingAddress.Id,
                        AddressLine1 = order.BillingAddress.AddressLine1,
                        City = order.BillingAddress.City,
                        Postcode = order.BillingAddress.Postcode,
                        Country = order.BillingAddress.Country,
                    } : null,
                    OrderItems = orderItems,
                    Payment = payment
                });
            }

            var totalOrdersCount = await _context.Orders.CountAsync();
            var totalShippedOrdersCount = await _context.Orders
                .Where(o => o.OrderStatus == (int)OrderStatusEnum.Shipped)
                .CountAsync();
            var totalProcessingOrdersCount = await _context.Orders
                .Where(o => o.OrderStatus == (int)OrderStatusEnum.Processing)
                .CountAsync();
            var totalDeliveredOrdersCount = await _context.Orders
                .Where(o => o.OrderStatus == (int)OrderStatusEnum.Delivered)
                .CountAsync();

            var stats = new AdminOrdersStatsDto()
            {
                TotalOrdersCount = totalOrdersCount,
                TotalShippedOrdersCount = totalShippedOrdersCount,
                TotalProcessingOrdersCount = totalProcessingOrdersCount,
                TotalDeliveredOrdersCount = totalDeliveredOrdersCount
            };

            return new AdminOrdersListDto
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
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.UserDetail)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                    .ThenInclude(ps => ps.Product)
                        .ThenInclude(p => p.Brand)

                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize)
                    .ThenInclude(ps => ps.Product)
                        .ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary))

                .Include(o => o.Payment)
                    .ThenInclude(p => p.PaymentMethod)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            var user = await _userManager.FindByIdAsync(order.UserId!);
            var userDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.AspNetUserId == order.UserId);

            var response = new AdminOrderDto
            {
                Id = order.Id,
                UserId = order.UserId!,
                UserEmail = user?.Email ?? "",
                UserName = userDetail != null ? $"{userDetail.FirstName} {userDetail.LastName}".Trim() : "",
                OrderStatus = (OrderStatusEnum)order.OrderStatus,
                Subtotal = order.Subtotal,
                ShippingCost = order.ShippingCost,
                Discount = order.Discount,
                Total = order.Total,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                ShippingAddress = order.ShippingAddress != null ? new AddressDto
                {
                    Id = order.ShippingAddress.Id,
                    AddressLine1 = order.ShippingAddress.AddressLine1,
                    City = order.ShippingAddress.City,
                    Postcode = order.ShippingAddress.Postcode,
                    Country = order.ShippingAddress.Country,
                } : null,

                BillingAddress = order.BillingAddress != null ? new AddressDto
                {
                    Id = order.BillingAddress.Id,
                    AddressLine1 = order.BillingAddress.AddressLine1,
                    City = order.BillingAddress.City,
                    Postcode = order.BillingAddress.Postcode,
                    Country = order.BillingAddress.Country,
                } : null,

                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductSize.ProductId,
                    ProductName = oi.ProductSize.Product.Name,
                    Quantity = oi.Quantity,
                    ProductPrice = oi.UnitPrice,
                    BrandName = oi.ProductSize.Product.Brand.Name,
                    Barcode = oi.ProductSize.Barcode,
                    MainImage = oi.ProductSize.Product.ProductImages
                        .Select(pi => pi.ImagePath)
                        .FirstOrDefault(),
                    Size = oi.ProductSize.UkSize.ToString()
                }).ToList(),

                Payment = new AdminPaymentDto
                {
                    Id = order.Payment.Id,
                    PaymentStatus = (PaymentStatusEnum)order.Payment.PaymentStatus,
                    Amount = order.Payment.Amount,
                    TransactionId = order.Payment.TransactionId,
                    CreatedAt = order.Payment.CreatedAt,
                    Currency = order.Payment.Currency,
                    CardBrand = order.Payment.CardBrand,
                    CardLast4 = order.Payment.CardLast4,
                    PaymentMethod = order.Payment.PaymentMethod.DisplayName,
                    ReceiptUrl = order.Payment.ReceiptUrl
                }
            };
            return response;
        }


        public async Task<bool> UpdateOrderStatusAsync(int orderId, AdminUpdateOrderStatusRequestDto request)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Payment)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null) return false;


                if (order.Payment.PaymentStatus == (int)PaymentStatusEnum.Refunded)
                {
                    throw new InvalidOperationException(
                        "Order status cannot be changed after a refund."
                    );
                }


                if (order.OrderStatus == (int)OrderStatusEnum.Cancelled ||
                    order.OrderStatus == (int)OrderStatusEnum.Returned)
                {
                    throw new InvalidOperationException("Cannot update status of a cancelled or returned order.");
                }

                //Conditions for request status == processing
                if (request.Status == OrderStatusEnum.Processing)
                {
                    throw new InvalidOperationException(
                        "Order status 'Processing' can only be set by the payment system."
                    );
                }

                // Conditions for current status == processing
                if (order.OrderStatus == (int)OrderStatusEnum.Processing &&
                    request.Status != OrderStatusEnum.Shipped &&
                    request.Status != OrderStatusEnum.Cancelled)
                {
                    throw new InvalidOperationException("Invalid status transition from Processing.");
                }

                // Conditions for current status == shipped
                if (order.OrderStatus == (int)OrderStatusEnum.Shipped &&
                    request.Status != OrderStatusEnum.Delivered)
                {
                    throw new InvalidOperationException("Invalid status transition from Shipped.");
                }

                // Conditions for current status == delivered
                if (request.Status == OrderStatusEnum.Returned &&
                    order.OrderStatus != (int)OrderStatusEnum.Delivered)
                {
                    throw new InvalidOperationException("Order can be returned only if it has been delivered.");
                }

                // If cancelling or returned => refund
                if ((request.Status == OrderStatusEnum.Cancelled ||
                    request.Status == OrderStatusEnum.Returned) &&
                    order.Payment.PaymentStatus != (int)PaymentStatusEnum.Refunded)
                {
                    var refundResult = await _paymentService.RefundPayment(order.Payment.PaymentIntentId);

                    if (refundResult != true)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                order.OrderStatus = (int)request.Status;
                order.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(request.Notes))
                {
                    order.Notes = $"{order.Notes}. Note added on {DateTime.UtcNow.ToString()} => {request.Notes}";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
