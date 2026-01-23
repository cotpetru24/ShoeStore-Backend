using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Admin;
using ShoeStore.Dto.Order;

namespace ShoeStore.Services
{
    public class AdminUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ShoeStoreContext _context;

        public AdminUserService(
            UserManager<IdentityUser> userManager,
            ShoeStoreContext context)
        {
            _userManager = userManager;
            _context = context;
        }

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
                var identityUser = user.AspNetUser;
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
                Roles = userRoles.ToList(),
                IsBlocked = userDetail.IsBlocked,
                CreatedAt = userDetail.CreatedAt,
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
            user.LockoutEnd = request.LockoutEnd ?? user.LockoutEnd;

            // Update email
            user.Email = request.Email ?? user.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return false;

            if (request.Roles.Any())
            {
                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded) return false;

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
                    .ThenInclude(ps => ps.ProductSize)
                    .ThenInclude(p => p.Product)
                .Include(o => o.Payment)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(o => o.Payment)
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
                    ProductId = oi.ProductSize.ProductId,
                    ProductName = oi.ProductName ?? "Unknown Product",
                    Quantity = oi.Quantity,
                    ProductPrice = oi.ProductPrice,
                    BrandName = oi.ProductSize.Product?.Brand?.Name
                }).ToList();

                var payment = new AdminPaymentDto
                {
                    Id = order.Payment.Id,
                    PaymentMethod = order.Payment.PaymentMethod.DisplayName,
                    PaymentStatus = order.Payment.PaymentStatus.DisplayName,
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
                    OrderStatusName = order.OrderStatus?.DisplayName,
                    OrderStatusCode = order.OrderStatus?.Code,
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
                        County = order.ShippingAddress.County,
                        Postcode = order.ShippingAddress.Postcode,
                        Country = order.ShippingAddress.Country,
                    } : null,
                    BillingAddress = order.BillingAddress != null ? new AddressDto
                    {
                        Id = order.BillingAddress.Id,
                        AddressLine1 = order.BillingAddress.AddressLine1,
                        City = order.BillingAddress.City,
                        County = order.BillingAddress.County,
                        Postcode = order.BillingAddress.Postcode,
                        Country = order.BillingAddress.Country,
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
    }
}
