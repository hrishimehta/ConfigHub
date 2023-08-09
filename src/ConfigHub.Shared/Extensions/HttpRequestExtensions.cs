using Microsoft.AspNetCore.Http;

namespace ConfigHub.Shared.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool ShouldIncludeValue(this HttpRequest request)
        {
            return request.Query.ContainsKey("includeValue") && request.Query["includeValue"] == "true";
        }
    }
}
