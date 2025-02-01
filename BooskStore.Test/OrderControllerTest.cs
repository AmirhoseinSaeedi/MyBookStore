using BookStore.Domain;
using BookStore.Domain.ViewModels;
using BookStore.Infrastructure.Repository.IRepositpry;
using BookStore.Presentation.Areas.Admin.Controllers;
using BookStore.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Stripe;
using System.Linq.Expressions;
using System.Security.Claims;
using Xunit;

namespace BookStore.Tests
{
    public class OrderControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly OrderController _orderController;
        private readonly Mock<RefundService> _mockRefundService;

        public OrderControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _orderController = new OrderController(_mockUnitOfWork.Object);
            _mockRefundService = new Mock<RefundService>();
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _orderController.TempData = tempData;
        }

        [Fact]
        public void Details_ReturnsViewResult_WithOrderVM()
        {
            // Arrange
            int orderId = 1;
            var orderHeader = new OrderHeader { Id = orderId, Name = "Test Order" };
            var orderDetails = new List<OrderDetail>
            {
                new OrderDetail { Id = 1, OrderHeaderId = orderId, ProductId = 1, Count = 1, Price = 10.0 }
            };

            _mockUnitOfWork.Setup(uow => uow.OrderHeader.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), false))
                .Returns(orderHeader);
            _mockUnitOfWork.Setup(uow => uow.OrderDetail.GetAll(It.IsAny<Expression<Func<OrderDetail, bool>>>(), It.IsAny<string>()))
                .Returns(orderDetails);

            // Act
            var result = _orderController.Details(orderId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<OrderVM>(viewResult.ViewData.Model);
            Assert.Equal(orderId, model.OrderHeader.Id);
        }

        [Fact]
        public void UpdateOrderDetail_ReturnsRedirectToActionResult_WithSuccessMessage()
        {
            // Arrange
            var orderHeader = new OrderHeader
            {
                Id = 1,
                Name = "Test Order",
                PhoneNumber = "1234567890",
                StreetAddress = "123 Test St",
                City = "Test City",
                State = "Test State",
                PostalCode = "12345",
                TrackingNumber = "123456",
                Carrier = "Test Carrier"
            };

            var orderVM = new OrderVM
            {
                OrderHeader = orderHeader
            };

            _orderController.OrderVM = orderVM;

            _mockUnitOfWork.Setup(uow => uow.OrderHeader.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(orderHeader);

            // Act
            var result = _orderController.UpdateOrderDetail();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(OrderController.Details), redirectToActionResult.ActionName);
            Assert.Equal(orderHeader.Id, redirectToActionResult.RouteValues["orderid"]);
            Assert.Equal("Order Details Updated Successfully", _orderController.TempData["Success"]);
        }

        [Fact]
        public void ShipOrder_ReturnsRedirectToActionResult_WithSuccessMessage()
        {
            // Arrange
            var orderHeader = new OrderHeader
            {
                Id = 1,
                TrackingNumber = "123456",
                Carrier = "Test Carrier",
                PaymentStatus = SD.PaymentStatusDelayedPayment
            };

            var orderVM = new OrderVM
            {
                OrderHeader = orderHeader
            };

            _orderController.OrderVM = orderVM;

            _mockUnitOfWork.Setup(uow => uow.OrderHeader.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(orderHeader);

            // Act
            var result = _orderController.ShipOrder();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(OrderController.Details), redirectToActionResult.ActionName);
            Assert.Equal(orderHeader.Id, redirectToActionResult.RouteValues["orderid"]);
            Assert.Equal("Order Shipped Successfully", _orderController.TempData["Success"]);
        }
        //[Fact]
        //public void CancelOrder_ReturnsRedirectToActionResult_WithSuccessMessage()
        //{
        //    // Arrange
        //    var orderHeader = new OrderHeader
        //    {
        //        Id = 1,
        //        PaymentStatus = SD.PaymentStatusApproved,
        //        PaymentIntendId = "pi_123456"
        //    };

        //    var orderVM = new OrderVM
        //    {
        //        OrderHeader = orderHeader
        //    };

        //    _orderController.OrderVM = orderVM;

        //    _mockUnitOfWork.Setup(uow => uow.OrderHeader.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()))
        //        .Returns(orderHeader);

        //    _mockRefundService.Setup(service => service.Create(It.IsAny<RefundCreateOptions>(), null))
        //        .Returns(new Refund());

        //    // Act
        //    var result = _orderController.CancelOrder();

        //    // Assert
        //    var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal(nameof(OrderController.Details), redirectToActionResult.ActionName);
        //    Assert.Equal(orderHeader.Id, redirectToActionResult.RouteValues["orderid"]);
        //    Assert.Equal("Order cancelled Successfully", _orderController.TempData["Success"]);
        //}

        [Fact]
        public void StartProcessing_ReturnsRedirectToActionResult_WithSuccessMessage()
        {
            // Arrange
            var orderHeader = new OrderHeader
            {
                Id = 1,
                OrderStatus = SD.StatusInProcess
            };

            var orderVM = new OrderVM
            {
                OrderHeader = orderHeader
            };

            _orderController.OrderVM = orderVM;

            _mockUnitOfWork.Setup(uow => uow.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusInProcess, null));
            _mockUnitOfWork.Setup(uow => uow.Save());

            // Act
            var result = _orderController.StartProcessing();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(OrderController.Details), redirectToActionResult.ActionName);
            Assert.Equal(orderHeader.Id, redirectToActionResult.RouteValues["orderid"]);
            Assert.Equal("Order Details Updated Successfully", _orderController.TempData["Success"]);
        }

        //Details_PAY_NOW
        // PaymentConfirmation

        [Fact]
        public void GetAll_ReturnsJsonResult_WithFilteredOrderHeaders()
        {
            // Arrange
            var orderHeaders = new List<OrderHeader>
            {
                new OrderHeader { Id = 1, OrderStatus = SD.StatusApproved, PaymentStatus = SD.PaymentStatusApproved },
                new OrderHeader { Id = 2, OrderStatus = SD.StatusShipped, PaymentStatus = SD.PaymentStatusApproved },
                new OrderHeader { Id = 3, OrderStatus = SD.StatusInProcess, PaymentStatus = SD.PaymentStatusDelayedPayment }
            };

            _mockUnitOfWork.Setup(uow => uow.OrderHeader.GetAll(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>()))
                .Returns(orderHeaders);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _orderController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = _orderController.GetAll("inprocess");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(jsonResult.Value.GetType().GetProperty("data").GetValue(jsonResult.Value));
            Assert.Single(data);
            Assert.Equal(SD.StatusInProcess, data.First().OrderStatus);
        }
    }
}
