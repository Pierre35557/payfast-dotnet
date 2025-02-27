using Payfast.DTO.Requests;
using Payfast.DTO.Responses;

namespace Payfast.Domain.Interfaces
{
    public interface IPayfastService
    {
        PayfastResponse GeneratePaymentRequestUrl(PayfastRequest request);
        bool ValidatePayfastIPN(Dictionary<string, string> responseData);
    }
}
