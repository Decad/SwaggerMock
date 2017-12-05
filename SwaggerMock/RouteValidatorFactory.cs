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

                    if (value != null && !ValidType(param.Type, value))
                    {
                        errors.Add(new SwaggerParameterError
                        {
                            Message = $"Invalid parameter type. Expected {param.Name} to be {param.Type}",
                        });
                    }
                }

                if (param.Kind == SwaggerParameterKind.Query)
                {
                    if (!context.Request.Query.TryGetValue(param.Name, out var value))
                    {
                        if (param.IsRequired)
                        {
                            errors.Add(new SwaggerParameterError
                            {
                                Message = $"Missing Required Parameter {param.Name}",
                            });
                        }
                    }
                    else if (!ValidType(param.Type, value))
                    {
                        errors.Add(new SwaggerParameterError
                        {
                            Message = $"Invalid parameter type. Expected {param.Name} to be {param.Type}",
                        });
                    }
                }

                if(param.Kind == SwaggerParameterKind.Body)
                {
                    // TODO check content type and swagger def
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
                        param.Schema.AllowAdditionalProperties = false;
                        param.ActualSchema.AllowAdditionalProperties = false;
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

            errors.AddRange(context.Request.Query.Where(x => operation.Parameters
                    .Where(p => p.Kind == SwaggerParameterKind.Query)
                    .All(c => c.Name != x.Key))
                    .Select(queryString => new SwaggerParameterError
                    {
                        Message = $"NoAdditionalQueryStringsAllowed: {queryString.Key}",
                    }));

            return errors;
        }

        private bool ValidType(JsonObjectType paramType, object value)
        {
            try
            {
                switch (paramType)
                {
                    case JsonObjectType.None:
                    case JsonObjectType.Array:
                        break;
                    case JsonObjectType.Boolean:
                        var valb = Convert.ToBoolean(value);
                        break;
                    case JsonObjectType.Integer:
                        var vali = Convert.ToInt32(value);
                        break;
                    case JsonObjectType.Null:
                        return value == null;
                    case JsonObjectType.Number:
                        var valn = Convert.ToDouble(value);
                        break;
                    case JsonObjectType.Object:
                        return value != null;
                    case JsonObjectType.String:
                        var vals = Convert.ToString(value);
                        break;
                    case JsonObjectType.File:
                        // TODO How do we handle this
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(paramType), paramType, null);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
