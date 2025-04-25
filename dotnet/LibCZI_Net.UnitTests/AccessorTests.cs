// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.UnitTests
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using LibCZI_Net.Interface;
    using Xunit.Abstractions;

    using SixLabors.ImageSharp;
    /*    using SixLabors.ImageSharp.PixelFormats;
        using SixLabors.ImageSharp.Processing;
        using SixLabors.ImageSharp.Formats.Png;
    */

    public class AccessorTests
    {
        private readonly ITestOutputHelper output;

        public AccessorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void OpenCziFromHttpAndGetBitmapFromAccessorBlackBackgroundAndCheck()
        {
            IInputStream inputStream;
            try
            {
                //inputStream = Factory.CreateInputStream(
                //    "curl_http_inputstream",
                //     //"https://zenodo.org/records/14968770/files/2025_01_27__0007_offline_Zen_3_9_5.czi?download=1",
                //     "https://ptahmose.de/nextcloud/s/ZaFwaZxKpaMrbZo/download/2025_01_27__0007_offline_Zen_3_9_5.czi",
                //    new Dictionary<string, object>() { { StreamClassPropertyKeys.CurlHttpUserAgent, "libCZI" }, });
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

            //// var statistics = reader.GetSimpleStatistics();

            using var accessor = reader.CreateSingleChannelTileAccessor();

            using var bitmap = accessor.Get(
                new Coordinate([new DimensionAndValue(DimensionIndex.C, 0)]),
                new IntRect(2000, 0, 3000, 3000),
                1);

            bitmap.BitmapInfo.PixelType.Should().Be(PixelType.Gray16);
            bitmap.BitmapInfo.Width.Should().Be(3000);
            bitmap.BitmapInfo.Height.Should().Be(3000);

            using Image image = TestUtilities.CreateImageSharpImageFromBitmap(bitmap);

            var hash = TestUtilities.CalculateImageHashGetAsString(image);
            hash.Should().Be("8c43c06424418877f33b4a5a11c974d3b47dacec667f3eb72206038ba9f81ff5");

            /*
            // Save the image as a PNG.
            image.Save("N:/output.png", new SixLabors.ImageSharp.Formats.Png.PngEncoder());
            */
        }

        [Fact]
        public void OpenCziFromHttpAndGetBitmapFromAccessorWhiteBackgroundAndCheck()
        {
            IInputStream inputStream;
            try
            {
                /*inputStream = Factory.CreateInputStream(
                    "curl_http_inputstream",
                     //"https://zenodo.org/records/14968770/files/2025_01_27__0007_offline_Zen_3_9_5.czi?download=1",
                     "https://ptahmose.de/nextcloud/s/ZaFwaZxKpaMrbZo/download/2025_01_27__0007_offline_Zen_3_9_5.czi",
                    new Dictionary<string, object>() { { StreamClassPropertyKeys.CurlHttpUserAgent, "libCZI" }, });*/
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

            //// var statistics = reader.GetSimpleStatistics();

            using var accessor = reader.CreateSingleChannelTileAccessor();

            var accessorOptions = AccessorOptions.Default;
            accessorOptions.BackGroundColorR = accessorOptions.BackGroundColorG = accessorOptions.BackGroundColorB = 255;
            using var bitmap = accessor.Get(
                new Coordinate([new DimensionAndValue(DimensionIndex.C, 0)]),
                new IntRect(2000, 0, 3000, 3000),
                1,
                accessorOptions);

            bitmap.BitmapInfo.PixelType.Should().Be(PixelType.Gray16);
            bitmap.BitmapInfo.Width.Should().Be(3000);
            bitmap.BitmapInfo.Height.Should().Be(3000);

            using Image image = TestUtilities.CreateImageSharpImageFromBitmap(bitmap);

            var hash = TestUtilities.CalculateImageHashGetAsString(image);
            hash.Should().Be("e4593f533beebc731ad397d79fa4e29e0d281ebe152b911977fcca66a99692ae");

            /*
            // Save the image as a PNG.
            image.Save("N:/output2.png", new SixLabors.ImageSharp.Formats.Png.PngEncoder());
            */
        }

    }
}
