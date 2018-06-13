using System.Collections.Generic;

namespace Benefit.Web.Models.ViewModels
{
    public class HierarchySelectViewModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public IEnumerable<HierarchySelectItem> Items { get; set; }
    }
}