using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestProject.WebMVC.Models;

namespace TestProject.WebMVC.Services
{
    public class CsvService : ICsvService
    {
        #region Private Members

        private readonly ILogger<CsvService> _logger;

        #endregion Private Members

        #region Constructor

        public CsvService(ILogger<CsvService> logger)
        {
            _logger = logger;
        }

        #endregion Constructor

        #region Public Methods

        public object[] Read(IFormFile file)
        {
            try
            {
                using var reader = new StreamReader(file?.OpenReadStream());
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                return csv.GetRecords<object>().ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public void Write(IFormFile file, object data)
        {
            throw new NotImplementedException();
        }

        public async Task<dynamic> ReadAsync(IFormFile file, CancellationToken token = default)
        {
            try
            {
                using var reader = new StreamReader(file?.OpenReadStream());
                using CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                return await csv.GetRecordsAsync<dynamic>().ToArrayAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public Task WriteAsync(IFormFile file, object data, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}