using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto;
using ShoeStore.Dto.Admin;
using ShoeStore.Dto.Order;
using ShoeStore.Dto.User;

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

            var totalOrders = await _context.Orders.CountAsync(o => o.UserId == userId);
            var completedOrders = await _context.Orders
                .CountAsync(o => o.UserId == userId && (OrderStatusEnum)o.OrderStatus == OrderStatusEnum.Delivered);
            var returnedOrders = await _context.Orders
                .CountAsync(o => o.UserId == userId &&
                    (OrderStatusEnum)o.OrderStatus == OrderStatusEnum.Returned);

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = userDetail?.FirstName ?? "",
                LastName = userDetail?.LastName ?? "",
                JoinDate = userDetail != null ? DateTime.UtcNow : DateTime.UtcNow,
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                ReturnedOrders = returnedOrders
            };
        }


        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (user.Email != request.Email)
            {
                user.Email = request.Email;
                user.UserName = request.Email;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded) return false;
            }

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
    }
}
