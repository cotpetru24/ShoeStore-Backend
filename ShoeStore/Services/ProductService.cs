using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Product;
using ShoeStore.Dto;

namespace ShoeStore.Services
{
    public class ProductService
    {
        private readonly ShoeStoreContext _context;

        public ProductService(ShoeStoreContext injectedContext)
        {
            _context = injectedContext;
        }


        public async Task<GetProductsResponseDto> GetProductsAsync(GetProductsRequest request)
        {
            IQueryable<Product> products = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Audience)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductFeatures)
                .Include(p => p.ProductSizes);

            products = products.Where(p => p.IsActive == true);

            if (request.Audience != null)
                products = products.Where(p => p.Audience != null && p.AudienceId == request.Audience);

            if (!string.IsNullOrEmpty(request.Brand))
                products = products.Where(p => p.Brand != null && p.Brand.Name == request.Brand);

            if (request.MinPrice.HasValue)
                products = products.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                products = products.Where(p => p.Price <= request.MaxPrice.Value);

            if (request.Size.HasValue)
                products = products.Where(p => p.ProductSizes.Any(ps => ps.UkSize == request.Size.Value));

            if (!string.IsNullOrEmpty(request.SearchTerm))
                products = products.Where(p =>
                EF.Functions.ILike(p.Name, $"%{request.SearchTerm}%") ||
                EF.Functions.ILike(p.Brand.Name, $"%{request.SearchTerm}%"));

            switch (request.SortBy)
            {

                case ProductSortByOption.NameAsc:
                    products = products.OrderBy(p => p.Name);
                    break;

                case ProductSortByOption.NameDesc:
                    products = products.OrderByDescending(p => p.Name);
                    break;

                case ProductSortByOption.PriceAsc:
                    products = products.OrderBy(p => p.Price);
                    break;

                case ProductSortByOption.PriceDesc:
                    products = products.OrderByDescending(p => p.Price);
                    break;

                case ProductSortByOption.BrandAsc:
                    products = products.OrderBy(p => p.Brand.Name);
                    break;

                case ProductSortByOption.BrandDesc:
                    products = products.OrderByDescending(p => p.Brand.Name);
                    break;

                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            request.Page = request.Page < 1 ? 1 : request.Page;
            request.PageSize = request.PageSize < 1 ? 30 : request.PageSize;

            products = products
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            var brands = await _context.Brands
                .Select(b => b.Name)
                .OrderBy(Name => Name)
                .ToArrayAsync();

            var result = await products
                .Select(p => new ProductDto()
                {
                    Audience = p.AudienceId,
                    BrandName = p.Brand.Name,
                    IsActive = p.IsActive,
                    DiscountPercentage = p.DiscountPercentage,
                    Id = p.Id,
                    Description = p.Description,
                    IsNew = p.IsNew,
                    Name = p.Name,
                    Price = p.Price,
                    Rating = p.Rating,
                    ReviewCount = p.ReviewCount,

                    ProductImages = p.ProductImages
                        .OrderBy(img => img.SortOrder)
                        .Select(img => new ProductImageDto()
                        {
                            Id = img.Id,
                            ImagePath = img.ImagePath,
                            IsPrimary = img.IsPrimary,
                            SortOrder = img.SortOrder,
                            ProductId = img.ProductId
                        })
                        .ToList(),

                    ProductSizes = p.ProductSizes
                        .OrderBy(size => size.UkSize)
                        .Select(size => new ProductSizeDto()
                        {
                            Id = size.Id,
                            Size = size.UkSize,
                            Stock = size.Stock,
                            Barcode = size.Barcode,
                            Sku = size.Sku
                        })
                        .ToList(),

                    ProductFeatures = p.ProductFeatures
                        .Select(feature => new ProductFeatureDto()
                        {
                            Id = feature.Id,
                            FeatureText = feature.FeatureText,
                            SortOrder = feature.SortOrder
                        })
                        .ToList()
                })
                .ToListAsync();

            GetProductsResponseDto response = new GetProductsResponseDto()
            {
                Products = result,
                Brands = brands
            };

            return response;
        }


        public async Task<GetProductsResponseDto> GetFeaturedProductsAsync()
        {
            var products = _context.Products
                .Where(p => p.IsActive == true && p.IsNew == true)
                .Include(p => p.Brand)
                .Include(p => p.Audience)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductFeatures)
                .Include(p => p.ProductSizes)
                .Take(3);

            var brands = await _context.Brands
                .Select(b => b.Name)
                .OrderBy(Name => Name)
                .ToArrayAsync();

