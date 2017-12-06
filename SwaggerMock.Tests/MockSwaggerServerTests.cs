using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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
        public async Task Should_Error_On_Unregistered_Routes()
        {
            var res = await _mockServer.Client.GetAsync("/api/goats");
            Assert.IsFalse(res.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Should_Validate_Required_Path_Parameters()
        {
            var res = await _mockServer.Client.GetAsync("/api/pets/1");
            Assert.IsTrue(res.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Should_Validate_Path_Parameters_Types()
        {
            var res = await _mockServer.Client.GetAsync("/api/pets/goat");
            var response = await res.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<List<SwaggerParameterError>>(response);

            Assert.IsFalse(res.IsSuccessStatusCode);
            Assert.IsTrue(errors.Count == 1);
            Assert.AreEqual("Invalid parameter type. Expected id to be Integer", errors[0].Message);

            var validRes = await _mockServer.Client.GetAsync("/api/pets/1");
            Assert.IsTrue(validRes.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Should_Validate_QueryString_Parameters()
        {
            var res = await _mockServer.Client.GetAsync("/api/pets/?invalid=true");
            var response = await res.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<List<SwaggerParameterError>>(response);

            Assert.IsFalse(res.IsSuccessStatusCode);
            Assert.IsTrue(errors.Count == 1);
            Assert.AreEqual("NoAdditionalQueryStringsAllowed: invalid", errors[0].Message);
        }

        [TestMethod]
        public async Task Should_Validate_Required_Body_Parameters()
        {
            var res = await _mockServer.Client.PostAsync("/api/pets/", new StringContent(string.Empty));
            var response = await res.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<List<SwaggerParameterError>>(response);

            Assert.IsFalse(res.IsSuccessStatusCode);
            Assert.IsTrue(errors.Count == 1);
            Assert.AreEqual("Missing required parameter pet", errors[0].Message);
        }

        [TestMethod]
        public async Task Should_Validate_Required_Body_Parameters_And_Properties()
        {
            var res = await _mockServer.Client.PostAsync("/api/pets/", new StringContent(@"{ ""invalid"": ""prop""}"));
            var response = await res.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<List<SwaggerParameterError>>(response);

            Assert.IsFalse(res.IsSuccessStatusCode);
            Assert.IsTrue(errors.Count == 2);
            Assert.AreEqual("PropertyRequired: #/name", errors[0].Message);
        }

        [TestMethod]
        public async Task Should_Validate_Invalid_Body_Parameters_And_Properties()
        {
            var res = await _mockServer.Client.PostAsync("/api/pets/", new StringContent(@"{ ""name"": ""cat"", ""invalid"": ""prop""}"));
            var response = await res.Content.ReadAsStringAsync();
            var errors = JsonConvert.DeserializeObject<List<SwaggerParameterError>>(response);

            Assert.IsFalse(res.IsSuccessStatusCode);
            Assert.IsTrue(errors.Count == 1);
            Assert.AreEqual("NoAdditionalPropertiesAllowed: #/invalid", errors[0].Message);
        }

        [TestMethod]
        public async Task Should_Validate_Valid_Array_QueryString()
        {
            var res = await _mockServer.Client.GetAsync("/api/pets/?tags=cat,dog");
            var response = await res.Content.ReadAsStringAsync();
            Assert.IsTrue(res.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task Should_Validate_InValid_Array_QueryString()
        {
            var res = await _mockServer.Client.GetAsync("/api/pets/?ids=cat,goat");
            var response = await res.Content.ReadAsStringAsync();
            Assert.IsFalse(res.IsSuccessStatusCode);
        }
    }
}
