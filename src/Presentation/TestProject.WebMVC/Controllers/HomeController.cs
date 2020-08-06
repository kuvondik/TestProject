using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TestProject.WebMVC.HelperMethods;
using TestProject.WebMVC.Infrastructure.Database;
using TestProject.WebMVC.Models;
using TestProject.WebMVC.Services;
using TestProject.WebMVC.ViewModels;

namespace TestProject.WebMVC.Controllers
{
    public class HomeController : Controller
    {
        #region Private Members

        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICsvService _csvService;

        #endregion Private Members

        #region Constructor

        public HomeController(ILogger<HomeController> logger,
            IUnitOfWork unitOfWork,
            ICsvService csvService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _csvService = csvService;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Brings to the home index page
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Loads data from uploaded file (to the specified table)
        /// </summary>
        /// <param name="model"></param>
        /// <returns>JSON data</returns>
        [HttpPost]
        public async Task<IActionResult> LoadFile(HomeViewModel model)
        {
            try
            {
                if (model == null || model.File == null)
                    return null;

                var tableName = model.TableName;
                var records = await _csvService.ReadAsync(model.File).ConfigureAwait(false);

                // Column names
                List<string> columnNames;

                // If a table isn't selected, create new one...
                if (string.IsNullOrEmpty(tableName))
                {
                    columnNames = DynamicObjectHelpers.GetPropertyNames(records[0]);

                    tableName = DbDefaults.DefaultTableName + "_"
                        + model.File.FileName.Replace(".csv", "", StringComparison.Ordinal).Replace(" ", "", StringComparison.Ordinal) + "_"
                        + DateTime.Now.ToString("yyyyMMddHHmmssffff", new CultureInfo("en-UK"));

                    await _unitOfWork.CreateTableAsync(tableName, columnNames: columnNames.ToArray()).ConfigureAwait(false);
                }
                else
                    columnNames = _unitOfWork.GetColumnNames(tableName).ToList();

                // Insert data into the table
                var newRecordsCount = _unitOfWork.InsertMany(tableName, records);
                var data = _unitOfWork.GetAll(tableName).ToList();

                return Json(new
                {
                    ColumnNames = columnNames,
                    Results = data,
                    TotalCount = data.Count,//await _unitOfWork.CountAsync(tableName).ConfigureAwait(false),
                    NewRecordsCount = newRecordsCount,
                    TableName = tableName,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            //return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Gets all data
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAllData(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return Json(new Response
                {
                    Success = false,
                    Message = "Table name can't be null or empty!"
                });
            try
            {
                var data = _unitOfWork.GetAll(tableName).ToList();

                // Column  names
                var columnNames = _unitOfWork.GetColumnNames(tableName).ToList();

                return Json(new
                {
                    Success = true,
                    ColumnNames = columnNames,
                    Results = data,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new Response
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        #region JsGrid controller functions

        /// <summary>
        /// Gets filtered data (jsgrid: loadData)
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFilteredData([FromQuery] string tableName, [FromQuery(Name = "columnNames[]")] List<string> columnNames, [FromQuery(Name = "dataFilter[]")] List<string> dataFilter)
        {
            if (string.IsNullOrEmpty(tableName))
                return Json(new Response
                {
                    Success = false,
                    Message = "Table name can't be null!"
                });
            if (!columnNames.Any())
                return Json(new Response
                {
                    Success = false,
                    Message = "Column names can't be null!"
                });
            //throw new ArgumentNullException(nameof(tableName));
            try
            {
                if (!dataFilter.Any())
                    return Json(new Response<List<dynamic>>
                    {
                        Success = true,
                        Data = (await _unitOfWork.GetAllAsync(tableName).ConfigureAwait(false)).ToList()
                    });

                // Convert data from List to dynamic
                var data = new ExpandoObject() as IDictionary<string, object>;

                // Make two objects common to use at the same time (especially in foreach)
                var columnNamesAndData = columnNames.Zip(dataFilter, (c, d) => new DataModel { ColumnName = c, Data = d });

                // Fill data
                foreach (var o in columnNamesAndData)
                    data.Add(o.ColumnName, o.Data);

                var filteredData = await _unitOfWork.FilterAsync(tableName, data).ConfigureAwait(false);
                return Json(new Response<List<dynamic>>
                {
                    Success = true,
                    Data = filteredData.ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new Response
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Inserts new data (jsgrid: insertItem)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> InsertData([FromBody] HomeViewModel model)
        {
            if (model == null ||
                string.IsNullOrEmpty(model.TableName) ||
                model.ColumnNames == null || !model.ColumnNames.Any() ||
                model.DataSet == null || !model.DataSet.Any())
            {
                return Json(new Response
                {
                    Success = false,
                    Message = "Passsed objects can't be null or empty!"
                });
            }

            try
            {
                // Convert data from List to dynamic for Insert command
                var data = new ExpandoObject() as IDictionary<string, object>;
                var columnNamesAndData = model.DataSet[0].Zip(model.ColumnNames, (d, c) => new { ColumnName = c, Data = d });

                foreach (var o in columnNamesAndData)
                    data.Add(o.ColumnName, o.Data);

                // Insert data
                var insertedData = await _unitOfWork.InsertAsync(model.TableName, data).ConfigureAwait(false);

                return Json(new
                {
                    Success = true,
                    Data = insertedData,
                    TotalCount = await _unitOfWork.CountAsync(model.TableName).ConfigureAwait(false)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new Response
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes data (jsgrid: deleteItem)
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteData(string tableName, string id)
        {
            try
            {
                // Remove data
                await _unitOfWork.RemoveAsync(tableName, id).ConfigureAwait(false);

                return Json(new
                {
                    Success = true,
                    TotalCount = await _unitOfWork.CountAsync(tableName).ConfigureAwait(false)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new Response
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates data (jsgrid: updateItem)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateData([FromBody] HomeViewModel model)
        {
            if (model == null)
                return Json(new { Success = false, Message = "Passsed objects can't be null or empty!" });
            try
            {
                // Convert data from List to dynamic for Insert command
                var data = new ExpandoObject() as IDictionary<string, object>;

                var columnNamesAndData = model.DataSet.First().Zip(model.ColumnNames, (d, c) => new { ColumnName = c, Data = d });

                foreach (var o in columnNamesAndData)
                    data.Add(o.ColumnName, o.Data);

                // Update data
                var d = await _unitOfWork.UpdateAsync(model.TableName, data).ConfigureAwait(false);
                return Json(new Response<dynamic>
                {
                    Success = true,
                    Data = d
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new Response
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }

        #endregion JsGrid controller functions

        /// <summary>
        /// Get all tables list for select
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAllTables()
        {
            try
            {
                var tables = _unitOfWork.GetAllTables()
                    .OrderBy(t => t.TABLE_NAME)
                    .ToList();

                return Json(new Response<List<TableModel>>
                {
                    Success = true,
                    Data = tables
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new Response<List<TableModel>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets first table for preselected option in select
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFirstTableAsync()
        {
            var table = await _unitOfWork.GetFirstTableAsync().ConfigureAwait(false);

            if (table != null)
                return Json(new Response<TableModel>
                {
                    Success = true,
                    Data = table,
                });

            return Json(new Response
            {
                Success = false,
                Message = "No table is found!",
            });
        }

        /// <summary>
        /// Deletes table permanently
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        /// <remarks>Just for being comfortable with unwanted tables</remarks>
        [HttpDelete]
        public async Task<IActionResult> DeleteTable(string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                return Json(new { Success = false, Message = "Table name can't be null or empty!" });

            await _unitOfWork.RemoveTableAsync(tableName).ConfigureAwait(false);

            return Json(new Response { Success = true });
        }

        /// <summary>
        /// Get paginated table list for select
        /// </summary>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetPaginatedTables(string search, int page, int pageSize = 4)
        {
            try
            {
                var tables = _unitOfWork.GetAllTables();

                var totalTables = tables.LongCount();

                var tablesOnPage = tables
                   .Where(t => t.TABLE_NAME.Contains(search ?? "", StringComparison.OrdinalIgnoreCase))
                   .Skip(pageSize * page)
                   .Take(pageSize)
                   .OrderBy(t => t.TABLE_NAME)
                   .ToList();

                var totalTablesOnPage = tablesOnPage.LongCount();

                var paginatedList = new PaginatedList<TableModel>(tablesOnPage, totalTables, page, pageSize);

                return Json(new
                {
                    Results = paginatedList,
                    Count = totalTables,
                    more = paginatedList.HasNextPage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion Public Methods
    }
}