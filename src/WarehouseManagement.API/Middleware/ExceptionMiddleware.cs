using System.Net;
using System.Text.Json;
using WarehouseManagement.Domain.Exceptions;

namespace WarehouseManagement.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
            _logger.LogError(
                ex,
                "An unhandled exception occurred: {Message}",
                ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception ex)
    {
        context.Response.ContentType = "application/json";

        var response = ex switch
        {
            // Specific exceptions - derived first
            NotFoundException e => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = e.Message
            },

            ValidationException e => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Validation failed",
                Errors = e.Errors
            },

            UnauthorizedException e => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = e.Message
            },

            ForbiddenException e => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Message = e.Message
            },

            InsufficientStockException e => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = e.Message
            },

            // Base business exception - after derived exceptions
            BusinessException e => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = e.Message
            },

            // Fallback
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occurred."
            }
        };

        context.Response.StatusCode = response.StatusCode;

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public IDictionary<string, string[]>? Errors { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}