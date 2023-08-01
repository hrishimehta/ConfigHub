using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ConfigHub.API.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var applicationId = context.Items["ApplicationId"]?.ToString();
            var component = context.Request.RouteValues["component"]?.ToString();
            var key = context.Request.RouteValues["key"]?.ToString();

            _logger.LogInformation($"Request for ApplicationId: {applicationId}, Component: {component}, Key: {key}");

            await _next(context);
        }
    }
}
