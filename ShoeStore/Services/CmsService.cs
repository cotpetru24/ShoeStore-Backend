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


        public async Task<CmsNavAndFooterDto> GetCmsNavAndFooterAsync()
        {
            var activeProfile = await _context.CmsProfiles.FirstOrDefaultAsync(p => p.IsActive);
            if (activeProfile == null)
                throw new Exception("No active profile found");

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


        public async Task<CmsLandingPageDto> GetCmsLandingPageAsync()
        {
            var activeProfile = await _context.CmsProfiles.FirstOrDefaultAsync(p => p.IsActive);
            if (activeProfile == null)
                throw new Exception("No active profile found");

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


        public async Task<CmsProfileDto> CreateCmsProfileAsync(CmsProfileDto profileToCreate)
        {
            var profile = new CmsProfile()
            {
                Name = profileToCreate.ProfileName,
                IsActive = false,
                IsDefault = false,
                SiteName = profileToCreate.WebsiteName,
                Tagline = profileToCreate.Tagline,
                LogoBase64 = profileToCreate.LogoBase64,
                FaviconBase64 = profileToCreate.FaviconBase64,
                ShowLogoInHeader = profileToCreate.ShowLogoInHeader,
                NavbarBg = profileToCreate.NavbarBgColor,
                NavbarText = profileToCreate.NavbarTextColor,
                NavbarLink = profileToCreate.NavbarLinkColor,
                FooterBg = profileToCreate.FooterBgColor,
                FooterText = profileToCreate.FooterTextColor,
                FooterLink = profileToCreate.FooterLinkColor,
                HeroTitle = profileToCreate.HeroTitle,
                HeroSubtitle = profileToCreate.HeroSubtitle,
                HeroDescription = profileToCreate.HeroDescription,
                HeroPrimaryBtn = profileToCreate.HeroPrimaryButtonText,
                HeroSecondaryBtn = profileToCreate.HeroSecondaryButtonText,
                HeroBgBase64 = profileToCreate.HeroBackgroundImageBase64,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.CmsProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();

            var profileId = profile.Id;

            if (profileToCreate.Features != null && profileToCreate.Features.Any())
            {
                var features = profileToCreate.Features.Select(f => new CmsFeature()
                {
                    ProfileId = profileId,
                    IconClass = f.IconClass,
                    Title = f.Title,
                    Description = f.Description,
                    SortOrder = f.SortOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _context.CmsFeatures.AddRangeAsync(features);
            }

            if (profileToCreate.Categories != null && profileToCreate.Categories.Any())
            {
                var categories = profileToCreate.Categories.Select(c => new CmsCategory()
                {
                    ProfileId = profileId,
                    Title = c.Title,
                    Description = c.Description,
                    ImageBase64 = c.ImageBase64,
                    ItemTagline = c.ItemTagline,
                    SortOrder = c.SortOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _context.CmsCategories.AddRangeAsync(categories);
            }

            await _context.SaveChangesAsync();

            return await GetCmsProfileByIdAsync(profileId);
        }


        public async Task<CmsProfileDto> UpdateCmsProfileAsync(CmsProfileDto updatedProfile)
        {
            var profileToUpdate = await _context.CmsProfiles
                .Include(p => p.CmsFeatures)
                .Include(p => p.CmsCategories)
                .FirstOrDefaultAsync(p => p.Id == updatedProfile.Id);

            if (profileToUpdate == null)
                throw new KeyNotFoundException($"CMS profile with ID {updatedProfile.Id} not found.");

            profileToUpdate.Name = updatedProfile.ProfileName;
            profileToUpdate.SiteName = updatedProfile.WebsiteName;
            profileToUpdate.Tagline = updatedProfile.Tagline;
            profileToUpdate.LogoBase64 = updatedProfile.LogoBase64;
            profileToUpdate.FaviconBase64 = updatedProfile.FaviconBase64;
            profileToUpdate.ShowLogoInHeader = updatedProfile.ShowLogoInHeader;
            profileToUpdate.NavbarBg = updatedProfile.NavbarBgColor;
            profileToUpdate.NavbarText = updatedProfile.NavbarTextColor;
            profileToUpdate.NavbarLink = updatedProfile.NavbarLinkColor;
            profileToUpdate.FooterBg = updatedProfile.FooterBgColor;
            profileToUpdate.FooterText = updatedProfile.FooterTextColor;
            profileToUpdate.FooterLink = updatedProfile.FooterLinkColor;
            profileToUpdate.HeroTitle = updatedProfile.HeroTitle;
            profileToUpdate.HeroSubtitle = updatedProfile.HeroSubtitle;
            profileToUpdate.HeroDescription = updatedProfile.HeroDescription;
            profileToUpdate.HeroPrimaryBtn = updatedProfile.HeroPrimaryButtonText;
            profileToUpdate.HeroSecondaryBtn = updatedProfile.HeroSecondaryButtonText;
            profileToUpdate.HeroBgBase64 = updatedProfile.HeroBackgroundImageBase64;
            profileToUpdate.UpdatedAt = DateTime.UtcNow;

            _context.CmsFeatures.RemoveRange(profileToUpdate.CmsFeatures);
            _context.CmsCategories.RemoveRange(profileToUpdate.CmsCategories);

            if (updatedProfile.Features != null && updatedProfile.Features.Any())
            {
                var features = updatedProfile.Features.Select(f => new CmsFeature()
                {
                    ProfileId = profileToUpdate.Id,
                    IconClass = f.IconClass,
                    Title = f.Title,
                    Description = f.Description,
                    SortOrder = f.SortOrder,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _context.CmsFeatures.AddRangeAsync(features);
            }

            if (updatedProfile.Categories != null && updatedProfile.Categories.Any())
            {
                var categories = updatedProfile.Categories.Select(c => new CmsCategory()
                {
                    ProfileId = profileToUpdate.Id,
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

            return await GetCmsProfileByIdAsync(profileToUpdate.Id);
        }


        public async Task<bool> DeleteCmsProfileAsync(int id)
        {
            var profileToDelete = await _context.CmsProfiles
                .Include(p => p.CmsFeatures)
                .Include(p => p.CmsCategories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profileToDelete == null)
                return false;

            if (profileToDelete.IsDefault)
                throw new InvalidOperationException("Cannot delete the default profile.");

            if (profileToDelete.IsActive)
                throw new InvalidOperationException("Cannot delete the active profile. Please activate another profile first.");

            _context.CmsFeatures.RemoveRange(profileToDelete.CmsFeatures);
            _context.CmsCategories.RemoveRange(profileToDelete.CmsCategories);
            _context.CmsProfiles.Remove(profileToDelete);

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<CmsProfileDto> ActivateCmsProfileAsync(int id)
        {
            var profileToActivate = await _context.CmsProfiles
               .AnyAsync(p => p.Id == id);

            if (!profileToActivate)
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
