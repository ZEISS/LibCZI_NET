// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using Zeiss.Micro.LibCzi.Net.Interface;
    using Zeiss.Micro.LibCzi.Net.Interop;
    using Xunit;

    public class LibCziApiInteropTests
    {
        [Fact]
        public void CallGetVersionInfoAndCheckResult()
        {
            var interopInstance = LibCziApiInterop.Instance;
            var versionInfo = interopInstance.GetVersionInfo();
            Assert.True(versionInfo.Major >= 0);
            Assert.True(versionInfo.Minor >= 0);
            Assert.True(versionInfo.Patch >= 0);
            Assert.True(versionInfo.Tweak >= 0);
        }

        [Fact]
        public void CallGetBuildInformationAndCheckResult()
        {
            var interopInstance = LibCziApiInterop.Instance;
            var versionInfo = interopInstance.GetBuildInformation();

            // Check that at least one of the fields is not empty - there is not much more we can do here
            Assert.False(
                string.IsNullOrEmpty(versionInfo.CompilerIdentification) &&
                string.IsNullOrEmpty(versionInfo.RepositoryUrl) &&
                string.IsNullOrEmpty(versionInfo.RepositoryBranch) &&
                string.IsNullOrEmpty(versionInfo.RepositoryTag));
        }

        [Fact]
        public void ConstructExternalInputStreamAndCheckBasicOperation()
        {
            Stream stream = new MemoryStream(Data.GetTestCzi());
            var inputStreamHandler = new InputStreamObject(stream, true);
            var interopInstance = LibCziApiInterop.Instance;

            var inputStreamObjectHandle = interopInstance.CreateInputStreamFromExternal(inputStreamHandler);

            var readerObjectHandle = interopInstance.CreateReader();

            interopInstance.ReaderOpen(readerObjectHandle, inputStreamObjectHandle);

            interopInstance.ReleaseInputStream(inputStreamObjectHandle);

            var subBlockStatistics = interopInstance.ReaderGetSubBlockStatistics(readerObjectHandle);
            Assert.Equal(1, subBlockStatistics.SubBlockCount);
            Assert.Equal(0, subBlockStatistics.MinimumMIndex);
            Assert.Equal(0, subBlockStatistics.MaximumMIndex);
            Assert.True(subBlockStatistics.BoundingBox.X == 0 && subBlockStatistics.BoundingBox.Y == 0 && subBlockStatistics.BoundingBox.Width == 1 && subBlockStatistics.BoundingBox.Height == 1);
            Assert.True(subBlockStatistics.BoundingBoxLayer0.X == 0 && subBlockStatistics.BoundingBoxLayer0.Y == 0 && subBlockStatistics.BoundingBoxLayer0.Width == 1 && subBlockStatistics.BoundingBoxLayer0.Height == 1);
            Assert.True(subBlockStatistics.DimensionBounds.TryGetDimension(DimensionIndex.C, out int startC, out int sizeC) && startC == 0 && sizeC == 1);
            Assert.True(subBlockStatistics.DimensionBounds.TryGetDimension(DimensionIndex.C, out int startT, out int sizeT) && startT == 0 && sizeT == 1);

            interopInstance.ReleaseReader(readerObjectHandle);
        }

        [Fact]
        public void ConstructExternalInputStreamAndTestBitmapOperation()
        {
            Stream stream = new MemoryStream(Data.GetTestCzi());
            var inputStreamHandler = new InputStreamObject(stream, true);
            var interopInstance = LibCziApiInterop.Instance;
            var inputStreamObjectHandle = interopInstance.CreateInputStreamFromExternal(inputStreamHandler);
            var readerObjectHandle = interopInstance.CreateReader();
            interopInstance.ReaderOpen(readerObjectHandle, inputStreamObjectHandle);
            interopInstance.ReleaseInputStream(inputStreamObjectHandle);

            var subBlockHandle = interopInstance.ReaderReadSubBlock(readerObjectHandle, 0);
            var bitmapHandle = interopInstance.SubBlockCreateBitmap(subBlockHandle);

            var bitmapInfo = interopInstance.BitmapGetInfo(bitmapHandle);
            Assert.Equal(1, bitmapInfo.Width);
            Assert.Equal(1, bitmapInfo.Height);
            Assert.Equal(PixelType.Gray8, bitmapInfo.PixelType);

            var bitmapLockInfo = interopInstance.BitmapLock(bitmapHandle);
            Assert.True(bitmapLockInfo.Stride >= 1);
            Assert.True(bitmapLockInfo.BitmapData != IntPtr.Zero);

            interopInstance.BitmapUnlock(bitmapHandle);

            interopInstance.ReleaseBitmap(bitmapHandle);
            interopInstance.ReleaseSubBlock(subBlockHandle);
            interopInstance.ReleaseReader(readerObjectHandle);
        }

        [Fact]
        public void ConstructExternalInputStreamAndTestReaderGetPyramidStatisticsAsJson()
        {
            Stream stream = new MemoryStream(Data.Get3ScenesCzi());
            var inputStreamHandler = new InputStreamObject(stream, true);
            var interopInstance = LibCziApiInterop.Instance;
            var inputStreamObjectHandle = interopInstance.CreateInputStreamFromExternal(inputStreamHandler);
            var readerObjectHandle = interopInstance.CreateReader();
            interopInstance.ReaderOpen(readerObjectHandle, inputStreamObjectHandle);
            interopInstance.ReleaseInputStream(inputStreamObjectHandle);

            string pyramidStatisticsJson = interopInstance.ReaderGetPyramidStatisticsAsJson(readerObjectHandle);

            Assert.False(string.IsNullOrEmpty(pyramidStatisticsJson));

            interopInstance.ReleaseReader(readerObjectHandle);
        }
    }
}