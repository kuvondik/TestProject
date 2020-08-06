using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestProject.WebMVC.Controllers;
using TestProject.WebMVC.HelperMethods;
using TestProject.WebMVC.Infrastructure.Database;
using TestProject.WebMVC.Services;
using TestProject.WebMVC.ViewModels;
using Xunit;

namespace TestProject.UnitTests.Application
{
    public class HomeControllerTest
    {
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ICsvService _csvService;

        public HomeControllerTest()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            // Include this object to test its functions
            _csvService = new CsvService(new Mock<ILogger<CsvService>>().Object);
        }

        [Theory]
        [InlineData("tableName")]
        public void Get_all_data_success(string tableName)
        {
            // Arrange
            var tableData = new TableDataBuilder().Build();
            var columnNames = DynamicObjectHelpers.GetPropertyNames(tableData);
            var data = DynamicObjectHelpers.GetPropertyValues(tableData);

            _unitOfWorkMock.Setup(u => u.GetAll(tableName, It.IsAny<string>()))
                .Returns(data);

            _unitOfWorkMock.Setup(u => u.GetColumnNames(tableName))
                .Returns(columnNames);

            var homeController = new HomeController(_loggerMock.Object,
                _unitOfWorkMock.Object,
                _csvService);

            // Act
            var result = homeController.GetAllData(tableName) as JsonResult;

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<Response<List<dynamic>>>(result.Value);
            Assert.True(response.Success);
        }

        [Fact]
        public async Task Load_file_success()
        {
            // Arrange
            using var homeViewModelBuilder = new HomeViewModelBuilder();

            var homeViewModel = homeViewModelBuilder.Build();
            var tableName = homeViewModel.TableName;
            var columnNames = homeViewModel.ColumnNames;
            var data = homeViewModel.DataSet;

            _unitOfWorkMock.Setup(u => u.CreateTableAsync(homeViewModel.TableName, default, columnNames.ToArray()))
               .ReturnsAsync(It.IsAny<int>());

            _unitOfWorkMock.Setup(u => u.GetColumnNamesAsync(homeViewModel.TableName, default))
                .ReturnsAsync(columnNames);

            _unitOfWorkMock.Setup(u => u.GetAllAsync(tableName, It.IsAny<string>(), default))
                .ReturnsAsync(data);

            var homeController = new HomeController(_loggerMock.Object,
                _unitOfWorkMock.Object,
                _csvService);

            // Act
            var result = await homeController.LoadFile(homeViewModel) as JsonResult;

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<Response<List<dynamic>>>(result.Value);
            Assert.True(response.Success);
        }

        [Theory]
        [InlineData("tableName",
            new string[] { "FirstName", "LastName", "Age" },
            new string[] { "K", "S", "2" })]
        public async Task Get_filtered_data_success(string tableName, string[] columnNames, string[] dataFilter)
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.GetAllAsync(tableName, It.IsAny<string>(), default))
                .ReturnsAsync(It.IsAny<IEnumerable<dynamic>>);

            _unitOfWorkMock.Setup(u => u.FilterAsync(tableName, dataFilter, It.IsAny<string>(), default))
                .ReturnsAsync(It.IsAny<IEnumerable<dynamic>>);

            var homeController = new HomeController(_loggerMock.Object,
                _unitOfWorkMock.Object,
                _csvService);

            // Act
            var result = await homeController.GetFilteredData(tableName, columnNames.ToList(), dataFilter.ToList()) as JsonResult;

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<Response<List<dynamic>>>(result.Value);
            Assert.True(response.Success);
        }

        [Fact]
        public void Index_returns_a_ViewResult_with_null_view_name()
        {
            // Arrange
            var homeController = new HomeController(_loggerMock.Object,
                _unitOfWorkMock.Object,
                _csvService);

            // Act
            var result = homeController.Index() as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public void About_returns_a_ViewResult_with_null_view_name()
        {
            // Arrange
            var homeController = new HomeController(_loggerMock.Object,
                _unitOfWorkMock.Object,
                _csvService);

            // Act
            var result = homeController.About() as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }
    }
}