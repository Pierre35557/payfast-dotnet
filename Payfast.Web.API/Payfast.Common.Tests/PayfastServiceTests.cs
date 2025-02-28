using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework.Legacy;
using Payfast.Domain.Services;
using Payfast.DTO.Requests;
using Payfast.DTO.Responses;
using System.Web;

namespace Payfast.Domain.Tests
{
    [TestFixture]
    public class PayfastServiceTests
    {
        private PayfastService _service;
        private ILogger<PayfastService> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = Substitute.For<ILogger<PayfastService>>();

            Environment.SetEnvironmentVariable("PAYFAST_MERCHANT_ID", "10000100");
            Environment.SetEnvironmentVariable("PAYFAST_MERCHANT_KEY", "46f0cd694581a");
            Environment.SetEnvironmentVariable("PAYFAST_PASSPHRASE", "securepassphrase");
            Environment.SetEnvironmentVariable("PAYFAST_URL", "https://www.payfast.co.za/eng/process");
            Environment.SetEnvironmentVariable("PAYFAST_SANDBOX_URL", "https://sandbox.payfast.co.za/eng/process");
            Environment.SetEnvironmentVariable("PAYFAST_RETURN_URL", "https://example.com/return");
            Environment.SetEnvironmentVariable("PAYFAST_NOTIFY_URL", "https://example.com/notify");
            Environment.SetEnvironmentVariable("PAYFAST_CANCEL_URL", "https://example.com/cancel");
            Environment.SetEnvironmentVariable("USE_PAYFAST_SANDBOX", "true");

            _service = new PayfastService(_mockLogger);
        }

        [Test]
        public void GeneratePaymentRequestUrl_ValidRequest_ReturnsUrlWithQueryString()
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

            //---------------Act-----------------------
            var response = _service.GeneratePaymentRequestUrl(request);

            //---------------Assert--------------------
            Assert.That(response, Is.Not.Null);
            Assert.That(response.PaymentUrl, Is.Not.Null);
            Assert.That(response.PaymentUrl, Does.StartWith("https://sandbox.payfast.co.za/eng/process?"));

            var uri = new Uri(response.PaymentUrl);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);

            Assert.Multiple(() =>
            {
                Assert.That(Environment.GetEnvironmentVariable("PAYFAST_MERCHANT_ID"), Is.EqualTo(queryParams["merchant_id"]));
                Assert.That(Environment.GetEnvironmentVariable("PAYFAST_MERCHANT_KEY"), Is.EqualTo(queryParams["merchant_key"]));
                Assert.That(Environment.GetEnvironmentVariable("PAYFAST_RETURN_URL"), Is.EqualTo(queryParams["return_url"]));
                Assert.That(Environment.GetEnvironmentVariable("PAYFAST_NOTIFY_URL"), Is.EqualTo(queryParams["notify_url"]));
                Assert.That(Environment.GetEnvironmentVariable("PAYFAST_CANCEL_URL"), Is.EqualTo(queryParams["cancel_url"]));

                Assert.That(request.Name, Is.EqualTo(queryParams["name_first"]));
                Assert.That(request.Surname, Is.EqualTo(queryParams["name_last"]));
                Assert.That(request.Email, Is.EqualTo(queryParams["email_address"]));
                Assert.That(request.ItemName, Is.EqualTo(queryParams["item_name"]));
                Assert.That(request.Email, Is.EqualTo(queryParams["confirmation_address"]));
                Assert.That(request.MobileNumber, Is.EqualTo(queryParams["cell_number"]));
                Assert.That(queryParams["amount"], Is.EqualTo(request.Amount.ToString()));
                Assert.That(queryParams["email_confirmation"], Is.EqualTo(Convert.ToInt32(request.ConfirmEmail).ToString()));
            });
        }
    }
}
