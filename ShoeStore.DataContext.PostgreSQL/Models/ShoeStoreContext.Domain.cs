using Microsoft.EntityFrameworkCore;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class ShoeStoreContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasOne(o => o.UserDetail)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .HasPrincipalKey(u => u.AspNetUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ProductImage>(entity =>
            entity.HasOne(d => d.Product)
              .WithMany(p => p.ProductImages)
              .HasForeignKey(d => d.ProductId)
              .OnDelete(DeleteBehavior.Cascade)
              .HasConstraintName("fk_product_id")
        );
    }
}
