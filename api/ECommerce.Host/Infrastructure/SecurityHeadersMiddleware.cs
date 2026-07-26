namespace ECommerce.Host.Infrastructure;

/// <summary>Baseline security headers for a pure JSON API (no cookies/forms, so CSRF/anti-forgery is not applicable).</summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";

        // Swagger UI needs inline scripts/styles to render; the JSON API surface does not, so it gets
        // the strict policy everywhere except under /swagger.
        headers["Content-Security-Policy"] = context.Request.Path.StartsWithSegments("/swagger")
            ? "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'"
            : "default-src 'none'; frame-ancestors 'none'";

        await next(context);
    }
}
