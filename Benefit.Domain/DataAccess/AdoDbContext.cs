using System;
using System.Configuration;

namespace Benefit.Domain.DataAccess
{
    public class AdoDbContext
    {
        protected readonly string connectionString;
        public AdoDbContext()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
        protected T ConvertFromDBVal<T>(object obj)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return default(T);
            }
            else
            {
                return (T)obj;
            }
        }
    }
}
