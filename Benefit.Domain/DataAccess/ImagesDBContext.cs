using System.Data.SqlClient;

namespace Benefit.Domain.DataAccess
{
    public class ImagesDBContext : AdoDbContext
    {
        private SqlCommand cmd;
        public ImagesDBContext()
        {
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(connectionString);
        }
        public void BatchDeleteIn(string whereParams, params DeleteInModel[] list)
        {
            base.BatchDeleteIn("Images", whereParams, list);
        }
    }
}
