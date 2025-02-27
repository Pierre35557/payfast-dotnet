using Microsoft.AspNetCore.Mvc;
using Payfast.Common.Models;
using Payfast.Domain.Interfaces;
using Payfast.DTO.Requests;
using Payfast.DTO.Responses;

namespace Payfast.Web.API.Controllers;

/// <summary>
/// Controller responsible for handling Payfast payment requests.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/payfast")]
[ApiVersion("1.0")]
public class PayfastController : ControllerBase
{
    private readonly IPayfastService _service;
    private readonly ILogger<PayfastController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayfastController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="service">The Payfast service used to generate payment request URLs.</param>
    public PayfastController(ILogger<PayfastController> logger, IPayfastService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Creates a payment request and returns a payment URL for Payfast integration.
    /// </summary>
    /// <param name="request">The payment request data.</param>
    /// <returns>
    /// A <see cref="ApiResponse{T}"/> containing the payment URL.
    /// </returns>
    /// <response code="201">Returns the generated payment URL.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="500">If an error occurred while processing the request.</response>
    [HttpPost("generate-payment-url")]
    [MapToApiVersion("1.0")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<PayfastResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public IActionResult CreatePaymentUrl([FromBody] PayfastRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            var response = new ApiResponse<object>(false, "Invalid request", StatusCodes.Status400BadRequest, errors: errors);
            return BadRequest(response);
        }

        try
        {
            var paymentUrl = _service.GeneratePaymentRequestUrl(request);
            var response = new ApiResponse<PayfastResponse>(true, "Payment URL generated", StatusCodes.Status201Created, paymentUrl);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Validates the Instant Payment Notification (IPN) response from Payfast.
    /// </summary>
    [HttpPost("validate-ipn")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ApiResponse<PayfastResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public IActionResult ValidateIPN([FromForm] Dictionary<string, string> ipnData)
    {
        try
        {
            if (_service.ValidatePayfastIPN(ipnData))
                return Ok(new ApiResponse<PayfastResponse>(true, "Valid IPN", StatusCodes.Status200OK));

            return BadRequest(new ApiResponse<PayfastResponse>(true, "Invalid IPN", StatusCodes.Status400BadRequest));
        }
        catch (Exception ex)
        {
            var response = new ApiResponse<object>(
                false,
                "An error occurred while processing the IPN",
                StatusCodes.Status500InternalServerError,
                null,
                [ex.Message]
            );

            return StatusCode(500, response);
        }
    }
}
