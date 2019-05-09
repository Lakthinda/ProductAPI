using RefactorThis.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RefactorThis.API.Services
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllProducts();
        Task<List<Product>> FindProductsByName(string name);
        Task<Product> FindProductById(Guid Id);
        void AddProduct(Product product);
        void RemoveProduct(Product product);
        Task<List<ProductOption>> FindOptionsByProductId(Guid productId);
        Task<ProductOption> FindOptionByProductIdAndOptionId(Guid productId, Guid productOptionId);
        void AddProductOption(ProductOption productOption);
        void RemoveProductOption(ProductOption productOption);
        Task<bool> Save();
    }
    
}
