using CCPV.Main.API.Metrics;
using System.Net;
using System.Text.Json;

namespace CCPV.Main.API.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, APIMetricsCollector metricsCollector, ILogger<ExceptionHandlingMiddleware> logger)
        {
            try
            {
                if (!context.Request.Headers.ContainsKey("UserName"))
                {
                    context.Request.Headers.Add("UserName", "default_user");
                }
                await next(context);
            }
            catch (Exception ex)
            {
                metricsCollector.IncrementExceptionCount();

                logger.LogError(ex, "Unhandled exception occurred.");

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    message = "An unexpected error occurred.",
                    traceId = context.TraceIdentifier
                };

                string json = JsonSerializer.Serialize(response);
                logger.LogError($"ExceptionHandlingMiddleware: Error Response: {json}");
                await context.Response.WriteAsync(json);
            }
        }
    }
}
