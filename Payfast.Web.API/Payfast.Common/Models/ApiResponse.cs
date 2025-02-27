namespace Payfast.Common.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public int StatusCode { get; set; }

        public ApiResponse(bool success, string message, int statusCode, T data = default, IEnumerable<string> errors = null)
        {
            Success = success;
            Message = message;
            StatusCode = statusCode;
            Data = data;
            Errors = errors;
        }
    }
}
