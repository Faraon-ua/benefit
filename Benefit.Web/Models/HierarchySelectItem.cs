using System.Web.Mvc;

namespace Benefit.Web.Models
{
    public class HierarchySelectItem : SelectListItem
    {
        public int Level { get; set; }
    }
}