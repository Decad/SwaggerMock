using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSwag;

namespace SwaggerMock
{
    public class MockSwaggerServer
    {
        private TestServer _server;

        public MockSwaggerServer(string spec)
        {
            var document = SwaggerDocument.FromJsonAsync(spec).Result;
            var webhost = new WebHostBuilder()
                .ConfigureServices(services => services.AddSingleton(document))
                .UseStartup<Startup>();

            _server = new TestServer(webhost);
        }

        public HttpClient Client => _server.CreateClient();

    }
}
