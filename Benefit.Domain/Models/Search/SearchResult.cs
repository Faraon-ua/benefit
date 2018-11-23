using System.Collections.Generic;

namespace Benefit.Domain.Models.Search
{
    public class RankedSqlResult
    {
        public string Id { get; set; }
        public int Rank { get; set; }
    }
    public class SearchResult
    {
        public SearchResult()
        {
            Products = new List<Product>();
            ProductParameters = new List<ProductParameter>();
            CurrentRegionSellers = new List<Seller>();
            Sellers = new List<Seller>();
        }
        public string Term { get; set; }
        public string SellerId { get; set; }
        public List<ProductParameter> ProductParameters { get; set; }
        public List<Product> Products { get; set; }
        public int PagesCount { get; set; }
        public List<Seller> CurrentRegionSellers { get; set; }
        public List<Seller> Sellers { get; set; }
    }
}
