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

            switch (exception)
            {
                case KeyNotFoundException e:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return context.Response.WriteAsync(JsonSerializer.Serialize(new { message = e.Message }));

                case UnauthorizedAccessException e:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Bạn không có quyền thực hiện hành động này." }));

                case InvalidOperationException e:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return context.Response.WriteAsync(JsonSerializer.Serialize(new { message = e.Message }));

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return context.Response.WriteAsync(JsonSerializer.Serialize(new { message = "Đã xảy ra lỗi máy chủ. Vui lòng thử lại sau." }));
            }
        }
    }
}