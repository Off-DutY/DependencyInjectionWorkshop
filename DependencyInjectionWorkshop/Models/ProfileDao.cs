using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfileDao
    {
        string GetPassword(string accountId);
    }

    public class ProfileDao : IProfileDao
    {
        public string GetPassword(string accountId)
        {
            using (var connection = new SqlConnection("my connection string"))
            {
                return SqlMapper.Query<string>(connection, "spGetUserPassword", new {Id = accountId},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }
        }
    }
}