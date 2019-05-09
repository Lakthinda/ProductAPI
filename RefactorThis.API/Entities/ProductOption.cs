using System;
using System.Collections.Generic;

namespace RefactorThis.API.Entities
{
    public partial class ProductOption
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
