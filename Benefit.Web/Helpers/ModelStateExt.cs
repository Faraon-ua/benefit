using System.Linq;
using System.Web.Mvc;

namespace Benefit.Web.Helpers
{
    public static class ModelStateExt
    {
        public static string ModelStateErrors(this ModelStateDictionary ModelState)
        {
            return string.Join("<br/>", ModelState.Values.SelectMany(entry => entry.Errors).Select(entry=>entry.ErrorMessage));
        }
    }
}