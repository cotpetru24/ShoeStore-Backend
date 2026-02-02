using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto;
using ShoeStore.Dto.Address;
using ShoeStore.Dto.Admin;
using ShoeStore.Dto.Order;
using ShoeStore.Dto.Payment;
using Stripe;
using Stripe.Climate;

namespace ShoeStore.Services
{
    public class OrderService
    {
        private readonly ShoeStoreContext _context;
        private readonly PaymentService _paymentService;

        public OrderService(ShoeStoreContext injectedContext, PaymentService paymentService)
        {
            _context = injectedContext;
            _paymentService = paymentService;
        }


        public async Task<PlaceOrderResponseDto> PlaceOrderAsync(PlaceOrderRequestDto request, string userId)
        {
            if (await _context.Payments.AnyAsync(p => p.PaymentIntentId == request.PaymentIntentId))
                throw new InvalidOperationException("PaymentIntent already used");


            var paymentIntent = await _paymentService.GetPaymentIntentFromStripe(request.PaymentIntentId);

            if (paymentIntent.Status != "succeeded")
                throw new InvalidOperationException($"Payment not completed. Status: {paymentIntent.Status}");

            bool orderCommitted = false;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var payment = await _paymentService.StorePaymentDetails(request.PaymentIntentId);
                var order = await CreateOrderAsync(request, userId, request.PaymentIntentId);

                await transaction.CommitAsync();
                orderCommitted = true;

                return order;
            }
            catch
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch
                {
                }

                if (!orderCommitted)
                {
                    await _paymentService.RefundPayment(request.PaymentIntentId);
                }

