using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.DataContext.PostgreSQL.Models
{
    public partial class UserDetail
    {
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }

}
