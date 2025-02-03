using BookStore.Domain;
using BookStore.Infrastructure.Repository.IRepositpry;
using BookStore.Presentation.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace BookStore.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _controller = new HomeController(_loggerMock.Object, _unitOfWorkMock.Object);
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
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
            _unitOfWorkMock.Setup(u => u.Product.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), It.IsAny<string>())).Returns(products);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public void Details_Get_ReturnsViewResult_WithShoppingCart()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Title = "Product 1" };
            _unitOfWorkMock.Setup(u => u.Product.Get(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(product);

            // Act
            var result = _controller.Details(productId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ShoppingCart>(viewResult.ViewData.Model);
            Assert.Equal(productId, model.ProductId);
            Assert.Equal(product, model.product);
        }

        [Fact]
        public void Details_Post_AddsToCart_AndRedirectsToIndex()
        {
            // Arrange
            var shoppingCart = new ShoppingCart { ProductId = 1, Count = 1 };
            var userId = "user1";
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _unitOfWorkMock.Setup(u => u.ShoppingCart.Get(It.IsAny<System.Linq.Expressions.Expression<Func<ShoppingCart, bool>>>(), null, false)).Returns(shoppingCart);

            // Act
            var result = _controller.Details(shoppingCart);

            // Assert
            _unitOfWorkMock.Verify(u => u.ShoppingCart.Update(It.IsAny<ShoppingCart>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Save(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }
    }
}
