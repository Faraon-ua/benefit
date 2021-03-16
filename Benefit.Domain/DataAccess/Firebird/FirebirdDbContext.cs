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
												inner join commoditytree gr on gr.idkey= com.idcommoditytree
												group by 1,2,3,4,5,6
												having Sum(Rem.quantity_rem) > 0";

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
                                    Id = values[0].ToString(),
                                    CategoryName = values[1].ToString(),
                                    CategoryId = values[2].ToString(),
                                    Name = values[3].ToString(),
                                    Price = System.Convert.ToDouble(values[4]),
                                    Barcode = values[5].ToString(),
                                    Quantity = System.Convert.ToInt32(values[6])
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
