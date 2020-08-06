using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TestProject.WebMVC.Models;

namespace TestProject.WebMVC.Services
{
    /// <summary>
    /// Csv file reader/writer contract
    /// </summary>
    public interface ICsvService
    {
        object[] Read(IFormFile file);

        void Write(IFormFile file, object data);

        Task<dynamic> ReadAsync(IFormFile file, CancellationToken token = default);

        Task WriteAsync(IFormFile file, object data, CancellationToken token = default);
    }
}