// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using FluentAssertions;
    using Net.Interface;

    public class WriterTests
    {
        [Fact]
        public void ConstructExternalOutStreamAndCreateEmptyCzi()
        {
            using var memoryStream = new MemoryStream();

            {
                using var outputStream = Factory.CreateOutputStreamFromExternalStream(new OutputStreamObject(memoryStream, false));

                using var writer = Factory.CreateWriter();
                writer.Open(outputStream);
                writer.Close();
            }

            Assert.True(memoryStream.Length > 0);
        }

        [Fact]
        public void ConstructExternalOutStreamAndCreateCziWithSingleSubBlockAndReadItAndCheckContent()
        {
            using MemoryStream memoryStream = new MemoryStream();

            {
                using var outputStream = Factory.CreateOutputStreamFromExternalStream(new OutputStreamObject(memoryStream, false));

                using var writer = Factory.CreateWriter();
                writer.Open(outputStream);

                byte[] pixelData = [1, 2, 3, 4, 5, 6];

                AddSubBlockInfoUncompressed addSubBlockInfoUncompressed = new AddSubBlockInfoUncompressed
                {
                    AddSubBlockInfo = new AddSubBlockInfo
                    {
                        Coordinate = new Coordinate([new DimensionAndValue(DimensionIndex.C, 0), new DimensionAndValue(DimensionIndex.T, 0)]),
                        MindexValid = false,
                        Mindex = 0,
                        X = 0,
                        Y = 0,
                        LogicalWidth = 2,
                        LogicalHeight = 3,
                        PhysicalWidth = 2,
                        PhysicalHeight = 3,
                        PixelType = PixelType.Gray8,
                    },
                    Stride = 2,
                };

                AddSubBlockData addSubBlockData = new AddSubBlockData { BitmapData = pixelData, };

                writer.AddSubBlockUncompressed(in addSubBlockInfoUncompressed, in addSubBlockData);
                writer.Close();
            }

            Assert.True(memoryStream.Length > 0);

            // Now, open the CZI (from memory) and check content
            {
                // Note that we are NOT giving away ownership of the memoryStream here (by using the InputStreamObject constructor with takeOwnership = false),
                //  since we still have a using-clause for it in this scope.
                using var inputStream = Factory.CreateInputStreamFromExternalStream(new InputStreamObject(memoryStream, false));

                using var reader = Factory.CreateReader();
                reader.Open(inputStream);

                var subBlockStatisticsEx = reader.GetExtendedStatistics();
                subBlockStatisticsEx.SubBlockCount.Should().Be(1);
                subBlockStatisticsEx.BoundingBox.X.Should().Be(0);
                subBlockStatisticsEx.BoundingBox.Y.Should().Be(0);
                subBlockStatisticsEx.BoundingBox.Width.Should().Be(2);
                subBlockStatisticsEx.BoundingBox.Height.Should().Be(3);

                using var subBlock = reader.ReadSubBlock(0);
                subBlock.SubBlockInfo.LogicalRect.X.Should().Be(0);
                subBlock.SubBlockInfo.LogicalRect.Y.Should().Be(0);
                subBlock.SubBlockInfo.LogicalRect.Width.Should().Be(2);
                subBlock.SubBlockInfo.LogicalRect.Height.Should().Be(3);
                subBlock.SubBlockInfo.PhysicalSize.Width.Should().Be(2);
                subBlock.SubBlockInfo.PhysicalSize.Height.Should().Be(3);
                Utilities.CoordinateToString(subBlock.SubBlockInfo.Coordinate).Should().Be("C0T0");
                using var bitmap = subBlock.GetBitmap();

                bitmap.ProcessReadOnlyLockedMemory(
                    (ReadOnlyMemory<byte> bitmapMemory, int stride) =>
                    {
                        ReadOnlySpan<byte> span = bitmapMemory.Span;
                        for (int y = 0; y < 3; y++)
                        {
                            for (int x = 0; x < 2; x++)
                            {
                                Assert.Equal(y * 2 + x + 1, span[y * stride + x]);
                            }
                        }
                    });
            }
        }

        [Fact]
        public void ConstructExternalOutStreamAndCreateCziWithSingleSubBlockAndAttachmentsAndReadItAndCheckContent()
        {
            using MemoryStream memoryStream = new MemoryStream();

            {
                using var outputStream = Factory.CreateOutputStreamFromExternalStream(new OutputStreamObject(memoryStream, false));

                using var writer = Factory.CreateWriter();
                writer.Open(outputStream);

                byte[] pixelData = [1, 2, 3, 4, 5, 6];

                AddSubBlockInfoUncompressed addSubBlockInfoUncompressed = new AddSubBlockInfoUncompressed
                {
                    AddSubBlockInfo = new AddSubBlockInfo
                    {
                        Coordinate = new Coordinate([new DimensionAndValue(DimensionIndex.C, 0), new DimensionAndValue(DimensionIndex.T, 0)]),
                        MindexValid = false,
                        Mindex = 0,
                        X = 0,
                        Y = 0,
                        LogicalWidth = 2,
                        LogicalHeight = 3,
                        PhysicalWidth = 2,
                        PhysicalHeight = 3,
                        PixelType = PixelType.Gray8,
                    },
                    Stride = 2,
                };

                AddSubBlockData addSubBlockData = new AddSubBlockData { BitmapData = pixelData, };

                writer.AddSubBlockUncompressed(in addSubBlockInfoUncompressed, in addSubBlockData);

                byte[] attachmentData = [1, 2, 3, 4, 5, 6];
                AddAttachmentInfo addAttachmentInfo = new AddAttachmentInfo
                {
                    Guid = Guid.Parse("A53682E6-BFD1-4D7A-A6AD-53FFE568DAF5"),
                    ContentFileType = "txt",
                    Name = "TEST-attachment",
                };

                writer.AddAttachment(in addAttachmentInfo, attachmentData);

                writer.Close();
            }

            Assert.True(memoryStream.Length > 0);

            // Now, open the CZI (from memory) and check content
            {
                // Note that we are NOT giving away ownership of the memoryStream here (by using the InputStreamObject constructor with takeOwnership = false),
                //  since we still have a using-clause for it in this scope.
                using var inputStream = Factory.CreateInputStreamFromExternalStream(new InputStreamObject(memoryStream, false));

                using var reader = Factory.CreateReader();
                reader.Open(inputStream);

                reader.GetAttachmentsCount().Should().Be(1);

                bool b = reader.TryGetAttachmentInfoForIndex(0, out AttachmentInfo attachmentInfo);
                b.Should().BeTrue();
                attachmentInfo.Guid.Should().Be(Guid.Parse("A53682E6-BFD1-4D7A-A6AD-53FFE568DAF5"));
                attachmentInfo.ContentFileType.Should().Be("txt");
                attachmentInfo.Name.Should().Be("TEST-attachment");
            }
        }

        [Fact]
        public void ConstructExternalOutputStreamAndSetFileGuidAndReadCziAndCheck()
        {
            using MemoryStream memoryStream = new MemoryStream();

            {
                using var outputStream = Factory.CreateOutputStreamFromExternalStream(new OutputStreamObject(memoryStream, false));

                using var writer = Factory.CreateWriter();
                writer.Open(
                    outputStream,
                    new Dictionary<string, object>()
                    {
                        { WriterOpenParameters.FileGuid, new Guid("12345678-9abc-def1-2345-6789abcdef12")}
                    });
                writer.Close();
            }

            Assert.True(memoryStream.Length > 0);

            // Now, open the CZI (from memory) and check content
            {
                using var inputStream = Factory.CreateInputStreamFromExternalStream(new InputStreamObject(memoryStream, false));

                using var reader = Factory.CreateReader();
                reader.Open(inputStream);

                var fileHeaderInfo = reader.GetFileHeaderInfo();
                Assert.Equal(new Guid("12345678-9abc-def1-2345-6789abcdef12"), fileHeaderInfo.Guid);
            }
        }
    }
}
