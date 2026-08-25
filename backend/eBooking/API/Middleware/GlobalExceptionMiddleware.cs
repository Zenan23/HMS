using System.Net;
using System.Text.Json;
using Contracts.Exceptions;

namespace API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                // Domenski (custom) tipovi provjeravaju se PRIJE generičkih BCL izuzetaka —
                // uputa (8.1) eksplicitno traži custom exception tipove umjesto generičkog
                // Exception-a. ArgumentException/KeyNotFoundException/InvalidOperationException
                // ostaju kao fallback za mjesta u kodu koja još nisu prebačena na ova dva tipa.
                case NotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = exception.Message;
                    break;
                case BusinessRuleException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Message = exception.Message;
                    break;
                case ArgumentNullException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Neispravni podaci u zahtjevu.";
                    break;
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = exception.Message;
                    break;
                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = "Resurs nije pronađen.";
                    break;
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Nemate pristup ovom resursu.";
                    break;
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Message = exception.Message;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "Došlo je do interne greške servera.";
                    break;
            }

            context.Response.StatusCode = response.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}