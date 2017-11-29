using System;
using Microsoft.AspNetCore.Http;
using NSwag;

namespace SwaggerMock
{
    public class RouteValidatorFactory
    {
        public RequestDelegate GetValidatorDelegate(SwaggerOperationMethod method, SwaggerOperation operation)
        {
            return context => context.Response.WriteAsync("Ok");
        }
    }
}
