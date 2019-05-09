using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RefactorThis.API.Controllers;
using RefactorThis.API.Entities;
using RefactorThis.API.Models;
using RefactorThis.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RefactorThis.API.Test
{
    public class ProductControllerTest : IDisposable
    {
        private Mock<IProductRepository> repository;
        private Mock<ILogger<ProductsController>> logger;
        private ProductsController sut;

        public ProductControllerTest()
        {
            repository = new Mock<IProductRepository>();
            logger = new Mock<ILogger<ProductsController>>();
            sut = new ProductsController(repository.Object,
                                         logger.Object);

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entities.Product, Models.ProductDto>();
                cfg.CreateMap<Entities.ProductOption, Models.ProductOptionDto>();

                cfg.CreateMap<Models.ProductForCreationDto, Entities.Product>();
                cfg.CreateMap<Models.ProductForUpdateDto, Entities.Product>();
                cfg.CreateMap<Models.ProductOptionForCreationDto, Entities.ProductOption>();
                cfg.CreateMap<Models.ProductOptionForUpdateDto, Entities.ProductOption>();
            });
        }

        [Fact]
        public async void GetProducts_Without_Name_Returns_All_Products()
        {
            // Arrange            
            var productList = TestProductList;
            repository.Setup(r => r.GetAllProducts())
                      .ReturnsAsync(productList);

            // Act
            var result = await sut.GetProducts(null); // null - no parameter passed
            
            // Assert
            Assert.IsType<OkObjectResult>(result);
            repository.Verify(m => m.GetAllProducts(), Times.Once);
            repository.Verify(m => m.FindProductsByName(""), Times.Never);
            
            var okObjectResult = result as OkObjectResult;

            Assert.True(okObjectResult != null);
            var okResult = okObjectResult.Value as ProductsDto;
            
            Assert.True(okResult.Items.Count == 2);
            Assert.True(okResult.Items.First().Id == productList.First().Id);
        }

        [Fact]
        public async void GetProducts_With_Incorrect_Name_Value_Returns_Not_Found()
        {
            // Arrange            
            List<Product> nullProductList = null;
            repository.Setup(r => r.FindProductsByName(It.IsAny<string>()))
                      .ReturnsAsync(nullProductList);

            // Act
            var result = await sut.GetProducts("Any String"); // No data with this param value

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void GetProducts_With_Correct_Name_Value_Returns_Correct_Value()
        {
            // Arrange            
            var productList = TestProductList;
            repository.Setup(r => r.FindProductsByName(It.IsAny<string>()))
                      .ReturnsAsync(productList);

            // Act
            var result = await sut.GetProducts("Any String"); // No data with this param value

            // Assert
            Assert.IsType<OkObjectResult>(result);
            repository.Verify(m => m.GetAllProducts(), Times.Never);
            //repository.Verify(m => m.FindProductsByName(""), Times.Once);

            var okObjectResult = result as OkObjectResult;

            Assert.True(okObjectResult != null);
            var okResult = okObjectResult.Value as ProductsDto;

            Assert.True(okResult.Items.Count == 2);
            Assert.True(okResult.Items.First().Id == productList.First().Id);            
        }

        [Fact]
        public async void FindProductsById_With_Incorrect_Id_Returns_BadRequest()
        {
            // Arrange            

            // Act
            var result = await sut.FindProductsById("Wrong GUID");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async void FindProductsById_With_correct_Id_Returns_Product()
        {
            // Arrange            
            string guid = Guid.NewGuid().ToString();
            Product product = TestProduct;
            repository.Setup(r => r.FindProductById(It.IsAny<Guid>()))
                      .ReturnsAsync(product);

            // Act
            var result = await sut.FindProductsById(guid);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            //repository.Verify(m => m.FindProductById(Guid.NewGuid()), Times.Once);

            var okObjectResult = result as OkObjectResult;
            
            Assert.True(okObjectResult != null);
            var okResult = okObjectResult.Value as ProductDto;
            
            Assert.True(okResult.Id == product.Id);
        }

        [Fact]
        public async void CreateProduct_Incorrect_NewProductValue_Returns_BadRequest()
        {
            // Arrange            
            ProductForCreationDto productDto = null;

            // Act
            var result = await sut.CreateProduct(productDto);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async void CreateProduct_Correct_NewProductValue_problem_Saving_Returns_StatusCode500()
        {
            // Arrange            
            var product = TestProduct;
            ProductForCreationDto productDto = new ProductForCreationDto
            {
                Name = "Test Product",
                Description = "Test Product Description",
                Price = 10,
                DeliveryPrice = 5
            };
            repository.Setup(r => r.AddProduct(product));
            repository.Setup(r => r.Save())
                      .ReturnsAsync(false);

            // Act
            var result = await sut.CreateProduct(productDto);

            // Assert
            Assert.IsType<ObjectResult>(result);
        }

        [Fact]
        public async void CreateProduct_Correct_NewProductValue_Returns_No_Content()
        {
            // Arrange            
            var product = TestProduct;
            ProductForCreationDto productDto = new ProductForCreationDto
            {
                Name = "Test Product",
                Description = "Test Product Description",
                Price = 10,
                DeliveryPrice = 5
            };
            repository.Setup(r => r.AddProduct(product));
            repository.Setup(r => r.Save())
                      .ReturnsAsync(true);

            // Act
            var result = await sut.CreateProduct(productDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void UpdateProduct_UnAvailalbe_Product_Returns_Not_Found()
        {
            // Arrange            
            Product product = null;
            string guid = Guid.NewGuid().ToString();
            repository.Setup(r => r.FindProductById(It.IsAny<Guid>()))
                      .ReturnsAsync(product);
            ProductForUpdateDto productDto = new ProductForUpdateDto
            {
                Name = "Test Product",
                Description = "Test Product Description",
                Price = 10,
                DeliveryPrice = 5
            };

            // Act
            var result = await sut.UpdateProduct(guid, productDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async void UpdateProduct_Correct_Product_Returns_No_Content()
        {
            // Arrange            
            Product product = TestProduct;
            string guid = Guid.NewGuid().ToString();
            repository.Setup(r => r.FindProductById(It.IsAny<Guid>()))
                      .ReturnsAsync(product);
            repository.Setup(r => r.Save())
                      .ReturnsAsync(true);
            ProductForUpdateDto productDto = new ProductForUpdateDto
            {
                Name = "Test Product",
                Description = "Test Product Description",
                Price = 10,
                DeliveryPrice = 5
            };

            // Act
            var result = await sut.UpdateProduct(guid, productDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void DeleteProduct_CorrectID_Return_No_Content()
        {
            // Arrange
            Product product = TestProduct;
            string guid = Guid.NewGuid().ToString();
            repository.Setup(r => r.FindProductById(It.IsAny<Guid>()))
                      .ReturnsAsync(product);
            repository.Setup(r => r.Save())
                      .ReturnsAsync(true);

            // Act
            var result = await sut.DeleteProduct(guid);

            // Assert            
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void FindOptionByProductId_CorrectID_Return_Correct_Result()
        {
            // Arrange
            Product product = TestProduct;
            string guid = Guid.NewGuid().ToString();
            repository.Setup(r => r.FindProductById(It.IsAny<Guid>()))
                      .ReturnsAsync(product);

            List<ProductOption> productOptionList = TestProductOptionList;            
            repository.Setup(r => r.FindOptionsByProductId(It.IsAny<Guid>()))
                      .ReturnsAsync(productOptionList);


            // Act
            var result = await sut.FindOptionsByProductId(guid);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            var okObjectResult = result as OkObjectResult;

            Assert.True(okObjectResult != null);
            var okResult = okObjectResult.Value as ProductOptionsDto;

            Assert.True(okResult.Items.Count == 2);
            Assert.True(okResult.Items.First().Id == productOptionList.First().Id);
        }

        [Fact]
        public async void FindOptionByProductIdAndOptionId_CorrectID_And_OptionId_Return_Correct_Result()
        {
            // Arrange
            ProductOption productOption = TestProductOption;
            string guid = Guid.NewGuid().ToString();
            string guid2 = Guid.NewGuid().ToString();
            repository.Setup(r => r.FindOptionByProductIdAndOptionId(It.IsAny<Guid>(),It.IsAny<Guid>()))
                      .ReturnsAsync(productOption);
            

            // Act
            var result = await sut.FindOptionByProductIdAndOptionId(guid,guid2);
                        
            // Assert
            Assert.IsType<OkObjectResult>(result);            

            var okObjectResult = result as OkObjectResult;

            Assert.True(okObjectResult != null);
            var okResult = okObjectResult.Value as ProductOptionDto;

            Assert.True(okResult.Id == productOption.Id);
        }

        [Fact]
        public async void CreateProductOption_Correct_Product_And_Correct_ProductOptions_Returns_No_Content()
        {
            // Arrange            
            Product product = TestProduct;
            string guid = Guid.NewGuid().ToString();
            repository.Setup(r => r.FindProductById(It.IsAny<Guid>()))
                      .ReturnsAsync(product);
            repository.Setup(r => r.Save())
                      .ReturnsAsync(true);

            var productOptionCreation = new ProductOptionForCreationDto
            {
                Name = "New Product Option",
                Description = "New Product Description"
            };

            // Act
            var result = await sut.CreateProductOption(guid,productOptionCreation);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void UpdateProductOption_Correct_Product_And_Correct_ProductOptions_Returns_No_Content()
        {
            // Arrange            
            Product product = TestProduct;
            string guid = Guid.NewGuid().ToString();
            repository.Setup(r => r.FindProductById(It.IsAny<Guid>()))
                      .ReturnsAsync(product);
            repository.Setup(r => r.Save())
                      .ReturnsAsync(true);

            ProductOption productOption = TestProductOption;            
            string guid2 = Guid.NewGuid().ToString();
            repository.Setup(r => r.FindOptionByProductIdAndOptionId(It.IsAny<Guid>(), It.IsAny<Guid>()))
                      .ReturnsAsync(productOption);

            var productOptionUpdate = new ProductOptionForUpdateDto
            {
                Name = "Update Product Option",
                Description = "Update Product Description"
            };

            // Act
            var result = await sut.UpdateProductOption(guid,guid2, productOptionUpdate);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void DeleteProductOption_Correct_Product_And_Correct_ProductOptions_Returns_No_Content()
        {
            // Arrange            
            Product product = TestProduct;
            string guid = Guid.NewGuid().ToString();
            repository.Setup(r => r.FindProductById(It.IsAny<Guid>()))
                      .ReturnsAsync(product);
            repository.Setup(r => r.Save())
                      .ReturnsAsync(true);

            ProductOption productOption = TestProductOption;
            string guid2 = Guid.NewGuid().ToString();
            repository.Setup(r => r.FindOptionByProductIdAndOptionId(It.IsAny<Guid>(), It.IsAny<Guid>()))
                      .ReturnsAsync(productOption);
            
            // Act
            var result = await sut.DeleteProductOption(guid, guid2);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }


        #region TestData

        private Product TestProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Product Description",
            Price = 10,
            DeliveryPrice = 5
        };

        private ProductOption TestProductOption = new ProductOption
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Name = "Test Product Option",
            Description = "Test Product Option Description"            
        };

        private List<ProductOption> TestProductOptionList = new List<ProductOption>
        {
            new ProductOption
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Name = "Test Product Option 1",
                Description = "Test Product Option Description"
            },
           new ProductOption
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                Name = "Test Product Option 2",
                Description = "Test Product Option Description"
            }
        };

        private List<Product> TestProductList = new List<Product>
        {
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Test Product 1",
                Description = "Test Product Description",
                Price = 10,
                DeliveryPrice = 5
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Test Product 2",
                Description = "Test Product Description",
                Price = 20,
                DeliveryPrice = 5
            }
        };


        #endregion


        public void Dispose()
        {
            AutoMapper.Mapper.Reset();
        }
    }
}
