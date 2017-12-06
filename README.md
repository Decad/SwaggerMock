# SwaggerMock

[![Build status](https://ci.appveyor.com/api/projects/status/lesqp5mq3dbxq0yj?svg=true)](https://ci.appveyor.com/project/Decad/swaggermock)

Mock Server based on OpenAPI Spec (Swagger) for integration testing clients

# Why would I need this?

This project was designed to test Client Libraries for APIs to ensure that they are correctly implemented based on the APIs OpenAPI spec.

# Usage

```
var swaggerDocument = File.ReadAllText("petstore-simple.json")
var mockServer = new MockSwaggerServer(swaggerDocument);
var response = await mockerServer.Client.GetAsync("/api/pets/1");
```

