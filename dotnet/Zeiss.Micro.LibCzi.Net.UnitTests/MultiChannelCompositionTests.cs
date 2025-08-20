// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using FluentAssertions;
    using Net.Interface;
    using SixLabors.ImageSharp;
    using System;
    using Xunit.Abstractions;

    public class MultiChannelCompositionTests
    {
        private readonly ITestOutputHelper output;

        public MultiChannelCompositionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void MultiChannelCompositionWithLargeTiledDocument()
        {
            // In this test, we create a multi-channel composition from a large tiled document, which contains 4 channels.
            // What we do is:
            // * Create four multi-tile compositions from the document
            // * Retrieve the display-settings from the document's metadata
            // * For those display-settings, construct the "composition channel info" for each channel
            // * With this data (bitmaps and composition channel info), we create the multi-channel composition
            // * we then create a hash for the resulting bitmap, which we compare against a known hash
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

            // Now, retrieve the display-settings object - this is needed to create the composition channel info.
            // We need to create a metadata-segment object first, then a document-info object, and finally the display-settings object.
            using var metadataSegment = reader.GetMetadataSegment();
            using var documentInfo = metadataSegment.GetCziDocumentInfo();
            using var displaySettings = documentInfo.GetDisplaySettings();
            displaySettings.Should().NotBeNull();

            // Now, create the composition channel info for each channel.
            MultiChannelCompositionChannelInfo[] multichannelCompositionInfo = new MultiChannelCompositionChannelInfo[4];
            for (int i = 0; i < 4; i++)
            {
                multichannelCompositionInfo[i] = Factory.GetChannelCompositor().GetMultiChannelCompositionChannelInfoFromDisplaySettings(
                    displaySettings,
                    i,
                    true);
            }

            // Now, create the bitmaps for each channel.
            using var accessor = reader.CreateSingleChannelTileAccessor();
            IBitmap[] bitmaps = new IBitmap[4];
            for (int i = 0; i < 4; i++)
            {
                bitmaps[i] = accessor.Get(
                    new Coordinate([new DimensionAndValue(DimensionIndex.C, i)]),
                    new IntRect(0, 0, 27246, 18620),
                    0.1f);
            }

            // with the bitmaps and the composition channel info, we can now create the multi-channel composition.
            using var composition = Factory.GetChannelCompositor().ComposeMultiChannelImage(bitmaps, multichannelCompositionInfo);

            // Dispose all elements in the bitmaps array
            foreach (var bitmap in bitmaps)
            {
                bitmap.Dispose();
            }

            using Image composedImage = TestUtilities.CreateImageSharpImageFromBitmap(composition);

            var hash = TestUtilities.CalculateImageHashGetAsString(composedImage);
            hash.Should().Be("ece6aa3cb04a79cfed36da38d41e7f2c16590e64a02baea880ba58f0f7f044f9");

            ////composedImage.Save("N:/composition.png", new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        }

        [Fact]
        public void MultiChannelCompositionWithLut()
        {
            // In this test, we create a multi-channel composition from a document that contains 3 channels, and
            // where the display-settings use a LUT.
            IInputStream inputStream;
            try
            {
                inputStream = InputStreamsRepository.CreateStreamFor3chWithLutDocument();
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
            using var displaySettings = documentInfo.GetDisplaySettings();
            displaySettings.Should().NotBeNull();

            MultiChannelCompositionChannelInfo[] multichannelCompositionInfo = new MultiChannelCompositionChannelInfo[3];
            for (int i = 0; i < 3; i++)
            {
                multichannelCompositionInfo[i] = Factory.GetChannelCompositor().GetMultiChannelCompositionChannelInfoFromDisplaySettings(
                    displaySettings,
                    i,
                    true);
            }

            using var accessor = reader.CreateSingleChannelTileAccessor();
            IBitmap[] bitmaps = new IBitmap[3];
            for (int i = 0; i < 3; i++)
            {
                bitmaps[i] = accessor.Get(
                    new Coordinate([new DimensionAndValue(DimensionIndex.C, i), new DimensionAndValue(DimensionIndex.Z, 10)]),
                    new IntRect(0, 0, 512, 512),
                    1f);
            }

            using var composition = Factory.GetChannelCompositor().ComposeMultiChannelImage(bitmaps, multichannelCompositionInfo);

            // Dispose all elements in the bitmaps array
            foreach (var bitmap in bitmaps)
            {
                bitmap.Dispose();
            }

            using Image composedImage = TestUtilities.CreateImageSharpImageFromBitmap(composition);

            var hash = TestUtilities.CalculateImageHashGetAsString(composedImage);
            hash.Should().Be("acae64450bad845663b7c0c48e4277273be5c8e1c1b8b10f2da935ecb3a222d5");

            ////composedImage.Save("N:/composition2.png", new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        }
    }
}
