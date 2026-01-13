using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Admin;
using ShoeStore.Dto.Product;

namespace ShoeStore.Services
{
    public class AdminProductService
    {
        private readonly ShoeStoreContext _context;

        public AdminProductService(ShoeStoreContext context)
        {
            _context = context;
        }

        public async Task<AdminProductListDto> GetProductsAsync(GetProductsRequestDto request)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Audience)
                .Include(p => p.ProductSizes)
                .Include(p => p.ProductFeatures)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, $"%{request.SearchTerm}%")
                    || (p.Description != null &&
                        EF.Functions.ILike(p.Description, $"%{request.SearchTerm}%"))
                    || p.ProductSizes.Any(ps =>
                        ps.Barcode != null &&
                        ps.Barcode.Contains(request.SearchTerm))
                    || p.ProductFeatures.Any(pf =>
                        pf.FeatureText != null &&
                        pf.FeatureText.Contains(request.SearchTerm))
                );
            }

            if (request.IsActive != null)
            {
                query = query.Where(p => p.IsActive == request.IsActive);
            }

            if (request.ProductBrand != null)
            {
                query = query.Where(p => EF.Functions.ILike(p.Brand.Name, request.ProductBrand));
            }

            if (request.ProductCategory != null)
            {
                query = query.Where(p => EF.Functions.ILike(p.Audience.DisplayName, request.ProductCategory));
            }

            if (request.ProductStockStatus != null)
            {
                var productStockStatus = AdminProductEnumMappings.MapAdminProductStockStatus(request.ProductStockStatus);

                switch (productStockStatus)
                {
                    case AdminProductStockStatus.LowStock:
                        query = query.Where(p =>
                            p.ProductSizes.Sum(s => s.Stock) > 0 &&
                            p.ProductSizes.Sum(s => s.Stock) < 10);
                        break;

                    case AdminProductStockStatus.HighStock:
                        query = query.Where(p =>
                            p.ProductSizes.Sum(s => s.Stock) > 50);
                        break;

                    case AdminProductStockStatus.InStock:
                        query = query.Where(p =>
                            p.ProductSizes.Sum(s => s.Stock) > 0);
                        break;

                    case AdminProductStockStatus.OutOfStock:
                        query = query.Where(p =>
                            p.ProductSizes.Sum(s => s.Stock) <= 0);
                        break;

                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(request.SortBy))
            {
                var sortBy = AdminProductEnumMappings.MapAdminProductsSortBy(request.SortBy);
                var direction = AdminProductEnumMappings.MapAdminProductsSortDirection(request.SortDirection);

                switch (sortBy)
                {
                    case AdminProductsSortBy.Name:
                        query = direction == AdminProductsSortDirection.Ascending
                            ? query.OrderBy(p => p.Name).ThenBy(p => p.Id)
                            : query.OrderByDescending(p => p.Name).ThenByDescending(p => p.Id);
                        break;

                    case AdminProductsSortBy.Stock:
                        query = direction == AdminProductsSortDirection.Ascending
                            ? query.OrderBy(p => p.ProductSizes.Sum(s => s.Stock)).ThenBy(p => p.Id)
                            : query.OrderByDescending(p => p.ProductSizes.Sum(s => s.Stock)).ThenByDescending(p => p.Id);
                        break;

                    case AdminProductsSortBy.DateCreated:
                    default:
                        query = direction == AdminProductsSortDirection.Ascending
                            ? query.OrderBy(p => p.CreatedAt).ThenBy(p => p.Id)
                            : query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);
                        break;
                }
            }

            var totalQueryCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalQueryCount / request.PageSize);

            var products = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var adminProducts = new List<AdminProductDto>();

            foreach (var product in products)
            {
                var productSizes = product.ProductSizes.Select(ps => new ProductSizeDto
                {
                    Id = ps.Id,
                    Size = ps.UkSize,
                    Stock = ps.Stock
                }).ToList();

                var productImages = product.ProductImages
                    .OrderBy(img => img.SortOrder)
                    .Select(img => new ProductImageDto()
                    {
                        Id = img.Id,
                        ImagePath = img.ImagePath,
                        SortOrder = img.SortOrder,
                        IsPrimary = img.IsPrimary,
                        ProductId = img.ProductId
                    })
                    .ToList();

                adminProducts.Add(new AdminProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    OriginalPrice = product.OriginalPrice,
                    BrandId = product.BrandId,
                    BrandName = product.Brand?.Name,
                    AudienceId = product.AudienceId,
                    Audience = product.Audience?.DisplayName,
                    Rating = product.Rating,
                    ReviewCount = product.ReviewCount,
                    IsNew = product.IsNew,
                    DiscountPercentage = product.DiscountPercentage,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    ProductSizes = productSizes,
                    ProductImages = productImages,
                    IsActive = product.IsActive
                });
            }

            var totalProductsCount = await _context.Products.CountAsync();
            var totalActiveProductsCount = await _context.Products
                .Where(p => p.IsActive == true)
                .CountAsync();
            var totalLowStockProductsCount = await _context.Products
                .Where(p =>
                    p.ProductSizes.Sum(s => s.Stock) > 0 &&
                    p.ProductSizes.Sum(s => s.Stock) < 10)
                .CountAsync();

            var totalOutOfStockProductsCount = await _context.Products
                .Where(p =>
                    p.ProductSizes.Sum(s => s.Stock) <= 0)
                .CountAsync();

            var stats = new AdminProductsStatsDto()
            {
                TotalProductsCount = totalProductsCount,
                TotalLowStockProductsCount = totalLowStockProductsCount,
                TotalOutOfStockProductsCount = totalOutOfStockProductsCount,
                TotalActiveProductsCount = totalActiveProductsCount,
            };

            var allBrands = await _context.Brands
                .Select(b => b.Name)
                .ToArrayAsync();

            return new AdminProductListDto
            {
                Products = adminProducts,
                TotalQueryCount = totalQueryCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                AdminProductsStats = stats,
                AllBrands = allBrands
            };
        }

        public async Task<List<AdminBrandDto>> GetProductBrandsAsync()
        {
            var allBrands = await _context.Brands
                .Select(b => new AdminBrandDto()
                {
                    BrandId = b.Id,
                    BrandName = b.Name
                })
                .ToListAsync();

            return allBrands;
        }

        public async Task<List<AdminAudienceDto>> GetProductAudienceAsync()
        {
            var allAudience = await _context.Audiences
                .Select(a => new AdminAudienceDto()
                {
                    AudienceId = a.Id,
                    AudienceName = a.DisplayName
                })
                .ToListAsync();

            return allAudience;
        }

        public async Task<AdminProductDto?> GetProductByIdAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Audience)
                .Include(p => p.ProductSizes)
                .Include(p => p.ProductFeatures)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return null;

            return new AdminProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name,
                AudienceId = product.AudienceId,
                Audience = product.Audience?.DisplayName,
                Rating = product.Rating,
                ReviewCount = product.ReviewCount,
                IsNew = product.IsNew,
                DiscountPercentage = product.DiscountPercentage,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsActive = product.IsActive,
                ProductSizes = product.ProductSizes.Select(ps => new ProductSizeDto
                {
                    Id = ps.Id,
                    Size = ps.UkSize,
                    Stock = ps.Stock,
                    Barcode = ps.Barcode,
                    Sku = ps.Sku
                }).ToList(),
                ProductImages = new List<ProductImageDto>(),
                ProductFeatures = product.ProductFeatures.Select(pf => new ProductFeatureDto()
                {
                    FeatureText = pf.FeatureText,
                    SortOrder = pf.SortOrder,
                    Id = pf.Id
                })
                .ToList()
            };
        }

        public async Task<AdminProductDto> CreateProductAsync(AdminProductDto productToAdd)
        {
            var product = new Product
            {
                Name = productToAdd.Name,
                Description = productToAdd.Description,
                Price = productToAdd.Price,
                BrandId = productToAdd.BrandId,
                AudienceId = productToAdd.AudienceId,
                DiscountPercentage = productToAdd.DiscountPercentage,
                CreatedAt = DateTime.Now,
                IsNew = productToAdd.IsNew,
                IsActive = productToAdd.IsActive
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (productToAdd.ProductFeatures != null)
            {
                foreach (var featureToAdd in productToAdd.ProductFeatures)
                {
                    await _context.ProductFeatures.AddAsync(new ProductFeature
                    {
                        SortOrder = featureToAdd.SortOrder,
                        FeatureText = featureToAdd.FeatureText,
                        ProductId = product.Id
                    });
                }
            }

            foreach (var productSize in productToAdd.ProductSizes)
            {
                _context.ProductSizes.Add(new ProductSize
                {
                    ProductId = product.Id,
                    UkSize = productSize.Size,
                    Stock = productSize.Stock,
                    Barcode = productSize.Barcode,
                    Sku = $"{product.BrandId}-{productSize.Size}-{productSize.Barcode}"
                });
            }

            await _context.SaveChangesAsync();

            var response = new AdminProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name,
                AudienceId = product.AudienceId,
                Audience = product.Audience?.DisplayName,
                Rating = product.Rating,
                ReviewCount = product.ReviewCount,
                IsNew = product.IsNew,
                DiscountPercentage = product.DiscountPercentage,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsActive = product.IsActive,
                ProductSizes = product.ProductSizes.Select(ps => new ProductSizeDto
                {
                    Id = ps.Id,
                    Size = ps.UkSize,
                    Stock = ps.Stock,
                    Barcode = ps.Barcode,
                    Sku = ps.Sku
                }).ToList(),
                ProductImages = new List<ProductImageDto>(),
                ProductFeatures = product.ProductFeatures.Select(pf => new ProductFeatureDto()
                {
                    FeatureText = pf.FeatureText,
                    SortOrder = pf.SortOrder,
                    Id = pf.Id
                })
                .ToList()
            };
            return response;
        }

        public async Task<bool> UpdateProductAsync(int productId, AdminProductDto productToUpdate)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.Name = productToUpdate.Name;
            product.IsActive = productToUpdate.IsActive;
            product.Description = productToUpdate.Description;
            product.Price = productToUpdate.Price;
            product.DiscountPercentage = productToUpdate.DiscountPercentage;
            product.UpdatedAt = DateTime.Now;
            product.BrandId = productToUpdate.BrandId;
            product.AudienceId = productToUpdate.AudienceId;

            await _context.ProductFeatures
                .Where(pf => pf.ProductId == productId)
                .ExecuteDeleteAsync();

            if (productToUpdate.ProductFeatures != null)
            {
                foreach (var featureToAdd in productToUpdate.ProductFeatures)
                {
                    await _context.ProductFeatures.AddAsync(new ProductFeature
                    {
                        SortOrder = featureToAdd.SortOrder,
                        FeatureText = featureToAdd.FeatureText,
                        ProductId = product.Id
                    });
                }
            }

            var existingSizes = await _context.ProductSizes
                .Where(ps => ps.ProductId == product.Id)
                .ToListAsync();

            if (productToUpdate.ProductSizes != null)
            {
                foreach (var incoming in productToUpdate.ProductSizes)
                {
                    var existing = existingSizes
                        .FirstOrDefault(s => s.UkSize == incoming.Size);

                    if (existing != null)
                    {
                        existing.Stock = incoming.Stock;
                    }
                    else
                    {
                        _context.ProductSizes.Add(new ProductSize
                        {
                            ProductId = product.Id,
                            UkSize = incoming.Size,
                            Stock = incoming.Stock,
                            Barcode = incoming.Barcode,
                            Sku = $"{productToUpdate.BrandId}-{incoming.Size}-{incoming.Barcode}"
                        });
                    }
                }

                var incomingSizes = productToUpdate.ProductSizes
                    .Select(s => s.Size)
                    .ToHashSet();

                var sizesToDelete = existingSizes
                    .Where(s => !incomingSizes.Contains(s.UkSize));

                _context.ProductSizes.RemoveRange(sizesToDelete);
            }

            product.OriginalPrice = productToUpdate.OriginalPrice;
            product.IsNew = productToUpdate.IsNew;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
