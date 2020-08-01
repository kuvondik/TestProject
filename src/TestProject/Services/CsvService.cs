using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.Services
{
    public class CsvService : ICsvService
    {
        public object Read(string fullPath)
        {
            throw new NotImplementedException();
        }

        public T Read<T>(string fullPath)
        {
            throw new NotImplementedException();
        }

        public Task<object> ReadAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<T> ReadAsync<T>(string filePath)
        {
            throw new NotImplementedException();
        }

        public void Write(string filePath, object data)
        {
            throw new NotImplementedException();
        }

        public void Write<T>(string filePath, T data)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string filePath, object data)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync<T>(string filePath, T data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}