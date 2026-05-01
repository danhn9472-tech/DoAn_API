using System.Net;
using System.Text.Json;

namespace DoAn_API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Cho phép Request đi tiếp tới các Controller
                await _next(context);
            }
            catch (Exception ex)
            {
                // Bắt toàn bộ lỗi tại đây
                _logger.LogError(ex, "Đã xảy ra lỗi không mong muốn trong hệ thống.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new { message = "Đã xảy ra lỗi máy chủ. Vui lòng thử lại sau." }; // Thông báo an toàn cho Client
            
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}