using Microsoft.AspNetCore.Http;

namespace ConfigHub.Shared.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool ShouldIncludeValue(this HttpRequest request)
        {
            return request.Query.ContainsKey("includeValue") && request.Query["includeValue"] == "true";
        }

        public static int GetTake(this HttpRequest request, int defaultTake = Constants.DefaultPagingSize)
        {
            int.TryParse(request.Query[Constants.PagingTakeAttributeName], out int take);
            return take <= 0 ? defaultTake : take;
        }

        public static int GetSkip(this HttpRequest request)
        {
            int.TryParse(request.Query[Constants.PagingSkipAttributeName], out int skip);
            return skip < 0 ? 0 : skip;
        }
    }
}
