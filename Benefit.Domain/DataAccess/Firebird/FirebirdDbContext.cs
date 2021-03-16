using System;
using System.Collections.Generic;
using Benefit.Domain.Models.Firebird;
using FirebirdSql.Data.FirebirdClient;

namespace Benefit.Domain.DataAccess.Firebird
{
    public class FirebirdDbContext
    {
        protected readonly string connectionString;
        private const string getProductsSql = @"Select
												com.idkey as Id,
												gr.text as CategoryName,
												gr.idkey as CategoryId,
												com.text as Name,
												rem.saleprice as Price,
												rem.barcode as Barcode,
												Sum(Rem.quantity_rem) as Quantity
												from commodity com
												left join Remainder_quick rem on rem.idcommodity = com.idkey
												inner join commoditytree gr on gr.idkey = com.idcommoditytree
												group by 1,2,3,4,5,6";
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
        public List<FirebirdProduct> GetProducts(string connectionString)
        {
            var result = new List<FirebirdProduct>();
            using (var connection = new FbConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new FbCommand(getProductsSql, connection, transaction))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var values = new object[reader.FieldCount];
                                reader.GetValues(values);

                                var fbProduct = new FirebirdProduct()
                                {
                                    Id = ConvertFromDBVal<Int32>(values[0]).ToString(),
                                    CategoryName = ConvertFromDBVal<String>(values[1]),
                                    CategoryId = ConvertFromDBVal<Int32>(values[2]).ToString(),
                                    Name = ConvertFromDBVal<String>(values[3]),
                                    Price = ConvertFromDBVal<Decimal>(values[4]),
                                    Barcode = ConvertFromDBVal<String>(values[5]),
                                    Quantity = ConvertFromDBVal<Decimal>(values[6])
                                };
                                result.Add(fbProduct);

                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
