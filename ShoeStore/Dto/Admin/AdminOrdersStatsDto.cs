namespace ShoeStore.Dto.Admin
{
    public class AdminOrdersStatsDto
    {
        public int TotalOrdersCount { get; set; }
        public int TotalShippedOrdersCount { get; set; }
        public int TotalProcessingOrdersCount { get; set; }
        public int TotalDeliveredOrdersCount { get; set; }
    }
}
