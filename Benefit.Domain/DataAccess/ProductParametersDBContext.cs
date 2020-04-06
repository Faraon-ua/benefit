using Benefit.Common.Extensions;
using Benefit.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Benefit.Domain.DataAccess
{
    public class ProductParametersDBContext : AdoDbContext
    {
        private SqlCommand cmd;
        public ProductParametersDBContext(string connectionString)
        {
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
        }

        public List<ProductParameter> Get(string categoryId, string sellerId)
        {
            cmd.CommandText = string.Format(@"select distinct pp.*
            from ProductParameters pp
            join ProductParameterProducts ppp on ppp.ProductParameterId = pp.Id
            join Products p on p.Id = ppp.ProductId
            where (pp.CategoryId = '{0}' or pp.CategoryId in (Select Id from Categories where MappedParentCategoryId = pp.CategoryId)) and 
	            DisplayInFilters = 1 and
	            p.SellerId = '{1}'", categoryId, sellerId);

            List<ProductParameter> returnList = new List<ProductParameter>();
            var adapter = new SqlDataAdapter(cmd);
            var result = new DataSet();
            var valuesResult = new DataSet();
            adapter.Fill(result);

            var ppIds = result.Tables[0].AsEnumerable().Select(dataRow => string.Format("'{0}'", dataRow["Id"].ToString())).ToList();
            if (!ppIds.Any())
            {
                ppIds.Add("''");
            }
            cmd.CommandText = string.Format("Select * from ProductParameterValues where ProductParameterId in ({0})", string.Join(",", ppIds));
            adapter.Fill(valuesResult);
            foreach (DataRow drwRow in result.Tables[0].Rows)
            {
                var pp = drwRow.ToObject<ProductParameter>();
                pp.ProductParameterValues = valuesResult.Tables[0].AsEnumerable().Where(entry => entry["ProductParameterId"].ToString() == (String)drwRow["Id"]).Select(img =>
                          img.ToObject<ProductParameterValue>()).ToList();
                returnList.Add(pp);
            }
            return returnList;
        }
    }
}
