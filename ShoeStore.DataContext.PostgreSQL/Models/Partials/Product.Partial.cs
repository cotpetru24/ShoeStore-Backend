using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class Product
{
    //Delete from other partial the anvigation to ProductImages and keep the one below.
    //This happens due to unique is_primary check
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
