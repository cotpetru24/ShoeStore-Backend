using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ShoeStore.DataContext.PostgreSQL.Models;
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

        public async Task<PlaceOrderResponseDto> PlaceOrderAsync(
    PlaceOrderRequestDto request,
    string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
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

                    // Reduce stock at SIZE level
                    productSize.Stock -= item.Quantity;

                    var orderItem = new OrderItem
                    {
                        UnitPrice = product.Price,
                        Quantity = item.Quantity,
                        CreatedAt = DateTime.UtcNow,




                        //--------- amend the front end to send the ProductSizeId instead of Size -----------
                        ProductSizeId = product.ProductSizes.First(ps => ps.Barcode == item.ProductSizeBarcode).Id
                    };

                    orderItems.Add(orderItem);
                    subtotal += product.Price * item.Quantity;
                }

                // Validate shipping address
                var shippingAddress = await _context.UserAddresses
                    .FirstOrDefaultAsync(a =>
                        a.Id == request.ShippingAddressId &&
                        a.UserId == userId);

                if (shippingAddress == null)
                    throw new ArgumentException("Invalid shipping address");

                // Billing address
                UserAddress billingAddress;

                if (request.BillingAddressSameAsShipping)
                {
                    billingAddress = new UserAddress
                    {
                        AddressLine1 = shippingAddress.AddressLine1,
                        City = shippingAddress.City,
                        Country = shippingAddress.Country,
                        Postcode = shippingAddress.Postcode,
                        UserId = userId
                    };
                }
                else
                {
                    billingAddress = new UserAddress
                    {
                        AddressLine1 = request.BillingAddressRequest.AddressLine1,
                        City = request.BillingAddressRequest.City,
                        Country = request.BillingAddressRequest.Country,
                        Postcode = request.BillingAddressRequest.Postcode,
                        UserId = userId
                    };
                }

                _context.UserAddresses.Add(billingAddress);
                await _context.SaveChangesAsync();


                var total = subtotal + request.ShippingCost - request.Discount;

                var order = new DataContext.PostgreSQL.Models.Order
                {
                    UserId = userId,
                    OrderStatus = (int)OrderStatusEnum.Processing,
                    Subtotal = subtotal,
                    ShippingCost = request.ShippingCost,
                    Discount = request.Discount,
                    Total = total,
                    ShippingAddressId = shippingAddress.Id,
                    BillingAddressId = billingAddress.Id,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in orderItems)
                    item.OrderId = order.Id;

                _context.OrderItems.AddRange(orderItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new PlaceOrderResponseDto
                {
                    OrderId = order.Id,
                    Message = "Order placed successfully",
                    Total = order.Total,
                    CreatedAt = order.CreatedAt
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
                .Include(o => o.Payment)
                    .ThenInclude(s => s.PaymentStatus)
                .FirstOrDefaultAsync(o => o.Id == orderId);




            if (order == null)
                return null;

            var response = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId!,
                OrderStatusId = order.OrderStatus,
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
                    Status = ((PaymentStatusEnum)order.Payment.PaymentStatus).ToString()
                }
            };

            return response;


        }

        public async Task<GetOrdersResponseDto> GetOrdersAsync(GetOrdersRequestDto request, string userId)
        {
            //var query = _context.Orders
            //    .Where(o => o.UserId == userId)
            //    .Include(o => o.OrderStatus)
            //    .Include(o => o.OrderItems)
            //    .AsQueryable();


            var query = _context.Orders
                .Where(o => o.UserId == userId)
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

                .Include(o => o.Payment)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(o => o.Payment)
                    .ThenInclude(s => s.PaymentStatus)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.OrderStatus))
            {
                query = query.Where(o => o.OrderStatus != null && ((OrderStatusEnum)o.OrderStatus).ToString() == request.OrderStatus);
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

            var orderDtos = orders.Select(order => new OrderDto()
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderStatusId = order.OrderStatus,
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
                    Status = ((PaymentStatusEnum)order.Payment.PaymentStatus).ToString()
                }
            }).ToList();

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
        public async Task<CreateAddressResponseDto> CreateShippingAddressAsync(CreateAddressRequestDto request, string userId)
        {
            var newAddress = new UserAddress
            {
                UserId = userId,
                AddressLine1 = request.AddressLine1,
                City = request.City,
                Postcode = request.Postcode,
                Country = request.Country
            };

            _context.UserAddresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return new CreateAddressResponseDto
            {
                Id = newAddress.Id,
                Message = "Shipping address created successfully",
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<List<AddressDto>> GetShippingAddressesAsync(string userId)
        {
            var addresses = await _context.UserAddresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                UserId = a.UserId,
                AddressLine1 = a.AddressLine1,
                City = a.City,
                Postcode = a.Postcode,
                Country = a.Country
            }).ToList();
        }

        public async Task<AddressDto?> GetShippingAddressByIdAsync(int addressId, string userId)
        {
            var address = await _context.UserAddresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                return null;

            return new AddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                AddressLine1 = address.AddressLine1,
                City = address.City,
                Postcode = address.Postcode,
                Country = address.Country
            };
        }

        public async Task<CreateAddressResponseDto> UpdateShippingAddressAsync(int addressId, AddressDto request, string userId)
        {
            var address = await _context.UserAddresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                throw new ArgumentException("Shipping address not found");

            address.AddressLine1 = request.AddressLine1;
            address.City = request.City;
            address.Postcode = request.Postcode;
            address.Country = request.Country;

            await _context.SaveChangesAsync();

            return new CreateAddressResponseDto
            {
                Id = address.Id,
                Message = "Shipping address updated successfully",
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> DeleteShippingAddressAsync(int addressId, string userId)
        {
            var address = await _context.UserAddresses
                .Where(a => a.Id == addressId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (address == null)
                return false;

            // Check if address is being used in any orders
            var isUsedInOrders = await _context.Orders
                .AnyAsync(o => o.ShippingAddressId == addressId || o.BillingAddressId == addressId);

            if (isUsedInOrders)
                throw new InvalidOperationException("Cannot delete address that is being used in orders");

            _context.UserAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return true;
        }

        // Billing Address Methods
        public async Task<CreateAddressResponseDto> CreateBillingAddressAsync(CreateAddressRequestDto request, string userId)
        {
            var newAddress = new UserAddress
            {
                UserId = userId,
                AddressLine1 = request.AddressLine1,
                City = request.City,
                Postcode = request.Postcode,
                Country = request.Country
            };

            _context.UserAddresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return new CreateAddressResponseDto
            {
                Id = newAddress.Id,
                Message = "Billing address created successfully",
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<List<AddressDto>> GetBillingAddressesAsync(int id)
        {
            var addresses = await _context.UserAddresses
                .Where(a => a.Id == id)
                .ToListAsync();

            return addresses.Select(a => new AddressDto
            {
                Id = a.Id,
                UserId = a.UserId,
                AddressLine1 = a.AddressLine1,
                City = a.City,
                Postcode = a.Postcode,
                Country = a.Country
            }).ToList();
        }

        public async Task<AddressDto?> GetBillingAddressByIdAsync(int addressId, string userId)
        {
            var address = await _context.UserAddresses
                .Where(a => a.Id == addressId)
                .FirstOrDefaultAsync();

            if (address == null)
                return null;

            return new AddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                AddressLine1 = address.AddressLine1,
                City = address.City,
                Postcode = address.Postcode,
                Country = address.Country
            };
        }


        public async Task<OrderDto?> CancelOrder(int orderId, string userId)
        {
            await using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingOrder = await _context.Orders
                .Where(o => o.Id == orderId && o.UserId == userId)
                .FirstOrDefaultAsync();

                if (existingOrder == null
                    || existingOrder.OrderStatus == 5
                    || existingOrder.OrderStatus == 6
                    || existingOrder.OrderStatus == 7) return null;

                existingOrder.OrderStatus = 5;

                bool? refundResponse = await _paymentService.RefundPayment(orderId);

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
