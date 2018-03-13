using System.Collections.Generic;
using Benefit.DataTransfer.ViewModels.Base;
using Benefit.Domain.Models;

namespace Benefit.DataTransfer.ViewModels.NavigationEntities
{
    public class SellersViewModel : NavigationEntitiesViewModel<Seller>
    {
        public SellersViewModel()
        {
            Items = new List<Seller>();
        }
    }
}
