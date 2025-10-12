using System;
using System.Collections.Generic;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class BillingAddress
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    public string AddressLine1 { get; set; } = null!;

    public string City { get; set; } = null!;

    public string County { get; set; } = null!;

    public string Postcode { get; set; } = null!;

    public string Country { get; set; } = null!;

    public virtual ICollection<Order> OrderBillingAddresses { get; set; } = new List<Order>();
}









