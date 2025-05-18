// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.UnitTests
{
    using FluentAssertions;
    using LibCZI_Net.Implementation;
    using LibCZI_Net.Interface;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SixLabors.ImageSharp;
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

        //[Fact]
        //public void UseGetDisplaySettingsAndCheck()
        //{
        //    IInputStream inputStream;
        //    try
        //    {
        //        //inputStream = Factory.CreateInputStreamFromFile(@"d:\2025_01_27__0007_offline_Zen_3_9_5.czi");
        //        inputStream = InputStreamsRepository.CreateStreamFor4chLargeTiledDocument();
        //    }
        //    catch (Exception ex)
        //    {
        //        this.output.WriteLine(
        //            $"Creating the input-stream failed to exception: {ex.Message}. Assuming this is due to problems like 'resource not available', no internet or the like, we skip this test.");
        //        return; // Exit the test early
        //    }

        //    using var reader = Factory.CreateReader();
        //    reader.Open(inputStream);

        //    using var metadataSegment = reader.GetMetadataSegment();
        //    using var documentInfo = metadataSegment.GetCziDocumentInfo();

        //    using var displaySettings = documentInfo.GetDisplaySettings();
        //    displaySettings.Should().NotBeNull();

        //    MultiChannelCompositionChannelInfo[] multichannelCompositionInfo = new MultiChannelCompositionChannelInfo[4];

        //    for (int i = 0; i < 4; i++)
        //    {
        //        multichannelCompositionInfo[i] = Factory.GetChannelCompositor().GetMultiChannelCompositionChannelInfoFromDisplaySettings(
        //            displaySettings,
        //            i,
        //            true);
        //    }

        //    using var accessor = reader.CreateSingleChannelTileAccessor();
        //    IBitmap[] bitmaps = new IBitmap[4];
        //    for (int i = 0; i < 4; i++)
        //    {
        //        bitmaps[i] = accessor.Get(
        //            new Coordinate([new DimensionAndValue(DimensionIndex.C, i)]),
        //            new IntRect(0, 0, 27246, 18620),
        //            0.1f);
        //    }

        //    using var composition = Factory.GetChannelCompositor().ComposeMultiChannelImage(
        //        4,
        //        bitmaps.Zip(multichannelCompositionInfo, (bitmap, info) => (bitmap, info)));

        //    // Dispose all elements in the bitmaps array
        //    foreach (var bitmap in bitmaps)
        //    {
        //        bitmap.Dispose();
        //    }

        //    using Image composedImage = TestUtilities.CreateImageSharpImageFromBitmap(composition);

        //    var hash = TestUtilities.CalculateImageHashGetAsString(composedImage);
        //    hash.Should().Be("ece6aa3cb04a79cfed36da38d41e7f2c16590e64a02baea880ba58f0f7f044f9");

        //    composedImage.Save("N:/composition.png", new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        //}
    }
}
