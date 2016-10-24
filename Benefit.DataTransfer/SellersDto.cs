using System.Collections.Generic;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer
{
    public class SellersDto
    {
        public List<Seller> Items { get; set; }
        public Category Category { get; set; }
        public List<Category> Breadcrumbs { get; set; }
    }
}
