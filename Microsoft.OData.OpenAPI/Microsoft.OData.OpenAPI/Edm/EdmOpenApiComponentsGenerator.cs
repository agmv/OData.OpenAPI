﻿//---------------------------------------------------------------------
// <copyright file="EdmOpenApiComponentsGenerator.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.Edm;
using System.Linq;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Visit Edm model to generate <see cref="OpenApiComponents"/>
    /// </summary>
    internal class EdmOpenApiComponentsGenerator : EdmOpenApiGenerator
    {
        private OpenApiComponents _components;
        private IDictionary<string, OpenApiSchema> _schemas;
        private IDictionary<string, IEdmStructuredType> _refSchemas;

        /// <summary>
        /// Initializes a new instance of the <see cref="EdmOpenApiComponentsGenerator" /> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="settings">The Open Api writer settings.</param>
        public EdmOpenApiComponentsGenerator(IEdmModel model, OpenApiWriterSettings settings)
            : base(model, settings)
        {
        }

        /// <summary>
        /// Create the <see cref="OpenApiComponents"/>
        /// </summary>
        /// <returns>the components object.</returns>
        public OpenApiComponents Generate()
        {
            if (_components == null)
            {
                _schemas = new Dictionary<string, OpenApiSchema>();
                _refSchemas = new Dictionary<string, IEdmStructuredType>();
                VisitSchemas();

                _components = new OpenApiComponents
                {
                    Schemas = _schemas,
                    Parameters = VisitParameters(),
                    Responses = VisitResponses()
                };
            }

            return _components;
        }

        private void VisitSchemas()
        {
            foreach (var element in Model.SchemaElements.Where(s => (s.SchemaElementKind == EdmSchemaElementKind.TypeDefinition) &&
                                                                    EdmHelper.ReferencedEntities.Contains((s as IEdmType).FullTypeName())))
            {
                switch (element.SchemaElementKind)
                {
                    case EdmSchemaElementKind.TypeDefinition:
                        {
                            IEdmType reference = (IEdmType)element;
                            _schemas.Add(reference.FullTypeName(), VisitSchemaType(reference));
                        }
                        break;                    
                }
            }

            foreach (var element in _refSchemas) {
                _schemas.Add(element.Key, VisitStructuredType(element.Value, true, true));
            }

            AppendODataErrors(_schemas);            
        }


        private OpenApiSchema VisitSchemaType(IEdmType definition)
        {
            switch (definition.TypeKind)
            {
                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    return VisitStructuredType((IEdmStructuredType)definition, true);
                case EdmTypeKind.Enum:
                    return VisitEnumType((IEdmEnumType)definition);
                
                case EdmTypeKind.TypeDefinition:
                case EdmTypeKind.None:
                default:
                    throw Error.NotSupported("Not support");
            }
        }

        private OpenApiSchema VisitEnumType(IEdmEnumType enumType)
        {
            OpenApiSchema schema = new OpenApiSchema
            {
                Type = "string",
                Enum = new List<string>()
            };

            foreach (IEdmEnumMember member in enumType.Members)
            {
                
                schema.Enum.Add(member.Name);
            }

            schema.Title = (enumType as IEdmSchemaElement).Name;
            return schema;
        }

        private OpenApiSchema VisitStructuredType(IEdmStructuredType structuredType, bool processBase, bool expanded = false)
        {
            if (processBase && structuredType.BaseType != null)
            {
                return new OpenApiSchema
                {
                    AllOf = new List<OpenApiSchema>
                    {
                        new OpenApiSchema
                        {
                            Reference = EdmHelper.ReferenceToEntity(Settings.OpenApiVersion, structuredType.BaseType.FullTypeName(), false)
                        },

                        VisitStructuredType(structuredType, false, expanded)
                    }
                };
            }
            else
            {
                return new OpenApiSchema
                {
                    Title = (structuredType as IEdmSchemaElement).Name,
                    Type = "object",
                    Properties = VisitStructuredTypeProperties(structuredType, expanded)
                };
            }
        }

        private IDictionary<string, OpenApiSchema> VisitStructuredTypeProperties(IEdmStructuredType structuredType, bool expanded)
        {
            IDictionary<string, OpenApiSchema> properties = new Dictionary<string, OpenApiSchema>();

            foreach (var property in structuredType.DeclaredStructuralProperties())
            {
                if (!expanded || structuredType.FullTypeName() != property.Type.FullName())
                {
                    OpenApiSchema propertySchema = VisitTypeReference(property.Type);
                    properties.Add(property.Name, propertySchema);

                    IEdmStructuredType refEntity = property.Type.IsCollection() ? property.Type.AsCollection().ElementType().ToStructuredType() : property.Type.ToStructuredType();
                    if (refEntity != null && !_refSchemas.ContainsKey(refEntity.FullTypeName() + EdmHelper.Expanded) && !expanded)
                    {
                        _refSchemas.Add(refEntity.FullTypeName() + EdmHelper.Expanded, refEntity);
                    }
                }
            }

            if (!expanded) // Ref elements do not container navigation properties to avoid recursion
            {
                foreach (var property in structuredType.DeclaredNavigationProperties())
                {
                    OpenApiSchema propertySchema = VisitTypeReference(property.Type);
                    properties.Add(property.Name, propertySchema);

                    IEdmStructuredType refEntity = property.Type.IsCollection() ? property.Type.AsCollection().ElementType().ToStructuredType() : property.Type.ToStructuredType();
                    if (refEntity != null && !_refSchemas.ContainsKey(refEntity.FullTypeName() + EdmHelper.Expanded) && !expanded)
                    {
                        _refSchemas.Add(refEntity.FullTypeName() + EdmHelper.Expanded, refEntity);
                    }
                }
            }
            return properties;
        }

        private OpenApiSchema VisitTypeReference(IEdmTypeReference reference)
        {
            OpenApiSchema schema = new OpenApiSchema();

            switch (reference.TypeKind())
            {
                case EdmTypeKind.Collection:
                    schema.Type = "array";
                    schema.Items = VisitTypeReference(reference.AsCollection().ElementType());
                    break;

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                case EdmTypeKind.EntityReference:
                    schema.Reference = EdmHelper.ReferenceToEntity(Settings.OpenApiVersion, reference.Definition.FullTypeName(), false, true);
                    break;
                case EdmTypeKind.Enum:
                    schema.Reference = EdmHelper.ReferenceToEntity(Settings.OpenApiVersion, reference.Definition.FullTypeName(), false, false);
                    break;

                case EdmTypeKind.Primitive:
                    if (reference.IsInt64() && Settings.OpenApiVersion == OpenApiVersion.version3)
                    {
                        schema.OneOf = new List<OpenApiSchema>
                        {
                            new OpenApiSchema
                            {
                                Type = "integer"
                            },
                            new OpenApiSchema
                            {
                                Type = "string"
                            }
                        };                        
                        schema.Format = "int64";
                    }
                    else if (reference.IsDouble() && Settings.OpenApiVersion == OpenApiVersion.version3)
                    {
                        schema.OneOf = new List<OpenApiSchema>
                        {
                            new OpenApiSchema
                            {
                                Type = "number"
                            },
                            new OpenApiSchema
                            {
                                Type = "string"
                            }
                        };
                        schema.Format = "double";
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
                        schema.Type = reference.AsPrimitive().GetOpenApiDataType().GetTypeName();
                        schema.Format = reference.AsPrimitive().GetOpenApiDataType().GetFormat();
                    }
                    schema.Nullable = reference.IsNullable ? (bool?)true : null;
                    break;

                case EdmTypeKind.TypeDefinition:
                    throw Error.NotSupported("Not supported!");

                case EdmTypeKind.None:
                default:
                    return null;
            }

            return schema;
        }

        private IDictionary<string, OpenApiParameter> VisitParameters()
        {
            return new Dictionary<string, OpenApiParameter>
            {
                { "$top", VisitTop() },
                { "$skip", VisitSkip() },
                { "$count", VisitCount() },
                { "$filter", VisitFilter() },
                { "$search", VisitSearch() },
            };
        }

        private OpenApiParameter VisitTop()
        {
            return new OpenApiParameter
            {
                Name = "$top",
                In = ParameterLocation.query,
                Description = "Show only the first n items",
                Schema = new OpenApiSchema
                {
                    Type = "integer",
                    Minimum = 0,
                },
                Example = new OpenApiAny
                {
                    { "example", 50 } // TODO: it looks wrong here.
                }
            };
        }

        private OpenApiParameter VisitSkip()
        {
            return new OpenApiParameter
            {
                Name = "$skip",
                In = ParameterLocation.query,
                Description = "Skip only the first n items",
                Schema = new OpenApiSchema
                {
                    Type = "integer",
                    Minimum = 0,
                }
            };
        }

        private OpenApiParameter VisitCount()
        {
            return new OpenApiParameter
            {
                Name = "$count",
                In = ParameterLocation.query,
                Description = "Include count of items",
                Schema = new OpenApiSchema
                {
                    Type = "boolean"
                }
            };
        }

        private OpenApiParameter VisitFilter()
        {
            return new OpenApiParameter
            {
                Name = "$filter",
                In = ParameterLocation.query,
                Description = "Filter items by property values",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            };
        }

        private OpenApiParameter VisitSearch()
        {
            return new OpenApiParameter
            {
                Name = "$search",
                In = ParameterLocation.query,
                Description = "Search items by search phrases",
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            };
        }

        private void AppendODataErrors(IDictionary<string, OpenApiSchema> schemas)
        {
            schemas.Add("odata.error", new OpenApiSchema
            {
                Type = "object",
                Required = new List<string>
                {
                    "error"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "error",
                        new OpenApiSchema
                        {
                            Reference = EdmHelper.ReferenceToEntity(Settings.OpenApiVersion, "odata.error.main")
                        }
                    }
                }
            });

            schemas.Add("odata.error.main", new OpenApiSchema
            {
                Type = "object",
                Required = new List<string>
                {
                    "code", "message"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "code", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "message", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "target", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "details",
                        new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Reference = EdmHelper.ReferenceToEntity(Settings.OpenApiVersion, "odata.error.detail")
                            }
                        }
                    },
                    {
                        "innererror",
                        new OpenApiSchema
                        {
                            Type = "object",
                            Description = "The structure of this object is service-specific"
                        }
                    }
                }
            });

            schemas.Add("odata.error.detail", new OpenApiSchema
            {
                Type = "object",
                Required = new List<string>
                {
                    "code", "message"
                },
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    {
                        "code", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "message", new OpenApiSchema { Type = "string" }
                    },
                    {
                        "target", new OpenApiSchema { Type = "string" }
                    }
                }
            });
        }
        private IDictionary<string, OpenApiResponse> VisitResponses()
        {
            return new Dictionary<string, OpenApiResponse>
            {
                { "error", VisitError() }
            };
        }

        private OpenApiResponse VisitError()
        {
            return new OpenApiResponse
            {
                Description = "error",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    {
                        "application/json",
                        new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Reference = EdmHelper.ReferenceToEntity(Settings.OpenApiVersion, "odata.error")
                            }
                        }
                    }
                }
            };
        }        

    }
    
}
