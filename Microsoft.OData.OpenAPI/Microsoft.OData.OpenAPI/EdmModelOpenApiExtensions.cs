//---------------------------------------------------------------------
// <copyright file="EdmModelOpenApiExtensions.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System.IO;
using Microsoft.OData.Edm;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Extension methods to write Entity Data Model (EDM) to Open API.
    /// </summary>
    public static class EdmModelOpenApiExtensions
    {
        /// <summary>
        /// Outputs Edm model to an Open API artifact to the give stream.
        /// </summary>
        /// <param name="model">Edm model to be written.</param>
        /// <param name="serviceName">The name to give to the service</param>
        /// <param name="stream">The output stream.</param>
        /// <param name="target">The Open API target.</param>
        /// <param name="settings">Settings for the generated Open API.</param>
        public static void WriteOpenApi(this IEdmModel model, string serviceName, Stream stream, OpenApiTarget target, OpenApiVersion version, OpenApiWriterSettings settings = null)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (stream == null)
            {
                throw Error.ArgumentNull(nameof(stream));
            }

            IOpenApiWriter openApiWriter = BuildWriter(stream, version, target, serviceName);
            model.WriteOpenApi(openApiWriter, settings);
        }

        /// <summary>
        /// Outputs Edm model to an Open API artifact to the give text writer.
        /// </summary>
        /// <param name="model">Edm model to be written.</param>
        /// <param name="writer">The output text writer.</param>
        /// <param name="target">The Open API target.</param>
        /// <param name="settings">Settings for the generated Open API.</param>
        public static void WriteOpenApi(this IEdmModel model, string serviceName, TextWriter writer, OpenApiVersion version, OpenApiTarget target, OpenApiWriterSettings settings = null)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (writer == null)
            {
                throw Error.ArgumentNull(nameof(writer));
            }

            IOpenApiWriter openApiWriter = BuildWriter(writer, version, target, serviceName);
            model.WriteOpenApi(openApiWriter, settings);
        }

        /// <summary>
        /// Outputs an Open API artifact to the provided Open Api writer.
        /// </summary>
        /// <param name="model">Model to be written.</param>
        /// <param name="writer">The generated Open API writer <see cref="IOpenApiWriter"/>.</param>
        /// <param name="settings">Settings for the generated Open API.</param>
        public static void WriteOpenApi(this IEdmModel model, IOpenApiWriter writer, OpenApiWriterSettings settings = null)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (settings == null)
            {
                settings = new OpenApiWriterSettings();
            }

            EdmOpenApiDocumentGenerator converter = new EdmOpenApiDocumentGenerator(model, writer.Version, settings);
            OpenApiDocument doc = converter.Generate();
            doc.Write(writer);
        }

        private static IOpenApiWriter BuildWriter(Stream stream, OpenApiVersion version, OpenApiTarget target, string serviceName)
        {
            StreamWriter writer = new StreamWriter(stream)
            {
                NewLine = "\n"
            };

            return BuildWriter(writer, version, target, serviceName);
        }

        private static IOpenApiWriter BuildWriter(TextWriter writer, OpenApiVersion version, OpenApiTarget target, string serviceName)
        {
            if (target == OpenApiTarget.Json)
            {
                return new OpenApiJsonWriter(writer, version, serviceName);
            }
            else
            {
                return new OpenApiYamlWriter(writer, version, serviceName);
            }            
        }
    }
}
