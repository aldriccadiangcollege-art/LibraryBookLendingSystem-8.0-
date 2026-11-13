using System.Text.Json;

namespace LibraryBookLendingSystem.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";
            var body = JsonSerializer.Serialize(new
            {
                type = "[tools.ietf.org](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
                title = "An unexpected error occurred.",
                status = 500,
                detail = ex.Message
            });
            await context.Response.WriteAsync(body);
        }
    }
}