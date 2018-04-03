//---------------------------------------------------------------------
// <copyright file="EdmNavigationSourceExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Entension methods for navigation source
    /// </summary>
    internal static class EdmNavigationSourceExtensions
    {
        public static OpenApiOperation CreateGetOperationForEntitySet(this IEdmEntitySet entitySet, OpenApiVersion version)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get entities from " + entitySet.Name,
                Description = "Get entities from " + entitySet.Name,
                Tags = new List<string>
                {
                    entitySet.Name
                },
                OperationId = "Get" + entitySet.Name
            };

            operation.Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter
                {
                    Reference = EdmHelper.ReferenceToParameter(version, "$top")
                },
                new OpenApiParameter
                {
                    Reference = EdmHelper.ReferenceToParameter(version, "$skip")
                },
                new OpenApiParameter
                {
                    Reference = EdmHelper.ReferenceToParameter(version, "$search")
                },
                new OpenApiParameter
                {
                    Reference = EdmHelper.ReferenceToParameter(version, "$filter")
                },
                new OpenApiParameter
                {
                    Reference = EdmHelper.ReferenceToParameter(version, "$count")
                },

                CreateOrderByParameter(entitySet),

                CreateSelectParameter(entitySet),

                CreateExpandParameter(entitySet),
            };

            operation.Responses = new OpenApiResponses
            {
                {
                    "200",
                    new OpenApiResponse
                    {
                        Description = "Retrieved entities",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json",
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Title = "Collection of " + entitySet.Name,
                                        Type = "object",
                                        Properties = new Dictionary<string, OpenApiSchema>
                                        {
                                            {
                                                "@odata.count",
                                                new OpenApiSchema
                                                {
                                                    Type = "integer",
                                                    Format = "int64"
                                                }
                                            },
                                            {
                                                "value",
                                                new OpenApiSchema
                                                {
                                                    Type = "array",
                                                    Items = new OpenApiSchema
                                                    {
                                                        Reference = EdmHelper.ReferenceToEntity(version, entitySet.EntityType().FullName())
                                                    }
                                                }
                                            }
                                            
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses.Add("default".GetResponse(version));

            return operation;
        }

        public static OpenApiOperation CreatePostOperationForEntitySet(this IEdmEntitySet entitySet, OpenApiVersion version)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Add new entity to " + entitySet.Name,
                Description = "Add new entity to " + entitySet.Name,
                Tags = new List<string>
                {
                    entitySet.Name
                },
                OperationId = "Post" + entitySet.Name,
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "New entity",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/json", new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Reference = EdmHelper.ReferenceToEntity(version,  entitySet.EntityType().FullName())
                                }
                            }
                        }
                    }
                }
            };

            operation.Responses = new OpenApiResponses
            {
                {
                    "201",
                    new OpenApiResponse
                    {
                        Description = "Created entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json",
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Reference = EdmHelper.ReferenceToEntity(version, entitySet.EntityType().FullName())
                                    }
                                }
                            }
                        }
                    }
                }
            };
            operation.Responses.Add("default".GetResponse(version));

            return operation;
        }

        public static string CreatePathNameForEntity(this IEdmEntitySet entitySet)
        {
            string keyString;
            IList<IEdmStructuralProperty> keys = entitySet.EntityType().Key().ToList();
            if (keys.Count() == 1)
            {
                keyString = "{" + keys.First().Name + "}";
            }
            else
            {
                IList<string> temps = new List<string>();
                foreach (var keyProperty in entitySet.EntityType().Key())
                {
                    temps.Add(keyProperty.Name + "={" + keyProperty.Name + "}");
                }
                keyString = String.Join(",", temps);
            }

            return "/" + entitySet.Name + "('" + keyString + "')";
        }

        public static string CreatePathNameForSingleton(this IEdmSingleton singleton)
        {
            return "/" + singleton.Name;
        }

        public static OpenApiOperation CreateGetOperationForEntity(this IEdmEntitySet entitySet, OpenApiVersion version)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get entity from " + entitySet.Name + " by Id",
                Description = "Get entity from " + entitySet.Name + " by Id",
                Tags = new List<string>
                {
                    entitySet.Name
                },
                OperationId = "Get" + entitySet.Name + "ById"
            };

            operation.Parameters = CreateKeyParameters(entitySet.EntityType(), version);

            operation.Parameters.Add(CreateSelectParameter(entitySet));

            operation.Parameters.Add(CreateExpandParameter(entitySet));

            operation.Responses = new OpenApiResponses
            {
                {
                    "200",
                    new OpenApiResponse
                    {
                        Description = "Retrieved entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json",
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Reference = EdmHelper.ReferenceToEntity(version,  entitySet.EntityType().FullName())
                                    }
                                }
                            }
                        }
                    }
                }
            };
            operation.Responses.Add("default".GetResponse(version));

            return operation;
        }

        public static OpenApiOperation CreateGetOperationForSingleton(this IEdmSingleton singleton, OpenApiVersion version)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Get " + singleton.Name,
                Description = "Get " + singleton.Name,
                Tags = new List<string>
                {
                    singleton.Name
                },
                OperationId = "Get" + singleton.Name
            };

            operation.Parameters = new List<OpenApiParameter>();
            operation.Parameters.Add(CreateSelectParameter(singleton));

            operation.Parameters.Add(CreateExpandParameter(singleton));

            operation.Responses = new OpenApiResponses
            {
                {
                    "200",
                    new OpenApiResponse
                    {
                        Description = "Retrieved entity",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json",
                                new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Reference = EdmHelper.ReferenceToEntity(version, singleton.EntityType().FullName())
                                    }
                                }
                            }
                        }
                    }
                },
            };
            operation.Responses.Add("default".GetResponse(version));

            return operation;
        }

        public static OpenApiOperation CreatePatchOperationForEntity(this IEdmEntitySet entitySet, OpenApiVersion version)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Update entity in " + entitySet.Name,
                Description = "Update entity in " + entitySet.Name,
                Tags = new List<string>
                {
                    entitySet.Name
                },
                OperationId = "Patch" + entitySet.Name
            };

            operation.Parameters = CreateKeyParameters(entitySet.EntityType(), version);

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New property values",
                Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/json", new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Reference = EdmHelper.ReferenceToEntity(version, entitySet.EntityType().FullName())
                                }
                            }
                        }
                    }
            };

            operation.Responses = new OpenApiResponses
            {
                "204".GetResponse(version),
                "default".GetResponse(version)
            };
            return operation;
        }

        public static OpenApiOperation CreatePatchOperationForSingleton(this IEdmSingleton singleton, OpenApiVersion version)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Update " + singleton.Name,
                Description = "Update " + singleton.Name,
                Tags = new List<string>
                {
                    singleton.Name
                },
                OperationId = "Patch" + singleton.Name
            };

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Description = "New property values",
                Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/json", new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Reference = EdmHelper.ReferenceToEntity(version, singleton.EntityType().FullName())
                                }
                            }
                        }
                    }
            };

            operation.Responses = new OpenApiResponses
            {
                "204".GetResponse(version),
                "default".GetResponse(version)
            };
            return operation;
        }

        public static OpenApiOperation CreateDeleteOperationForEntity(this IEdmEntitySet entitySet, OpenApiVersion version)
        {
            OpenApiOperation operation = new OpenApiOperation
            {
                Summary = "Delete entity from " + entitySet.Name,
                Description = "Delete entity from " + entitySet.Name,
                Tags = new List<string>
                {
                    entitySet.Name
                },
                OperationId = "Delete" + entitySet.Name
            };
            operation.Parameters = CreateKeyParameters(entitySet.EntityType(), version);
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "If-Match",
                In = ParameterLocation.header,
                Description = "ETag",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });

            operation.Responses = new OpenApiResponses
            {
                "204".GetResponse(version),
                "default".GetResponse(version)
            };
            return operation;
        }

        public static OpenApiParameter CreateOrderByParameter(this IEdmEntitySet entitySet)
        {
            OpenApiParameter parameter = new OpenApiParameter
            {
                Name = "$orderby",
                In = ParameterLocation.query,
                Description = "Order items by property values",
                Schema = new OpenApiSchema
                {
                    Type = "array",
                    UniqueItems = true,
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = CreateOrderbyItems(entitySet)
                    }
                }
            };

            return parameter;
        }

        public static IList<string> CreateOrderbyItems(this IEdmEntitySet entitySet)
        {
            IList<string> orderByItems = new List<string>();

            IEdmEntityType entityType = entitySet.EntityType();

            foreach (var property in entityType.StructuralProperties())
            {
                orderByItems.Add(property.Name);
                orderByItems.Add(property.Name + " desc");
            }

            return orderByItems;
        }

        public static OpenApiParameter CreateSelectParameter(this IEdmNavigationSource navigationSource)
        {
            OpenApiParameter parameter = new OpenApiParameter
            {
                Name = "$select",
                In = ParameterLocation.query,
                Description = "Select properties to be returned",
                Schema = new OpenApiSchema
                {
                    Type = "array",
                    UniqueItems = true,
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = CreateSelectItems(navigationSource.EntityType())
                    }
                }
            };

            return parameter;
        }

        public static IList<string> CreateSelectItems(this IEdmEntityType entityType)
        {
            IList<string> selectItems = new List<string>();

            foreach (var property in entityType.StructuralProperties())
            {
                selectItems.Add(property.Name);
            }

            return selectItems;
        }

        public static OpenApiParameter CreateExpandParameter(this IEdmNavigationSource navigationSource)
        {
            OpenApiParameter parameter = new OpenApiParameter
            {
                Name = "$expand",
                In = ParameterLocation.query,
                Description = "Expand related entities",
                Schema = new OpenApiSchema
                {
                    Type = "array",
                    UniqueItems = true,
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Enum = CreateExpandItems(navigationSource.EntityType())
                    }
                }
            };

            return parameter;
        }

        public static IList<string> CreateExpandItems(this IEdmEntityType entityType)
        {
            IList<string> expandItems = new List<string>
            {
                "*"
            };

            foreach (var property in entityType.NavigationProperties())
            {
                expandItems.Add(property.Name + "Expanded"); // avoid creating recursive definitions
            }

            return expandItems;
        }

        public static IList<OpenApiParameter> CreateKeyParameters(this IEdmEntityType entityType, OpenApiVersion version)
        {
            IList<OpenApiParameter> parameters = new List<OpenApiParameter>();

            // append key parameter
            foreach (var keyProperty in entityType.Key())
            {
                OpenApiParameter parameter = new OpenApiParameter
                {
                    Name = keyProperty.Name,
                    In = ParameterLocation.path,
                    Required = true,
                    Description = "key: " + keyProperty.Name,
                    Schema = keyProperty.Type.CreateSchema(version)
                };

                parameters.Add(parameter);
            }

            return parameters;
        }
    }
}
