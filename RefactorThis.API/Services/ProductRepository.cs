using Microsoft.EntityFrameworkCore;
using RefactorThis.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RefactorThis.API.Services
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDBContext context;
        public ProductRepository(ProductDBContext context)
        {
            this.context = context;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            return await context.Product.OrderBy(p => p.Name).ToListAsync<Product>();            
        }

        public async Task<List<Product>> FindProductsByName(string name)
        {
            return await context.Product.Where(p => p.Name.Contains(name, System.StringComparison.OrdinalIgnoreCase)).OrderBy(p => p.Name).ToListAsync<Product>();
        }

        public async Task<Product> FindProductById(Guid Id)
        {
            return await context.Product.Where(p => p.Id.Equals(Id)).FirstOrDefaultAsync<Product>();
        }

        public void AddProduct(Product product)
        {
            context.Product.Add(product);
        }

        public async void RemoveProduct(Product product)
        {
            // Delete Options in the product
            var productOptions = await FindOptionsByProductId(product.Id);
            context.RemoveRange(productOptions);
            
            // Delete product
            context.Product.Remove(product);
        }

        public async Task<List<ProductOption>> FindOptionsByProductId(Guid productId)
        {
            return await context.ProductOption.Where(p => p.ProductId.Equals(productId)).OrderBy(p => p.Name).ToListAsync<ProductOption>();
        }

        public async Task<ProductOption> FindOptionByProductIdAndOptionId(Guid productId, Guid productOptionId)
        {
            return await context.ProductOption.Where(p => p.ProductId.Equals(productId) && p.Id.Equals(productOptionId)).FirstOrDefaultAsync<ProductOption>();
        }

        public void AddProductOption(ProductOption productOption)
        {
            context.ProductOption.Add(productOption);
        }

        public void RemoveProductOption(ProductOption productOption)
        {
            context.ProductOption.Remove(productOption);
        }

        public async Task<bool> Save()
        {
            return await context.SaveChangesAsync() > 0;
        }
    }
}