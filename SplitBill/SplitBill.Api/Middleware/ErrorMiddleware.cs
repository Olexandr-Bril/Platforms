using SplitBill.Business.Exceptions;

namespace SplitBill.Api.Middleware;

public class ErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorMiddleware> _logger;

    public ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger)
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
            int status = ex switch
            {
                BadRequestException => StatusCodes.Status400BadRequest,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError,
            };

            string message = ex.Message;

            if (status == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(ex, "Unhandled error");
                message = "Something went wrong.";
            }

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new { status, error = message });
        }
    }
}
