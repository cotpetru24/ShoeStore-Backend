using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.DataContext.PostgreSQL.Models
{
    public partial class ProductFeature
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string FeatureText { get; set; } = null!;

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual Product Product { get; set; } = null!;
    }
}
