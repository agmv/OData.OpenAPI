//---------------------------------------------------------------------
// <copyright file="OpenApiDocument.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Describes an Open API Document. See: https://swagger.io/specification/
    /// </summary>
    internal class OpenApiDocument : IOpenApiElement, IOpenApiExtensible, IOpenApiWritable
    {
        /// <summary>
        /// REQUIRED.This string MUST be the semantic version number of the OpenAPI Specification version that the OpenAPI document uses.
        /// </summary>
        public Version OpenApi { get; set; } = new Version(3, 0, 0);

        /// <summary>
        /// REQUIRED. Provides metadata about the API. The metadata MAY be used by tooling as required.
        /// </summary>
        public OpenApiInfo Info { get; set; } = new OpenApiInfo();

        /// <summary>
        /// An array of Server Objects, which provide connectivity information to a target server.
        /// </summary>
        public IList<OpenApiServer> Servers { get; set; }

        /// <summary>
        /// REQUIRED. The available paths and operations for the API.
        /// </summary>
        public OpenApiPaths Paths { get; set; } = new OpenApiPaths();

        /// <summary>
        /// An element to hold various schemas for the specification.
        /// </summary>
        public OpenApiComponents Components { get; set; }

        /// <summary>
        /// A declaration of which security mechanisms can be used across the API.
        /// </summary>
        public IList<OpenApiSecurity> Security { get; set; }

        /// <summary>
        /// A list of tags used by the specification with additional metadata. 
        /// </summary>
        public IList<OpenApiTag> Tags { get; set; }

        /// <summary>
        /// Additional external documentation.
        /// </summary>
        public OpenApiExternalDocs ExternalDoc { get; set; }

        /// <summary>
        /// This object MAY be extended with Specification Extensions.
        /// </summary>
        public IList<OpenApiExtension> Extensions { get; set; }
        

        // OpenAPI 2.0 objects

        /// <summary>
        /// The host (name or ip) serving the API. This MUST be the host only and does not include the scheme nor sub-paths. It MAY include a port.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// TThe base path on which the API is served, which is relative to the <see cref="Host"/>.
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// The transfer protocol of the API. Values MUST be from the list: "http", "https", "ws", "wss".
        /// </summary>
        public IList<string> Schemes { get; set; }

        /// <summary>
        /// An array of MIME types the API can consume.
        /// </summary>
        public IList<string> Consumes { get; set; }

        /// <summary>
        /// An array of MIME types the API can produce.
        /// </summary>
        public IList<string> Produces { get; set; }


        public IList<OpenApiSchema> Definitions { get; set; }

        public IList<OpenApiParameter> Parameters{ get; set; }
        

        /// <summary>
        /// Write Open API document to given stream using default writer.
        /// </summary>
        /// <param name="stream">The stream to write.</param>
        public virtual void Write(Stream stream)
        {
            Write(stream, DefaultOpenApiWriter);
        }

        /// <summary>
        /// Write Open API document to given stream using given writer factory
        /// </summary>
        /// <param name="stream">The stream to write.</param>
        /// <param name="writerFactory">The writer factory.</param>
        public virtual void Write(Stream stream, Func<Stream, IOpenApiWriter> writerFactory)
        {
            if (stream == null)
            {
                throw Error.ArgumentNull("stream");
            }

            if (writerFactory == null)
            {
                throw Error.ArgumentNull("writerFactory");
            }

            IOpenApiWriter writer = writerFactory(stream);
            this.Write(writer);
            writer.Flush();
        }

        /// <summary>
        /// Write Open API document to the given writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void Write(IOpenApiWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            // { for json, empty for YAML
            writer.WriteStartObject();

            if (writer.Version == OpenApiVersion.version3)
            {
                // openapi:3.0.0
                writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocOpenApi, OpenApi.ToString());

                // info
                writer.WriteRequiredObject(OpenApiConstants.OpenApiDocInfo, Info);

                // servers
                writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocServers, Servers);

                // paths
                writer.WriteRequiredObject(OpenApiConstants.OpenApiDocPaths, Paths);

                // components
                writer.WriteOptionalObject(OpenApiConstants.OpenApiDocComponents, Components);

                // security
                writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocSecurity, Security);

                // tags
                writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocTags, Tags);

                // external docs
                writer.WriteOptionalObject(OpenApiConstants.OpenApiDocExternalDocs, ExternalDoc);

                // specification extensions
                writer.WriteDictionary(Extensions);
            }
            else {
                // swagger:2.0
                writer.WriteRequiredProperty(OpenApiConstants.OpenApiDocSwagger, OpenApi.ToString());

                // info
                writer.WriteRequiredObject(OpenApiConstants.OpenApiDocInfo, Info);

                // host
                writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocHost, Host);

                // basePath
                writer.WriteOptionalProperty(OpenApiConstants.OpenApiDocBasePath, BasePath);

                // schemes
                writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocSchemes, Schemes);

                // consumes
                writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocConsumes, Consumes);

                // produces
                writer.WriteOptionalCollection(OpenApiConstants.OpenApiDocProduces, Produces);

                // definitions
                writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocDefinitions, Components.Schemas);

                // parameters
                writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocParameters, Components.Parameters);

                // responses
                writer.WriteOptionalDictionary(OpenApiConstants.OpenApiDocResponses, Components.Responses);

                // paths
                writer.WriteRequiredObject(OpenApiConstants.OpenApiDocPaths, Paths);
            }

            // } for json, empty for YAML
            writer.WriteEndObject();

            // flush
            writer.Flush();
        }

        private static IOpenApiWriter DefaultOpenApiWriter(Stream stream)
        {
            StreamWriter writer = new StreamWriter(stream)
            {
                NewLine = "\n"
            };

            return new OpenApiJsonWriter(writer);
        }
    }
}
