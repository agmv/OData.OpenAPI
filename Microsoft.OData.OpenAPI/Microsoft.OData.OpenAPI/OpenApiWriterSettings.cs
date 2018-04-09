//---------------------------------------------------------------------
// <copyright file="OpenApiWriterSettings.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.OData.OpenAPI
{
    /// <summary>
    /// Configuration settings for OData to Open API writers.
    /// </summary>
    public sealed class OpenApiWriterSettings
    {
        /// <summary>
        /// The base URL of the service
        /// </summary>
        public Uri BaseUri { get; set; } = new Uri("http://localhost");

        /// <summary>
        /// The service version number
        /// </summary>
        public Version Version { get; set; } = new Version(1, 0, 1);

        /// <summary>
        /// The OpenAPI version that will be used
        /// </summary>
        public OpenApiVersion OpenApiVersion { get; set; } = OpenApiVersion.version3;

        /// <summary>
        /// The list of EntitySets used to be converted from oData.
        /// This allows to restrict a service definition to a smaller one.
        /// </summary>
        public IList<string> EntitySets { get; set; } = new List<string> { };

        /// <summary>
        /// The targert output format
        /// </summary>
        public OpenApiTarget Target { get; set; } = OpenApiTarget.Json;

        /// <summary>
        /// The targert output format
        /// </summary>
        public string ServiceName { get; set; } = null;

        public bool IncludeInOutput(string entitySetName) {
            return EntitySets.Count == 0 || EntitySets.Contains(entitySetName);
        }
    }
}
