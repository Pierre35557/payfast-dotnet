using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Payfast.Common.Models;
using Payfast.Domain.Interfaces;
using Payfast.DTO.Requests;
using Payfast.DTO.Responses;
using Payfast.Web.API.Controllers;

namespace Payfast.Tests
{
    [TestFixture]
    public class PayfastControllerTests
    {
        private IPayfastService _service;
        private ILogger<PayfastController> _logger;
        private PayfastController _controller;

        [SetUp]
        public void Setup()
        {
            _service = Substitute.For<IPayfastService>();
            _logger = Substitute.For<ILogger<PayfastController>>();
            _controller = new PayfastController(_logger, _service);
        }

        [Test]
        public void CreatePaymentUrl_ValidRequest_ReturnsOkWithPaymentUrl()
        {
            //---------------Arrange-------------------
            var request = new PayfastRequest
            {
                Name = "John",
                Surname = "Doe",
                Email = "john.doe@example.com",
                Amount = 100.00m,
                ItemName = "Test Item",
                ConfirmEmail = true,
                MobileNumber = "0123456789"
            };

            var expectedPaymentResponse = new PayfastResponse
            {
                PaymentUrl = "https://sandbox.payfast.co.za/eng/process?dummy"
            };

            _service.GeneratePaymentRequestUrl(Arg.Any<PayfastRequest>())
                    .Returns(expectedPaymentResponse);

            //---------------Act-----------------------
            var actionResult = _controller.CreatePaymentUrl(request);
            var okResult = actionResult as OkObjectResult;
            var apiResponse = okResult?.Value as ApiResponse<PayfastResponse>;

            //---------------Assert--------------------
            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult");
                Assert.That(okResult.StatusCode, Is.EqualTo(200), "Status code should be 200");
                Assert.That(apiResponse, Is.Not.Null, "ApiResponse should not be null");
                Assert.That(apiResponse.Success, Is.True, "Success flag should be true");
                Assert.That(apiResponse.Message, Is.EqualTo("Payment URL generated"));
                Assert.That(apiResponse.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
                Assert.That(apiResponse.Data, Is.Not.Null, "Payment response data should not be null");
                Assert.That(apiResponse.Data.PaymentUrl, Is.EqualTo(expectedPaymentResponse.PaymentUrl));
            });
        }

