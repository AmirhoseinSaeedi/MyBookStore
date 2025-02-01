using BookStore.Domain;
using BookStore.Infrastructure.Repository.IRepositpry;
using BookStore.Presentation.Areas.Admin.Controllers;
using BooskStore.Test.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace BookStore.Tests
{
    public class CompanyControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CompanyController _controller;

        public CompanyControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _controller = new CompanyController(_mockUnitOfWork.Object);

            var tempData = new Mock<ITempDataDictionary>();
            _controller.TempData = tempData.Object;
        }

        [Fact]
        public void Index_ReturnsViewResult_WithListOfCompanies()
        {
            // Arrange
            var companies = new List<Company> { new Company { Id = 1, Name = "Test Company" } };
            _mockUnitOfWork.Setup(u => u.Company.GetAll(null, null)).Returns(companies.AsQueryable());

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Company>>(viewResult.ViewData.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Upsert_Get_ReturnsViewResult_ForCreate()
        {
            // Act
            var result = _controller.Upsert((int?)null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Company>(viewResult.ViewData.Model);
            Assert.Equal(0, model.Id);
        }

        [Fact]
        public void Upsert_Get_ReturnsViewResult_ForUpdate()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Test Company" };
            _mockUnitOfWork.Setup(u => u.Company.Get(It.IsAny<System.Linq.Expressions.Expression<System.Func<Company, bool>>>(), null, false)).Returns(company);

            // Act
            var result = _controller.Upsert(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Company>(viewResult.ViewData.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public void Upsert_Post_ReturnsRedirectToActionResult_WhenModelStateIsValid()
        {
            // Arrange
            var company = new Company { Id = 0, Name = "Test Company" };
            _mockUnitOfWork.Setup(u => u.Company.Add(It.IsAny<Company>()));
            _mockUnitOfWork.Setup(u => u.Company.Update(It.IsAny<Company>()));
            _mockUnitOfWork.Setup(u => u.Save());

            // Act
            var result = _controller.Upsert(company);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Company", redirectToActionResult.ControllerName);
        }

        [Fact]
        public void Upsert_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var company = new Company { Id = 0, Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.Upsert(company);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Company>(viewResult.ViewData.Model);
            Assert.Equal(company, model);
        }

        [Fact]
        public void GetAll_ReturnsJsonResult_WithListOfCompanies()
        {
            // Arrange
            var companies = new List<Company> { new Company { Id = 1, Name = "Test Company" } };
            _mockUnitOfWork.Setup(u => u.Company.GetAll(null, null)).Returns(companies.AsQueryable());

            // Act
            var result = _controller.GetAll();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            dynamic data = jsonResult.Value;
            Assert.NotNull(data);
        }

        [Fact]
        public void Delete_ReturnsJsonResult_WhenCompanyExists()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Test Company" };
            _mockUnitOfWork.Setup(u => u.Company.Get(It.IsAny<System.Linq.Expressions.Expression<System.Func<Company, bool>>>(), null, false)).Returns(company);

            // Act
            var result = _controller.Delete(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);

            // Serialize and deserialize the value
            var json = JsonSerializer.Serialize(jsonResult.Value);
            JsonResponse response = JsonSerializer.Deserialize<JsonResponse>(json);

            Assert.True((bool)response.success);
            Assert.Equal("Delete Succesfull", (string)response.message);
        }


        [Fact]
        public void Delete_ReturnsJsonResult_WhenCompanyDoesNotExist()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Company.Get(It.IsAny<System.Linq.Expressions.Expression<System.Func<Company, bool>>>(), null, false)).Returns((Company)null);

            // Act
            var result = _controller.Delete(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);

            // Serialize and deserialize the value
            var json = JsonSerializer.Serialize(jsonResult.Value);
            JsonResponse response = JsonSerializer.Deserialize<JsonResponse>(json);

            Assert.False((bool)response.success);
            Assert.Equal("Error While Deleting", (string)response.message);
        }


    }
}
