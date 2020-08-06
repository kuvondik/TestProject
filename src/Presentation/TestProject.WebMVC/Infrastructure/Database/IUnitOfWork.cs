using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using TestProject.WebMVC.Models;

namespace TestProject.WebMVC.Infrastructure.Database
{
    public interface IUnitOfWork
    {
        string DatabaseName { get; set; }

        int CreateTable(string tableName = DbDefaults.DefaultTableName, params string[] columnNames);

        IEnumerable<string> GetColumnNames(string tableName);

        IEnumerable<TableModel> GetAllTables(string datbaseName = DbDefaults.DatabaseName);

        TableModel GetFirstTable(string datbaseName = DbDefaults.DatabaseName);

        /// <summary>
        /// Removes a table permanently
        /// </summary>
        /// <param name="datbaseName"></param>
        /// <returns></returns>
        /// <remarks> Just for to be comfortable with unwanted tables </remarks>
        int RemoveTable(string tableName, string schema = DbDefaults.DefaultDatabaseSchema);

        int Count(string tableName, string schema = DbDefaults.DefaultDatabaseSchema);

        IEnumerable<dynamic> GetAll(string tableName, string schema = DbDefaults.DefaultDatabaseSchema);

        IEnumerable<dynamic> Filter(string tableName, dynamic dataFilter, string schema = DbDefaults.DefaultDatabaseSchema);

        dynamic GetById<TKey>(string tableName, TKey id, string schema = DbDefaults.DefaultDatabaseSchema);

        dynamic Insert(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema);

        int InsertMany(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema);

        dynamic Update(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema);

        int Remove<TKey>(string tableName, TKey id, string schema = DbDefaults.DefaultDatabaseSchema);

        #region TODOs

        dynamic Query(string sql);

        void Execute(string sql);

        dynamic QueryFirstOrDefault(string sql);

        public dynamic QuerySingleOrDefault(string sql);

        #endregion TODOs

        void Commit(IDbTransaction transaction);

        bool EnsureDatabase(string databaseName);

        Task<int> CreateTableAsync(string tableName = DbDefaults.DefaultTableName, CancellationToken token = default, params string[] columnNames);

        Task<IEnumerable<string>> GetColumnNamesAsync(string tableName, CancellationToken token = default);

        Task<int> CountAsync(string tableName, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default);

        Task<IEnumerable<TableModel>> GetAllTablesAsync(string datbaseName = DbDefaults.DatabaseName, CancellationToken token = default);

        Task<IEnumerable<dynamic>> GetAllAsync(string tableName, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default);

        Task<TableModel> GetFirstTableAsync(string datbaseName = DbDefaults.DatabaseName, CancellationToken token = default);

        /// <summary>
        /// Removes a table (asynchronously) permanently
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="schema"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <remarks> Just for to be comfortable with unwanted tables </remarks>
        Task<int> RemoveTableAsync(string tableName, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default);

        Task<IEnumerable<dynamic>> FilterAsync(string tableName, dynamic dataFilter, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default);

        Task<dynamic> GetByIdAsync<TKey>(string tableName, TKey id, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default);

        Task<dynamic> InsertAsync(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default);

        Task<int> InsertManyAsync(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default);

        Task<dynamic> UpdateAsync(string tableName, dynamic data, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default);

        Task<int> RemoveAsync<TKey>(string tableName, TKey id, string schema = DbDefaults.DefaultDatabaseSchema, CancellationToken token = default);

        Task<bool> EnsureDatabaseAsyc(string databaseName, CancellationToken token = default);
    }
}