namespace ShoeStore.DataContext.PostgreSQL.Models;

public partial class CmsProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public string SiteName { get; set; } = null!;
    public string? Tagline { get; set; }
    public bool ShowLogoInHeader { get; set; }
    public string? LogoBase64 { get; set; }
    public string? FaviconBase64 { get; set; }
    public string NavbarBg { get; set; } = null!;
    public string NavbarText { get; set; } = null!;
    public string NavbarLink { get; set; } = null!;
    public string FooterBg { get; set; } = null!;
    public string FooterText { get; set; } = null!;
    public string FooterLink { get; set; } = null!;
    public string? HeroTitle { get; set; }
    public string? HeroSubtitle { get; set; }
    public string? HeroDescription { get; set; }
    public string? HeroPrimaryBtn { get; set; }
    public string? HeroSecondaryBtn { get; set; }
    public string? HeroBgBase64 { get; set; }
    public string? NewsletterTitle { get; set; }
    public string? NewsletterDescription { get; set; }
    public string? NewsletterButtonText { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CmsCategory> CmsCategories { get; set; } = new List<CmsCategory>();

    public virtual ICollection<CmsFeature> CmsFeatures { get; set; } = new List<CmsFeature>();
}
