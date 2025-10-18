namespace SurveyTalkService.API.Middlewares.Auth
{
    public class GlobalCorsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalCorsMiddleware> _logger;

    public GlobalCorsMiddleware(RequestDelegate next, ILogger<GlobalCorsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers["Origin"].FirstOrDefault();
        
        _logger.LogDebug("Request: {Method} {Path} from Origin: {Origin}", 
            context.Request.Method, context.Request.Path, origin ?? "null");

        // Add CORS headers for all file-related requests
        if (context.Request.Path.StartsWithSegments("/api") && 
            (context.Request.Path.Value?.Contains("/api/Misc/file-io-test/preview/") == true || 
             context.Request.Path.Value?.Contains("/download/") == true))
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Range");
                    
                    _logger.LogDebug("Global CORS headers added for: {Path}", context.Request.Path);
                }
                return Task.CompletedTask;
            });
        }

        // Handle preflight requests globally
        if (context.Request.Method == "OPTIONS")
        {
            context.Response.StatusCode = 200;
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, HEAD, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization, Range");
            context.Response.Headers.Add("Access-Control-Max-Age", "86400");
            
            _logger.LogInformation("Global OPTIONS preflight handled for: {Path}", context.Request.Path);
            return;
        }

        await _next(context);
    }
}
}
