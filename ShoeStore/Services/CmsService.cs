using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Cms;
using System.Linq;

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





        public async Task<CmsNavAndFooterDto> GetCmsNavAndFooterAsync()
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
                WebsiteLogo = activeProfile.LogoBase64,
                ShowLogo = activeProfile.ShowLogoInHeader,

                Favicon = activeProfile.FaviconBase64
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

        public async Task<List<CmsStoredProfileDto>> GetCmsProfilesAsync()
        {
            return await _context.CmsProfiles
                .Select(p => new CmsStoredProfileDto()
                {
                    Id = p.Id,
                    ProfileName = p.Name,
                    IsActive = p.IsActive,
                    IsDefault = p.IsDefault,
                    LastModified = p.UpdatedAt,
                    CreatedAt = p.CreatedAt
                })
                .OrderByDescending(p => p.LastModified)
                .ToListAsync();
        }

        public async Task<CmsProfileDto> GetCmsProfileByIdAsync(int id)
        {
            var profile = await _context.CmsProfiles
                .Include(p => p.CmsFeatures)
                .Include(p => p.CmsCategories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profile == null)
                throw new KeyNotFoundException($"CMS profile with ID {id} not found.");

            var features = profile.CmsFeatures
                .OrderBy(f => f.SortOrder)
                .Select(f => new CmsFeatureDto()
                {
                    Id = f.Id,
                    IconClass = f.IconClass ?? "",
                    Title = f.Title,
                    Description = f.Description ?? "",
                    SortOrder = f.SortOrder
                })
                .ToList();

            var categories = profile.CmsCategories
                .OrderBy(c => c.SortOrder)
                .Select(c => new CmsCategoryDto()
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description ?? "",
                    ImageBase64 = c.ImageBase64,
                    ItemTagline = c.ItemTagline,
                    SortOrder = c.SortOrder
                })
                .ToList();

            return new CmsProfileDto()
            {
                Id = profile.Id,
                ProfileName = profile.Name,
                IsDefault = profile.IsDefault,
                IsActive = profile.IsActive,
                WebsiteName = profile.SiteName,
                Tagline = profile.Tagline,
                LogoBase64 = profile.LogoBase64,
                FaviconBase64 = profile.FaviconBase64,
                ShowLogoInHeader = profile.ShowLogoInHeader,
                NavbarBgColor = profile.NavbarBg,
                NavbarTextColor = profile.NavbarText,
                NavbarLinkColor = profile.NavbarLink,
                FooterBgColor = profile.FooterBg,
                FooterTextColor = profile.FooterText,
                FooterLinkColor = profile.FooterLink,
                HeroTitle = profile.HeroTitle ?? "",
                HeroSubtitle = profile.HeroSubtitle ?? "",
                HeroDescription = profile.HeroDescription ?? "",
                HeroPrimaryButtonText = profile.HeroPrimaryBtn ?? "",
                HeroSecondaryButtonText = profile.HeroSecondaryBtn ?? "",
                HeroBackgroundImageBase64 = profile.HeroBgBase64,
                Features = features,
                Categories = categories
            };
        }

        public async Task<CmsProfileDto> CreateCmsProfileAsync(CmsProfileDto dto)
        {
            var profile = new CmsProfile()
            {
                Name = dto.ProfileName,
                IsActive = false,
                IsDefault = false,
                SiteName = dto.WebsiteName,
                Tagline = dto.Tagline,
                LogoBase64 = dto.LogoBase64,
                FaviconBase64 = dto.FaviconBase64,
                ShowLogoInHeader = dto.ShowLogoInHeader,
                NavbarBg = dto.NavbarBgColor,
                NavbarText = dto.NavbarTextColor,
                NavbarLink = dto.NavbarLinkColor,
                FooterBg = dto.FooterBgColor,
                FooterText = dto.FooterTextColor,
                FooterLink = dto.FooterLinkColor,
                HeroTitle = dto.HeroTitle,
                HeroSubtitle = dto.HeroSubtitle,
                HeroDescription = dto.HeroDescription,
                HeroPrimaryBtn = dto.HeroPrimaryButtonText,
                HeroSecondaryBtn = dto.HeroSecondaryButtonText,
                HeroBgBase64 = dto.HeroBackgroundImageBase64,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.CmsProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();

            var profileId = profile.Id;

            if (dto.Features != null && dto.Features.Any())
            {
                var features = dto.Features.Select(f => new CmsFeature()
                {
                    ProfileId = profileId,
                    IconClass = f.IconClass,
                    Title = f.Title,
                    Description = f.Description,
                    SortOrder = f.SortOrder,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _context.CmsFeatures.AddRangeAsync(features);
            }

            if (dto.Categories != null && dto.Categories.Any())
            {
                var categories = dto.Categories.Select(c => new CmsCategory()
                {
                    ProfileId = profileId,
                    Title = c.Title,
                    Description = c.Description,
                    ImageBase64 = c.ImageBase64,
                    ItemTagline = c.ItemTagline,
                    SortOrder = c.SortOrder,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _context.CmsCategories.AddRangeAsync(categories);
            }

            await _context.SaveChangesAsync();

            return await GetCmsProfileByIdAsync(profileId);
        }

        public async Task<CmsProfileDto> UpdateCmsProfileAsync(CmsProfileDto dto)
        {
            var profile = await _context.CmsProfiles
                .Include(p => p.CmsFeatures)
                .Include(p => p.CmsCategories)
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (profile == null)
                throw new KeyNotFoundException($"CMS profile with ID {dto.Id} not found.");

            profile.Name = dto.ProfileName;
            profile.SiteName = dto.WebsiteName;
            profile.Tagline = dto.Tagline;
            profile.LogoBase64 = dto.LogoBase64;
            profile.FaviconBase64 = dto.FaviconBase64;
            profile.ShowLogoInHeader = dto.ShowLogoInHeader;
            profile.NavbarBg = dto.NavbarBgColor;
            profile.NavbarText = dto.NavbarTextColor;
            profile.NavbarLink = dto.NavbarLinkColor;
            profile.FooterBg = dto.FooterBgColor;
            profile.FooterText = dto.FooterTextColor;
            profile.FooterLink = dto.FooterLinkColor;
            profile.HeroTitle = dto.HeroTitle;
            profile.HeroSubtitle = dto.HeroSubtitle;
            profile.HeroDescription = dto.HeroDescription;
            profile.HeroPrimaryBtn = dto.HeroPrimaryButtonText;
            profile.HeroSecondaryBtn = dto.HeroSecondaryButtonText;
            profile.HeroBgBase64 = dto.HeroBackgroundImageBase64;
            profile.UpdatedAt = DateTime.UtcNow;

            _context.CmsFeatures.RemoveRange(profile.CmsFeatures);
            _context.CmsCategories.RemoveRange(profile.CmsCategories);

            if (dto.Features != null && dto.Features.Any())
            {
                var features = dto.Features.Select(f => new CmsFeature()
                {
                    ProfileId = profile.Id,
                    IconClass = f.IconClass,
                    Title = f.Title,
                    Description = f.Description,
                    SortOrder = f.SortOrder,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _context.CmsFeatures.AddRangeAsync(features);
            }

            if (dto.Categories != null && dto.Categories.Any())
            {
                var categories = dto.Categories.Select(c => new CmsCategory()
                {
                    ProfileId = profile.Id,
                    Title = c.Title,
                    Description = c.Description,
                    ImageBase64 = c.ImageBase64,
                    ItemTagline = c.ItemTagline,
                    SortOrder = c.SortOrder,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _context.CmsCategories.AddRangeAsync(categories);
            }

            await _context.SaveChangesAsync();

            return await GetCmsProfileByIdAsync(profile.Id);
        }

        public async Task<bool> DeleteCmsProfileAsync(int id)
        {
            var profile = await _context.CmsProfiles
                .Include(p => p.CmsFeatures)
                .Include(p => p.CmsCategories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profile == null)
                return false;

            if (profile.IsDefault)
                throw new InvalidOperationException("Cannot delete the default profile.");

            if (profile.IsActive)
                throw new InvalidOperationException("Cannot delete the active profile. Please activate another profile first.");

            _context.CmsFeatures.RemoveRange(profile.CmsFeatures);
            _context.CmsCategories.RemoveRange(profile.CmsCategories);
            _context.CmsProfiles.Remove(profile);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CmsProfileDto> ActivateCmsProfileAsync(int id)
        {
            var profile = await _context.CmsProfiles
               .AnyAsync(p => p.Id == id);

            if (!profile)
            {
                throw new KeyNotFoundException($"Profile with ID {id} not found.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.CmsProfiles
                    .Where(p => p.IsActive)
                    .ExecuteUpdateAsync(setters =>
                        setters
                            .SetProperty(p => p.IsActive, false)
                    );

                await _context.CmsProfiles
                    .Where(p => p.Id == id)
                    .ExecuteUpdateAsync(setters =>
                        setters
                            .SetProperty(p => p.IsActive, true)
                            .SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
                    );

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetCmsProfileByIdAsync(id);
        }
    }
}
