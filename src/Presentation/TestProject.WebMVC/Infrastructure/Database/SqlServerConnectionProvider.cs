using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.WebMVC.Infrastructure.Database
{
    public class SqlServerConnectionProvider
    {
        private readonly string _connectionString;
        private readonly string _baseConnectionString;

        public SqlServerConnectionProvider(string baseConnectionString, string connectionString)
        {
            _baseConnectionString = baseConnectionString;
            _connectionString = connectionString;
        }

        public IDbConnection GetDbConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public IDbConnection GetBaseDbConnection()
        {
            return new SqlConnection(_baseConnectionString);
        }
    }
}