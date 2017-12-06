using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSwag;
using SwaggerMock.Validator;

namespace SwaggerMock
{
    public class RouteValidatorFactory
    {
        public RequestDelegate GetValidatorDelegate(SwaggerOperationMethod method, SwaggerOperation operation)
        {
            var parameterValidator = new SwaggerValidator();
            return context =>
            {
                var errors = parameterValidator.Validate(context, operation);

                if (errors.Any())
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(errors));
                }

                return context.Response.WriteAsync(@"{ ""status"": ""ok""}");
            };
        }
    }
}
