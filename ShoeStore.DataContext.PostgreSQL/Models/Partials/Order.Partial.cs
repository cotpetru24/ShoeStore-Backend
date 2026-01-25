namespace ShoeStore.DataContext.PostgreSQL.Models
{
    public partial class Order
    {
        public virtual UserDetail UserDetail { get; set; }
    }
}
