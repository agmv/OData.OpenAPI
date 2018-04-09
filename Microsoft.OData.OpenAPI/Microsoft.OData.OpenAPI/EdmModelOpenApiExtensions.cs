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
        /// <param name="entitySets">The list of entity sets to import</param>
        /// <param name="stream">The output stream.</param>
        /// <param name="settings">Settings for the generated Open API.</param>
        public static void WriteOpenApi(this IEdmModel model, Stream stream, OpenApiWriterSettings settings = null)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (stream == null)
            {
                throw Error.ArgumentNull(nameof(stream));
            }

            if (settings == null)
            {
                settings = new OpenApiWriterSettings();
            }

            IOpenApiWriter openApiWriter = BuildWriter(stream, settings);
            model.WriteOpenApi(openApiWriter, settings);
        }

        /// <summary>
        /// Outputs Edm model to an Open API artifact to the give text writer.
        /// </summary>
        /// <param name="model">Edm model to be written.</param>
        /// <param name="writer">The output text writer.</param>
        /// <param name="target">The Open API target.</param>
        /// <param name="settings">Settings for the generated Open API.</param>
        public static void WriteOpenApi(this IEdmModel model, TextWriter writer, OpenApiWriterSettings settings = null)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (writer == null)
            {
                throw Error.ArgumentNull(nameof(writer));
            }

            if (settings == null)
            {
                settings = new OpenApiWriterSettings();
            }

            IOpenApiWriter openApiWriter = BuildWriter(writer, settings);
            model.WriteOpenApi(openApiWriter, settings);
        }

        /// <summary>
        /// Outputs an Open API artifact to the provided Open Api writer.
        /// </summary>
        /// <param name="model">Model to be written.</param>
        /// <param name="writer">The generated Open API writer <see cref="IOpenApiWriter"/>.</param>
        /// <param name="settings">Settings for the generated Open API.</param>
        public static void WriteOpenApi(this IEdmModel model, IOpenApiWriter writer, OpenApiWriterSettings settings)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }
            
            EdmOpenApiDocumentGenerator converter = new EdmOpenApiDocumentGenerator(model, settings);
            OpenApiDocument doc = converter.Generate();
            doc.Write(writer);
        }

        private static IOpenApiWriter BuildWriter(Stream stream, OpenApiWriterSettings settings)
        {
            StreamWriter writer = new StreamWriter(stream)
            {
                NewLine = "\n"
            };

            return BuildWriter(writer, settings);
        }

        private static IOpenApiWriter BuildWriter(TextWriter writer, OpenApiWriterSettings settings)
        {
            if (settings.Target == OpenApiTarget.Json)
            {
                return new OpenApiJsonWriter(writer, settings);
            }
            else
            {
                return new OpenApiYamlWriter(writer, settings);
            }            
        }
    }
}
