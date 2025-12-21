using System;
using System.Collections.Generic;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal? OriginalPrice { get; set; }

    public string? ImagePath { get; set; }

    public int Stock { get; set; }

    public int? BrandId { get; set; }

    public int? AudienceId { get; set; }

    public decimal? Rating { get; set; }

    public int? ReviewCount { get; set; }

    public bool? IsNew { get; set; }

    public bool IsActive { get; set; }

    public int? DiscountPercentage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Audience? Audience { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();

}
