using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SwaggerMock.Tests
{
    [TestClass]
    public class MockSwaggerServerTests
    {
        [TestMethod]
        public async Task Should()
        {
            var minimal = File.ReadAllText("json/petstore-simple.json");
            var mockServer = new MockSwaggerServer(minimal);

            var res = await mockServer.Client.GetAsync("/api/pets/1");

            Assert.IsTrue(res.IsSuccessStatusCode);
        }
    }
}
