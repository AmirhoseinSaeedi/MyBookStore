using BookStore.Domain;
using BookStore.Domain.ViewModels;
using BookStore.Infrastructure.Repository.IRepositpry;
using BookStore.Presentation.Areas.Customer.Controllers;
using BookStore.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace BookStore.Tests
{
    public class CartControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<SessionService> _mockSessionService;
        private readonly CartController _cartController;

        public CartControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockSessionService = new Mock<SessionService>();
            _cartController = new CartController(_mockUnitOfWork.Object, _mockSessionService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _cartController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public void Index_ReturnsViewResult_WithShoppingCartVM()
        {
            // Arrange
            var shoppingCartList = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1, Count = 1, product = new Product { Price = 10 } }
            };

            _mockUnitOfWork.Setup(u => u.ShoppingCart.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<ShoppingCart, bool>>>(), "product"))
                .Returns(shoppingCartList);

            // Act
            var result = _cartController.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ShoppingCartVM>(viewResult.ViewData.Model);
            Assert.Equal(10, model.OrderHeader.OrderTotal);
        }

        [Fact]
        public void Summary_ReturnsViewResult_WithShoppingCartVM()
        {
            // Arrange
            var shoppingCartList = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1, Count = 1, product = new Product { Price = 10 } }
            };

            var applicationUser = new ApplicationUser
            {
                Id = "1",
                Name = "Test User",
                PhoneNumber = "1234567890",
                StreetAddress = "123 Test St",
                City = "Test City",
                State = "Test State",
                PostalCode = "12345"
            };

            _mockUnitOfWork.Setup(u => u.ShoppingCart.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<ShoppingCart, bool>>>(), "product"))
                .Returns(shoppingCartList);

            _mockUnitOfWork.Setup(u => u.ApplicationUser.Get(It.IsAny<System.Linq.Expressions.Expression<System.Func<ApplicationUser, bool>>>(), null, false))
                .Returns(applicationUser);

            // Act
            var result = _cartController.Summary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ShoppingCartVM>(viewResult.ViewData.Model);
            Assert.Equal(10, model.OrderHeader.OrderTotal);
            Assert.Equal("Test User", model.OrderHeader.Name);
            Assert.Equal("1234567890", model.OrderHeader.PhoneNumber);
            Assert.Equal("123 Test St", model.OrderHeader.StreetAddress);
            Assert.Equal("Test City", model.OrderHeader.City);
            Assert.Equal("Test State", model.OrderHeader.State);
            Assert.Equal("12345", model.OrderHeader.PostalCode);
        }

        [Fact]
        public void SummaryPOST_ReturnsRedirectToActionResult_WhenCompanyIdIsZero()
        {
            // Arrange
            var shoppingCartList = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1, Count = 1, product = new Product { Price = 10 } }
            };

            var applicationUser = new ApplicationUser
            {
                Id = "1",
                CompanyId = 0
            };

            var mockSession = new Session
            {
                Id = "session_id",
                PaymentIntentId = "payment_intent_id",
                Url = "https://mockurl.com"
            };

            _mockUnitOfWork.Setup(u => u.ShoppingCart.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<ShoppingCart, bool>>>(), "product"))
                .Returns(shoppingCartList);

            _mockUnitOfWork.Setup(u => u.ApplicationUser.Get(It.IsAny<System.Linq.Expressions.Expression<System.Func<ApplicationUser, bool>>>(), null, false))
                .Returns(applicationUser);

            _mockUnitOfWork.Setup(u => u.OrderHeader.Add(It.IsAny<OrderHeader>()));
            _mockUnitOfWork.Setup(u => u.OrderDetail.Add(It.IsAny<OrderDetail>()));
            _mockUnitOfWork.Setup(u => u.Save());

            _mockSessionService.Setup(s => s.Create(It.IsAny<SessionCreateOptions>(), null)).Returns(mockSession);

            // Initialize ShoppingCartVM
            _cartController.ShoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = shoppingCartList,
                OrderHeader = new OrderHeader()
            };

            // Act
            var result = _cartController.SummaryPOST();

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(303, statusCodeResult.StatusCode);
        }



        [Fact]
        public void SummaryPOST_ReturnsStatusCodeResult_WhenCompanyIdIsNotZero()
        {
            // Arrange
            var shoppingCartList = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1, Count = 1, product = new Product { Price = 10 } }
            };

            var applicationUser = new ApplicationUser
            {
                Id = "1",
                CompanyId = 1
            };

            var mockSession = new Session
            {
                Id = "session_id",
                PaymentIntentId = "payment_intent_id",
                Url = "https://mockurl.com"
            };

            _mockUnitOfWork.Setup(u => u.ShoppingCart.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<ShoppingCart, bool>>>(), "product"))
                .Returns(shoppingCartList);

            _mockUnitOfWork.Setup(u => u.ApplicationUser.Get(It.IsAny<System.Linq.Expressions.Expression<System.Func<ApplicationUser, bool>>>(), null, false))
                .Returns(applicationUser);

            _mockUnitOfWork.Setup(u => u.OrderHeader.Add(It.IsAny<OrderHeader>()));
            _mockUnitOfWork.Setup(u => u.OrderDetail.Add(It.IsAny<OrderDetail>()));
            _mockUnitOfWork.Setup(u => u.Save());

            _mockSessionService.Setup(s => s.Create(It.IsAny<SessionCreateOptions>(), null)).Returns(mockSession);

            // Initialize ShoppingCartVM
            _cartController.ShoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = shoppingCartList,
                OrderHeader = new OrderHeader()
            };

            // Act
            var result = _cartController.SummaryPOST();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("OrderConfirmation", redirectToActionResult.ActionName);
        }

        [Fact]
        public void OrderConfirmation_ReturnsViewResult_WhenPaymentStatusIsNotDelayedPayment()
        {
            // Arrange
            var orderHeader = new OrderHeader
            {
                Id = 1,
                ApplicationUserId = "1",
                PaymentStatus = SD.PaymentStatusPending,
                SessionId = "session_id"
            };

            var mockSession = new Session
            {
                Id = "session_id",
                PaymentIntentId = "payment_intent_id",
                PaymentStatus = "paid"
            };

            var shoppingCartList = new List<ShoppingCart>
    {
        new ShoppingCart { ProductId = 1, Count = 1, product = new Product { Price = 10 } }
    };

            _mockUnitOfWork.Setup(u => u.OrderHeader.Get(It.IsAny<System.Linq.Expressions.Expression<System.Func<OrderHeader, bool>>>(), "ApplicationUser", false))
                .Returns(orderHeader);

            _mockUnitOfWork.Setup(u => u.ShoppingCart.GetAll(It.IsAny<System.Linq.Expressions.Expression<System.Func<ShoppingCart, bool>>>(), It.IsAny<string>()))
                .Returns(shoppingCartList);

            _mockUnitOfWork.Setup(u => u.ShoppingCart.RemoveRange(It.IsAny<IEnumerable<ShoppingCart>>()));
            _mockUnitOfWork.Setup(u => u.Save());

            _mockSessionService.Setup(s => s.Get(It.IsAny<string>(), null, null)).Returns(mockSession);


            // Act
            var result = _cartController.OrderConfirmation(orderHeader.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            _mockUnitOfWork.Verify(u => u.OrderHeader.UpdateStripePaymentID(orderHeader.Id, mockSession.Id, mockSession.PaymentIntentId), Times.Once);
            _mockUnitOfWork.Verify(u => u.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusApproved, SD.PaymentStatusApproved), Times.Once);
            _mockUnitOfWork.Verify(u => u.ShoppingCart.RemoveRange(shoppingCartList), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Exactly(2));
        }

        [Fact]
        public void GetPriceVaseQuantity_ReturnsCorrectPrice()
        {
            // Arrange
            var product = new Product { Price = 10, Price50 = 9, Price100 = 8 };
            var shoppingCart = new ShoppingCart { Count = 60, product = product };

            // Act
            var methodInfo = typeof(CartController).GetMethod("GetPriceVaseQuantity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (double)methodInfo.Invoke(_cartController, new object[] { shoppingCart });

            // Assert
            Assert.Equal(9, result);
        }

        [Fact]
        public void Plus_IncrementsCountAndUpdatesShoppingCart()
        {
            // Arrange
            var shoppingCart = new ShoppingCart { Id = 1, Count = 1 };
            _mockUnitOfWork.Setup(u => u.ShoppingCart.Get(It.IsAny<System.Linq.Expressions.Expression<Func<ShoppingCart, bool>>>(), null, false)).Returns(shoppingCart);

            // Act
            var result = _cartController.Plus(shoppingCart.Id);

            // Assert
            _mockUnitOfWork.Verify(u => u.ShoppingCart.Update(It.Is<ShoppingCart>(c => c.Count == 2)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public void Minus_DecrementsCountAndUpdatesShoppingCart()
        {
            // Arrange
            var shoppingCart = new ShoppingCart { Id = 1, Count = 2 };
            _mockUnitOfWork.Setup(u => u.ShoppingCart.Get(It.IsAny<System.Linq.Expressions.Expression<Func<ShoppingCart, bool>>>(), null, false)).Returns(shoppingCart);

            // Act
            var result = _cartController.Minus(shoppingCart.Id);

            // Assert
            _mockUnitOfWork.Verify(u => u.ShoppingCart.Update(It.Is<ShoppingCart>(c => c.Count == 1)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public void Minus_RemovesShoppingCartWhenCountIsOne()
        {
            // Arrange
            var shoppingCart = new ShoppingCart { Id = 1, Count = 1 };
            _mockUnitOfWork.Setup(u => u.ShoppingCart.Get(It.IsAny<System.Linq.Expressions.Expression<Func<ShoppingCart, bool>>>(), null, false)).Returns(shoppingCart);

            // Act
            var result = _cartController.Minus(shoppingCart.Id);

            // Assert
            _mockUnitOfWork.Verify(u => u.ShoppingCart.Remove(It.Is<ShoppingCart>(c => c.Id == shoppingCart.Id)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public void Remove_RemovesShoppingCart()
        {
            // Arrange
            var shoppingCart = new ShoppingCart { Id = 1 };
            _mockUnitOfWork.Setup(u => u.ShoppingCart.Get(It.IsAny<System.Linq.Expressions.Expression<Func<ShoppingCart, bool>>>(), null, false)).Returns(shoppingCart);

            // Act
            var result = _cartController.Remove(shoppingCart.Id);

            // Assert
            _mockUnitOfWork.Verify(u => u.ShoppingCart.Remove(It.Is<ShoppingCart>(c => c.Id == shoppingCart.Id)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Save(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }


    }
}

