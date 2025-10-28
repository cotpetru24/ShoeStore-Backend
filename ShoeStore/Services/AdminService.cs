using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Admin;
using ShoeStore.Dto.Order;
using System.Linq;
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
            var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var totalUsers = await _userManager.Users.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Products.CountAsync();

            // Calculate new orders today
            var newOrdersToday = await _context.Orders
                .Where(o => o.CreatedAt >= today)
                .CountAsync();

            // Get total revenue from orders with processing, shipped, and delivered status
            var totalRevenueOrders = await _context.Orders
                .Include(o => o.OrderStatus)
                .Where(o => o.OrderStatus != null
                && (o.OrderStatus.Code == "delivered"
                || o.OrderStatus.Code == "processing"
                || o.OrderStatus.Code == "shipped"))
                .ToListAsync();
            var totalRevenue = totalRevenueOrders.Sum(o => o.Total);

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
            var todayRevenue = totalRevenueOrders
                .Where(o => o.CreatedAt >= today)
                .Sum(o => o.Total);

            // Calculate this month's revenue
            var thisMonthRevenue = totalRevenueOrders
                .Where(o => o.CreatedAt >= thisMonth)
                .Sum(o => o.Total);

            // Calculate new users today
            var newUsersToday = await _context.UserDetails
                .Where(u => u.CreatedAt >= today)
                .CountAsync();

            // Calculate new users this month
            var newUsersThisMonth = await _context.UserDetails
                .Where(u => u.CreatedAt >= thisMonth)
                .CountAsync();


            var lowStockProducts = await _context.Products
                .CountAsync(p => p.Stock > 0 && p.Stock <= 10);
            var outOfStockProducts = await _context.Products
                .CountAsync(p => p.Stock == 0);


            //Query to the latest 10 activities
            //When audit entity is acreated and added, user the audit to cet the recent activity
            var userQuery = _context.UserDetails
                .Select(u => new RecentActivityDto
                {
                    Source = "User",
                    UserGuid = u.AspNetUserId,
                    UserEmail = u.AspNetUser.Email,
                    Id = null,
                    Description = string.Empty,
                    CreatedAt = u.CreatedAt
                });

            var productQuery = _context.Products
                .Select(p => new RecentActivityDto
                {
                    Source = "Product",
                    UserGuid = null,
                    UserEmail = null,
                    Id = p.Id,
                    Description = string.Empty,
                    CreatedAt = p.CreatedAt
                });

            var orderQuery = _context.Orders
                .Select(o => new RecentActivityDto
                {
                    Source = "Order",
                    UserGuid = null,
                    UserEmail = null,
                    Id = o.Id,
                    Description = string.Empty,
                    CreatedAt = o.CreatedAt
                });

            var unifiedQuery = userQuery
                .Union(productQuery)
                .Union(orderQuery)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10);

            var recentActivities = unifiedQuery
                .AsEnumerable()
                .Select(x => new RecentActivityDto
                {
                    Source = x.Source,
                    UserGuid = x.UserGuid,
                    Id = x.Id,
                    CreatedAt = x.CreatedAt,
                    Description = x.Source switch
                    {
                        "User" => $"User {x.UserEmail} created",
                        "Product" => $"Product #{x.Id} added",
                        "Order" => $"Order #{x.Id} placed",
                        _ => ""
                    }
                })
                .ToList();



            var test = new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalRevenue = totalRevenue,
                NewOrdersToday = newOrdersToday,
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
                OutOfStockProducts = outOfStockProducts,
                RecentActivity = recentActivities
            };
            return test;
        }

        #endregion

        #region Users

        public async Task<AdminUsersListDto> GetUsersAsync(GetAdminUsersRequestDto request)
        {
            var query = _context.UserDetails
                .Include(u => u.AspNetUser)
                .Where(u => !u.IsHidden)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();

                query = query.Where(o =>
                    o.AspNetUserId.Contains(term)
                    || (o.FirstName + " " + o.LastName).ToLower().Contains(term)
                    || (o.AspNetUser != null && o.AspNetUser.Email.ToLower().Contains(term))
                );

            }

            if (request.UserStatus != null)
            {
                query = request.UserStatus == UserStatus.Blocked
                        ? query.Where(u => u.IsBlocked == true)
                        : query.Where(u => u.IsBlocked == false);
            }


            if (request.UserRole != null)
            {
                var roleName = request.UserRole == UserRole.Administrator
                    ? "Administrator"
                    : "Customer";

                query =
                    from d in query
                    join ur in _context.UserRoles on d.AspNetUser.Id equals ur.UserId
                    join r in _context.Roles on ur.RoleId equals r.Id
                    where r.Name == roleName
                    select d;
            }



            // Apply sorting
            // Default sort by date if no SortBy provided
            var sortBy = request.SortBy;
            var sortDir = request.SortDirection;

            IQueryable<UserDetail> sortedQuery = query;

            switch (sortBy)
            {
                case AdminUserSortBy.Name:
                    sortedQuery = sortDir == AdminUserSortDirection.Ascending
                        ? query.OrderBy(u => u.FirstName)
                        : query.OrderByDescending(u => u.FirstName);
                    break;

                case AdminUserSortBy.DateCreated:
                    sortedQuery = sortDir == AdminUserSortDirection.Ascending
                        ? query.OrderBy(u => u.CreatedAt)
                        : query.OrderByDescending(u => u.CreatedAt);
                    break;

                default:
                    sortedQuery = sortDir == AdminUserSortDirection.Ascending
                        ? query.OrderBy(u => u.CreatedAt)
                        : query.OrderByDescending(u => u.CreatedAt);
                    break;
            }

            query = sortedQuery;







            var totalQueryCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalQueryCount / request.PageSize);



            var users = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var adminUsers = new List<AdminUserDto>();

            // Get user details and roles for each user
            foreach (var user in users)
            {
                // Get the actual IdentityUser instance
                var identityUser = user.AspNetUser;

                // Fetch roles from Identity
                var userRoles = await _userManager.GetRolesAsync(identityUser);

                // Get user statistics
                var userOrders = await _context.Orders
                    .Include(o => o.OrderStatus)
                    .Where(o => o.UserId == user.AspNetUserId)
                    .ToListAsync();

                var totalOrders = userOrders.Count;
                var totalSpent = userOrders
                    .Where(o => o.OrderStatus?.Code == "delivered")
                    .Sum(o => o.Total);

                adminUsers.Add(new AdminUserDto
                {
                    Id = user.AspNetUserId,
                    Email = identityUser.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsBlocked = user.IsBlocked,
                    EmailConfirmed = identityUser.EmailConfirmed,
                    LockoutEnabled = identityUser.LockoutEnabled,
                    LockoutEnd = identityUser.LockoutEnd?.DateTime,
                    AccessFailedCount = identityUser.AccessFailedCount,
                    TotalOrders = totalOrders,
                    TotalSpent = totalSpent,
                    CreatedAt = user.CreatedAt,
                    Roles = userRoles.ToList()
                });
            }


            //Admin users stats 
            var totalUsersCount = await _context.UserDetails.CountAsync();
            var totalActiveUsersCount = await _context.UserDetails
                .Where(u => u.IsBlocked != true && u.IsHidden != true)
                .CountAsync();
            var totalBlockedUsersCount = await _context.UserDetails
                .Where(u => u.IsBlocked == true)
                .CountAsync();
            var totalNewUsersCountThisMonth = await _context.UserDetails
                .Where(u => u.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc))
                .CountAsync();


            var stats = new AdminUsersStatsDto()
            {
                TotalUsersCount = totalUsersCount,
                TotalActiveUsersCount = totalActiveUsersCount,
                TotalBlockedUsersCount = totalBlockedUsersCount,
                TotalNewUsersCountThisMonth = totalNewUsersCountThisMonth
            };
            return new AdminUsersListDto
            {
                Users = adminUsers,
                AdminUsersStats = stats,
                TotalQueryCount = totalQueryCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
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
                userDetail.IsBlocked = request.IsBlocked ?? userDetail.IsBlocked;
            }

            // Update lockout settings
            user.LockoutEnabled = request.LockoutEnabled ?? user.LockoutEnabled;
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
            var userDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.AspNetUserId == userId);

            if (userDetail == null)
                return false;

            userDetail.IsHidden = true;

            var affected = await _context.SaveChangesAsync();
            return affected > 0;
        }


        public async Task<bool> UpdateUserPasswordAsync(string userId, UpdateUserPasswordRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Remove current password
            await _userManager.RemovePasswordAsync(user);

            // Add new password
            var result = await _userManager.AddPasswordAsync(user, request.NewPassword);
            return result.Succeeded;
        }

        public async Task<AdminOrderListDto> GetUserOrdersAsync(GetUserOrdersRequestDto request)
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
                .Include(o => o.UserDetail)
                    .ThenInclude(u => u.AspNetUser)
                .Where(o => o.UserId == request.UserId)
                .AsQueryable();

            // Apply filters
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

            // Sort by creation date descending by default
            query = query.OrderByDescending(o => o.CreatedAt);

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
                    BrandName = oi.Product?.Brand?.Name
                }).ToList();

                var payment = order.Payments.Select(p => new AdminPaymentDto
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod?.DisplayName ?? "Unknown",
                    PaymentStatusName = p.PaymentStatus?.DisplayName ?? "Unknown",
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

            return new AdminOrderListDto
            {
                Orders = adminOrders,
                TotalQueryCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
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
            // Default sort by date if no SortBy provided
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
                    ProductId = oi.ProductId ?? 0,
                    ProductName = oi.ProductName ?? "Unknown Product",
                    ImagePath = oi.Product?.ImagePath,
                    Quantity = oi.Quantity,
                    ProductPrice = oi.ProductPrice,
                    //BrandName=oi.Product.Brand.Name
                }).ToList();

                var payment = order.Payments.Select(p => new AdminPaymentDto
                {
                    Id = p.Id,
                    PaymentMethod = p.PaymentMethod?.DisplayName ?? "Unknown",
                    PaymentStatusName = p.PaymentStatus?.DisplayName ?? "Unknown",
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
                    Payment = payment
                });
            }


            //Add an enum for order status ids


            //Admin orders stats 
            var totalOrdersCount = await _context.Orders.CountAsync();
            var totalPendingOrdersCount = await _context.Orders
                .Where(o => o.OrderStatus!.Id == 1 )
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
                .Include(o => o.BillingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Brand)
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
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId ?? 0,
                    ProductName = oi.ProductName ?? "Unknown Product",
                    ImagePath = oi.Product?.ImagePath,
                    Quantity = oi.Quantity,
                    ProductPrice = oi.ProductPrice,
                    BrandName = oi.Product.Brand.Name

                }).ToList(),
                Payment = order.Payments.Select(p => new AdminPaymentDto
                {
                    Id = p.Id,
                    PaymentStatusName = p.PaymentStatus?.DisplayName ?? "Unknown",
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

            return rowsAffected == 1 ? true : false;
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
