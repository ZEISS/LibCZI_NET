// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using Net.Interface;
    using System;
    using System.Xml.XPath;
    using Xunit;
    using Xunit.Abstractions;

    public class MetadataSegmentTest
    {
        private readonly ITestOutputHelper output;

        public MetadataSegmentTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void LoadDocumentAndGetMetadataTest()
        {
            IInputStream inputStream;
            try
            {
                inputStream = Factory.CreateInputStream(
                    "curl_http_inputstream",
                    "https://media.githubusercontent.com/media/ptahmose/libCZI_testdata/main/CZICheck_Testdata/sparse_planes.czi");
            }
            catch (Exception ex)
            {
                this.output.WriteLine(
                    $"Creating the input-stream failed to exception: {ex.Message}. Assuming this is due to problems like 'resource not available', no internet or the like, we skip this test.");
                return; // Exit the test early
            }

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            var metadataSegment = reader.GetMetadataSegment();

            var xmlDoc = metadataSegment.GetMetadataAsXDocument();
            string? userName = xmlDoc.XPathSelectElement("//Document/UserName")?.Value;
            Assert.NotNull(userName);
            Assert.Equal("M1NMESQU", userName);
        }
    }
}