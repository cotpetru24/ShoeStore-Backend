using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Cms;

namespace ShoeStore.Services
{
    public class CmsService
    {
        private readonly ShoeStoreContext _context;

        public CmsService(ShoeStoreContext context)
        {
            _context = context;

        }


        internal async Task SeedDefaultProfile()
        {
            var anyProfiles = await _context.CmsProfiles.AnyAsync(p => p.IsDefault == true);

            if (anyProfiles) return;
            else
            {
                var defaultProfile = new CmsProfile()
                {
                    Name = "Default",
                    SiteName = "ShoeStore",
                    Tagline = "Step into Style",
                    IsActive = true,
                    IsDefault = true,
                    LogoBase64 = "",
                    ShowLogoInHeader = false,

                    NavbarBg = "#1a1a1a",
                    NavbarLink = "#ffffff",
                    NavbarText = "#ffffff",

                    FooterBg = "#1a1a1a",
                    FooterLink = "#ffffff",
                    FooterText = "#ffffff",

                    HeroBgBase64 = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", "hero_section.png"))),
                    HeroDescription = "Discover the perfect blend of comfort and fashion. From casual sneakers to elegant formal wear, we have the shoes that match your lifestyle and personality.",
                    HeroPrimaryBtn = "Shop Now",
                    HeroSecondaryBtn = "Explore Categories",
                    HeroSubtitle = "Walk with Confidence",
                    HeroTitle = "Step into Style",

                    NewsletterButtonText = "Subscribe",
                    NewsletterDescription = "Be the first to know about new arrivals, exclusive offers, and style tips. Join our community of fashion-forward individuals.",
                    NewsletterTitle = "Stay in the Loop",

                };

                await _context.CmsProfiles.AddAsync(defaultProfile);
                await _context.SaveChangesAsync();

                var profileId = defaultProfile.Id;

                var features = new List<CmsFeature>
                {
                    new CmsFeature
                    {
                        ProfileId = profileId,
                        SortOrder = 0,
                        IconClass = "bi-truck",
                        Title = "Free Shipping",
                        Description = "Free standard shipping on orders over $50. Fast delivery to your doorstep.",
                        CreatedAt = DateTime.UtcNow
                    },
                    new CmsFeature
                    {
                        ProfileId = profileId,
                        SortOrder = 1,
                        IconClass = "bi-shield-check",
                        Title = "Secure Payment",
                        Description = "100% secure payment processing. Your data is protected with bank-level security.",
                        CreatedAt = DateTime.UtcNow
                    },
                    new CmsFeature
                    {
                        ProfileId = profileId,
                        SortOrder = 2,
                        IconClass = "bi-arrow-repeat",
                        Title = "Easy Returns",
                        Description = "30-day return policy. Not satisfied? Return for free, no questions asked.",
                        CreatedAt = DateTime.UtcNow
                    },
                    new CmsFeature
                    {
                        ProfileId = profileId,
                        SortOrder = 3,
                        IconClass = "bi-headset",
                        Title = "24/7 Support",
                        Description = "Round-the-clock customer support. We're here to help whenever you need us.",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await _context.CmsFeatures.AddRangeAsync(features);


                var categories = new List<CmsCategory>
                {
                    new CmsCategory
                    {
                        ProfileId = profileId,
                        SortOrder = 0,
                        Title = "Men's Collection",
                        Description = "From athletic performance to business casual",
                        ItemTagline = "200+ Styles",
                        ImageBase64 = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", "nike-sneaker-white.png"))),
                        CreatedAt = DateTime.UtcNow
                    },
                    new CmsCategory
                    {
                        ProfileId = profileId,
                        SortOrder = 1,
                        Title = "Women's Collection",
                        Description = "Elegant designs that empower your style",
                        ItemTagline = "250+ Styles",
                        ImageBase64 = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", "nike-yellow.png"))),
                        CreatedAt = DateTime.UtcNow
                    },
                    new CmsCategory
                    {
                        ProfileId = profileId,
                        SortOrder = 2,
                        Title = "Children's Collection",
                        Description = "Comfortable and durable for active kids",
                        ItemTagline = "100+ Styles",
                        ImageBase64 = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", "children-blue.png"))),
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await _context.CmsCategories.AddRangeAsync(categories);

                await _context.SaveChangesAsync();
            }

        }





        internal async Task<CmsNavAndFooterDto> GetCmsNavAndFooterAsync()
        {
            var activeProfile = await _context.CmsProfiles.FirstOrDefaultAsync(p => p.IsActive);


            return new CmsNavAndFooterDto()
            {
                FooterBgColor = activeProfile.FooterBg,
                FooterLinkColor = activeProfile.FooterLink,
                FooterTextColor = activeProfile.FooterText,

                NavbarBgColor = activeProfile.NavbarBg,
                NavbarLinkColor = activeProfile.NavbarLink,
                NavbarTextColor = activeProfile.NavbarText,

                WebsiteName = activeProfile.SiteName,
            };
        }


        internal async Task<CmsLandingPageDto> GetCmsLandingPageAsync()
        {
            var activeProfile = await _context.CmsProfiles.FirstOrDefaultAsync(p => p.IsActive);

            var features = await _context.CmsFeatures
                .Where(f => f.ProfileId == activeProfile.Id)
                .OrderBy(f => f.Id)
                .Select(f => new CmsFeatureDto()
                {
                    Id = f.Id,
                    Description = f.Description,
                    IconClass = f.IconClass,
                    SortOrder = f.SortOrder,
                    Title = f.Title
                })
                .ToListAsync();

            var categories = await _context.CmsCategories
                .Where(c => c.ProfileId == activeProfile.Id)
                .OrderBy(f => f.Id)
                .Select(c => new CmsCategoryDto()
                {
                    Title = c.Title,
                    SortOrder = c.SortOrder,
                    Description = c.Description,
                    Id = c.Id,
                    ImageBase64 = c.ImageBase64,
                    ItemTagline = c.ItemTagline,

                })
                .ToListAsync();

            return new CmsLandingPageDto()
            {
                WebsiteName = activeProfile.SiteName,
                Tagline = activeProfile.Tagline,

                LogoBase64 = activeProfile.LogoBase64,
                FaviconBase64 = activeProfile.LogoBase64,

                HeroBackgroundImageBase64 = activeProfile.HeroBgBase64,
                HeroDescription = activeProfile.HeroDescription,
                HeroPrimaryButtonText = activeProfile.HeroPrimaryBtn,
                HeroSecondaryButtonText = activeProfile.HeroSecondaryBtn,
                HeroSubtitle = activeProfile.HeroSubtitle,
                HeroTitle = activeProfile.HeroTitle,

                ShowLogoInHeader = activeProfile.ShowLogoInHeader,

                Features = features,
                Categories = categories

            };
        }


        //internal async Task<CmsProfileDto> GetCmsActiveProfileAsync()
        //{

        //    //return the active cms profile from the database
        //    var testProfile = new CmsProfileDto();


        //    return testProfile;
        //}


        internal async Task<List<CmsStoredProfileDto>> GetCmsProfilesAsync()
        {
            var storedProfiles = await _context.CmsProfiles
                .OrderBy(p => p.UpdatedAt)
                .Select(p => new CmsStoredProfileDto()
                {
                    CreatedAt = p.CreatedAt,
                    Id = p.Id,
                    IsActive = p.IsActive,
                    LastModified = p.UpdatedAt,
                    ProfileName = p.Name,
                    IsDefault = p.IsDefault,
                })
                .ToListAsync();

            return storedProfiles;
        }

        internal async Task<CmsProfileDto> GetCmsProfileByIdAsync(int profileId)
        {
            var testProfile = new CmsProfileDto();


            return testProfile;
        }

        internal async Task<CmsProfileDto> CreateCmsProfileAsync(CmsProfileDto profileToCreate)
        {
            var testProfile = new CmsProfileDto();


            return testProfile;
        }

        internal async Task<CmsProfileDto> ActivateCmsProfileAsync(int profileId)
        {
            var testProfile = new CmsProfileDto();


            return testProfile;
        }

        internal async Task<CmsProfileDto> UpdateCmsProfileAsync(CmsProfileDto profileToUpdate)
        {
            var testProfile = new CmsProfileDto();


            return testProfile;
        }

        internal async Task<bool> DeleteCmsProfileAsync(int profileId)
        {
            throw new NotImplementedException();
        }


    }
}
