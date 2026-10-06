namespace UserManagementAPI.Middleware;

// Simple token-based authentication: expects  Authorization: Bearer <token>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _validToken;

    public AuthenticationMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _validToken = config["Auth:Token"] ?? "secret-token-123";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var header = context.Request.Headers.Authorization.ToString();
        var token = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..].Trim()
            : string.Empty;

        if (token != _validToken)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized. Invalid or missing token." });
            return;
        }

        await _next(context);
    }
}