                throw;
            }
        }


        public async Task<PlaceOrderResponseDto> CreateOrderAsync(PlaceOrderRequestDto request, string userId, string paymentIntentId)
        {
            decimal subtotal = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.OrderItems)
            {
                var product = await _context.Products
                    .Include(p => p.ProductSizes)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product == null)
                    throw new ArgumentException($"Product with ID {item.ProductId} not found");

                var productSize = product.ProductSizes
                    .FirstOrDefault(s => s.Barcode == item.ProductSizeBarcode);

                if (productSize == null)
                    throw new ArgumentException(
                        $"Size {item.ProductSizeBarcode} not found for product {product.Name}");

                if (productSize.Stock < item.Quantity)
                    throw new ArgumentException(
                        $"Insufficient stock for product {product.Name}, size {item.ProductSizeBarcode}. " +
                        $"Available: {productSize.Stock}, Requested: {item.Quantity}");

                // Reduce SIZE STOCK
                productSize.Stock -= item.Quantity;

                var orderItem = new OrderItem
                {
                    UnitPrice = product.Price,
                    Quantity = item.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    ProductSizeId = product.ProductSizes.First(ps => ps.Barcode == item.ProductSizeBarcode).Id
                };

                orderItems.Add(orderItem);
                subtotal += product.Price * item.Quantity;
            }

            var shippingAddress = await _context.UserAddresses
                .FirstOrDefaultAsync(a =>
                    a.Id == request.ShippingAddressId &&
                    a.UserId == userId);

            if (shippingAddress == null)
                throw new ArgumentException("Invalid shipping address");

            UserAddress? billingAddress = null;

            if (!request.BillingAddressSameAsShipping)
            {
                billingAddress = new UserAddress
                {
                    AddressLine1 = request.BillingAddressRequest.AddressLine1,
                    City = request.BillingAddressRequest.City,
                    Country = request.BillingAddressRequest.Country,
                    Postcode = request.BillingAddressRequest.Postcode,
                    UserId = userId
                };

                _context.UserAddresses.Add(billingAddress);
                await _context.SaveChangesAsync();
            }


            var total = subtotal + request.ShippingCost - request.Discount;
            var paymentId = await _context.Payments
                .Where(p => p.PaymentIntentId == paymentIntentId)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();


            var order = new DataContext.PostgreSQL.Models.Order
            {
                UserId = userId,
                OrderStatus = (int)OrderStatusEnum.Processing,
                Subtotal = subtotal,
                ShippingCost = request.ShippingCost,
                Discount = request.Discount,
                Total = total,
                ShippingAddressId = shippingAddress.Id,
                BillingAddressId = request.BillingAddressSameAsShipping == true ? shippingAddress.Id : billingAddress!.Id,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PaymentId = paymentId
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in orderItems)
                item.OrderId = order.Id;

            _context.OrderItems.AddRange(orderItems);
            await _context.SaveChangesAsync();


            return new PlaceOrderResponseDto
            {
                OrderId = order.Id,
                Message = "Order placed successfully",
                Total = order.Total,
                CreatedAt = order.CreatedAt
            };
        }


        public async Task<OrderDto?> GetOrderByIdAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .Where(o => o.Id == orderId && o.UserId == userId)
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
                        .ThenInclude(p => p.ProductImages.Where(pi => pi.IsPrimary))
                .Include(o => o.Payment)
                    .ThenInclude(p => p.PaymentMethod)
                .FirstOrDefaultAsync();

            if (order == null)
                return null;

            var response = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId!,
                OrderStatusId = (OrderStatusEnum)order.OrderStatus,
                OrderStatusName = ((OrderStatusEnum)order.OrderStatus).ToString(),
                Subtotal = order.Subtotal,
                ShippingCost = order.ShippingCost,
                Discount = order.Discount,
                Total = order.Total,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                ShippingAddress = new AddressDto
                {
                    Id = order.ShippingAddress.Id,
                    UserId = order.ShippingAddress.UserId,
                    AddressLine1 = order.ShippingAddress.AddressLine1,
                    City = order.ShippingAddress.City,
                    Postcode = order.ShippingAddress.Postcode,
                    Country = order.ShippingAddress.Country,
                },

                BillingAddress = new AddressDto
                {
                    Id = order.BillingAddress.Id,
                    UserId = order.BillingAddress.UserId,
                    AddressLine1 = order.BillingAddress.AddressLine1,
                    City = order.BillingAddress.City,
                    Postcode = order.BillingAddress.Postcode,
                    Country = order.BillingAddress.Country,
                },

                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    OrderId = order.Id,
                    CreatedAt = oi.CreatedAt,
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

                Payment = new PaymentDto
                {
                    Amount = order.Payment.Amount,
                    Currency = order.Payment.Currency,
                    CardBrand = order.Payment.CardBrand,
                    CardLast4 = order.Payment.CardLast4,
                    PaymentMethod = order.Payment.PaymentMethod.DisplayName,
                    ReceiptUrl = order.Payment.ReceiptUrl,
                    OrderId = order.Id,
                    Status = (PaymentStatusEnum)order.Payment.PaymentStatus
                }
            };

            return response;
        }


        public async Task<GetOrdersResponseDto> GetOrdersAsync(GetOrdersRequestDto request, string userId)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId)
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
                .AsQueryable();

            if (request.OrderStatus != null)
            {
                query = query.Where(o => o.OrderStatus != null && o.OrderStatus == (int)request.OrderStatus);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= request.ToDate.Value);
            }

            var totalCount = await query.CountAsync();

            request.Page = request.Page < 1 ? 1 : request.Page;
            request.PageSize = request.PageSize < 1 ? 30 : request.PageSize;

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var order = orders.Select(order => new OrderDto()
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderStatusId = (OrderStatusEnum)order.OrderStatus,
                OrderStatusName = ((OrderStatusEnum)order.OrderStatus).ToString(),
                Subtotal = order.Subtotal,
                ShippingCost = order.ShippingCost,
                Discount = order.Discount,
                Total = order.Total,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                ShippingAddress = order.ShippingAddress == null
                        ? null!
                        : new AddressDto
                        {
                            Id = order.ShippingAddress.Id,
                            UserId = order.ShippingAddress.UserId,
                            AddressLine1 = order.ShippingAddress.AddressLine1,
                            City = order.ShippingAddress.City,
                            Postcode = order.ShippingAddress.Postcode,
                            Country = order.ShippingAddress.Country
                        },

                BillingAddress = order.BillingAddress == null
                        ? null!
                        : new AddressDto
                        {
                            Id = order.BillingAddress.Id,
                            UserId = order.BillingAddress.UserId,
                            AddressLine1 = order.BillingAddress.AddressLine1,
                            City = order.BillingAddress.City,
                            Postcode = order.BillingAddress.Postcode,
                            Country = order.BillingAddress.Country
                        },

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

                Payment = new PaymentDto
                {
                    OrderId = order.Id,
                    CardBrand = order.Payment.CardBrand,
                    CardLast4 = order.Payment.CardLast4,
                    Currency = order.Payment.Currency,
                    PaymentMethod = order.Payment.PaymentMethod.DisplayName,
                    ReceiptUrl = order.Payment.ReceiptUrl,
                    Status = (PaymentStatusEnum)order.Payment.PaymentStatus
                }
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new GetOrdersResponseDto
            {
                Orders = order,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }


        public async Task<OrderDto?> CancelOrder(int orderId, string userId)
        {
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingOrder = await _context.Orders
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync();

                if (existingOrder == null
                    || existingOrder.OrderStatus != (int)OrderStatusEnum.Processing)
                {
                    throw new InvalidOperationException("Invalid status transition.");
                }

                existingOrder.OrderStatus = (int)OrderStatusEnum.Cancelled;

                bool refundResponse = await _paymentService.RefundPayment(existingOrder.Payment.PaymentIntentId);

                if (refundResponse != true)
                {
                    await transaction.RollbackAsync();
                    return null;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetOrderByIdAsync(orderId, userId);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
