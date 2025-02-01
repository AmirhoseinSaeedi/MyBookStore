using BookStore.Domain;
using BookStore.Infrastructure.Repository.IRepositpry;
using BookStore.Presentation.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace BookStore.Tests
{
    public class CategoryControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly CategoryController _controller;

        public CategoryControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork.Setup(u => u.Category).Returns(_mockCategoryRepository.Object);
            _controller = new CategoryController(_mockUnitOfWork.Object);

            // Mock TempData
            var tempData = new Mock<ITempDataDictionary>();
            _controller.TempData = tempData.Object;
        }

        [Fact]
        public void Index_ReturnsViewResult_WithListOfCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Category1", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Category2", DisplayOrder = 2 }
            };
            _mockCategoryRepository.Setup(repo => repo.GetAll(null, null)).Returns(categories);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Category>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void Create_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var category = new Category { Name = "Test", DisplayOrder = 1 };

            // Act
            var result = _controller.Create(category);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Category", redirectToActionResult.ControllerName);
            _mockCategoryRepository.Verify(c => c.Add(It.IsAny<Category>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void Create_Post_InvalidModel_ReturnsViewResult()
        {
            // Arrange
            var category = new Category { Name = "Test", DisplayOrder = 1 };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.Create(category);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            _mockCategoryRepository.Verify(c => c.Add(It.IsAny<Category>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }

        [Fact]
        public void Create_Post_DisplayOrderMatchesName_ReturnsViewResultWithModelError()
        {
            // Arrange
            var category = new Category { Name = "1", DisplayOrder = 1 };

            // Act
            var result = _controller.Create(category);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("name"));
            _mockCategoryRepository.Verify(c => c.Add(It.IsAny<Category>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }

        [Fact]
        public void Edit_Get_ValidId_ReturnsViewResultWithCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Category1", DisplayOrder = 1 };
            _mockCategoryRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false)).Returns(category);

            // Act
            var result = _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Category>(viewResult.ViewData.Model);
            Assert.Equal(category.Id, model.Id);
        }

        [Fact]
        public void Edit_Get_InvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            _mockCategoryRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false)).Returns((Category)null);

            // Act
            var result = _controller.Edit(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Category1", DisplayOrder = 1 };

            // Act
            var result = _controller.Edit(category);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Category", redirectToActionResult.ControllerName);
            _mockCategoryRepository.Verify(c => c.Update(It.IsAny<Category>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void Edit_Post_InvalidModel_ReturnsViewResult()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Category1", DisplayOrder = 1 };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.Edit(category);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            _mockCategoryRepository.Verify(c => c.Update(It.IsAny<Category>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Never);
        }

        [Fact]
        public void Delete_Get_ValidId_ReturnsViewResultWithCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Category1", DisplayOrder = 1 };
            _mockCategoryRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false)).Returns(category);

            // Act
            var result = _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Category>(viewResult.ViewData.Model);
            Assert.Equal(category.Id, model.Id);
        }

        [Fact]
        public void Delete_Get_InvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            _mockCategoryRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false)).Returns((Category)null);

            // Act
            var result = _controller.Delete(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeletePost_ValidId_ReturnsRedirectToActionResult()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Category1", DisplayOrder = 1 };
            _mockCategoryRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false)).Returns(category);

            // Act
            var result = _controller.DeletePost(1);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Category", redirectToActionResult.ControllerName);
            _mockCategoryRepository.Verify(c => c.Remove(It.IsAny<Category>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void DeletePost_InvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            _mockCategoryRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false)).Returns((Category)null);

            // Act
            var result = _controller.DeletePost(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
