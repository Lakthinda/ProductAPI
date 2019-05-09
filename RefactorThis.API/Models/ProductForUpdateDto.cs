using System;

namespace RefactorThis.API.Models
{
    public class ProductForUpdateDto
    {        
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal DeliveryPrice { get; set; }
    }
}