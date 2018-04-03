//---------------------------------------------------------------------
// <copyright file="ODataOpenApiConvert.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    internal static class ODataOpenApiConvert
    {
        public static OpenApiDocument ConvertTo(this IEdmModel model, OpenApiVersion openApiVersion = OpenApiVersion.version3)
        {
            return model.ConvertTo(openApiVersion, new OpenApiWriterSettings());
        }

        public static OpenApiDocument ConvertTo(this IEdmModel model, OpenApiVersion openApiVersion, OpenApiWriterSettings settings)
        {
            return new EdmOpenApiDocumentGenerator(model, openApiVersion, settings).Generate();
        }
    }
}
