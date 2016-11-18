using Benefit.Domain.DataAccess;
using Benefit.Domain.Models;

namespace Benefit.Services.Domain
{
    public class OrderService
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public Order GetCartOrder()
        {
            return null;
        }
    }
}
