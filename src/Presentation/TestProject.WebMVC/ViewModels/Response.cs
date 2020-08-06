using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.WebMVC.ViewModels
{
    public class Response<T> : Response
    {
        public new T Data { get; set; }
    }

    public class Response
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public List<string> ColumnNames { get; set; }
        public long TotalCount { get; set; }
        public int NewRecordsCount { get; set; }
        public string TableName { get; set; }
    }
}