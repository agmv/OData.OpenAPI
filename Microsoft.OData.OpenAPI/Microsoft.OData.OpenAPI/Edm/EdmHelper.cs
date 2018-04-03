//---------------------------------------------------------------------
// <copyright file="EdmHelper.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Edm;
using System.Diagnostics;

namespace Microsoft.OData.OpenAPI
{
    internal static class EdmHelper
    {
        public static string GetOpenApiTypeName(this IEdmTypeReference edmType)
        {
            Debug.Assert(edmType != null);

            return edmType.Definition.GetOpenApiTypeName();
        }

        public static string GetOpenApiTypeName(this IEdmType edmType)
        {
            Debug.Assert(edmType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Collection:
                    return OpenApiTypeKind.Array.GetDisplayName();

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                case EdmTypeKind.EntityReference:
                    return OpenApiTypeKind.Object.GetDisplayName();

                case EdmTypeKind.Enum:
                    return OpenApiTypeKind.String.GetDisplayName();

                case EdmTypeKind.Primitive:
                    return ((IEdmPrimitiveType)(edmType)).GetOpenApiDataType().GetCommonName();

                default:
                    return OpenApiTypeKind.None.GetDisplayName();
            }
        }

        public static OpenApiReference ReferenceToEntity(OpenApiVersion version, string to, bool referenced = false)
        {
            return new OpenApiReference((version == OpenApiVersion.version3 ? "#/components/schemas/" : "#/definitions/") + (referenced ? to + "Ref" : to));
        }

        public static OpenApiReference ReferenceToParameter(OpenApiVersion version, string to)
        {
            return new OpenApiReference((version == OpenApiVersion.version3 ? "#/components/parameters/" : "#/parameters/") + to);
        }

        public static OpenApiReference ReferenceToResponse(OpenApiVersion version, string to)
        {
            return new OpenApiReference((version == OpenApiVersion.version3 ? "#/components/responses/" : "#/responses/") + to);
        }
    }
}