        [Test]
        public void CreatePaymentUrl_InvalidModelState_ReturnsBadRequest()
        {
            //---------------Arrange-------------------
            var request = new PayfastRequest();
            _controller.ModelState.AddModelError("TestError", "Invalid data");

            //---------------Act-----------------------
            var actionResult = _controller.CreatePaymentUrl(request);
            var badRequestResult = actionResult as BadRequestObjectResult;
            var apiResponse = badRequestResult?.Value as ApiResponse<object>;

            //---------------Assert--------------------
            Assert.Multiple(() =>
            {
                Assert.That(badRequestResult, Is.Not.Null, "Result should be BadRequestObjectResult");
                Assert.That(badRequestResult.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(apiResponse, Is.Not.Null, "ApiResponse should not be null");
                Assert.That(apiResponse.Success, Is.False, "Success flag should be false");
                Assert.That(apiResponse.Message, Is.EqualTo("Invalid request"));
                Assert.That(apiResponse.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(apiResponse.Errors, Is.Not.Empty, "There should be errors in the response");
            });
        }

        [Test]
        public void CreatePaymentUrl_ServiceThrowsException_ReturnsStatusCode500()
        {
            //---------------Arrange-------------------
            var request = new PayfastRequest
            {
                Name = "John",
                Surname = "Doe",
                Email = "john.doe@example.com",
                Amount = 100.00m,
                ItemName = "Test Item",
                ConfirmEmail = true,
                MobileNumber = "0123456789"
            };

            _service.GeneratePaymentRequestUrl(Arg.Any<PayfastRequest>())
                    .Returns(x => { throw new System.Exception("Service error"); });

            //---------------Act-----------------------
            var actionResult = _controller.CreatePaymentUrl(request);
            var objectResult = actionResult as ObjectResult;

            //---------------Assert--------------------
            Assert.Multiple(() =>
            {
                Assert.That(objectResult, Is.Not.Null, "Result should be ObjectResult");
                Assert.That(objectResult.StatusCode, Is.EqualTo(500));
                Assert.That(objectResult.Value, Is.EqualTo("An error occurred while processing your request."));
            });
        }

        [Test]
        public void ValidateIPN_ValidIPN_ReturnsOkResponse()
        {
            //---------------Arrange-------------------
            var ipnData = new Dictionary<string, string> { { "key", "value" } };

            _service.ValidatePayfastIPN(Arg.Any<Dictionary<string, string>>())
                    .Returns(true);

            //---------------Act-----------------------
            var actionResult = _controller.ValidateIPN(ipnData);
            var okResult = actionResult as OkObjectResult;
            var apiResponse = okResult?.Value as ApiResponse<PayfastResponse>;

            //---------------Assert--------------------
            Assert.Multiple(() =>
            {
                Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult");
                Assert.That(okResult.StatusCode, Is.EqualTo(200));
                Assert.That(apiResponse, Is.Not.Null, "ApiResponse should not be null");
                Assert.That(apiResponse.Success, Is.True, "Success flag should be true");
                Assert.That(apiResponse.Message, Is.EqualTo("Valid IPN"));
                Assert.That(apiResponse.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            });
        }

        [Test]
        public void ValidateIPN_InvalidIPN_ReturnsBadRequestResponse()
        {
            //---------------Arrange-------------------
            var ipnData = new Dictionary<string, string> { { "key", "value" } };

            _service.ValidatePayfastIPN(Arg.Any<Dictionary<string, string>>())
                    .Returns(false);

            //---------------Act-----------------------
            var actionResult = _controller.ValidateIPN(ipnData);
            var badRequestResult = actionResult as BadRequestObjectResult;
            var apiResponse = badRequestResult?.Value as ApiResponse<PayfastResponse>;

            //---------------Assert--------------------
            Assert.Multiple(() =>
            {
                Assert.That(badRequestResult, Is.Not.Null, "Result should be BadRequestObjectResult");
                Assert.That(badRequestResult.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(apiResponse, Is.Not.Null, "ApiResponse should not be null");
                Assert.That(apiResponse.Success, Is.True, "Success flag should be true even for invalid IPN");
                Assert.That(apiResponse.Message, Is.EqualTo("Invalid IPN"));
                Assert.That(apiResponse.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            });
        }

        [Test]
        public void ValidateIPN_ServiceThrowsException_ReturnsStatusCode500()
        {
            //---------------Arrange-------------------
            var ipnData = new Dictionary<string, string> { { "key", "value" } };

            _service.ValidatePayfastIPN(Arg.Any<Dictionary<string, string>>())
                    .Returns(x => { throw new Exception("IPN error"); });

            //---------------Act-----------------------
            var actionResult = _controller.ValidateIPN(ipnData);
            var objectResult = actionResult as ObjectResult;
            var apiResponse = objectResult?.Value as ApiResponse<object>;

            //---------------Assert--------------------
            Assert.Multiple(() =>
            {
                Assert.That(objectResult, Is.Not.Null, "Result should be ObjectResult");
                Assert.That(objectResult.StatusCode, Is.EqualTo(500));
                Assert.That(apiResponse, Is.Not.Null, "ApiResponse should not be null");
                Assert.That(apiResponse.Success, Is.False, "Success flag should be false");
                Assert.That(apiResponse.Message, Is.EqualTo("An error occurred while processing the IPN"));
                Assert.That(apiResponse.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
                Assert.That(apiResponse.Errors, Is.Not.Empty, "Errors should contain the exception message");
                Assert.That(apiResponse.Errors.Any(e => e.Contains("IPN error")), Is.True, "Error message should mention 'IPN error'");
            });
        }
    }
}
