using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using NJsonSchema;
using NSwag;

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

    public class SwaggerValidator
    {
        public List<SwaggerParameterError> Validate(HttpContext context, SwaggerOperation operation)
        {
            var errors = new List<SwaggerParameterError>();
            foreach (var param in operation.Parameters)
            {
                if (param.Kind == SwaggerParameterKind.Path)
                {
                    var value = context.GetRouteValue(param.Name);
                    if (value == null && param.IsRequired)
                    {
                        errors.Add(new SwaggerParameterError
                        {
                            Message = $"Missing Required Parameter {param.Name}",
                        });
                    }

                    if (param.IsRequired && !ValidType(param.Type, value))
                    {
                        errors.Add(new SwaggerParameterError
                        {
                            Message = $"Invalid parameter type. Expected {param.Name} to be {param.Type}",
                        });
                    }
                }

                if(param.Kind == SwaggerParameterKind.Body)
                {
                    //TODO check content type and swagger def
                    var body = ReadBody(context.Request.Body);
                    if (param.IsRequired && string.IsNullOrEmpty(body))
                    {
                        errors.Add(new SwaggerParameterError
                        {
                            Message = $"Missing required parameter {param.Name}",
                        });
                    }

                    if (!string.IsNullOrEmpty(body))
                    {
                        var validationErrors = param.Schema.Validate(body);
                        if (validationErrors.Any())
                        {
                            errors.AddRange(validationErrors.Select(x => new SwaggerParameterError
                            {
                                Message = x.ToString(),
                            }));
                        }
                    }
                }
            }

            return errors;
        }

        private bool ValidType(JsonObjectType paramType, object value)
        {
            try
            {
                switch (paramType)
                {
                    case JsonObjectType.None:
                        break;
                    case JsonObjectType.Array:
                        break;
                    case JsonObjectType.Boolean:
                        break;
                    case JsonObjectType.Integer:
                        var val = Convert.ToInt32(value);
                        return true;
                    case JsonObjectType.Null:
                        break;
                    case JsonObjectType.Number:
                        break;
                    case JsonObjectType.Object:
                        break;
                    case JsonObjectType.String:
                        break;
                    case JsonObjectType.File:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(paramType), paramType, null);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        private string ReadBody(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }
    }

    public class SwaggerParameterError
    {
        public string Message { get; set; }
    }
}
