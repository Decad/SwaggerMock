using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

            var routeBuilder = new RouteBuilder(app);

            foreach (var path in swagger.Paths)
            {
                foreach (var op in path.Value.Keys)
                {
                    if (op == SwaggerOperationMethod.Get)
                    {
                        routeBuilder.MapGet($"{swagger.BasePath.Substring(1)}{path.Key}", context =>
                        {
                            return context.Response.WriteAsync("Ok");
                        });
                    }
                }
            }

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }
    }
}