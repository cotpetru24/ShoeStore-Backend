namespace ShoeStore.Dto.Admin
{
    public class GetAdminUsersRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public SortDirectionEnum SortDirection { get; set; } = SortDirectionEnum.Descending;
        public AdminUsersSortByEnum SortBy { get; set; } = AdminUsersSortByEnum.DateCreated;
        public UserStatusEnum? UserStatus { get; set; }
        public UserRoleEnum? UserRole { get; set; }
    }
}