            var result = await products
                .Select(p => new ProductDto()
                {
                    Audience = p.AudienceId,
                    BrandName = p.Brand.Name,
                    IsActive = p.IsActive,
                    DiscountPercentage = p.DiscountPercentage,
                    Id = p.Id,
                    Description = p.Description,
                    IsNew = p.IsNew,
                    Name = p.Name,
                    Price = p.Price,
                    Rating = p.Rating,
                    ReviewCount = p.ReviewCount,

                    ProductImages = p.ProductImages
                        .OrderBy(img => img.SortOrder)
                        .Select(img => new ProductImageDto()
                        {
                            Id = img.Id,
                            ImagePath = img.ImagePath,
                            IsPrimary = img.IsPrimary,
                            SortOrder = img.SortOrder,
                            ProductId = img.ProductId
                        })
                        .ToList(),

                    ProductSizes = p.ProductSizes
                        .OrderBy(size => size.UkSize)
                        .Select(size => new ProductSizeDto()
                        {
                            Id = size.Id,
                            Size = size.UkSize,
                            Stock = size.Stock,
                            Barcode = size.Barcode,
                            Sku = size.Sku
                        })
                        .ToList(),

                    ProductFeatures = p.ProductFeatures
                        .Select(feature => new ProductFeatureDto()
                        {
                            Id = feature.Id,
                            FeatureText = feature.FeatureText,
                            SortOrder = feature.SortOrder

                        })
                        .ToList()
                })
                .ToListAsync();

            GetProductsResponseDto response = new GetProductsResponseDto()
            {
                Products = result,
                Brands = brands
            };

            return response;
        }


        public async Task<GetSingleProductResponseDto?> GetProductByIdAsync(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Audience)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductFeatures)
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return null;

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                IsActive = product.IsActive,
                IsNew = product.IsNew,
                DiscountPercentage = product.DiscountPercentage,
                Rating = product.Rating,
                ReviewCount = product.ReviewCount,
                BrandName = product.Brand.Name,
                Audience = product.AudienceId,

                ProductImages = product.ProductImages
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ProductImageDto
                    {
                        Id = i.Id,
                        ImagePath = i.ImagePath,
                        IsPrimary = i.IsPrimary,
                        SortOrder = i.SortOrder,
                        ProductId = i.ProductId
                    })
                    .ToList(),

                ProductSizes = product.ProductSizes
                    .OrderBy(s => s.UkSize)
                    .Select(s => new ProductSizeDto
                    {
                        Id = s.Id,
                        Size = s.UkSize,
                        Stock = s.Stock,
                        Barcode = s.Barcode,
                        Sku = s.Sku
                    })
                    .ToList(),

                ProductFeatures = product.ProductFeatures
                    .OrderBy(f => f.SortOrder)
                    .Select(f => new ProductFeatureDto
                    {
                        Id = f.Id,
                        FeatureText = f.FeatureText,
                        SortOrder = f.SortOrder
                    })
                    .ToList()
            };

            var relatedProducts = await _context.Products
                .Where(p =>
                    p.Id != productId &&
                    p.IsActive == true &&
                    p.AudienceId == product.AudienceId)
                .Include(p => p.Brand)
                .Include(p => p.Audience)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductFeatures)
                .Include(p => p.ProductSizes)
                .Take(3)
                .ToListAsync();

            var relatedProductDtos = relatedProducts
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    IsActive = p.IsActive,
                    IsNew = p.IsNew,
                    DiscountPercentage = p.DiscountPercentage,
                    Rating = p.Rating,
                    ReviewCount = p.ReviewCount,
                    BrandName = p.Brand.Name,
                    Audience = p.AudienceId,

                    ProductImages = p.ProductImages
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new ProductImageDto
                        {
                            Id = i.Id,
                            ImagePath = i.ImagePath,
                            IsPrimary = i.IsPrimary,
                            SortOrder = i.SortOrder,
                            ProductId = i.ProductId
                        })
                        .ToList(),

                    ProductSizes = p.ProductSizes
                        .OrderBy(s => s.UkSize)
                        .Select(s => new ProductSizeDto
                        {
                            Id = s.Id,
                            Size = s.UkSize,
                            Stock = s.Stock,
                            Barcode = s.Barcode,
                            Sku = s.Sku
                        })
                        .ToList(),

                    ProductFeatures = p.ProductFeatures
                        .OrderBy(f => f.SortOrder)
                        .Select(f => new ProductFeatureDto
                        {
                            Id = f.Id,
                            FeatureText = f.FeatureText,
                            SortOrder = f.SortOrder
                        })
                        .ToList()
                })
                .ToList();

            return new GetSingleProductResponseDto
            {
                Product = productDto,
                RelatedProducts = relatedProductDtos
            };
        }
    }
}
