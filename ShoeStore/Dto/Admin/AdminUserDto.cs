using System;
using System.Collections.Generic;

namespace ShoeStore.Dto.Admin
{
    public class AdminUserDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class AdminUsersListDto
    {
        public List<AdminUserDto> Users { get; set; } = new List<AdminUserDto>();
        public int TotalQueryCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public AdminUsersStatsDto AdminUsersStats { get; set; }
    }

    public class UpdateUserRequestDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? LockoutEnabled { get; set; }
        public bool? IsBlocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class CreateUserRequestDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class UpdateUserPasswordRequestDto
    {
        public string NewPassword { get; set; } = null!;
    }

    public class GetUserOrdersRequestDto
    {
        public string? UserId { get; set; } = null!;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }


    public class GetAdminUsersRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public AdminUserSortDirection SortDirection { get; set; } = AdminUserSortDirection.Descending;
        public AdminUserSortBy SortBy { get; set; } = AdminUserSortBy.DateCreated;
        public UserStatus? UserStatus { get; set; }
        public UserRole? UserRole { get; set; }
    }


    public class AdminUsersStatsDto
    {
        public int TotalUsersCount { get; set; }
        public int TotalActiveUsersCount { get; set; }
        public int TotalBlockedUsersCount { get; set; }
        public int TotalNewUsersCountThisMonth { get; set; }


    }

    public enum AdminUserSortBy
    {
        DateCreated = 1,
        Name = 2,
    }

    public enum AdminUserSortDirection
    {
        Ascending = 1,
        Descending = 2,
    }

    public enum UserStatus
    {
        Active = 1,
        Blocked = 2,
    }

    public enum UserRole
    {
        Administrator = 1,
        Customer = 2,
    }
}
