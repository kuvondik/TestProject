using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.Services
{
    /// <summary>
    /// Csv file reader/writer contract
    /// </summary>
    public interface ICsvService : IDisposable
    {
        Task<object> ReadAsync(string filePath);

        Task WriteAsync(string filePath, object data);

        object Read(string fullPath);

        void Write(string filePath, object data);

        Task<T> ReadAsync<T>(string filePath);

        Task WriteAsync<T>(string filePath, T data);

        T Read<T>(string fullPath);

        void Write<T>(string filePath, T data);
    }
}