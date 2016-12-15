using System.Collections.Generic;

namespace Benefit.DataTransfer.ViewModels
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int Pages { get; set; }
        public int ActivePage { get; set; }
    }
}
