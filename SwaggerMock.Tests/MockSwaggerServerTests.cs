using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SwaggerMock.Tests
{
    [TestClass]
    public class MockSwaggerServerTests
    {
        private MockSwaggerServer _mockServer;

        [TestInitialize]
        public void Setup()
        {
            var swaggerDocument = File.ReadAllText("json/petstore-simple.json");
            _mockServer = new MockSwaggerServer(swaggerDocument);
        }

        [TestMethod]
        public async Task Should_Handle_GET_Requests()
        {
            var res = await _mockServer.Client.GetAsync("/api/pets/");
            Assert.IsTrue(res.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Should_Handle_GET_Requests_With_Params()
        {
            var res = await _mockServer.Client.GetAsync("/api/pets/4");
            Assert.IsTrue(res.IsSuccessStatusCode);
        }

       /// <summary>
       /// asdasd
       /// </summary>
        [TestMethod]
        public async Task Should_Handle_GET_Requests_With_Params_And_Query()
        {
            var res = await _mockServer.Client.GetAsync("/api/pets/4?tags=goat,duck");
            Assert.IsTrue(res.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Should_Handle_POST_Requests()
        {
            var res = await _mockServer.Client.PostAsync("/api/pets/", new StringContent(""));
            Assert.IsTrue(res.IsSuccessStatusCode);
        }

        // Todo Add PUT to JSON
        // [TestMethod]
        // public async Task Should_Handle_PUT_Requests()
        // {
        //    var res = await _mockServer.Client.PutAsync("/api/pets/1", new StringContent(""));
        //    Assert.IsTrue(res.IsSuccessStatusCode);
        // }

        [TestMethod]
        public async Task Should_Error_On_Unregistered_Routes()
        {
            var res = await _mockServer.Client.GetAsync("/api/goats");
            Assert.IsFalse(res.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Should_Validate_Get_Parameters()
        {
            var res = await _mockServer.Client.GetAsync("/api/pets/goat");
            Assert.IsFalse(res.IsSuccessStatusCode);
        }
    }
}
