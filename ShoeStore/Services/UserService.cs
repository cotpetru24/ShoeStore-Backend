using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.User;
using System.Security.Claims;

namespace ShoeStore.Services
{
    public class UserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ShoeStoreContext _context;

        public UserService(UserManager<IdentityUser> userManager, ShoeStoreContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var userDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.AspNetUserId == userId);

            // Get user statistics
            var totalOrders = await _context.Orders.CountAsync(o => o.UserId == userId);
            var completedOrders = await _context.Orders
                .CountAsync(o => o.UserId == userId && o.OrderStatus!.Code == "delivered");
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.UserId == userId && 
                    (o.OrderStatus!.Code == "pending" || o.OrderStatus!.Code == "processing"));

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = userDetail?.FirstName ?? "",
                LastName = userDetail?.LastName ?? "",
                JoinDate = userDetail != null ? DateTime.UtcNow : DateTime.UtcNow, // Default to now if no user detail
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                PendingOrders = pendingOrders
            };
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Update Identity user email if changed
            if (user.Email != request.Email)
            {
                user.Email = request.Email;
                user.UserName = request.Email; // Assuming email is used as username
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) return false;
            }

            // Update or create user detail
            var userDetail = await _context.UserDetails
                .FirstOrDefaultAsync(ud => ud.AspNetUserId == userId);

            if (userDetail == null)
            {
                userDetail = new UserDetail
                {
                    AspNetUserId = userId,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };
                _context.UserDetails.Add(userDetail);
            }
            else
            {
                userDetail.FirstName = request.FirstName;
                userDetail.LastName = request.LastName;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            return result.Succeeded;
        }

        public async Task<UserStatsDto> GetUserStatsAsync(string userId)
        {
            var totalOrders = await _context.Orders.CountAsync(o => o.UserId == userId);
            var completedOrders = await _context.Orders
                .CountAsync(o => o.UserId == userId && o.OrderStatus!.Code == "delivered");
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.UserId == userId && 
                    (o.OrderStatus!.Code == "pending" || o.OrderStatus!.Code == "processing"));
            var totalSpent = await _context.Orders
                .Where(o => o.UserId == userId && o.OrderStatus!.Code == "delivered")
                .SumAsync(o => o.Total);

            return new UserStatsDto
            {
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                PendingOrders = pendingOrders,
                TotalSpent = totalSpent
            };
        }
    }
}
