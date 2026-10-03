using System.Text.Json;
using DevOpsLab.Application.Common;
using DevOpsLab.Domain.Common;

namespace DevOpsLab.WebApi.Middleware;

/// <summary>
/// Translates domain and application exceptions into RFC 7807 problem responses.
/// A broken business rule is a 400, a missing resource is a 404, and anything
/// unexpected is a 500 that is logged with full detail but never leaked to the caller.
/// </summary>
public sealed class ExceptionHandlingMiddleware
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
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Business rule {Rule} violated on {Path}.", ex.Rule, context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Business rule violated", ex.Message, ex.Rule);
        }
        catch (NotFoundException ex)
        {
            _logger.LogInformation("Resource not found on {Path}: {Message}", context.Request.Path, ex.Message);
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Resource not found", ex.Message, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Path}.", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                "Internal server error", "An unexpected error occurred.", null);
        }
    }

    private static async Task WriteProblemAsync(
        HttpContext context, int statusCode, string title, string detail, string? rule)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json; charset=utf-8";

        var problem = new Dictionary<string, object?>
        {
            ["type"] = $"https://httpstatuses.io/{statusCode}",
            ["title"] = title,
            ["status"] = statusCode,
            ["detail"] = detail,
            ["instance"] = context.Request.Path.Value,
            ["traceId"] = context.TraceIdentifier
        };

        if (rule is not null)
            problem["rule"] = rule;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseApplicationExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
