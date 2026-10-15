using System.Diagnostics;

namespace UserManagementAPI.Middleware;

public sealed class RequestAuditMiddleware(RequestDelegate next, ILogger<RequestAuditMiddleware> logger)
{
	public async Task InvokeAsync(HttpContext context)
	{
		var startedAt = Stopwatch.GetTimestamp();
		var method = context.Request.Method;
		var path = context.Request.Path;
		var traceId = context.TraceIdentifier;

		logger.LogInformation("HTTP request {Method} {Path} started. TraceId: {TraceId}", method, path, traceId);

		try
		{
			await next(context);
		}
		finally
		{
			var elapsed = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
			logger.LogInformation(
				"HTTP response {Method} {Path} returned {StatusCode} in {ElapsedMilliseconds} ms. TraceId: {TraceId}",
				method,
			path,
			context.Response.StatusCode,
			elapsed,
			traceId);
		}
	}
}
