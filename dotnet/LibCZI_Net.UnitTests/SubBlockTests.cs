// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.UnitTests
{
    using LibCZI_Net.Interface;

    public class SubBlockTests
    {
        [Fact]
        public void GetSubBlockInfoAndCheckContentScenario1()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.GetTestCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var subBlock = reader.ReadSubBlock(0);
            Assert.NotNull(subBlock);

            var subBlockInfo = subBlock.SubBlockInfo;
            Assert.Equal(PixelType.Gray8, subBlockInfo.PixelType);
            Assert.Equal(1, subBlockInfo.PhysicalSize.Width);
            Assert.Equal(1, subBlockInfo.PhysicalSize.Height);
            Assert.Equal(0, subBlockInfo.LogicalRect.X);
            Assert.Equal(0, subBlockInfo.LogicalRect.Y);
            Assert.Equal(1, subBlockInfo.LogicalRect.Width);
            Assert.Equal(1, subBlockInfo.LogicalRect.Height);
            Assert.Equal(0, subBlockInfo.Mindex.GetValueOrDefault(-1));
            Assert.Equal("C0T0", subBlockInfo.Coordinate.AsString());
        }

        [Fact]
        public void GetSubDataAndCheckContentScenario1()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.GetTestCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var subBlock = reader.ReadSubBlock(0);
            Assert.NotNull(subBlock);

            var data = subBlock.GetRawData();
            Assert.Equal(1, data.Length);
            Assert.Equal(0xff, data[0]);
        }

        [Fact]
        public void GetSubBlockMetaDataAndCheckContentScenario1()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.GetTestCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var subBlock = reader.ReadSubBlock(0);
            Assert.NotNull(subBlock);

            var data = subBlock.GetRawMetadataData();
            Assert.Equal(0, data.Length);

            var xmlDoc = subBlock.GetMetadataAsXDocument();
            Assert.Null(xmlDoc);
        }

        [Fact]
        public void GetSubBlockInfoAndCheckCompressionModeScenarioUnCompressed()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.GetTestCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);

            using var subBlock = reader.ReadSubBlock(0);
            Assert.NotNull(subBlock);

            var subBlockInfo = subBlock.SubBlockInfo;
            Assert.Equal(0, subBlockInfo.CompressionModeRaw);
            Assert.Equal(CompressionMode.UnCompressed, subBlockInfo.CompressionMode);
        }

        [Fact]
        public void GetSubBlockInfoAndCheckCompressionModeScenarioJpgXr()
        {
            using var inputStream =
                Factory.CreateInputStreamFromExternalStream(new InputStreamObject(new MemoryStream(Data.GetTestCompressJpgXrCzi()),
                    true));

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);
            
            using var subBlock = reader.ReadSubBlock(0);
            Assert.NotNull(subBlock);

            var subBlockInfo = subBlock.SubBlockInfo;
            Assert.Equal(4, subBlockInfo.CompressionModeRaw);
            Assert.Equal(CompressionMode.JpgXr, subBlockInfo.CompressionMode);
        }
    }
}