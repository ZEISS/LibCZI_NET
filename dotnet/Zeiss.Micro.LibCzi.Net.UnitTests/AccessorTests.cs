// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using System;
    using FluentAssertions;
    using Net.Interface;
    using Xunit.Abstractions;

    using SixLabors.ImageSharp;

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

        [Fact]
        public void SingleChannelScalingTileAccessorWithMaskGray8Scenario1MaskAware()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.GetTwoOverlappingSubBlocksGray8WithMaskDataCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var accessor = reader.CreateSingleChannelTileAccessor();

            var accessorOptions = AccessorOptions.Default;
            accessorOptions.BackGroundColorR = accessorOptions.BackGroundColorG = accessorOptions.BackGroundColorB = 0.5f;
            accessorOptions.AdditionalOptionsPropertyBag = new Dictionary<string, object>
            {
                {AccessorOptionsAdditionalOptionsKeys.MaskAwareness, true},
            };

            using var bitmap = accessor.Get(
                new Coordinate([new DimensionAndValue(DimensionIndex.C, 0)]),
                new IntRect(0, 0, 6, 6),
                1,
                accessorOptions);

            bitmap.BitmapInfo.PixelType.Should().Be(PixelType.Gray8);
            bitmap.BitmapInfo.Width.Should().Be(6);
            bitmap.BitmapInfo.Height.Should().Be(6);

            var bitmapLockInfo = bitmap.Lock();
            try
            {
                // The expected result is a 6x6 image where:
                // - The background is gray (128,128,128).
                // - then, the first sub-block (black, 0) is drawn at (0,0) - (4,4)
                // - then, the second sub-block (white, 255) is drawn at (2,2) - (6,6) with the checkerboard mask applied
                byte[] expectedResult =
                [
                    0x00, 0x00, 0x00, 0x00, 0x80, 0x80,
                    0x00, 0x00, 0x00, 0x00, 0x80, 0x80,
                    0x00, 0x00, 0xff, 0x00, 0xff, 0x80,
                    0x00, 0x00, 0x00, 0xff, 0x80, 0xff,
                    0x80, 0x80, 0xff, 0x80, 0xff, 0x80,
                    0x80, 0x80, 0x80, 0xff, 0x80, 0xff,
                ];

                unsafe
                {
                    fixed (byte* pExpected = expectedResult)
                    {
                        for (int y = 0; y < 6; y++)
                        {
                            Span<byte> resultData = new((void*)(bitmapLockInfo.BitmapData + y * bitmapLockInfo.Stride), 6);
                            Span<byte> expectedResultData = new(pExpected + y * 6, 6);
                            bool areEqual = resultData.SequenceEqual(expectedResultData);
                            areEqual.Should().BeTrue();
                        }
                    }
                }
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        [Fact]
        public void SingleChannelScalingTileAccessorWithMaskGray8Scenario1MaskUnaware()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.GetTwoOverlappingSubBlocksGray8WithMaskDataCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var accessor = reader.CreateSingleChannelTileAccessor();

            var accessorOptions = AccessorOptions.Default;
            accessorOptions.BackGroundColorR = accessorOptions.BackGroundColorG = accessorOptions.BackGroundColorB = 0.5f;
            accessorOptions.AdditionalOptionsPropertyBag = new Dictionary<string, object>
            {
                {AccessorOptionsAdditionalOptionsKeys.MaskAwareness, false},
            };

            using var bitmap = accessor.Get(
                new Coordinate([new DimensionAndValue(DimensionIndex.C, 0)]),
                new IntRect(0, 0, 6, 6),
                1,
                accessorOptions);

            bitmap.BitmapInfo.PixelType.Should().Be(PixelType.Gray8);
            bitmap.BitmapInfo.Width.Should().Be(6);
            bitmap.BitmapInfo.Height.Should().Be(6);

            var bitmapLockInfo = bitmap.Lock();
            try
            {
                // The expected result is a 6x6 image where:
                // - The background is gray (128,128,128).
                // - then, the first sub-block (black, 0) is drawn at (0,0) - (4,4)
                // - then, the second sub-block (white, 255) is drawn at (2,2) - (6,6) *without* the checkerboard mask applied
                byte[] expectedResult =
                [
                    0x00, 0x00, 0x00, 0x00, 0x80, 0x80,
                    0x00, 0x00, 0x00, 0x00, 0x80, 0x80,
                    0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
                    0x00, 0x00, 0xff, 0xff, 0xff, 0xff,
                    0x80, 0x80, 0xff, 0xff, 0xff, 0xff,
                    0x80, 0x80, 0xff, 0xff, 0xff, 0xff,
                ];

                unsafe
                {
                    fixed (byte* pExpected = expectedResult)
                    {
                        for (int y = 0; y < 6; y++)
                        {
                            Span<byte> resultData = new((void*)(bitmapLockInfo.BitmapData + y * bitmapLockInfo.Stride), 6);
                            Span<byte> expectedResultData = new(pExpected + y * 6, 6);
                            bool areEqual = resultData.SequenceEqual(expectedResultData);
                            areEqual.Should().BeTrue();
                        }
                    }
                }
            }
            finally
            {
                bitmap.Unlock();
            }
        }
    }
}
