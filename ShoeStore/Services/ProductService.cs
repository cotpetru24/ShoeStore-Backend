﻿using Microsoft.EntityFrameworkCore;
using ShoeStore.DataContext.PostgreSQL.Models;
using ShoeStore.Dto.Product;

namespace ShoeStore.Services
{
    public class ProductService
    {
        private readonly ShoeStoreContext _context;


        public ProductService(ShoeStoreContext injectedContext)
        {
            _context = injectedContext;
        }



        public async Task<List<ProductDto>> GetProductsAsync(GetProductsRequest request)
        {

            IQueryable<Product> products = _context.Products
                .Include(p=>p.BrandId)
                .Include(p=>p.Audience);


            if (!string.IsNullOrEmpty(request.Category))
                products = products.Where(p => p.Audience.Code.ToLower() == request.Category.ToLower());

            if (!string.IsNullOrEmpty(request.Brand))
                products = products.Where(p => p.Brand.Name == request.Brand);

            if (request.MinPrice.HasValue)
                products = products.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                products = products.Where(p => p.Price <= request.MaxPrice.Value);

            if (!string.IsNullOrEmpty(request.Search))
                products = products.Where(p => p.Name.Contains(request.Search));

            var result = await products
                .Select(pDto => new ProductDto())
                .ToListAsync();

            //to add here pagination

            return result;

        }



    }
}
