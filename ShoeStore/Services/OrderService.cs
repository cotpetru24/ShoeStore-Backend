using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Order;

namespace ShoeStore.Services
{
    public class OrderService
    {
        private readonly ShoeStoreContext _context;
        private readonly IMapper _mapper;
        private readonly PaymentService _paymentService;

        public OrderService(ShoeStoreContext injectedContext, IMapper injectedMapper, PaymentService paymentService)
        {
            _context = injectedContext;
            _mapper = injectedMapper;
            _paymentService = paymentService;

        }

        public async Task<PlaceOrderResponseDto> PlaceOrderAsync(PlaceOrderRequestDto request, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate products and calculate totals
                decimal subtotal = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in request.OrderItems)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product == null)
                        throw new ArgumentException($"Product with ID {item.ProductId} not found");
                    if (product.Stock < item.Quantity)
                        throw new ArgumentException($"Insufficient stock for product {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}");

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        ProductPrice = product.Price,
                        Quantity = item.Quantity,
                        Size = item.Size,
                        CreatedAt = DateTime.Now
                    };

                    orderItems.Add(orderItem);
                    subtotal += product.Price * item.Quantity;

                    // Update product stock
                    product.Stock -= item.Quantity;
                }

                // Validate addresses
                var shippingAddress = await _context.ShippingAddresses
                    .FirstOrDefaultAsync(a => a.Id == request.ShippingAddressId && a.UserId == userId);

                if (shippingAddress == null)
                    throw new ArgumentException("Invalid shipping address");

                #region Billing Address

                BillingAddress billingAddressToStore = new BillingAddress();

                if (request.BillingAddressSameAsShipping)
                {
                    billingAddressToStore = new BillingAddress
                    {
                        AddressLine1 = shippingAddress.AddressLine1,
                        City = shippingAddress.City,
                        County = shippingAddress.County,
                        Country = shippingAddress.Country,
                        Postcode = shippingAddress.Postcode,
                        UserId = userId
                    };
                }
                else
                {
                    billingAddressToStore = new BillingAddress
                    {
                        AddressLine1 = request.BillingAddressRequest.AddressLine1,
                        City = request.BillingAddressRequest.City,
                        County = request.BillingAddressRequest.County,
                        Country = request.BillingAddressRequest.Country,
                        Postcode = request.BillingAddressRequest.Postcode,
                        UserId = userId
                    };

                }

                _context.BillingAddresses.Add(billingAddressToStore);
                await _context.SaveChangesAsync();

                var billingAddressId = billingAddressToStore.Id;

                if (billingAddressId <= 0)
                    throw new ArgumentException("Invalid billing address");

                #endregion

                // Get default order status (assuming "pending" is the default)
                var defaultOrderStatus = await _context.OrderStatuses
                    .FirstOrDefaultAsync(s => s.Code == "processing");

                if (defaultOrderStatus == null)
                    throw new InvalidOperationException("Default order status not found");

                // Calculate total
                var total = subtotal + request.ShippingCost - request.Discount;

                // Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderStatusId = defaultOrderStatus.Id,
                    Subtotal = subtotal,
                    ShippingCost = request.ShippingCost,
                    Discount = request.Discount,
                    Total = total,
                    ShippingAddressId = request.ShippingAddressId,
                    BillingAddressId = billingAddressId,
                    Notes = request.Notes,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order items with order ID
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                }

                _context.OrderItems.AddRange(orderItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new PlaceOrderResponseDto
                {
                    OrderId = order.Id,
                    Message = "Order placed successfully",
                    Total = order.Total,
                    CreatedAt = order.CreatedAt ?? DateTime.UtcNow
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Brand)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.Payments)
                    .ThenInclude(p=>p.PaymentStatus)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .FirstOrDefaultAsync();



            if (order == null)
                return null;

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<GetOrdersResponseDto> GetOrdersAsync(GetOrdersRequestDto request, string userId)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.OrderStatus))
            {
                query = query.Where(o => o.OrderStatus != null && o.OrderStatus.Code == request.OrderStatus);
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= request.ToDate.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            request.Page = request.Page < 1 ? 1 : request.Page;
            request.PageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new GetOrdersResponseDto
            {
                Orders = orderDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }

        // Shipping Address Methods
        public async Task<ShippingAddressResponseDto> CreateShippingAddressAsync(CreateShippingAddressRequestDto request, string userId)
        {
            var newAddress = new ShippingAddress
            {
                UserId = userId,
                AddressLine1 = request.AddressLine1,
                City = request.City,
                County = request.County,
                Postcode = request.Postcode,
                Country = request.Country
            };

            _context.ShippingAddresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return new ShippingAddressResponseDto
            {
                Id = newAddress.Id,
                Message = "Shipping address created successfully",
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<List<ShippingAddressDto>> GetShippingAddressesAsync(string userId)
        {
            var addresses = await _context.ShippingAddresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return _mapper.Map<List<ShippingAddressDto>>(addresses);
        }

        public async Task<ShippingAddressDto?> GetShippingAddressByIdAsync(int addressId, string userId)
        {
            var address = await _context.ShippingAddresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                return null;

            return _mapper.Map<ShippingAddressDto>(address);
        }

        public async Task<ShippingAddressResponseDto> UpdateShippingAddressAsync(int addressId, UpdateShippingAddressRequestDto request, string userId)
        {
            var address = await _context.ShippingAddresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                throw new ArgumentException("Shipping address not found");

            address.AddressLine1 = request.AddressLine1;
            address.City = request.City;
            address.County = request.County;
            address.Postcode = request.Postcode;
            address.Country = request.Country;

            await _context.SaveChangesAsync();

            return new ShippingAddressResponseDto
            {
                Id = address.Id,
                Message = "Shipping address updated successfully",
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> DeleteShippingAddressAsync(int addressId, string userId)
        {
            var address = await _context.ShippingAddresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                return false;

            // Check if address is being used in any orders
            var isUsedInOrders = await _context.Orders
                .AnyAsync(o => o.ShippingAddressId == addressId || o.BillingAddressId == addressId);

            if (isUsedInOrders)
                throw new InvalidOperationException("Cannot delete address that is being used in orders");

            _context.ShippingAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return true;
        }

        // Billing Address Methods
        public async Task<BillingAddressResponseDto> CreateBillingAddressAsync(CreateShippingAddressRequestDto request, string userId)
        {
            var newAddress = new BillingAddress
            {
                UserId = userId,
                AddressLine1 = request.AddressLine1,
                City = request.City,
                County = request.County,
                Postcode = request.Postcode,
                Country = request.Country
            };

            _context.BillingAddresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return new BillingAddressResponseDto
            {
                Id = newAddress.Id,
                Message = "Billing address created successfully",
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<List<BillingAddressDto>> GetBillingAddressesAsync(int id)
        {
            var addresses = await _context.BillingAddresses
                .Where(a => a.Id == id)
                .ToListAsync();

            return _mapper.Map<List<BillingAddressDto>>(addresses);
        }

        public async Task<BillingAddressDto?> GetBillingAddressByIdAsync(int addressId, string userId)
        {
            var address = await _context.BillingAddresses
                .Where(a => a.Id == addressId)
                //.Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                return null;

            return _mapper.Map<BillingAddressDto>(address);
        }


        public async Task<OrderDto?> CancelOrder(int orderId, string userId)
        {
            var existingOrder = await _context.Orders
                .Where(o => o.Id == orderId && o.UserId == userId)
                .FirstOrDefaultAsync();

            if (existingOrder == null
                || existingOrder.OrderStatusId == 5
                || existingOrder.OrderStatusId == 6
                || existingOrder.OrderStatusId == 7) return null;

            existingOrder.OrderStatusId = 5;



            try
            {
                var rowsaffected = await _context.SaveChangesAsync();

                bool? refundResponse = null;

                if ( rowsaffected == 1)
                {
                    refundResponse = await _paymentService.RefundPayment(orderId);

                }
                if (refundResponse != true) return null;


                return await GetOrderByIdAsync(orderId, userId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
