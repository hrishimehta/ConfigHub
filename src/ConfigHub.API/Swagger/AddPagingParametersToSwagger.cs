using ConfigHub.API.CustomAttribute;
using ConfigHub.Shared;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ConfigHub.API.Swagger
{
    public class AddPagingParametersToSwagger : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var pagingSupported = context.MethodInfo.GetCustomAttributes(true).OfType<PagingAttribute>().Any();

            if (pagingSupported)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = Constants.PagingTakeAttributeName,
                    In = ParameterLocation.Query,
                    Description = $"Number of items to take (default: {Constants.DefaultPagingSize})",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "integer",
                        Format = "int32"
                    }
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = Constants.PagingSkipAttributeName,
                    In = ParameterLocation.Query,
                    Description = "Number of items to skip (default: 0)",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "integer",
                        Format = "int32"
                    }
                });
                
                var totalCountHeader = new OpenApiHeader
                {
                    Description = "Total count of items",
                    Schema = new OpenApiSchema
                    {
                        Type = "integer"
                    }
                };

                foreach (var response in operation.Responses.Values)
                {
                    response.Headers[Constants.TotalCountResponseHeader] = totalCountHeader;
                }
            }
        }
    }
}
