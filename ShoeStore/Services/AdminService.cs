using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Admin;
using ShoeStore.Dto.Order;
using System.Security.Claims;

namespace ShoeStore.Services
{
    public class AdminService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ShoeStoreContext _context;

        public AdminService(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ShoeStoreContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        #region Dashboard

        public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var totalUsers = await _userManager.Users.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            
            // Get total revenue from delivered orders
            var deliveredOrders = await _context.Orders
                .Include(o => o.OrderStatus)
                .Where(o => o.OrderStatus != null && o.OrderStatus.Code == "delivered")
                .ToListAsync();
            var totalRevenue = deliveredOrders.Sum(o => o.Total);

            // Get order counts by status
            var ordersWithStatus = await _context.Orders
                .Include(o => o.OrderStatus)
                .ToListAsync();

            var pendingOrders = ordersWithStatus.Count(o => o.OrderStatus?.Code == "pending");
            var processingOrders = ordersWithStatus.Count(o => o.OrderStatus?.Code == "processing");
            var shippedOrders = ordersWithStatus.Count(o => o.OrderStatus?.Code == "shipped");
            var deliveredOrdersCount = ordersWithStatus.Count(o => o.OrderStatus?.Code == "delivered");
            var cancelledOrders = ordersWithStatus.Count(o => o.OrderStatus?.Code == "cancelled");

            // Calculate today's revenue
            var todayRevenue = deliveredOrders
                .Where(o => o.CreatedAt >= today)
                .Sum(o => o.Total);

            // Calculate this month's revenue
            var thisMonthRevenue = deliveredOrders
                .Where(o => o.CreatedAt >= thisMonth)
                .Sum(o => o.Total);

            // For new users, we'll use a simplified approach since we don't have creation dates
            var newUsersToday = 0; // Would need to track user creation dates
            var newUsersThisMonth = 0; // Would need to track user creation dates

            var lowStockProducts = await _context.Products
                .CountAsync(p => p.Stock > 0 && p.Stock <= 10);
            var outOfStockProducts = await _context.Products
                .CountAsync(p => p.Stock == 0);

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalRevenue = totalRevenue,
                PendingOrders = pendingOrders,
                ProcessingOrders = processingOrders,
                ShippedOrders = shippedOrders,
                DeliveredOrders = deliveredOrdersCount,
                CancelledOrders = cancelledOrders,
                TodayRevenue = todayRevenue,
                ThisMonthRevenue = thisMonthRevenue,
                NewUsersToday = newUsersToday,
                NewUsersThisMonth = newUsersThisMonth,
                LowStockProducts = lowStockProducts,
                OutOfStockProducts = outOfStockProducts
            };
        }

        #endregion

        #region Users

        public async Task<AdminUserListDto> GetUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.Email!.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var adminUsers = new List<AdminUserDto>();

            // Get user details and roles for each user
            foreach (var user in users)
            {
                var userDetail = await _context.UserDetails
                    .FirstOrDefaultAsync(ud => ud.AspNetUserId == user.Id);
                
                var userRoles = await _userManager.GetRolesAsync(user);

                // Get user statistics
                var userOrders = await _context.Orders
                    .Include(o => o.OrderStatus)
                    .Where(o => o.UserId == user.Id)
                    .ToListAsync();

                var totalOrders = userOrders.Count;
                var totalSpent = userOrders
                    .Where(o => o.OrderStatus?.Code == "delivered")
                    .Sum(o => o.Total);

                adminUsers.Add(new AdminUserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = userDetail?.FirstName,
                    LastName = userDetail?.LastName,
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd?.DateTime,
                    AccessFailedCount = user.AccessFailedCount,
                    TotalOrders = totalOrders,
                    TotalSpent = totalSpent,
                    Roles = userRoles.ToList()
                });
            }

            return new AdminUserListDto
            {
                Users = adminUsers,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<AdminUserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var userDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.AspNetUserId == userId);

            var userRoles = await _userManager.GetRolesAsync(user);

            var totalOrders = await _context.Orders.CountAsync(o => o.UserId == userId);
            var totalSpent = await _context.Orders
                .Where(o => o.UserId == userId && o.OrderStatus!.Code == "delivered")
                .SumAsync(o => o.Total);

            return new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = userDetail?.FirstName,
                LastName = userDetail?.LastName,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd?.DateTime,
                AccessFailedCount = user.AccessFailedCount,
                TotalOrders = totalOrders,
                TotalSpent = totalSpent,
                Roles = userRoles.ToList()
            };
        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Update user details
            var userDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.AspNetUserId == userId);

            if (userDetail == null)
            {
                userDetail = new UserDetail
                {
                    AspNetUserId = userId,
                    FirstName = request.FirstName ?? "",
                    LastName = request.LastName ?? ""
                };
                _context.UserDetails.Add(userDetail);
            }
            else
            {
                userDetail.FirstName = request.FirstName ?? userDetail.FirstName;
                userDetail.LastName = request.LastName ?? userDetail.LastName;
            }

            // Update lockout settings
            user.LockoutEnabled = request.LockoutEnabled;
            user.LockoutEnd = request.LockoutEnd;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            // Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return false;

            if (request.Roles.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!addResult.Succeeded) return false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateUserAsync(CreateUserRequestDto request)
        {
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded) return false;

            // Create user detail
            var userDetail = new UserDetail
            {
                AspNetUserId = user.Id,
                FirstName = request.FirstName ?? "",
                LastName = request.LastName ?? ""
            };
            _context.UserDetails.Add(userDetail);

            // Add roles
            if (request.Roles.Any())
            {
                await _userManager.AddToRolesAsync(user, request.Roles);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Remove user details
            var userDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.AspNetUserId == userId);
            if (userDetail != null)
            {
                _context.UserDetails.Remove(userDetail);
            }

            // Delete user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return false;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Orders

        public async Task<AdminOrderListDto> GetOrdersAsync(GetAdminOrdersRequestDto request)
        {
            var query = _context.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.ShippingAddress)
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.PaymentStatus)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(o => o.Id.ToString().Contains(request.SearchTerm) ||
                                       o.UserId!.Contains(request.SearchTerm));
            }

            if (!string.IsNullOrEmpty(request.StatusFilter))
            {
                query = query.Where(o => o.OrderStatus!.Code == request.StatusFilter);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= request.StartDate);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt <= request.EndDate);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "total" => request.SortDirection == "asc" ? query.OrderBy(o => o.Total) : query.OrderByDescending(o => o.Total),
                "status" => request.SortDirection == "asc" ? query.OrderBy(o => o.OrderStatus!.DisplayName) : query.OrderByDescending(o => o.OrderStatus!.DisplayName),
                _ => request.SortDirection == "asc" ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

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
                    ProductId = oi.ProductId ?? 0,
                    ProductName = oi.ProductName ?? "Unknown Product",
                    ImagePath = oi.Product?.ImagePath,
                    Quantity = oi.Quantity,
                    ProductPrice = oi.ProductPrice,
                    //BrandName=oi.Product.Brand.Name
                }).ToList();

                var payments = order.Payments.Select(p => new AdminPaymentDto
                {
                    Id = p.Id,
                    PaymentMethodName = p.PaymentMethod?.DisplayName ?? "Unknown",
                    PaymentStatusName = p.PaymentStatus?.DisplayName ?? "Unknown",
                    Amount = p.Amount,
                    TransactionId = p.TransactionId,
                    CreatedAt = p.CreatedAt
                }).ToList();

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
                        FirstName = "", // Not available in current model
                        LastName = "", // Not available in current model
                        AddressLine1 = order.ShippingAddress.AddressLine1,
                        AddressLine2 = "", // Not available in current model
                        City = order.ShippingAddress.City,
                        State = order.ShippingAddress.County,
                        PostalCode = order.ShippingAddress.Postcode,
                        Country = order.ShippingAddress.Country,
                        PhoneNumber = "" // Not available in current model
                    } : null,
                    BillingAddress = order.BillingAddress != null ? new AdminBillingAddressDto
                    {
                        Id = order.BillingAddress.Id,
                        FirstName = "", // Not available in current model
                        LastName = "", // Not available in current model
                        AddressLine1 = order.BillingAddress.AddressLine1,
                        AddressLine2 = "", // Not available in current model
                        City = order.BillingAddress.City,
                        State = order.BillingAddress.County,
                        PostalCode = order.BillingAddress.Postcode,
                        Country = order.BillingAddress.Country,
                        PhoneNumber = "" // Not available in current model
                    } : null,
                    OrderItems = orderItems,
                    Payments = payments
                });
            }

            return new AdminOrderListDto
            {
                Orders = adminOrders,
                TotalCount = totalCount,
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
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.PaymentStatus)
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
                    FirstName = "", // Not available in current model
                    LastName = "", // Not available in current model
                    AddressLine1 = order.ShippingAddress.AddressLine1,
                    AddressLine2 = "", // Not available in current model
                    City = order.ShippingAddress.City,
                    State = order.ShippingAddress.County,
                    PostalCode = order.ShippingAddress.Postcode,
                    Country = order.ShippingAddress.Country,
                    PhoneNumber = "" // Not available in current model
                } : null,
                BillingAddress = order.BillingAddress != null ? new AdminBillingAddressDto
                {
                    Id = order.BillingAddress.Id,
                    FirstName = "", // Not available in current model
                    LastName = "", // Not available in current model
                    AddressLine1 = order.BillingAddress.AddressLine1,
                    AddressLine2 = "", // Not available in current model
                    City = order.BillingAddress.City,
                    State = order.BillingAddress.County,
                    PostalCode = order.BillingAddress.Postcode,
                    Country = order.BillingAddress.Country,
                    PhoneNumber = "" // Not available in current model
                } : null,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId ?? 0,
                    ProductName = oi.ProductName ?? "Unknown Product",
                    ImagePath= oi.Product?.ImagePath,
                    Quantity = oi.Quantity,
                    ProductPrice = oi.ProductPrice,
                    //BrandName = oi.Product.Brand.Name
                    
                }).ToList(),
                Payments = order.Payments.Select(p => new AdminPaymentDto
                {
                    Id = p.Id,
                    PaymentMethodName = p.PaymentMethod?.DisplayName ?? "Unknown",
                    PaymentStatusName = p.PaymentStatus?.DisplayName ?? "Unknown",
                    Amount = p.Amount,
                    TransactionId = p.TransactionId,
                    CreatedAt = p.CreatedAt
                }).ToList()
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
                order.Notes = request.Notes;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Products

        public async Task<AdminProductListDto> GetProductsAsync(GetProductsRequestDto request)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Audience)
                .Include(p => p.ProductSizes)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(request.SearchTerm) ||
                                       (p.Description != null && p.Description.Contains(request.SearchTerm)));
            }

            if (request.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == request.BrandId);
            }

            if (request.AudienceId.HasValue)
            {
                query = query.Where(p => p.AudienceId == request.AudienceId);
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= request.MinPrice);
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= request.MaxPrice);
            }

            if (request.IsNew.HasValue)
            {
                query = query.Where(p => p.IsNew == request.IsNew);
            }

            if (request.LowStock == true)
            {
                query = query.Where(p => p.Stock <= 10);
            }

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDirection == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "price" => request.SortDirection == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                "stock" => request.SortDirection == "asc" ? query.OrderBy(p => p.Stock) : query.OrderByDescending(p => p.Stock),
                _ => request.SortDirection == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var products = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var adminProducts = new List<AdminProductDto>();

            foreach (var product in products)
            {
                var productSizes = product.ProductSizes.Select(ps => new AdminProductSizeDto
                {
                    Id = ps.Id,
                    Size = $"{ps.UkSize} UK / {ps.UsSize} US / {ps.EuSize} EU",
                    Stock = ps.Stock
                }).ToList();

                var productImages = new List<AdminProductImageDto>(); // ProductImages not available in current model

                adminProducts.Add(new AdminProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    OriginalPrice = product.OriginalPrice,
                    ImagePath = product.ImagePath,
                    Stock = product.Stock,
                    BrandId = product.BrandId,
                    BrandName = product.Brand?.Name,
                    AudienceId = product.AudienceId,
                    AudienceName = product.Audience?.DisplayName,
                    Rating = product.Rating,
                    ReviewCount = product.ReviewCount,
                    IsNew = product.IsNew,
                    DiscountPercentage = product.DiscountPercentage,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    ProductSizes = productSizes,
                    ProductImages = productImages
                });
            }

            return new AdminProductListDto
            {
                Products = adminProducts,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<AdminProductDto?> GetProductByIdAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Audience)
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return null;

            return new AdminProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                ImagePath = product.ImagePath,
                Stock = product.Stock,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name,
                AudienceId = product.AudienceId,
                AudienceName = product.Audience?.DisplayName,
                Rating = product.Rating,
                ReviewCount = product.ReviewCount,
                IsNew = product.IsNew,
                DiscountPercentage = product.DiscountPercentage,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                ProductSizes = product.ProductSizes.Select(ps => new AdminProductSizeDto
                {
                    Id = ps.Id,
                    Size = $"{ps.UkSize} UK / {ps.UsSize} US / {ps.EuSize} EU",
                    Stock = ps.Stock
                }).ToList(),
                ProductImages = new List<AdminProductImageDto>() // ProductImages not available in current model
            };
        }

        public async Task<bool> CreateProductAsync(CreateProductRequestDto request)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                OriginalPrice = request.OriginalPrice,
                ImagePath = request.ImagePath,
                Stock = request.Stock,
                BrandId = request.BrandId,
                AudienceId = request.AudienceId,
                IsNew = request.IsNew,
                DiscountPercentage = request.DiscountPercentage,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Add product sizes
            foreach (var sizeRequest in request.ProductSizes)
            {
                var productSize = new ProductSize
                {
                    ProductId = product.Id,
                    UkSize = 0, // Default values - would need to be parsed from sizeRequest.Size
                    UsSize = 0,
                    EuSize = 0,
                    Stock = sizeRequest.Stock
                };
                _context.ProductSizes.Add(productSize);
            }

            // ProductImages not available in current model - skip image creation

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateProductAsync(int productId, UpdateProductRequestDto request)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.OriginalPrice = request.OriginalPrice;
            product.ImagePath = request.ImagePath;
            product.Stock = request.Stock;
            product.BrandId = request.BrandId;
            product.AudienceId = request.AudienceId;
            product.IsNew = request.IsNew;
            product.DiscountPercentage = request.DiscountPercentage;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}
