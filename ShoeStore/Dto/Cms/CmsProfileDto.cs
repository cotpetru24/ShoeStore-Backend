namespace ShoeStore.Dto.Cms
{
    public class CmsProfileDto
    {
        public int Id { get; set; }
        public string ProfileName { get; set; } = null!;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string WebsiteName { get; set; } = null!;
        public string? Tagline { get; set; }
        public string? LogoBase64 { get; set; }
        public string? FaviconBase64 { get; set; }
        public bool ShowLogoInHeader { get; set; }
        public string NavbarBgColor { get; set; } = "#ffffff";
        public string NavbarTextColor { get; set; } = "#000000";
        public string NavbarLinkColor { get; set; } = "#007bff";
        public string FooterBgColor { get; set; } = "#343a40";
        public string FooterTextColor { get; set; } = "#ffffff";
        public string FooterLinkColor { get; set; } = "#17a2b8";
        public string HeroTitle { get; set; } = null!;
        public string HeroSubtitle { get; set; } = null!;
        public string HeroDescription { get; set; } = null!;
        public string HeroPrimaryButtonText { get; set; } = null!;
        public string HeroSecondaryButtonText { get; set; } = null!;
        public string? HeroBackgroundImageBase64 { get; set; } = null;
        public List<CmsFeatureDto> Features { get; set; } = new();
        public List<CmsCategoryDto> Categories { get; set; } = new();
    }
}
