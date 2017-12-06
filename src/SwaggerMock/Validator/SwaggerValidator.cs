using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NJsonSchema;
using NSwag;

namespace SwaggerMock.Validator
{
    public class SwaggerValidator
    {
        public List<SwaggerParameterError> Validate(HttpContext context, SwaggerOperation operation)
        {
            var errors = new List<SwaggerParameterError>();

            foreach (var param in operation.Parameters)
            {
                switch (param.Kind)
                {
                    case SwaggerParameterKind.Path:
                        ValidatePath(context, param, errors);
                        break;
                    case SwaggerParameterKind.Query:
                        ValidateQuery(context, param, errors);
                        break;
                    case SwaggerParameterKind.Body:
                        ValidateBody(context, param, errors);
                        break;
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

        private bool UrlValidType(SwaggerParameter param, object value)
        {
            try
            {
                switch (param.Type)
                {
                    case JsonObjectType.None:
                        break;
                    case JsonObjectType.Array:
                        return IsValidArray(param, value.ToString());
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
                        throw new ArgumentOutOfRangeException(nameof(param.Type), param.Type, null);
                }
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }

        private void ValidatePath(HttpContext context, SwaggerParameter param, List<SwaggerParameterError> errors)
        {
            var value = context.GetRouteValue(param.Name);
            if (value == null && param.IsRequired)
            {
                errors.Add(new SwaggerParameterError
                {
                    Message = $"Missing Required Parameter {param.Name}",
                });
            }

            if (value != null && !UrlValidType(param, value))
            {
                errors.Add(new SwaggerParameterError
                {
                    Message = $"Invalid parameter type. Expected {param.Name} to be {param.Type}",
                });
            }
        }

        private void ValidateQuery(HttpContext context, SwaggerParameter param, List<SwaggerParameterError> errors)
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
            else if (!UrlValidType(param, value))
            {
                errors.Add(new SwaggerParameterError
                {
                    Message = $"Invalid parameter type. Expected {param.Name} to be {param.Type}",
                });
            }
        }

        private void ValidateBody(HttpContext context, SwaggerParameter param, List<SwaggerParameterError> errors)
        {
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

        private bool IsValidArray(SwaggerParameter parameter, string value)
        {
            if (parameter.CollectionFormat == SwaggerParameterCollectionFormat.Csv)
            {
                var jsonify = $@"[""{string.Join(@""",""", value.Split(','))}""]";
                var schemaErrors = parameter.Validate(jsonify);
                return schemaErrors.Count == 0;
            }

            throw new NotImplementedException();
        }

        private string ReadBody(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}