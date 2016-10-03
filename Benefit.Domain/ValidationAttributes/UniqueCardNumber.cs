using System.ComponentModel.DataAnnotations;
using System.Linq;
using Benefit.Domain.DataAccess;

namespace Benefit.Domain.ValidationAttributes
{
    public class UniqueCardNumber : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var strValue = value as string;
            using (var db = new ApplicationDbContext())
            {
                return !db.Users.Any(entry => entry.CardNumber == strValue);
            }
        }
    }
}
