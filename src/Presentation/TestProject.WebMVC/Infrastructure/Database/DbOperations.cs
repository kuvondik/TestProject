using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestProject.WebMVC.HelperMethods;
using TestProject.WebMVC.Models;

namespace TestProject.WebMVC.Infrastructure.Database
{
    public class DbOperations : IUnitOfWork
    {
        #region Private Members

        private bool _disposed;
        private readonly ILogger<DbOperations> _logger;
        private readonly SqlServerConnectionProvider _connectionProvider;

        #endregion Private Members

        #region Construtor

        public DbOperations(ILogger<DbOperations> logger,
            SqlServerConnectionProvider connectionProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));

            EnsureDatabase(DbDefaults.DatabaseName);
        }

        #endregion Construtor

        public string DatabaseName { get; set; } = DbDefaults.DatabaseName;

        #region Public Methods

        #region Synchronous Methods

        public int CreateTable(string tableName = DbDefaults.DefaultTableName, params string[] columnNames)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                //Create a table
                var sql = @"CREATE TABLE " + tableName + @"([Id] UNIQUEIDENTIFIER PRIMARY KEY default NEWID() CLUSTRED,";

                for (int i = 0; i < columnNames.Length; i++)
                    sql += " [" + columnNames[i] + "] VARCHAR(MAX) NULL, ";
                sql += " ) ";

                return connection.Execute(sql);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public IEnumerable<string> GetColumnNames(string tableName)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();
                var sql = @"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + tableName + "'";

                return connection.Query<string>(sql);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public int Count(string tableName, string schema = DbDefaults.DefaultDatabaseSchema)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                var sql = "SELECT COUNT([Id]) FROM [" + schema + "].[" + tableName + "]";

                return connection.QuerySingle<int>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public IEnumerable<TableModel> GetAllTables(string datbaseName = DbDefaults.DatabaseName)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                return connection.Query<TableModel>("SELECT * FROM " + datbaseName + ".INFORMATION_SCHEMA.TABLES");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public TableModel GetFirstTable(string datbaseName = DbDefaults.DatabaseName)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                return connection.QuerySingleOrDefault<TableModel>("SELECT TOP 1 * FROM " + datbaseName + ".INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME ASC;");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Removes a table permanently
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// <remarks> Just for being comfortable with unwanted tables </remarks>
        public int RemoveTable(string tableName, string schema = DbDefaults.DefaultDatabaseSchema)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                var sql = "DROP TABLE [" + schema + "].[" + tableName + "];";

                return connection.Execute(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public IEnumerable<dynamic> GetAll(string tableName, string schema = DbDefaults.DefaultDatabaseSchema)
        {
            if (string.IsNullOrEmpty(tableName))
                return Array.Empty<object>();

            //throw new ArgumentNullException(nameof(tableName));
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                var sql = "SELECT * FROM [" + schema + "].[" + tableName + "]";
                return connection.Query<dynamic>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public IEnumerable<dynamic> Filter(string tableName, dynamic dataFilter, string schema = DbDefaults.DefaultDatabaseSchema)
        {
            if (string.IsNullOrEmpty(tableName))
                return Array.Empty<object>();

            //throw new ArgumentNullException(nameof(tableName));
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                List<string> columnNames = DynamicObjectHelpers.GetPropertyNames(dataFilter);
                List<string> _dataFilter = DynamicObjectHelpers.GetPropertyValues(dataFilter);

                var colNamesAndDataSet = columnNames.Zip(_dataFilter, (col, d) => new { ColumnName = col, Data = d });

                var sql = "SELECT * FROM [" + schema + "].[" + tableName + "] WHERE ";

                foreach (var o in colNamesAndDataSet)
                    sql += "[" + o.ColumnName + "] LIKE '%" + o.Data + "%' AND ";

                sql = sql.Remove(sql.LastIndexOf("AND", StringComparison.OrdinalIgnoreCase)).Trim() + ";";

                return connection.Query<dynamic>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public dynamic GetById<TKey>(string tableName, TKey id, string schema = DbDefaults.DefaultDatabaseSchema)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name can't be null or empty!", nameof(tableName));
            if (id is string idString && string.IsNullOrEmpty(idString))
                throw new ArgumentException("Id can't be empty!", nameof(id));
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                string sql = "SELECT * FROM [" + schema + "].[" + tableName + "] WHERE [Id]='" + id + "'";

                return connection.QuerySingleOrDefault(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public dynamic Insert(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name can't be null or empty!", nameof(tableName));

            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                List<string> columnNames = DynamicObjectHelpers.GetPropertyNames(data);
                List<string> _data = DynamicObjectHelpers.GetPropertyValues(data);

                var id = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "");

                if (!columnNames.Contains("Id"))
                {
                    columnNames.Insert(0, "Id");
                    _data.Insert(0, "CAST('" + id + "' as UNIQUEIDENTIFIER)");
                }

                var sql = @"INSERT INTO [" + schema + "].[" + tableName + "] (";

                // Add column names
                foreach (var columnName in columnNames)
                    sql += " [" + columnName + "], ";

                sql = sql.Remove(sql.LastIndexOf(',')).Trim();
                sql += " ) VALUES ( ";

                // Cast string to UID
                sql += "CAST('" + id + "' as UNIQUEIDENTIFIER), ";
                // Add values
                foreach (var d in _data.Skip(1))
                    sql += " '" + d + "', ";

                sql = sql.Remove(sql.LastIndexOf(',')).Trim();
                sql += " );";

                connection.Execute(sql);

                var sqlQuery = "SELECT * FROM [" + schema + "].[" + tableName + "] WHERE [Id]='" + id + "';"; ;

                return connection.QuerySingle(sqlQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public int InsertMany(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name can't be null or empty!", nameof(tableName));

            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                // Dynamic rows collection
                object[] expandoObjectList = (object[])data;

                // Column  names
                List<string> columnNames = DynamicObjectHelpers.GetPropertyNames(data[0]);
                // Typed rows collection
                List<List<string>> dataSet = new List<List<string>>();

                // Map from a dynamic to typed variable
                foreach (ExpandoObject o in expandoObjectList)
                    dataSet.Add(DynamicObjectHelpers.GetPropertyValues(o));

                List<string> sqls = new List<string>();

                StringBuilder scriptBuilder = new StringBuilder();

                // Read dataSet and create InsertMany sql script
                foreach (var d in dataSet)
                {
                    var sql = @"INSERT INTO [" + schema + "].[" + tableName + "] (";

                    // Add column names
                    foreach (var columnName in columnNames)
                        sql += " [" + columnName + "], ";

                    sql = sql.Remove(sql.LastIndexOf(',')).Trim();
                    sql += " ) VALUES ( ";

                    // Add values
                    foreach (var o in d)
                        sql += " '" + o + "', ";

                    sql = sql.Remove(sql.LastIndexOf(',')).Trim();
                    sql += " ); ";

                    scriptBuilder.Append(sql);
                }

                var a = scriptBuilder.ToString();

                return connection.Execute(scriptBuilder.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public dynamic Update(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name can't be null or empty!", nameof(tableName));

            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                List<string> columnNames = DynamicObjectHelpers.GetPropertyNames(data);
                List<string> _data = DynamicObjectHelpers.GetPropertyValues(data);

                var sql = "UPDATE [" + schema + "].[" + tableName + "] SET ";
                var colNamesAndDataSet = columnNames.Zip(_data, (col, d) => new { ColumnName = col, Data = d });

                // We don't need Id, so skip the first iteration
                foreach (var colNameAndData in colNamesAndDataSet.Skip(1))
                {
                    sql += "[" + colNameAndData.ColumnName + "]='" + colNameAndData.Data + "',";
                }

                sql = sql.Remove(sql.LastIndexOf(',')).Trim();
                sql += " WHERE [Id]='" + _data[0] + "';";

                string sqlQuery = "SELECT * FROM [" + schema + "].[" + tableName + "] WHERE [Id]='" + _data + "'";

                var result = connection.Execute(sql);

                return connection.QuerySingleOrDefault(sqlQuery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public int Remove<TKey>(string tableName, TKey id, string schema = DbDefaults.DefaultDatabaseSchema)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name can't be null or empty!", nameof(tableName));
            if (id is string idString && string.IsNullOrEmpty(idString))
                throw new ArgumentException("Id can't be empty!", nameof(id));
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                var sql = "DELETE FROM [" + schema + "].[" + tableName + "] WHERE [Id] = '" + id + "';";

                return connection.Execute(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        #region TODOs

        public void Execute(string sql)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();
                // return _connection.Query<dynamic>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public dynamic Query(string sql)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                return connection.Query<dynamic>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public dynamic QueryFirstOrDefault(string sql)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                return connection.QueryFirstOrDefault<dynamic>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public dynamic QuerySingleOrDefault(string sql)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                return connection.QuerySingleOrDefault<dynamic>(sql);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        #endregion TODOs

        public void Commit(IDbTransaction transaction)
        {
            try
            {
                transaction?.Commit();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "");
                transaction?.Rollback();
            }
        }

        public bool EnsureDatabase(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentException("Database name can't be null or empty!", nameof(databaseName));
            try
            {
                using var connection = _connectionProvider.GetBaseDbConnection();
                connection.Open();

                var sql = @"IF (NOT EXISTS (SELECT name FROM master.dbo.sysdatabases
                    WHERE ('[' + name + ']' ='" + databaseName + @"'
                    OR name = '" + databaseName + @"')))
                    CREATE DATABASE " + databaseName + ";";

                var result = connection.Execute(sql);

                return true;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }

        #endregion Synchronous Methods

        #region Asynchronous Methods

        public async Task<int> CreateTableAsync(string tableName = DbDefaults.DefaultTableName, CancellationToken token = default, params string[] columnNames)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                //Create a table
                var sql = @"CREATE TABLE " + tableName + @"([Id] UNIQUEIDENTIFIER PRIMARY KEY CLUSTERED default NEWID(), ";

                for (int i = 0; i < columnNames.Length; i++)
                    sql += " [" + columnNames[i] + "] VARCHAR(MAX) NULL, ";
                sql += " ) ";

                return await connection.ExecuteAsync(sql, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetColumnNamesAsync(string tableName, CancellationToken token = default)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                var sql = @"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + tableName + "'";

                return await connection.QueryAsync<string>(sql, token).ConfigureAwait(false);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<int> CountAsync(string tableName, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                var sql = "SELECT COUNT([Id]) FROM [" + schema + "].[" + tableName + "]";

                return await connection.QuerySingleAsync<int>(sql, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<TableModel>> GetAllTablesAsync(string datbaseName = DbDefaults.DatabaseName, CancellationToken token = default)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                return await connection.QueryAsync<TableModel>("SELECT * FROM " + datbaseName + ".INFORMATION_SCHEMA.TABLES", token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<TableModel> GetFirstTableAsync(string datbaseName = DbDefaults.DatabaseName, CancellationToken token = default)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                return await connection.QuerySingleOrDefaultAsync<TableModel>("SELECT TOP 1 * FROM " + datbaseName + ".INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME ASC;", token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Removes a table (asynchronously) permanently
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <remarks> Just for being comfortable with unwanted tables </remarks>
        public async Task<int> RemoveTableAsync(string tableName, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default)
        {
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                var sql = "DROP TABLE [" + schema + "].[" + tableName + "];";

                return await connection.ExecuteAsync(sql).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<dynamic>> GetAllAsync(string tableName, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                var sql = "SELECT * FROM [" + schema + "].[" + tableName + "]";
                return await connection.QueryAsync<dynamic>(sql, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<dynamic>> FilterAsync(string tableName, dynamic dataFilter, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(tableName))
                return Array.Empty<object>();

            //throw new ArgumentNullException(nameof(tableName));
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                List<string> columnNames = DynamicObjectHelpers.GetPropertyNames(dataFilter);
                List<string> _dataFilter = DynamicObjectHelpers.GetPropertyValues(dataFilter);

                var colNamesAndDataSet = columnNames.Zip(_dataFilter, (col, d) => new { ColumnName = col, Data = d });

                var sql = "SELECT * FROM [" + schema + "].[" + tableName + "] WHERE ";

                foreach (var o in colNamesAndDataSet)
                    sql += "[" + o.ColumnName + "] LIKE '%" + o.Data + "%' AND ";

                sql = sql.Remove(sql.LastIndexOf("AND", StringComparison.OrdinalIgnoreCase)).Trim() + ";";

                return await connection.QueryAsync<dynamic>(sql, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<dynamic> GetByIdAsync<TKey>(string tableName, TKey id, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name can't be null or empty!", nameof(tableName));
            if (id is string idString && string.IsNullOrEmpty(idString))
                throw new ArgumentException("Id can't be empty!", nameof(id));
            try
            {
                using var connection = _connectionProvider.GetDbConnection();

                string sql = "SELECT * FROM [" + schema + "].[" + tableName + "] WHERE [Id]='" + id + "'";

                connection.Open();
                return await connection.QuerySingleOrDefaultAsync(sql, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<dynamic> InsertAsync(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name can't be null or empty!", nameof(tableName));

            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                List<string> columnNames = DynamicObjectHelpers.GetPropertyNames(data);
                List<string> _data = DynamicObjectHelpers.GetPropertyValues(data);
                var id = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "");

                if (!columnNames.Contains("Id"))
                {
                    columnNames.Insert(0, "Id");
                    _data.Insert(0, "CAST('" + id + "' as UNIQUEIDENTIFIER)");
                }

                var sql = @"INSERT INTO [" + schema + "].[" + tableName + "] (";

                // Add column names
                foreach (var columnName in columnNames)
                    sql += " [" + columnName + "], ";

                sql = sql.Remove(sql.LastIndexOf(',')).Trim();
                sql += " ) VALUES ( ";

                sql += "CAST('" + id + "' as UNIQUEIDENTIFIER), ";
                // Add values
                foreach (var d in _data.Skip(1))
                    sql += " '" + d + "', ";

                sql = sql.Remove(sql.LastIndexOf(',')).Trim();
                sql += " );";

                await connection.ExecuteAsync(sql, token).ConfigureAwait(false);

                var sqlQuery = "SELECT * FROM [" + schema + "].[" + tableName + "] WHERE [Id] = '" + id + "'";

                return await connection.QuerySingleAsync<dynamic>(sqlQuery, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<int> InsertManyAsync(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name can't be null or empty!", nameof(tableName));

            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                // Dynamic rows collection
                object[] expandoObjectList = (object[])data;

                // Column  names
                List<string> columnNames = DynamicObjectHelpers.GetPropertyNames(data[0]);
                // Typed rows collection
                List<List<string>> dataSet = new List<List<string>>();

                // Map from a dynamic to typed variable
                foreach (ExpandoObject o in expandoObjectList)
                    dataSet.Add(DynamicObjectHelpers.GetPropertyValues(o));

                List<string> sqls = new List<string>();

                StringBuilder scriptBuilder = new StringBuilder();

                // Read dataSet and create InsertMany sql script
                foreach (var d in dataSet)
                {
                    var sql = @"INSERT INTO [" + schema + "].[" + tableName + "] (";

                    // Add column names
                    foreach (var columnName in columnNames)
                        sql += " [" + columnName + "], ";

                    sql = sql.Remove(sql.LastIndexOf(',')).Trim();
                    sql += " ) VALUES ( ";

                    // Add values
                    foreach (var o in d)
                        sql += " '" + o + "', ";

                    sql = sql.Remove(sql.LastIndexOf(',')).Trim();
                    sql += " ); ";

                    scriptBuilder.Append(sql);
                }

                var a = scriptBuilder.ToString();

                return await connection.ExecuteAsync(scriptBuilder.ToString(), token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<dynamic> UpdateAsync(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name can't be null or empty!", nameof(tableName));

            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();

                List<string> columnNames = DynamicObjectHelpers.GetPropertyNames(data);
                List<string> _data = DynamicObjectHelpers.GetPropertyValues(data);

                var sql = "UPDATE [" + schema + "].[" + tableName + "] SET ";
                var colNamesAndDataSet = columnNames.Zip(_data, (col, d) => new { ColumnName = col, Data = d });

                // We don't need Id, so skip the first iteration
                foreach (var colNameAndData in colNamesAndDataSet.Skip(1))
                {
                    sql += "[" + colNameAndData.ColumnName + "]='" + colNameAndData.Data + "',";
                }

                sql = sql.Remove(sql.LastIndexOf(',')).Trim();
                sql += " WHERE [Id]='" + _data[0] + "';";

                string sqlQuery = "SELECT * FROM [" + schema + "].[" + tableName + "] WHERE [Id]='" + _data[0] + "'";

                var result = await connection.ExecuteAsync(sql, token).ConfigureAwait(false);

                return await connection.QuerySingleOrDefaultAsync(sqlQuery, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<int> RemoveAsync<TKey>(string tableName, TKey id, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name can't be null or empty!", nameof(tableName));
            if (id is string idString && string.IsNullOrEmpty(idString))
                throw new ArgumentException("Id can't be empty!", nameof(id));

            try
            {
                using var connection = _connectionProvider.GetDbConnection();

                var sql = "DELETE FROM [" + schema + "].[" + tableName + "] WHERE [Id] = '" + id + "';";

                connection.Open();
                return await connection.ExecuteAsync(sql, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<bool> EnsureDatabaseAsyc(string databaseName, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentException("Database name can't be null or empty!", nameof(databaseName));
            try
            {
                using var connection = _connectionProvider.GetDbConnection();
                connection.Open();
                var sql = @"IF (NOT EXISTS (SELECT name FROM master.dbo.sysdatabases
                    WHERE ('[' + name + ']' ='" + databaseName + @"'
                    OR name = '" + databaseName + @"')))
                    CREATE DATABASE " + databaseName + ";";

                var result = await connection.ExecuteAsync(sql, token).ConfigureAwait(false);

                return true;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        #endregion Asynchronous Methods

        #endregion Public Methods
    }
}