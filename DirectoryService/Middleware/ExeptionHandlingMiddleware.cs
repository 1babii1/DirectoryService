namespace DirectoryService.Middleware;

public class ExeptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExeptionHandlingMiddleware> _logger;

    public ExeptionHandlingMiddleware(RequestDelegate next, ILogger<ExeptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            _logger.LogInformation("Middleware started");
            await _next(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in middleware");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Internal server error");
        }
    }
}