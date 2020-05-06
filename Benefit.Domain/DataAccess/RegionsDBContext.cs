using System.Data.SqlClient;

namespace Benefit.Domain.DataAccess
{
    public class RegionsDBContext : AdoDbContext
    {
        private SqlCommand cmd;
        public RegionsDBContext()
        {
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
        }
        public T GetMin<T>(string field)
        {
            cmd.CommandText = string.Format("SELECT MIN({0}) FROM Regions", field);
            cmd.Connection.Open();
            var result = (T)cmd.ExecuteScalar();
            cmd.Connection.Close();
            return result;
        }
    }
}
