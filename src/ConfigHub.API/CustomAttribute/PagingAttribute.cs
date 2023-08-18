using ConfigHub.Shared;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection.Metadata;

namespace ConfigHub.API.CustomAttribute
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PagingAttribute : ActionFilterAttribute
    {
        private readonly int _defaultPageSize;

        public PagingAttribute(int defaultPageSize = Constants.DefaultPagingSize)
        {
            _defaultPageSize = defaultPageSize;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            if (!int.TryParse(request.Query[Constants.PagingTakeAttributeName], out int take) || take <= 0)
            {
                take = _defaultPageSize;
            }

            if (!int.TryParse(request.Query[Constants.PagingSkipAttributeName], out int skip) || skip < 0)
            {
                skip = 0;
            }

            context.HttpContext.Items[Constants.PagingTakeAttributeName] = take;
            context.HttpContext.Items[Constants.PagingSkipAttributeName] = skip;

            base.OnActionExecuting(context);
        }
    }

}
