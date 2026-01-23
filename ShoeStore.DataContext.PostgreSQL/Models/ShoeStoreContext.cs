using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class ShoeStoreContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public ShoeStoreContext()
    {
    }

    public ShoeStoreContext(DbContextOptions<ShoeStoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Audience> Audiences { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<ProductSize> ProductSizes { get; set; }

    public virtual DbSet<ShippingAddress> ShippingAddresses { get; set; }

    public virtual DbSet<BillingAddress> BillingAddresses { get; set; }

    public virtual DbSet<UserDetail> UserDetails { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }
    public virtual DbSet<ProductFeature> ProductFeatures { get; set; }

    public virtual DbSet<CmsCategory> CmsCategories { get; set; }

    public virtual DbSet<CmsFeature> CmsFeatures { get; set; }

    public virtual DbSet<CmsProfile> CmsProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Audience>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("audience_pkey");

            entity.ToTable("audience");

            entity.HasIndex(e => e.Code, "audience_code_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("brands_pkey");

            entity.ToTable("brands");

            entity.HasIndex(e => e.Name, "brands_name_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.LogoUrl).HasColumnName("logo_url");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.BillingAddressId).HasColumnName("billing_address_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Discount)
                .HasPrecision(10, 2)
                .HasColumnName("discount");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OrderStatusId).HasColumnName("order_status_id");
            entity.Property(e => e.ShippingAddressId).HasColumnName("shipping_address_id");
            entity.Property(e => e.ShippingCost)
                .HasPrecision(10, 2)
                .HasColumnName("shipping_cost");
            entity.Property(e => e.Subtotal)
                .HasPrecision(10, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasPrecision(10, 2)
                .HasColumnName("total");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.BillingAddress).WithMany(p => p.OrderBillingAddresses)
                .HasForeignKey(d => d.BillingAddressId)
                .HasConstraintName("orders_billing_address_id_fkey");

            entity.HasOne(d => d.OrderStatus).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderStatusId)
                .HasConstraintName("orders_order_status_id_fkey");

            entity.HasOne(d => d.ShippingAddress).WithMany(p => p.OrderShippingAddresses)
                .HasForeignKey(d => d.ShippingAddressId)
                .HasConstraintName("orders_shipping_address_id_fkey");

            entity.HasOne(o => o.UserDetail)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .HasConstraintName("orders_user_id_fkey")
                .OnDelete(DeleteBehavior.SetNull);





        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_items_pkey");

            entity.ToTable("order_items");

            entity.HasIndex(e => e.ProductSizeId, "fki_order_items_product_sizes_id_fkey");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductName).HasColumnName("product_name");
            entity.Property(e => e.ProductPrice)
                .HasPrecision(10, 2)
                .HasColumnName("product_price");
            entity.Property(e => e.ProductSizeId)
                .HasDefaultValue(19)
                .HasColumnName("product_size_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("order_items_order_id_fkey");

            entity.HasOne(d => d.ProductSize).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductSizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_items_product_sizes_id_fkey");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_statuses_pkey");

            entity.ToTable("order_statuses");

            entity.HasIndex(e => e.Code, "order_statuses_code_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasDefaultValueSql("'GBP'::text")
                .HasColumnName("currency");
            entity.Property(e => e.GatewayResponse).HasColumnName("gateway_response");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentMethodId).HasColumnName("payment_method_id");
            entity.Property(e => e.PaymentStatusId).HasColumnName("payment_status_id");
            entity.Property(e => e.PaymentIntentId).HasColumnName("payment_intent_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CardBrand).HasColumnName("card_brand");
            entity.Property(e => e.CardLast4).HasColumnName("card_last4");
            entity.Property(e => e.BillingName).HasColumnName("billing_name");
            entity.Property(e => e.BillingEmail).HasColumnName("billing_email");
            entity.Property(e => e.ReceiptUrl).HasColumnName("receipt_url");

            entity.Property(e => e.ProcessedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("processed_at");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Order)
                  .WithOne(p => p.Payment)
                  .HasForeignKey<Payment>(d => d.OrderId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("payments_order_id_fkey");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentMethodId)
                .HasConstraintName("payments_payment_method_id_fkey");

            entity.HasOne(d => d.PaymentStatus).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentStatusId)
                .HasConstraintName("payments_payment_status_id_fkey");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_methods_pkey");

            entity.ToTable("payment_methods");

            entity.HasIndex(e => e.Code, "payment_methods_code_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_statuses_pkey");

            entity.ToTable("payment_statuses");

            entity.HasIndex(e => e.Code, "payment_statuses_code_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.ToTable("products");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.AudienceId).HasColumnName("audience_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountPercentage)
                .HasDefaultValue(0)
                .HasColumnName("discount_percentage");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsNew)
                .HasDefaultValue(false)
                .HasColumnName("is_new");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.OriginalPrice)
                .HasPrecision(10, 2)
                .HasColumnName("original_price");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Rating)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("rating");
            entity.Property(e => e.ReviewCount)
                .HasDefaultValue(0)
                .HasColumnName("review_count");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Audience).WithMany(p => p.Products)
                .HasForeignKey(d => d.AudienceId)
                .HasConstraintName("products_audience_id_fkey");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("products_brand_id_fkey");
        });

        modelBuilder.Entity<ProductFeature>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_features_pkey");

            entity.ToTable("product_features");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FeatureText).HasColumnName("feature_text");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductFeatures)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_product_id");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_images_pkey");

            entity.ToTable("product_images");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ImagePath).HasColumnName("image_path");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValue(false)
                .HasColumnName("is_primary");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_id");
        });


        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_reviews_pkey");

            entity.ToTable("product_reviews");

            entity.HasIndex(e => new { e.ProductId, e.UserId }, "product_reviews_product_id_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("product_reviews_product_id_fkey");


        });

        modelBuilder.Entity<ProductSize>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_sizes_pkey");

            entity.ToTable("product_sizes");

            entity.HasIndex(e => e.Barcode, "uq_product_sizes_barcode").IsUnique();

            entity.HasIndex(e => new { e.ProductId, e.UkSize }, "uq_product_sizes_product_uk_size").IsUnique();

            entity.HasIndex(e => e.Sku, "uq_product_sizes_sku").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.Barcode).HasColumnName("barcode");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Sku).HasColumnName("sku");
            entity.Property(e => e.Stock)
                .HasDefaultValue(0)
                .HasColumnName("stock");
            entity.Property(e => e.UkSize)
                .HasPrecision(3, 1)
                .HasColumnName("uk_size");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductSizes)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_sizes_product_id_fkey");
        });

        modelBuilder.Entity<ShippingAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shipping_address_pkey");

            entity.ToTable("shipping_address");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AddressLine1).HasColumnName("address_line_1");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.County).HasColumnName("county");
            entity.Property(e => e.Postcode).HasColumnName("postcode");
            entity.Property(e => e.Country).HasColumnName("country");
        });

        modelBuilder.Entity<BillingAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("billing_address_pkey");

            entity.ToTable("billing_address");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AddressLine1).HasColumnName("address_line_1");
            entity.Property(e => e.City).HasColumnName("city");
            entity.Property(e => e.County).HasColumnName("county");
            entity.Property(e => e.Postcode).HasColumnName("postcode");
            entity.Property(e => e.Country).HasColumnName("country");

        });

        modelBuilder.Entity<UserDetail>(entity =>
        {
            entity.ToTable("user_details");

            entity.HasKey(e => e.AspNetUserId).HasName("PK_user_details");

            entity.Property(e => e.AspNetUserId)
                  .HasColumnName("asp_net_user_id")
                  .IsRequired();

            entity.Property(e => e.FirstName)
                  .HasColumnName("first_name")
                  .IsRequired();

            entity.Property(e => e.LastName)
                  .HasColumnName("last_name")
                  .IsRequired();

            entity.Property(e => e.IsHidden)
                  .HasColumnName("is_hidden");

            entity.Property(e => e.IsBlocked)
                .HasColumnName("is_blocked");

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd()
                  .IsRequired();

            entity.Property(e => e.UpdatedAt)
                  .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAddOrUpdate()
                  .IsRequired();

            entity.HasOne(e => e.AspNetUser)
                  .WithOne()
                  .HasForeignKey<UserDetail>(e => e.AspNetUserId)
                  .HasConstraintName("FK_asp_net_user")
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ImagePath).HasColumnName("image_path");
        });

        modelBuilder.Entity<CmsCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cms_categories_pkey");

            entity.ToTable("cms_categories");

            entity.HasIndex(e => new { e.ProfileId, e.SortOrder, e.Id }, "ix_cms_categories_profile_sort");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageBase64).HasColumnName("image_base64");
            entity.Property(e => e.ItemTagline).HasColumnName("item_tagline");
            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.Profile).WithMany(p => p.CmsCategories)
                .HasForeignKey(d => d.ProfileId)
                .HasConstraintName("cms_categories_profile_id_fkey");
        });

        modelBuilder.Entity<CmsFeature>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cms_features_pkey");

            entity.ToTable("cms_features");

            entity.HasIndex(e => new { e.ProfileId, e.SortOrder, e.Id }, "ix_cms_features_profile_sort");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IconClass).HasColumnName("icon_class");
            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0)
                .HasColumnName("sort_order");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.Profile).WithMany(p => p.CmsFeatures)
                .HasForeignKey(d => d.ProfileId)
                .HasConstraintName("cms_features_profile_id_fkey");
        });

        modelBuilder.Entity<CmsProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cms_profiles_pkey");

            entity.ToTable("cms_profiles");

            entity.HasIndex(e => e.Name, "cms_profiles_name_key").IsUnique();

            entity.HasIndex(e => e.IsActive, "ux_cms_profiles_single_active")
                .IsUnique()
                .HasFilter("(is_active = true)");

            entity.Property(e => e.IsDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_default");


            entity.HasIndex(e => e.IsDefault, "ux_cms_profiles_single_default")
                .IsUnique()
                .HasFilter("(is_default = true)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FaviconBase64).HasColumnName("favicon_base64");
            entity.Property(e => e.FooterBg).HasColumnName("footer_bg");
            entity.Property(e => e.FooterLink).HasColumnName("footer_link");
            entity.Property(e => e.FooterText).HasColumnName("footer_text");
            entity.Property(e => e.HeroBgBase64).HasColumnName("hero_bg_base64");
            entity.Property(e => e.HeroDescription).HasColumnName("hero_description");
            entity.Property(e => e.HeroPrimaryBtn).HasColumnName("hero_primary_btn");
            entity.Property(e => e.HeroSecondaryBtn).HasColumnName("hero_secondary_btn");
            entity.Property(e => e.HeroSubtitle).HasColumnName("hero_subtitle");
            entity.Property(e => e.HeroTitle).HasColumnName("hero_title");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(false)
                .HasColumnName("is_active");
            entity.Property(e => e.LogoBase64).HasColumnName("logo_base64");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.NavbarBg).HasColumnName("navbar_bg");
            entity.Property(e => e.NavbarLink).HasColumnName("navbar_link");
            entity.Property(e => e.NavbarText).HasColumnName("navbar_text");
            entity.Property(e => e.NewsletterButtonText).HasColumnName("newsletter_button_text");
            entity.Property(e => e.NewsletterDescription).HasColumnName("newsletter_description");
            entity.Property(e => e.NewsletterTitle).HasColumnName("newsletter_title");
            entity.Property(e => e.ShowLogoInHeader)
                .HasDefaultValue(true)
                .HasColumnName("show_logo_in_header");
            entity.Property(e => e.SiteName).HasColumnName("site_name");
            entity.Property(e => e.Tagline).HasColumnName("tagline");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });




        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
