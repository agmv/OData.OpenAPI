﻿//---------------------------------------------------------------------
// <copyright file="EdmModelOpenApiExtensionsTest.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.OData.Edm;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OData.OpenAPI.Tests
{
    public class EdmModelOpenApiExtensionsTest
    {
        private readonly ITestOutputHelper output;

        public EdmModelOpenApiExtensionsTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void EmptyEdmModelToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;

            // Act
            string json = WriteEdmModelToOpenApi(model);

            // Assert
            Assert.Equal(Resources.GetString("Empty.OpenApi.json").Replace(), json);
        }

        [Fact]
        public void EmptyEdmModelToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.EmptyModel;
            OpenApiWriterSettings settings = new OpenApiWriterSettings
            {
                Target = OpenApiTarget.Yaml
            };

            // Act
            string yaml = WriteEdmModelToOpenApi(model, settings);

            // Assert
            Assert.Equal(Resources.GetString("Empty.OpenApi.yaml").Replace(), yaml);
        }

        [Fact]
        public void BasicEdmModelToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;

            // Act
            string json = WriteEdmModelToOpenApi(model);

            // Assert
            Assert.Equal(Resources.GetString("Basic.OpenApi.json").Replace(), json);
        }

        [Fact]
        public void BasicEdmModelToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.BasicEdmModel;
            OpenApiWriterSettings settings = new OpenApiWriterSettings
            {
                Target = OpenApiTarget.Yaml
            };

            // Act
            string yaml = WriteEdmModelToOpenApi(model, settings);

            // Assert
            Assert.Equal(Resources.GetString("Basic.OpenApi.yaml").Replace(), yaml);
        }

        [Fact]
        public void TripServiceMetadataToOpenApiJsonWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiWriterSettings settings = new OpenApiWriterSettings
            {
                Version = new Version(1, 0, 1),
                BaseUri = new Uri("http://services.odata.org/TrippinRESTierService/"),
                Target = OpenApiTarget.Json
            };

            // Act
            string json = WriteEdmModelToOpenApi(model, settings);
            output.WriteLine(json);
            // Assert
            Assert.Equal(Resources.GetString("TripService.OpenApi.json").Replace(), json);
        }

        [Fact]
        public void TripServiceMetadataToOpenApiYamlWorks()
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiWriterSettings settings = new OpenApiWriterSettings
            {
                Version = new Version(1, 0, 1),
                BaseUri = new Uri("http://services.odata.org/TrippinRESTierService/"),
                Target = OpenApiTarget.Yaml
            };

            // Act
            string yaml = WriteEdmModelToOpenApi(model, settings);

            // Assert
            Assert.Equal(Resources.GetString("TripService.OpenApi.yaml").Replace(), yaml);
        }

        private static string WriteEdmModelToOpenApi(IEdmModel model, OpenApiWriterSettings settings = null)
        {
            MemoryStream stream = new MemoryStream();
            model.WriteOpenApi(stream, settings);
            stream.Flush();
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
