using System.Net;
using System.Text.Json;

namespace DoAn_API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
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
                await HandleExceptionAsync(context, ex, _env);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, IWebHostEnvironment env)
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
                    object response;
                    if (env.IsDevelopment())
                    {
                        response = new { message = exception.Message, detail = exception.StackTrace };
                    }
                    else
                    {
                        response = new { message = "Đã xảy ra lỗi máy chủ. Vui lòng thử lại sau." };
                    }
                    return context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}