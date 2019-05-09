using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RefactorThis.API.Entities;
using RefactorThis.API.Models;
using RefactorThis.API.Services;

namespace RefactorThis.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository repository;
        private readonly ILogger<ProductsController> logger;
        public ProductsController(IProductRepository repository,
                                  ILogger<ProductsController> logger)
        {
            this.repository = repository;
            this.logger = logger;
        }


        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] string name)
        {
            List<Product> products = null;
            if(name == null)
            {
                products = await repository.GetAllProducts();
            }
            else
            {
                products = await repository.FindProductsByName(name);
            }


            if (products == null)
            {
                logger.LogInformation($"Product with Name {name} not found.");
                return NotFound();
            }

            var productDtos = Mapper.Map<IEnumerable<ProductDto>>(products);

            var result = new ProductsDto()
            {
                Items = productDtos.ToList()
            };
            return Ok(result);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> FindProductsById(string Id)
        {
            Guid newGuid;
            var isGuid = Guid.TryParse(Id, out newGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid Id provided.");
            }

            var product = await repository.FindProductById(newGuid);
            
            if (product == null)
            {
                return NotFound();
            }

            var result = Mapper.Map<ProductDto>(product);
                        
            return Ok(result);
        }
        
        [HttpPost()]
        public async Task<IActionResult> CreateProduct([FromBody] ProductForCreationDto productForCreation)
        {

            if(productForCreation == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = Mapper.Map<Entities.Product>(productForCreation);
            product.Id = Guid.NewGuid();

            repository.AddProduct(product);
            if (!await repository.Save())
            {
                logger.LogCritical($"Error when saving new Product. Product.Name: {product.Name}");
                return StatusCode(500, $"Error when saving new Product. Product.Name: {product.Name}");
            }

            return NoContent();
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> UpdateProduct(string Id, [FromBody] ProductForUpdateDto productForUpdate)
        {
            Guid newGuid;
            var isGuid = Guid.TryParse(Id, out newGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid Id provided.");
            }

            if (productForUpdate == null)
            {
                return BadRequest();
            }

            var product = await repository.FindProductById(newGuid);

            if (product == null)
            {
                return NotFound();
            }

            Mapper.Map(productForUpdate, product);

            if (!await repository.Save())
            {
                logger.LogCritical($"Error when updating Product. Product.Id:{product.Id} | Product.Name: {product.Name}");
                return StatusCode(500, $"Error when updating Product. Product.Id:{product.Id} | Product.Name: {product.Name}");
            }

            return NoContent();
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteProduct(string Id)
        {
            Guid newGuid;
            var isGuid = Guid.TryParse(Id, out newGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid Id provided.");
            }

            var product = await repository.FindProductById(newGuid);

            if (product == null)
            {
                return NotFound();
            }

            repository.RemoveProduct(product);

            if (!await repository.Save())
            {
                logger.LogCritical($"Error when deleting Product. Product.Id:{product.Id} | Product.Name: {product.Name}");
                return StatusCode(500, $"Error when deleting Product. Product.Id:{product.Id} | Product.Name: {product.Name}");
            }

            return NoContent();
        }

        [HttpGet("{Id}/options")]
        public async Task<IActionResult> FindOptionsByProductId(string Id)
        {
            Guid newGuid;
            var isGuid = Guid.TryParse(Id, out newGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid Id provided.");
            }

            var product = await repository.FindProductById(newGuid);

            if (product == null)
            {
                return NotFound();
            }

            var productOptions = await repository.FindOptionsByProductId(newGuid);

            var productOptionsDto = Mapper.Map<IEnumerable<ProductOptionDto>>(productOptions);

            var result = new ProductOptionsDto
            {
                Items = productOptionsDto.ToList()
            };

            return Ok(result);
        }

        [HttpGet("{Id}/options/{optionId}")]
        public async Task<IActionResult> FindOptionByProductIdAndOptionId(string Id,string optionId)
        {
            Guid newGuid;
            var isGuid = Guid.TryParse(Id, out newGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid Id provided.");
            }

            Guid newOptionGuid;
            isGuid = Guid.TryParse(Id, out newOptionGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid optionId provided.");
            }
            var productOption = await repository.FindOptionByProductIdAndOptionId(newGuid,newOptionGuid);

            if (productOption == null)
            {
                return NotFound();
            }

            var result = Mapper.Map<ProductOptionDto>(productOption);

            return Ok(result);
        }

        [HttpPost("{Id}/options")]
        public async Task<IActionResult> CreateProductOption(string Id, [FromBody] ProductOptionForCreationDto productOptionForCreation)
        {
            Guid newGuid;
            var isGuid = Guid.TryParse(Id, out newGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid Id provided.");
            }

            if (productOptionForCreation == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await repository.FindProductById(newGuid);
            if(product == null)
            {
                return NotFound();
            }

            var productOption = Mapper.Map<Entities.ProductOption>(productOptionForCreation);
            productOption.Id = Guid.NewGuid();
            productOption.ProductId = newGuid;

            repository.AddProductOption(productOption);
            if (!await repository.Save())
            {
                logger.LogCritical($"Error when creating Product Option. Product.Id:{product.Id} | Product.Name: {product.Name} | Product.OptionName:{productOption.Name}");
                return StatusCode(500, $"Error when creating Product Option. Product.Id:{product.Id} | Product.Name: {product.Name} | Product.OptionName:{productOption.Name}");
            }

            return NoContent();
        }

        [HttpPut("{Id}/options/{optionId}")]
        public async Task<IActionResult> UpdateProductOption(string Id,string optionId, [FromBody] ProductOptionForUpdateDto productOptionForUpdate)
        {
            Guid newGuid;
            var isGuid = Guid.TryParse(Id, out newGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid Id provided.");
            }

            Guid newOptionGuid;
            isGuid = Guid.TryParse(optionId, out newOptionGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid optionId provided.");
            }

            if (productOptionForUpdate == null)
            {
                return BadRequest();
            }

            var product = await repository.FindProductById(newGuid);
            if (product == null)
            {
                return NotFound("Product not Found");
            }

            var productOption = await repository.FindOptionByProductIdAndOptionId(newGuid, newOptionGuid);
            if (productOption == null)
            {
                return NotFound("Product Option not Found");
            }

            Mapper.Map(productOptionForUpdate, productOption);

            if (!await repository.Save())
            {
                logger.LogCritical($"Error when updating Product Option. Product.Id:{product.Id} | Product.Name: {product.Name} | Product.OptionId:{productOption.Id} | Product.OptionName:{productOption.Name}");
                return StatusCode(500, $"Error when updating Product Option. Product.Id:{product.Id} | Product.Name: {product.Name} | Product.OptionId:{productOption.Id} | Product.OptionName:{productOption.Name}");
            }

            return NoContent();
        }

        [HttpDelete("{Id}/options/{optionId}")]
        public async Task<IActionResult> DeleteProductOption(string Id, string optionId)
        {
            Guid newGuid;
            var isGuid = Guid.TryParse(Id, out newGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid Id provided.");
            }

            Guid newOptionGuid;
            isGuid = Guid.TryParse(optionId, out newOptionGuid);
            if (!isGuid)
            {
                return BadRequest("Invalid optionId provided.");
            }

            var product = await repository.FindProductById(newGuid);
            if (product == null)
            {
                return NotFound("Product not Found");
            }

            var productOption = await repository.FindOptionByProductIdAndOptionId(newGuid, newOptionGuid);
            if (productOption == null)
            {
                return NotFound("Product Option not Found");
            }

            repository.RemoveProductOption(productOption);
            if (!await repository.Save())
            {
                logger.LogCritical($"Error when deleting Product Option. Product.Id:{product.Id} | Product.Name: {product.Name} | Product.OptionId:{productOption.Id} | Product.OptionName:{productOption.Name}");
                return StatusCode(500, $"Error when deleting Product Option. Product.Id:{product.Id} | Product.Name: {product.Name} | Product.OptionId:{productOption.Id} | Product.OptionName:{productOption.Name}");
            }

            return NoContent();
        }

    }
}
