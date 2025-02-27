using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Payfast.Common.Models;

namespace Payfast.Common.Middleware
{
    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiResponseMiddleware> _logger;

        public ApiResponseMiddleware(RequestDelegate next, ILogger<ApiResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //TODO: investigate if there is a way to bypass this.
            if (context.Request.Path.StartsWithSegments("/openapi") ||
                context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/scalar"))
            {
                await _next(context);
                return;
            }


            var originalBodyStream = context.Response.Body;

            using (var newBodyStream = new MemoryStream())
            {
                context.Response.Body = newBodyStream;

                try
                {
                    // Continue with the request pipeline
                    await _next(context);

                    // Check for a successful response
                    if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                    {
                        // For success, wrap the response in ApiResponse
                        await HandleSuccessResponseAsync(context);
                    }
                    else
                    {
                        // For non-success responses, do nothing here; errors are handled in the catch block
                        await HandleNonSuccessResponseAsync(context);
                    }
                }
                catch (Exception ex)
                {
                    // Handle general exceptions
                    _logger.LogError(ex, "An unexpected error occurred");
                    await HandleErrorAsync(context, "An unexpected error occurred.", StatusCodes.Status500InternalServerError, ex);
                }

                // Copy the updated stream to the original body
                newBodyStream.Seek(0, SeekOrigin.Begin);
                await newBodyStream.CopyToAsync(originalBodyStream);
            }
        }

        private static async Task HandleSuccessResponseAsync(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();

            // Wrap the successful response in the ApiResponse
            var response = new ApiResponse<object>(true, "Request was successful", context.Response.StatusCode, data: bodyText);
            await WriteResponseAsync(context, response);
        }

        private static async Task HandleNonSuccessResponseAsync(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();

            // If the response was not successful, wrap it in ApiResponse
            var response = new ApiResponse<object>(false, "An error occurred", context.Response.StatusCode, errors: new List<string> { bodyText });
            await WriteResponseAsync(context, response);
        }

        private static async Task HandleErrorAsync(HttpContext context, string message, int statusCode, Exception ex)
        {
            var errors = new List<string> { "An internal error occurred." };
            var response = new ApiResponse<object>(false, message, statusCode, errors: errors);
            await WriteResponseAsync(context, response);
        }

        private static async Task WriteResponseAsync(HttpContext context, ApiResponse<object> response)
        {
            context.Response.ContentType = "application/json";
            context.Response.Body = new MemoryStream();

            var json = System.Text.Json.JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }
    }
}
