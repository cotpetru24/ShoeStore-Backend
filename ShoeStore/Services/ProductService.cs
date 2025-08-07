using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Product;
using ShoeStore.Dto;

namespace ShoeStore.Services
{
    public class ProductService
    {
        private readonly ShoeStoreContext _context;
        private readonly IMapper _mapper;


        public ProductService(ShoeStoreContext injectedContext, IMapper injectedMapper)
        {
            _context = injectedContext;
            _mapper = injectedMapper;
        }



        public async Task<GetProductsResposeDto> GetProductsAsync(GetProductsRequest request)
        {

            IQueryable<Product> products = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Audience);


            if (!string.IsNullOrEmpty(request.Audience))
                products = products.Where(p => p.Audience != null && EF.Functions.ILike(p.Audience.Code, request.Audience));

            if (!string.IsNullOrEmpty(request.Brand))
                products = products.Where(p => p.Brand != null && p.Brand.Name == request.Brand);

            if (request.MinPrice.HasValue)
                products = products.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                products = products.Where(p => p.Price <= request.MaxPrice.Value);

            if (!string.IsNullOrEmpty(request.SearchTerm))
                products = products.Where(p =>
                EF.Functions.ILike(p.Name, $"%{request.SearchTerm}%") ||
                EF.Functions.ILike(p.Brand.Name, $"%{request.SearchTerm}%"));


            switch (request.SortBy)
            {

                case SortByOption.NameAsc:
                    products = products.OrderBy(p => p.Name);
                    break;

                case SortByOption.NameDesc:
                    products = products.OrderByDescending(p => p.Name);
                    break;

                case SortByOption.PriceAsc:
                    products = products.OrderBy(p => p.Price);
                    break;

                case SortByOption.PriceDesc:
                    products = products.OrderByDescending(p => p.Price);
                    break;


                case SortByOption.BrandAsc:
                    products = products.OrderBy(p => p.Brand.Name);
                    break;

                case SortByOption.BrandDesc:
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

            var productEntities = await products.ToListAsync();

            var result = _mapper.Map<List<ProductDto>>(productEntities);

            GetProductsResposeDto response = new GetProductsResposeDto()
            {
                Products = result,
                Brands = brands
            };

            return response;

        }



        public async Task<ProductDto?> GetProductByIdAsync(int productId)
        {
            var product = await _context.Products
                .Where(p => p.Id == productId)
                .Include(p => p.Brand)
                .Include(p => p.Audience)
                .FirstOrDefaultAsync();

            if (product != null)
            {
                var response = _mapper.Map<ProductDto>(product);
                return response;
            }

            return null;
        }
    }
}
