using BookStore.Domain;
using BookStore.Domain.ViewModels;
using BookStore.Infrastructure.Repository.IRepositpry;
using BookStore.Presentation.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Moq;
using System.Linq.Expressions;

namespace BookStore.Tests
{
    public class ProductControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private readonly ProductController _controller;
        private readonly Mock<IFileSystem> _mockFileSystem;

        public ProductControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            _controller = new ProductController(_mockUnitOfWork.Object, _mockWebHostEnvironment.Object);
            _mockFileSystem = new Mock<IFileSystem>();
        }

        [Fact]
        public void Index_ReturnsViewResult_WithListOfProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1" },
                new Product { Id = 2, Title = "Product 2" }
            };
            _mockUnitOfWork.Setup(uow => uow.Product.GetAll(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string?>())).Returns(products.AsQueryable());

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Product>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void Upsert_Get_ReturnsViewResult_ForCreate()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Category 1" },
                new Category { Id = 2, Name = "Category 2" }
            };
            _mockUnitOfWork.Setup(uow => uow.Category.GetAll(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string?>())).Returns(categories.AsQueryable());

            // Act
            var result = _controller.Upsert(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProductVM>(viewResult.ViewData.Model);
            Assert.NotNull(model.Product);
            Assert.Equal(2, model.CategoryList.Count());
        }

        [Fact]
        public void Upsert_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Category 1" },
                new Category { Id = 2, Name = "Category 2" }
            };
            _mockUnitOfWork.Setup(uow => uow.Category.GetAll(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<string?>())).Returns(categories.AsQueryable());

            var productVM = new ProductVM
            {
                Product = new Product { Id = 0, Title = "New Product" },
                CategoryList = new List<SelectListItem>()
            };
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = _controller.Upsert(productVM, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProductVM>(viewResult.ViewData.Model);
            Assert.Equal(productVM, model);
            Assert.Equal(2, model.CategoryList.Count());
        }

        // mode state is valid
    }
}
