using System.Collections.Generic;

namespace Benefit.Domain.Models.Search
{
    public class SearchResult
    {
        public SearchResult()
        {
            Products = new List<Product>();
            CurrentRegionSellers = new List<Seller>();
            Sellers = new List<Seller>();
        }
        public string Term { get; set; }
        public string SellerId { get; set; }
        public List<Product> Products { get; set; } 
        public List<Seller> CurrentRegionSellers { get; set; } 
        public List<Seller> Sellers { get; set; } 
    }
}
