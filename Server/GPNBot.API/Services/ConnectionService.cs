using System.Data;

using MySql.Data.MySqlClient;

using GPNBot.API.Models;
using Microsoft.Extensions.Options;

namespace GPNBot.API.Services
{
    public interface IConnectionService
    {
        IDbConnection DataBase();
    }

    public class ConnectionService : IConnectionService
    {
        private readonly string connectionString;

        public ConnectionService(DBLogParams dbLogParams)
        {
            connectionString = dbLogParams.ConnectionString;
        }
        public ConnectionService(DBPIParams dbPIParams)
        {
            connectionString = dbPIParams.ConnectionString;
        }

        public ConnectionService(IOptions<DBServerParams> options)
        {
            connectionString = options.Value.ConnectionString;
        }

        public IDbConnection DataBase()
        {
            return new MySqlConnection(connectionString);
        }
    }
}