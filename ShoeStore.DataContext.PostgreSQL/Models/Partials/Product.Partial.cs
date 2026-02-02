using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class Product
{
    //Delete from other partial the navigation to ProductImages and keep the one below.
    //This happens because of the unique is_primary check
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
