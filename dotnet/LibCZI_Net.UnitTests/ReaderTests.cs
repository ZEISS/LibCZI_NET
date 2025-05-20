// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.UnitTests
{
    using LibCZI_Net.Interface;
    using Xunit;

    public class ReaderTests
    {
        [Fact]
        public void ConstructExternalInputStreamAndTestBitmapOperation()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.GetTestCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var subBlock = reader.ReadSubBlock(0);

            using var bitmap = subBlock.GetBitmap();

            Assert.Equal(1, bitmap.BitmapInfo.Width);
            Assert.Equal(1, bitmap.BitmapInfo.Height);
            Assert.Equal(PixelType.Gray8, bitmap.BitmapInfo.PixelType);

            byte[] pixel = new byte[1];
            bitmap.Copy(1, 1, PixelType.Gray8, 1, pixel);

            Assert.Equal(0xff, pixel[0]);
        }

        [Fact]
        public void ConstructExternalInputStreamWith4TilesCziAndTestBitmapOperation()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.Get4TilesCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            for (int i = 0; i < 4; i++)
            {
                using var subBlock = reader.ReadSubBlock(i);
                using var bitmap = subBlock.GetBitmap();

                Assert.Equal(5, bitmap.BitmapInfo.Width);
                Assert.Equal(5, bitmap.BitmapInfo.Height);
                Assert.Equal(PixelType.Gray8, bitmap.BitmapInfo.PixelType);

                byte[] pixel = new byte[5 * 5];
                bitmap.Copy(5, 5, PixelType.Gray8, 5, pixel);

                // Check that the array "pixel" has the same value for each element, which should be i + 1
                for (int j = 0; j < pixel.Length; j++)
                {
                    Assert.Equal(i + 1, pixel[j]);
                }
            }
        }

        [Fact]
        public void ConstructExternalInputStreamWith4TilesCziAndUseLockUnlockInOrderToTestBitmapOperation()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.Get4TilesCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            for (int i = 0; i < 4; i++)
            {
                using var subBlock = reader.ReadSubBlock(i);
                using var bitmap = subBlock.GetBitmap();

                Assert.Equal(5, bitmap.BitmapInfo.Width);
                Assert.Equal(5, bitmap.BitmapInfo.Height);
                Assert.Equal(PixelType.Gray8, bitmap.BitmapInfo.PixelType);

                unsafe
                {
                    var lockInfo = bitmap.Lock();
                    try
                    {
                        byte* ptr = (byte*)lockInfo.BitmapData;
                        for (int y = 0; y < 5; y++)
                        {
                            for (int x = 0; x < 5; x++)
                            {
                                Assert.Equal(i + 1, ptr[y * lockInfo.Stride + x]);
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

        [Fact]
        public void ConstructExternalInputStreamWith3ScenesAndCheckGetSubBlockStatisticsEx()
        {
            using var inputStream = Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.Get3ScenesCzi()), true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            var subBlockStatisticsEx = reader.GetExtendedStatistics();

            string dimensionBoundsAsString = subBlockStatisticsEx.DimensionBounds.ToString() ?? string.Empty;
            Assert.Equal("C0:1S0:3", dimensionBoundsAsString);

            Assert.Equal(0, subBlockStatisticsEx.PerScenesBoundingBoxes[0].BoundingBox.X);
            Assert.Equal(0, subBlockStatisticsEx.PerScenesBoundingBoxes[0].BoundingBox.Y);
            Assert.Equal(15, subBlockStatisticsEx.PerScenesBoundingBoxes[0].BoundingBox.Width);
            Assert.Equal(15, subBlockStatisticsEx.PerScenesBoundingBoxes[0].BoundingBox.Height);

            Assert.Equal(20, subBlockStatisticsEx.PerScenesBoundingBoxes[1].BoundingBox.X);
            Assert.Equal(20, subBlockStatisticsEx.PerScenesBoundingBoxes[1].BoundingBox.Y);
            Assert.Equal(6, subBlockStatisticsEx.PerScenesBoundingBoxes[1].BoundingBox.Width);
            Assert.Equal(3, subBlockStatisticsEx.PerScenesBoundingBoxes[1].BoundingBox.Height);

            Assert.Equal(30, subBlockStatisticsEx.PerScenesBoundingBoxes[2].BoundingBox.X);
            Assert.Equal(30, subBlockStatisticsEx.PerScenesBoundingBoxes[2].BoundingBox.Y);
            Assert.Equal(2, subBlockStatisticsEx.PerScenesBoundingBoxes[2].BoundingBox.Width);
            Assert.Equal(2, subBlockStatisticsEx.PerScenesBoundingBoxes[2].BoundingBox.Height);
        }

        [Fact]
        public void ConstructExternalInputStreamWith3ScenesAndCheckPyramidStatistics()
        {
            using var inputStream = Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.Get3ScenesCzi()), true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            var pyramidStatistics = reader.GetPyramidStatistics();

            Assert.Equal(3, pyramidStatistics.ScenePyramidStatistics.Count);
            Assert.True(pyramidStatistics.ScenePyramidStatistics.TryGetValue(0, out _));
            Assert.Single(pyramidStatistics.ScenePyramidStatistics[0]);
            Assert.Equal(0, pyramidStatistics.ScenePyramidStatistics[0][0].LayerInfo.MinificationFactor);
            Assert.Equal(0, pyramidStatistics.ScenePyramidStatistics[0][0].LayerInfo.PyramidLayerNo);
            Assert.Equal(4, pyramidStatistics.ScenePyramidStatistics[0][0].SubBlockCount);
            Assert.True(pyramidStatistics.ScenePyramidStatistics[0][0].LayerInfo.IsLayerZero);
            Assert.False(pyramidStatistics.ScenePyramidStatistics[0][0].LayerInfo.IsNotIdentifiedAsPyramidLayer);

            Assert.True(pyramidStatistics.ScenePyramidStatistics.TryGetValue(1, out _));
            Assert.Single(pyramidStatistics.ScenePyramidStatistics[1]);
            Assert.Equal(0, pyramidStatistics.ScenePyramidStatistics[1][0].LayerInfo.MinificationFactor);
            Assert.Equal(0, pyramidStatistics.ScenePyramidStatistics[1][0].LayerInfo.PyramidLayerNo);
            Assert.Equal(2, pyramidStatistics.ScenePyramidStatistics[1][0].SubBlockCount);
            Assert.True(pyramidStatistics.ScenePyramidStatistics[1][0].LayerInfo.IsLayerZero);
            Assert.False(pyramidStatistics.ScenePyramidStatistics[1][0].LayerInfo.IsNotIdentifiedAsPyramidLayer);

            Assert.True(pyramidStatistics.ScenePyramidStatistics.TryGetValue(2, out _));
            Assert.Single(pyramidStatistics.ScenePyramidStatistics[2]);
            Assert.Equal(0, pyramidStatistics.ScenePyramidStatistics[2][0].LayerInfo.MinificationFactor);
            Assert.Equal(0, pyramidStatistics.ScenePyramidStatistics[2][0].LayerInfo.PyramidLayerNo);
            Assert.Equal(1, pyramidStatistics.ScenePyramidStatistics[2][0].SubBlockCount);
            Assert.True(pyramidStatistics.ScenePyramidStatistics[2][0].LayerInfo.IsLayerZero);
            Assert.False(pyramidStatistics.ScenePyramidStatistics[2][0].LayerInfo.IsNotIdentifiedAsPyramidLayer);

        }

        [Fact]
        public void ConstructExternalInputStreamAndTestFileHeaderInfo()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.GetTestCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            var fileHeaderInfo = reader.GetFileHeaderInfo();

            Assert.True(fileHeaderInfo.MajorVersion >= 1);
            Assert.True(fileHeaderInfo.MinorVersion >= 0);
            Assert.Equal("ef3c9716-a924-4155-88cb-cbeb19ec0779", fileHeaderInfo.Guid.ToString());
        }
    }
}