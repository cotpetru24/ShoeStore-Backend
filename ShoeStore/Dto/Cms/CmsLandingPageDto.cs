namespace ShoeStore.Dto.Cms
{
    public class CmsLandingPageDto
    {
        public string WebsiteName { get; set; } = null!;
        public string? Tagline { get; set; }
        public string? LogoBase64 { get; set; }
        public string? FaviconBase64 { get; set; }
        public string HeroTitle { get; set; } = null!;
        public string HeroSubtitle { get; set; } = null!;
        public string HeroDescription { get; set; } = null!;
        public string HeroPrimaryButtonText { get; set; } = null!;
        public string HeroSecondaryButtonText { get; set; } = null!;
        public string? HeroBackgroundImageBase64 { get; set; }
        public bool ShowLogoInHeader { get; set; }
        public List<CmsFeatureDto> Features { get; set; } = new();
        public List<CmsCategoryDto> Categories { get; set; } = new();
    }
}
