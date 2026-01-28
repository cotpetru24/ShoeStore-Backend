using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Admin;

namespace ShoeStore.Services
{
    public class AdminDashboardService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ShoeStoreContext _context;

        public AdminDashboardService(
            UserManager<IdentityUser> userManager,
            ShoeStoreContext context)
        {
            _userManager = userManager;
            _context = context;
        }

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
                .Where(o => o.OrderStatus != null
                && ((OrderStatusEnum)o.OrderStatus == OrderStatusEnum.Delivered
                || (OrderStatusEnum)o.OrderStatus == OrderStatusEnum.Processing
                || (OrderStatusEnum)o.OrderStatus == OrderStatusEnum.Shipped))
                .ToListAsync();
            var totalRevenue = totalRevenueOrders.Sum(o => o.Total);

            // Get order counts by status
            var ordersWithStatus = await _context.Orders
                .ToListAsync();

            var returnedOrders = ordersWithStatus.Count(o => (OrderStatusEnum)o.OrderStatus == OrderStatusEnum.Returned);
            var processingOrders = ordersWithStatus.Count(o => (OrderStatusEnum)o.OrderStatus == OrderStatusEnum.Processing);
            var shippedOrders = ordersWithStatus.Count(o => (OrderStatusEnum)o.OrderStatus == OrderStatusEnum.Shipped);
            var deliveredOrdersCount = ordersWithStatus.Count(o => (OrderStatusEnum)o.OrderStatus == OrderStatusEnum.Delivered);
            var cancelledOrders = ordersWithStatus.Count(o => (OrderStatusEnum)o.OrderStatus == OrderStatusEnum.Cancelled);

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
                .Select(p => new
                {
                    TotalStock = p.ProductSizes.Sum(s => (int?)s.Stock) ?? 0
                })
                .CountAsync(p => p.TotalStock > 0 && p.TotalStock <= 10);

            var outOfStockProducts = await _context.Products
                .Select(p => new
                {
                    TotalStock = p.ProductSizes.Sum(s => (int?)s.Stock) ?? 0
                })
                .CountAsync(p => p.TotalStock == 0);

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

            return new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalRevenue = totalRevenue,
                NewOrdersToday = newOrdersToday,
                ReturnedOrders = returnedOrders,
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
        }
    }
}
