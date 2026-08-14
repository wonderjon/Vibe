using System.Text.Json;
using VibeCheck.Service.Exceptions;

namespace VibeCheckAPI.Middleware;

/// <summary>
/// Translates ApiException (and anything unhandled) into a consistent RFC7807 ProblemDetails
/// response, so controllers never need try/catch or status-code plumbing of their own.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (AppValidationException ex)
        {
            await WriteProblemAsync(context, ex.StatusCode, "Validation Failed", ex.Message, ex.Errors);
        }
        catch (ApiException ex)
        {
            await WriteProblemAsync(context, ex.StatusCode, ex.GetType().Name, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail, IReadOnlyDictionary<string, string[]>? errors = null)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new
        {
            type = $"https://httpstatuses.io/{statusCode}",
            title,
            status = statusCode,
            detail,
            errors
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
