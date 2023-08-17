using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ConfigHub.API.Swagger
{
    public class AddCustomHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the endpoint is the search endpoint
            if (context.ApiDescription.RelativePath.Contains("search"))
            {
                // Don't add the x-applicationid header parameter for this endpoint
                return;
            }

            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-ApplicationId",
                In = ParameterLocation.Header,
                Description = "Application ID",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }

}
