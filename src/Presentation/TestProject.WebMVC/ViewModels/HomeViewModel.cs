using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TestProject.WebMVC.Models;

namespace TestProject.WebMVC.ViewModels
{
    public class HomeViewModel
    {
        [Required]
        public IFormFile File { get; set; }

        public string TableName { get; set; }

        public List<string> ColumnNames { get; set; }

        public List<List<string>> DataSet { get; set; }

        public HomeViewModel()
        {
            ColumnNames = new List<string>();
            DataSet = new List<List<string>>();
        }
    }
}