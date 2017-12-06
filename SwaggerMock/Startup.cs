using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NSwag;

namespace SwaggerMock
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app)
        {
            var swagger = app.ApplicationServices.GetService<SwaggerDocument>();

            var routeValidatorFactory = new RouteValidatorFactory();
            var routeBuilder = new RouteBuilder(app);

            foreach (var path in swagger.Paths)
            {
                foreach (var method in path.Value.Keys)
                {
                    routeBuilder.MapVerb(
                        Enum.GetName(typeof(SwaggerOperationMethod), method),
                        $"{swagger.BasePath.Substring(1)}{path.Key}",
                        routeValidatorFactory.GetValidatorDelegate(method, path.Value[method]));
                }
            }

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }
    }
}