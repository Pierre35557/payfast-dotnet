using Microsoft.Extensions.Logging;
using Payfast.Common.Extensions;
using Payfast.Domain.Interfaces;
using Payfast.DTO.Requests;
using Payfast.DTO.Responses;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Payfast.Domain.Services
{
    //https://developers.payfast.co.za/docs#step_1_form_fields
    public class PayfastService : IPayfastService
    {
        private readonly string _merchantId;
        private readonly string _merchantKey;
        private readonly string _passPhrase;
        private readonly string _payfastUrl;
        private readonly string _payfastSandboxUrl;
        private readonly string _returnUrl;
        private readonly string _notifyUrl;
        private readonly string _cancelUrl;
        private readonly bool _useSandbox;

        private readonly ILogger<PayfastService> _logger;

        public PayfastService(ILogger<PayfastService> logger)
        {
            _logger = logger;

            //TODO: perhaps send important information through as part of headers?
            //what is the security implications of the above?
            _merchantId = Environment.GetEnvironmentVariable("PAYFAST_MERCHANT_ID");
            _merchantKey = Environment.GetEnvironmentVariable("PAYFAST_MERCHANT_KEY");
            _passPhrase = Environment.GetEnvironmentVariable("PAYFAST_PASSPHRASE");
            _payfastUrl = Environment.GetEnvironmentVariable("PAYFAST_URL");
            _payfastSandboxUrl = Environment.GetEnvironmentVariable("PAYFAST_SANDBOX_URL");
            _returnUrl = Environment.GetEnvironmentVariable("PAYFAST_RETURN_URL");
            _notifyUrl = Environment.GetEnvironmentVariable("PAYFAST_NOTIFY_URL");
            _cancelUrl = Environment.GetEnvironmentVariable("PAYFAST_CANCEL_URL");
            _useSandbox = bool.Parse(Environment.GetEnvironmentVariable("USE_PAYFAST_SANDBOX"));
        }

        public PayfastResponse GeneratePaymentRequestUrl(PayfastRequest request)
        {
            try
            {
                var queryString = GenerateSignatureAndCreateQueryString(request);
                var baseUrl = _useSandbox ? _payfastSandboxUrl : _payfastUrl;

                var paymentUrl = $"{baseUrl}?{queryString}";

                return new PayfastResponse { PaymentUrl = paymentUrl };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while generating the payment request url");
                throw new Exception("An unexpected error occurred while generating the payment request url", ex);
            }
        }

        private string GenerateSignatureAndCreateQueryString(PayfastRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                { "merchant_id", _merchantId.UrlEncode() },
                { "merchant_key", _merchantKey.UrlEncode() },
                { "return_url", _returnUrl.UrlEncode() },
                { "cancel_url", _cancelUrl.UrlEncode() },
                { "notify_url", _notifyUrl.UrlEncode() },
                { "name_first", request.Name.UrlEncode() },
                { "name_last", request.Surname.UrlEncode() },
                { "email_address", request.Email.UrlEncode() },
                { "amount", request.Amount.ToString().UrlEncode() },
                { "item_name", request.ItemName.UrlEncode() },
                //{ "custom_int1", "some custom integer" },
                //{ "custom_int2", "some custom integer" },
                { "email_confirmation", Convert.ToInt32(request.ConfirmEmail).ToString() },
                { "confirmation_address", request.Email.UrlEncode() },
                { "passphrase", _passPhrase.UrlEncode() }
            };

            if (!string.IsNullOrWhiteSpace(request.MobileNumber))
                parameters.Add("cell_number", request.MobileNumber.UrlEncode());

            var sortedParams = string.Join("&", parameters
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => $"{kvp.Key}={kvp.Value}"));

            return string.Join("&", sortedParams);
        }

        public bool ValidatePayfastIPN(Dictionary<string, string> responseData)
        {
            if (!responseData.ContainsKey("signature"))
                return false;

            string receivedSignature = responseData["signature"];
            responseData.Remove("signature");

            string computedSignature = GenerateSignature(responseData, _passPhrase);

            return computedSignature == receivedSignature;
        }

        private static string GenerateSignature(Dictionary<string, string> dataArray, string passPhrase = "")
        {
            var sortedParams = dataArray
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}");

            string payload = string.Join("&", sortedParams);

            if (!string.IsNullOrEmpty(passPhrase))
                payload += $"&passphrase={passPhrase}";

            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
