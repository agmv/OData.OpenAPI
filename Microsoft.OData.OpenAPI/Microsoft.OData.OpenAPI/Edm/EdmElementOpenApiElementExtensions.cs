//---------------------------------------------------------------------
// <copyright file="EdmElementOpenApiElementExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Extension methods for Edm Elements to Open Api Elements.
    /// </summary>
    internal static class EdmElementOpenApiElementExtensions
    {        

        public static KeyValuePair<string, OpenApiResponse> GetResponse(this string statusCode, OpenApiVersion version)
        {
            if (statusCode == "204")
                return new KeyValuePair<string, OpenApiResponse>(statusCode, new OpenApiResponse { Description = "Success" });
            else
                return new KeyValuePair<string, OpenApiResponse>(statusCode, new OpenApiResponse
                {
                    Reference = EdmHelper.ReferenceToResponse(version, "error")
                });
        }

        public static OpenApiSchema CreateSchema(this IEdmTypeReference reference, OpenApiVersion version, bool expanded = false)
        {
            if (reference == null)
            {
                return null;
            }

            switch (reference.TypeKind())
            {
                case EdmTypeKind.Collection:
                    return new OpenApiSchema
                    {
                        Type = "array",
                        Items = CreateSchema(reference.AsCollection().ElementType(), version, expanded)
                    };

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                case EdmTypeKind.EntityReference:
                    return new OpenApiSchema
                    {
                        Reference = EdmHelper.ReferenceToEntity(version, reference.Definition.FullTypeName(), true, expanded)
                    };
                case EdmTypeKind.Enum:
                    return new OpenApiSchema
                    {
                        Reference = EdmHelper.ReferenceToEnum(version, reference.Definition.FullTypeName())
                    };

                case EdmTypeKind.Primitive:
                    OpenApiSchema schema;
                    if (reference.IsInt64() && version == OpenApiVersion.version3)
                    {
                        schema = new OpenApiSchema
                        {
                            OneOf = new List<OpenApiSchema>
                            {
                                new OpenApiSchema { Type = "integer" },
                                new OpenApiSchema { Type = "string" }
                            },
                            Format = "int64"
                        };
                    }
                    else if (reference.IsDouble() && version == OpenApiVersion.version3)
                    {
                        schema = new OpenApiSchema
                        {
                            OneOf = new List<OpenApiSchema>
                            {
                                new OpenApiSchema { Type = "number" },
                                new OpenApiSchema { Type = "string" }
                            },
                            Format = "double",
                        };
                    }
                    else if (reference.IsGeography())
                    {
                        schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference(@"https://raw.githubusercontent.com/oasis-tcs/odata-openapi/master/examples/odata-definitions.json#/definitions/Edm.GeographyPoint")
                        };
                    }
                    else if (reference.IsGeometry())
                    {
                        schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference(@"https://raw.githubusercontent.com/oasis-tcs/odata-openapi/master/examples/odata-definitions.json#/definitions/Edm.GeometryPoint")
                        };
                    }
                    else if (reference.IsStream())
                    {
                        schema = new OpenApiSchema
                        {
                            Reference = new OpenApiReference(@"https://raw.githubusercontent.com/oasis-tcs/odata-openapi/master/examples/odata-definitions.json#/definitions/Edm.Stream")
                        };
                    }
                    else
                    {
                        schema = new OpenApiSchema
                        {
                            Type = reference.AsPrimitive().GetOpenApiDataType().GetTypeName(),
                            Format = reference.AsPrimitive().GetOpenApiDataType().GetFormat()
                        };
                    }
                    schema.Nullable = reference.IsNullable ? (bool?)true : null;
                    return schema;                    
                case EdmTypeKind.TypeDefinition:
                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported("Not supported!");
            }            
        }

        public static OpenApiPathItem CreatePathItem(this IEdmOperationImport operationImport, OpenApiVersion version)
        {            
            if (operationImport.Operation.IsAction())
            {
                return ((IEdmActionImport)operationImport).CreatePathItem(version);
            }

            return ((IEdmFunctionImport)operationImport).CreatePathItem(version);
        }

        public static OpenApiPathItem CreatePathItem(this IEdmOperation operation, IEdmEntitySet entitySet, OpenApiVersion version)
        {            
            if (operation.IsAction())
            {
                return ((IEdmAction)operation).CreatePathItem(entitySet, version);
            }

            return ((IEdmFunction)operation).CreatePathItem(entitySet, version);
        }

        public static OpenApiPathItem CreatePathItem(this IEdmActionImport actionImport, OpenApiVersion version)
        {
            return new OpenApiPathItem
            {
                Post = new OpenApiOperation
                {
                    Summary = "Invoke action " + actionImport.Action.Name,
                    Tags = CreateTags(actionImport),
                    Parameters = CreateParameters(actionImport.Action, null, version),
                    Responses = CreateResponses(actionImport.Action, version)
                }
            };            
        }

        public static OpenApiPathItem CreatePathItem(this IEdmAction action, IEdmEntitySet entitySet, OpenApiVersion version)
        {            
            return new OpenApiPathItem
            {
                Post = new OpenApiOperation
                {
                    Summary = "Invoke action " + action.Name,
                    Tags = CreateTags(action),
                    Parameters = CreateParameters(action, entitySet, version),
                    Responses = CreateResponses(action, version)
                }
            };
        }

        public static OpenApiPathItem CreatePathItem(this IEdmFunctionImport functionImport, OpenApiVersion version)
        {
            return new OpenApiPathItem
            {
                Get = new OpenApiOperation
                {
                    Summary = "Invoke function " + functionImport.Function.Name,
                    Tags = CreateTags(functionImport),
                    Parameters = CreateParameters(functionImport.Function, null, version),
                    Responses = CreateResponses(functionImport.Function, version)
                }
            };            
        }

        public static OpenApiPathItem CreatePathItem(this IEdmFunction function, IEdmEntitySet entitySet, OpenApiVersion version)
        {
            return new OpenApiPathItem
            {
                Get = new OpenApiOperation
                {
                    Summary = "Invoke function " + function.Name,
                    Tags = CreateTags(function),
                    Parameters = CreateParameters(function, entitySet, version),
                    Responses = CreateResponses(function, version)
                }
            };
        }

        public static string CreatePathItemName(this IEdmActionImport actionImport)
        {
            return CreatePathItemName(actionImport.Action);
        }

        public static string CreatePathItemName(this IEdmAction action)
        {
            return "/" + action.Name;
        }

        public static string CreatePathItemName(this IEdmFunctionImport functionImport)
        {
            return CreatePathItemName(functionImport.Function);
        }

        public static string CreatePathItemName(this IEdmFunction function)
        {
            StringBuilder functionName = new StringBuilder("/" + function.Name + "(");

            functionName.Append(String.Join(",",
                function.Parameters.Skip(function.IsBound?1:0).Select(p => p.Name + "=" + "{" + p.Name + "}")));

            functionName.Append(")");

            return functionName.ToString();
        }

        public static string CreatePathItemName(this IEdmOperationImport operationImport)
        {
            if (operationImport.Operation.IsAction())
            {
                return ((IEdmActionImport)operationImport).CreatePathItemName();
            }

            return ((IEdmFunctionImport)operationImport).CreatePathItemName();
        }

        public static string CreatePathItemName(this IEdmOperation operation)
        {
            if (operation.IsAction())
            {
                return ((IEdmAction)operation).CreatePathItemName();
            }

            return ((IEdmFunction)operation).CreatePathItemName();
        }

        private static OpenApiResponses CreateResponses(this IEdmAction action, OpenApiVersion version)
        {

            OpenApiResponses responses = new OpenApiResponses();

            if (action.ReturnType != null) {
                OpenApiResponse response = new OpenApiResponse
                {
                    Description = "Success",
                    Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        "application/json",
                        new OpenApiMediaType
                        {
                            Schema = action.ReturnType.CreateSchema(version)
                        }
                    }
                }
                };
                responses.Add("200", response);
            }
            else
            {
                responses.Add("204".GetResponse(version));

            }
            responses.Add("default".GetResponse(version));
            return responses;            
        }

        private static OpenApiResponses CreateResponses(this IEdmFunction function, OpenApiVersion version)
        {
            OpenApiResponses responses = new OpenApiResponses();

            OpenApiResponse response = new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        "application/json",
                        new OpenApiMediaType
                        {
                            Schema = function.ReturnType.CreateSchema(version)
                        }
                    }
                }
            };
            responses.Add("200", response);
            responses.Add("default".GetResponse(version));
            return responses;
        }

        private static IList<string> CreateTags(this IEdmOperationImport operationImport)
        {
            if (operationImport.EntitySet != null)
            {
                var pathExpression = operationImport.EntitySet as IEdmPathExpression;
                if (pathExpression != null)
                {
                    return new List<string>
                    {
                        PathAsString(pathExpression.PathSegments)
                    };
                }
            }

            return null;
        }

        private static IList<string> CreateTags(this IEdmOperation operation)
        {
            if (operation.EntitySetPath != null)
            {
                var pathExpression = operation.EntitySetPath as IEdmPathExpression;
                if (pathExpression != null)
                {
                    return new List<string>
                    {
                        PathAsString(pathExpression.PathSegments)
                    };
                }
            }

            return null;
        }

        private static IList<OpenApiParameter> CreateParameters(this IEdmOperation operation, IEdmEntitySet entitySet, OpenApiVersion version)
        {
            IList<OpenApiParameter> parameters = new List<OpenApiParameter>();

            IList<IEdmOperationParameter> opParameters;

            if (operation.IsBound)
            {
                opParameters = operation.Parameters.Skip(1).ToList();
                if (entitySet != null)
                {
                    parameters = EdmNavigationSourceExtensions.CreateKeyParameters(entitySet.EntityType(), version);
                }
            }
            else
            {
                opParameters = operation.Parameters.ToList();
            }

            if (operation.IsFunction() || opParameters.Count <= 1)
            {
                foreach (IEdmOperationParameter edmParameter in opParameters)
                {
                    parameters.Add(new OpenApiParameter
                    {
                        Name = edmParameter.Name,
                        In = operation.IsFunction()?ParameterLocation.path:ParameterLocation.body,
                        Required = true,
                        Schema = edmParameter.Type.CreateSchema(version)
                    });
                }
            }
            else {
                parameters.Add(new OpenApiParameter
                {
                    Name = operation.Name + "Post",
                    In = ParameterLocation.body,
                    Required = true,
                    Schema = CreateSchema(opParameters, version)
                });
            }

            return parameters;
        }
        public static OpenApiSchema CreateSchema(IList<IEdmOperationParameter> parameters, OpenApiVersion version)
        {
            OpenApiSchema schema = new OpenApiSchema
            {
                Type = "object",
                Required = new List<string>(),
                Properties = new Dictionary<string, OpenApiSchema>()                
            };
            foreach (IEdmOperationParameter p in parameters) {
                schema.Required.Add(p.Name);
                schema.Properties.Add(p.Name, p.Type.CreateSchema(version));
            }
            return schema;
        }

        internal static string PathAsString(IEnumerable<string> path)
        {
            return String.Join("/", path);
        }
    }
}
