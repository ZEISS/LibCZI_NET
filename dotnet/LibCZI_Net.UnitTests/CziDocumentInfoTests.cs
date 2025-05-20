// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.UnitTests
{
    using FluentAssertions;
    using LibCZI_Net.Interface;
    using System;
    using Xunit.Abstractions;

    public class CziDocumentInfoTests
    {
        private readonly ITestOutputHelper output;

        public CziDocumentInfoTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void GetScalingInformationScenario1()
        {
            IInputStream inputStream;
            try
            {
                inputStream = InputStreamsRepository.CreateStreamFor4chLargeTiledDocument();
            }
            catch (Exception ex)
            {
                this.output.WriteLine(
                    $"Creating the input-stream failed to exception: {ex.Message}. Assuming this is due to problems like 'resource not available', no internet or the like, we skip this test.");
                return; // Exit the test early
            }


            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var metadataSegment = reader.GetMetadataSegment();
            using var documentInfo = metadataSegment.GetCziDocumentInfo();

            var scalingInfo = documentInfo.ScalingInfo;
            Assert.True(Math.Abs(scalingInfo.XScale - 3.4430587934723594E-07) < 0.00001E-7);
            Assert.True(Math.Abs(scalingInfo.YScale - 3.4430587934723594E-07) < 0.00001E-7);
            Assert.True(double.IsNaN(scalingInfo.ZScale));
        }

        [Fact]
        public void GetGeneralDocumentInfoScenario1()
        {
            IInputStream inputStream;
            try
            {
                inputStream = InputStreamsRepository.CreateStreamFor4chLargeTiledDocument();
            }
            catch (Exception ex)
            {
                this.output.WriteLine(
                    $"Creating the input-stream failed to exception: {ex.Message}. Assuming this is due to problems like 'resource not available', no internet or the like, we skip this test.");
                return; // Exit the test early
            }

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var metadataSegment = reader.GetMetadataSegment();
            using var documentInfo = metadataSegment.GetCziDocumentInfo();

            var generalInfoPropertyBag = documentInfo.GetGeneralDocumentInfo();

            ((DateTime)(generalInfoPropertyBag[CziDocumentPropertyKeys.GeneralDocumentInfoCreationDateTime])).Should().Be(DateTime.FromBinary(-8584636381381332652L));
            ((string)(generalInfoPropertyBag[CziDocumentPropertyKeys.GeneralDocumentInfoUserName])).Should().Be("zeiss");
        }

        [Fact]
        public void UseGetAvailableDimensionsAndCheck()
        {
            IInputStream inputStream;
            try
            {
                inputStream = InputStreamsRepository.CreateStreamFor4chLargeTiledDocument();
            }
            catch (Exception ex)
            {
                this.output.WriteLine(
                    $"Creating the input-stream failed to exception: {ex.Message}. Assuming this is due to problems like 'resource not available', no internet or the like, we skip this test.");
                return; // Exit the test early
            }

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var metadataSegment = reader.GetMetadataSegment();
            using var documentInfo = metadataSegment.GetCziDocumentInfo();

            var availableDimensions = documentInfo.GetAvailableDimensions();

            // Check if availableDimensions contains DimensionIndex.C and DimensionIndex.S
            availableDimensions.Should().Contain(DimensionIndex.C);
            availableDimensions.Should().Contain(DimensionIndex.S);
        }

        [Fact]
        public void UseGetDimensionInfoAndCheck()
        {
            IInputStream inputStream;
            try
            {
                inputStream = InputStreamsRepository.CreateStreamFor4chLargeTiledDocument();
            }
            catch (Exception ex)
            {
                this.output.WriteLine(
                    $"Creating the input-stream failed to exception: {ex.Message}. Assuming this is due to problems like 'resource not available', no internet or the like, we skip this test.");
                return; // Exit the test early
            }

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var metadataSegment = reader.GetMetadataSegment();
            using var documentInfo = metadataSegment.GetCziDocumentInfo();

            var dimensionCInfo = documentInfo.GetDimensionInfo(DimensionIndex.C);
            dimensionCInfo.Should().NotBeNull();
        }
    }
}
