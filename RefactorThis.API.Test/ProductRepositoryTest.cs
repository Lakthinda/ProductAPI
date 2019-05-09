using Microsoft.EntityFrameworkCore;
using RefactorThis.API.Entities;
using RefactorThis.API.Services;
using System;
using System.Linq;
using Xunit;

namespace RefactorThis.API.Test
{
    public class ProductRepositoryTest
    {
        private readonly ProductDBContext context;
        public ProductRepositoryTest()
        {
            var testConnection = @"Server=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Projects\GitHub\RefactorThis\RefactorThis\RefactorThis.API.Test\App_Data\Database.mdf;Integrated Security=True;";
            
            var optionsBuilder = new DbContextOptionsBuilder<ProductDBContext>();
            optionsBuilder.UseSqlServer(testConnection);

            context = new ProductDBContext(optionsBuilder.Options);
        }

        [Fact]
        public async void ProductRepository_GetAllProducts_Return_ProductList_From_TestDB()
        {
            // Arrange
            ProductRepository repository = new ProductRepository(context);

            // Act
            var products = await repository.GetAllProducts();

            // Assert
            Assert.True(products.ToList().Count() > 0);
        }
    }
}
