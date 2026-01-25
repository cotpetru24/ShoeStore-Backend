using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class Product
{
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
