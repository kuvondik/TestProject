using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using TestProject.WebMVC.ViewModels;

namespace TestProject.UnitTests
{
    public class TableDataBuilder
    {
        public dynamic Build()
        {
            var table = new ExpandoObject();

            table.TryAdd("FirstName", "Kuvondik");
            table.TryAdd("LastName", "Sayfiddinov");
            table.TryAdd("Age", "22");
            table.TryAdd("Sex", "male");

            return table;
        }
    }

    public class HomeViewModelBuilder : IDisposable
    {
        private readonly FileStream stream = File.OpenRead(Directory.GetCurrentDirectory() + CsvFileResource.TestCsvFile);

        public HomeViewModel Build()
        {
            return new HomeViewModel
            {
                File = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name)),
                ColumnNames = new List<string> { "Payroll_Number", "Forenames", "Surname", "Date_of_Birth", "Telephone", "Mobile", "Address", "Address_2", "Postcode", "EMail_Home", "Start_Date" },
                DataSet = new List<List<string>>
                {
                    new List<string>{"COOP08,John" ,"William","26/01/1955","12345678","987654231","12 Foreman road","London","GU12 6JW","nomadic20@hotmail.co.uk","18/04/2013"},
                    new List<string>{"JACK13","Jerry","Jackson","11/5/1974","2050508","6987457","115 Spinney Road","Luton","LU33DF","gerry.jackson@bt.com","18/04/2013"}
                },
                TableName = "Employee"
            };
        }

        public void Dispose()
        {
            stream.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}