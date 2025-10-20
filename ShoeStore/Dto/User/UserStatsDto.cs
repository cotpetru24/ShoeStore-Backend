using System;

namespace ShoeStore.Dto.User
{
    public class UserStatsDto
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}










