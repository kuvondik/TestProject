using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.Models
{
    public class CsvModel
    {
        public List<string> Headers { get; set; }
        public List<string> Data { get; set; }

        public CsvModel()
        {
            Headers = new List<string>();
            Data = new List<string>();
        }
    }
}