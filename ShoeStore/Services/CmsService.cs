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
            var profile = await _context.CmsProfiles
                .Include(p => p.CmsFeatures)
                .Include(p => p.CmsCategories)
                .FirstOrDefaultAsync(p => p.Id == profileId);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profile with ID {profileId} not found.");
            }

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
                IsActive = profile.IsActive,
                WebsiteName = profile.SiteName,
                Tagline = profile.Tagline,
                LogoBase64 = profile.LogoBase64,
                FaviconBase64 = profile.FaviconBase64,
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

        internal async Task<CmsProfileDto> CreateCmsProfileAsync(CmsProfileDto profileToCreate)
        {
            // Create the profile entity
            var newProfile = new CmsProfile()
            {
                Name = profileToCreate.ProfileName,
                SiteName = profileToCreate.WebsiteName,
                Tagline = profileToCreate.Tagline,
                IsActive = false, // New profiles are inactive by default
                IsDefault = false,
                LogoBase64 = profileToCreate.LogoBase64,
                FaviconBase64 = profileToCreate.FaviconBase64,
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
                NewsletterTitle = null,
                NewsletterDescription = null,
                NewsletterButtonText = null,
                ShowLogoInHeader = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.CmsProfiles.AddAsync(newProfile);
            await _context.SaveChangesAsync();

            var profileId = newProfile.Id;

            // Add features
            if (profileToCreate.Features != null && profileToCreate.Features.Any())
            {
                var features = profileToCreate.Features.Select(f => new CmsFeature()
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

            // Add categories
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
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _context.CmsCategories.AddRangeAsync(categories);
            }

            await _context.SaveChangesAsync();

            // Return the created profile
            return await GetCmsProfileByIdAsync(profileId);
        }

        internal async Task<CmsProfileDto> ActivateCmsProfileAsync(int profileId)
        {
            var exists = await _context.CmsProfiles
                .AnyAsync(p => p.Id == profileId);

            if (!exists)
            {
                throw new KeyNotFoundException($"Profile with ID {profileId} not found.");
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
                    .Where(p => p.Id == profileId)
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

            return await GetCmsProfileByIdAsync(profileId);
        }


        internal async Task<CmsProfileDto> UpdateCmsProfileAsync(CmsProfileDto profileToUpdate)
        {
            var profile = await _context.CmsProfiles
                .Include(p => p.CmsFeatures)
                .Include(p => p.CmsCategories)
                .FirstOrDefaultAsync(p => p.Id == profileToUpdate.Id);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profile with ID {profileToUpdate.Id} not found.");
            }

            // Update profile properties
            profile.Name = profileToUpdate.ProfileName;
            profile.SiteName = profileToUpdate.WebsiteName;
            profile.Tagline = profileToUpdate.Tagline;
            profile.LogoBase64 = profileToUpdate.LogoBase64;
            profile.FaviconBase64 = profileToUpdate.FaviconBase64;
            profile.NavbarBg = profileToUpdate.NavbarBgColor;
            profile.NavbarText = profileToUpdate.NavbarTextColor;
            profile.NavbarLink = profileToUpdate.NavbarLinkColor;
            profile.FooterBg = profileToUpdate.FooterBgColor;
            profile.FooterText = profileToUpdate.FooterTextColor;
            profile.FooterLink = profileToUpdate.FooterLinkColor;
            profile.HeroTitle = profileToUpdate.HeroTitle;
            profile.HeroSubtitle = profileToUpdate.HeroSubtitle;
            profile.HeroDescription = profileToUpdate.HeroDescription;
            profile.HeroPrimaryBtn = profileToUpdate.HeroPrimaryButtonText;
            profile.HeroSecondaryBtn = profileToUpdate.HeroSecondaryButtonText;
            profile.HeroBgBase64 = profileToUpdate.HeroBackgroundImageBase64;
            profile.UpdatedAt = DateTime.UtcNow;

            // Update features - remove existing and add new ones
            _context.CmsFeatures.RemoveRange(profile.CmsFeatures);
            if (profileToUpdate.Features != null && profileToUpdate.Features.Any())
            {
                var features = profileToUpdate.Features.Select(f => new CmsFeature()
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

            // Update categories - remove existing and add new ones
            _context.CmsCategories.RemoveRange(profile.CmsCategories);
            if (profileToUpdate.Categories != null && profileToUpdate.Categories.Any())
            {
                var categories = profileToUpdate.Categories.Select(c => new CmsCategory()
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

            // Return the updated profile
            return await GetCmsProfileByIdAsync(profile.Id);
        }

        internal async Task<bool> DeleteCmsProfileAsync(int profileId)
        {
            var profile = await _context.CmsProfiles
                .Include(p => p.CmsFeatures)
                .Include(p => p.CmsCategories)
                .FirstOrDefaultAsync(p => p.Id == profileId);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profile with ID {profileId} not found.");
            }

            // Check if it's the last profile
            var totalProfiles = await _context.CmsProfiles.CountAsync();
            if (totalProfiles <= 1)
            {
                throw new InvalidOperationException("Cannot delete the last profile. At least one profile must exist.");
            }

            // Check if it's the active profile
            if (profile.IsActive)
            {
                throw new InvalidOperationException("Cannot delete the active profile. Please set another profile as active first.");
            }

            // Check if it's the default profile
            if (profile.IsDefault)
            {
                throw new InvalidOperationException("Cannot delete the default profile.");
            }

            // Delete associated features and categories (cascade delete should handle this, but being explicit)
            _context.CmsFeatures.RemoveRange(profile.CmsFeatures);
            _context.CmsCategories.RemoveRange(profile.CmsCategories);

            // Delete the profile
            _context.CmsProfiles.Remove(profile);
            await _context.SaveChangesAsync();

            return true;
        }


    }
}
