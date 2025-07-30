using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Product;

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



        public async Task<List<ProductDto>> GetProductsAsync(GetProductsRequest request)
        {

            IQueryable<Product> products = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Audience);


            if (!string.IsNullOrEmpty(request.Category))
                products = products.Where(p => p.Audience != null && EF.Functions.ILike(p.Audience.Code, request.Category));

            if (!string.IsNullOrEmpty(request.Brand))
                products = products.Where(p => p.Brand != null && p.Brand.Name == request.Brand);

            if (request.MinPrice.HasValue)
                products = products.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                products = products.Where(p => p.Price <= request.MaxPrice.Value);

            if (!string.IsNullOrEmpty(request.Search))
                products = products.Where(p => p.Name.Contains(request.Search));


            products = products.OrderByDescending(p => p.Brand.Name);

            request.Page = request.Page < 1 ? 1 : request.Page;
            request.PageSize = request.PageSize < 1 ? 30 : request.PageSize;

            products = products
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            var productEntities = await products.ToListAsync();

            var result = _mapper.Map<List<ProductDto>>(productEntities);

            return result;

        }



    }
}
