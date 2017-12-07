using System;
using System.Text.RegularExpressions;
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
                    var routeTemplate = $"{swagger.BasePath}{path.Key}";
                    if (routeTemplate.StartsWith("/"))
                    {
                        var leadingSlashes = new Regex("^/+");
                        routeTemplate = leadingSlashes.Replace(routeTemplate, string.Empty);
                    }

                    routeBuilder.MapVerb(
                        Enum.GetName(typeof(SwaggerOperationMethod), method),
                        routeTemplate,
                        routeValidatorFactory.GetValidatorDelegate(method, path.Value[method]));
                }
            }

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }
    }
}