using System.Collections.Generic;

namespace Benefit.Domain.Models.Search
{
    public class SearchResult
    {
        public string Term { get; set; }
        public List<Product> Products { get; set; } 
    }
}
