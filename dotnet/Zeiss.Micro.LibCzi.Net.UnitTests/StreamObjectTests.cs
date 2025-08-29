// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using Interface;
    using Interop;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary> 
    /// Unit-tests for the "stream-object functionality". This includes tests concerned
    /// with the "external-streams-functionality".
    /// </summary>
    public partial class StreamObjectTests
    {
        private readonly ITestOutputHelper output;

        public StreamObjectTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        public void EnumerateInputStreamClassesTest()
        {
            StreamClassesRepository repository = new StreamClassesRepository();
            IEnumerable<InputStreamClassInfo> streamClasses = repository.EnumerateStreamClasses();
            Assert.NotNull(streamClasses);
            Assert.NotEmpty(streamClasses);
            var listOfInputStreamClassInfo = streamClasses.ToList();

            // Check that all elements in listOfInputStreamClassInfo contain non-empty strings for Name and Description
            Assert.All(listOfInputStreamClassInfo, item =>
            {
                Assert.False(string.IsNullOrEmpty(item.Name), "Name should not be null or empty");
                Assert.False(string.IsNullOrEmpty(item.Description), "Description should not be null or empty");
            });
        }

        [Fact]
        public void TryToGetInvalidInputStreamClassAndExpectExceptionTest()
        {
            int streamClassesCount = LibCziApiInterop.Instance.GetStreamClassesCount();

            // Check that an exception is thrown when trying to get an invalid input stream class
            Assert.ThrowsAny<Exception>(() =>
            {
                var streamClassInfo = LibCziApiInterop.Instance.GetStreamClassInfo(streamClassesCount + 1);
            });

            Assert.ThrowsAny<Exception>(() =>
            {
                var streamClassInfo = LibCziApiInterop.Instance.GetStreamClassInfo(-1);
            });
        }

        [Fact]
        public void EnumerateInputStreamClassesWithFactoryTest()
        {
            var inputStreamClassesRepository = Factory.CreateStreamClassesRepository();
            Assert.NotNull(inputStreamClassesRepository);
            var listOfInputStreamClassInfo = inputStreamClassesRepository.EnumerateStreamClasses().ToList();

            // Check that all elements in listOfInputStreamClassInfo contain non-empty strings for Name and Description
            Assert.All(listOfInputStreamClassInfo, item =>
            {
                Assert.False(string.IsNullOrEmpty(item.Name), "Name should not be null or empty");
                Assert.False(string.IsNullOrEmpty(item.Description), "Description should not be null or empty");
            });
            ;
        }

        [Fact]
        public void TryToCreateInputStreamObjectWithInvalidUriAndExpectExceptionTest()
        {
            var listOfInputStreamClassInfo = Factory.CreateStreamClassesRepository().EnumerateStreamClasses().ToList();

            Assert.ThrowsAny<Exception>(() =>
            {
                LibCziApiInterop.Instance.CreateInputStream(listOfInputStreamClassInfo[0].Name, string.Empty, "an invalid uri");
            });
        }

        [Fact]
        public void TryToCreateHttpInputStreamObjectTest()
        {
            IntPtr handleInputStream;
            try
            {
                handleInputStream = LibCziApiInterop.Instance.CreateInputStream(
                    "curl_http_inputstream",
                    null,
                    "https://media.githubusercontent.com/media/ptahmose/libCZI_testdata/main/MD5/ff20e3a15d797509f7bf494ea21109d3");
            }
            catch (Exception ex)
            {
                this.output.WriteLine(
                    $"Creating the input-stream failed to exception: {ex.Message}. Assuming this is due to problems like 'resource not available', no internet or the like, we skip this test.");
                return; // Exit the test early
            }

            var handleReader = LibCziApiInterop.Instance.CreateReader();
            LibCziApiInterop.Instance.ReaderOpen(handleReader, handleInputStream);
            SubBlockStatistics subBlockStatistics = LibCziApiInterop.Instance.ReaderGetSubBlockStatistics(handleReader);

            LibCziApiInterop.Instance.ReleaseReader(handleReader);
            LibCziApiInterop.Instance.ReleaseInputStream(handleInputStream);

            Assert.Equal(2, subBlockStatistics.SubBlockCount);
            Assert.Equal(1, subBlockStatistics.MinimumMIndex);
            Assert.Equal(2, subBlockStatistics.MaximumMIndex);
            Assert.Equal("Z0:3C0:1T0:1S0:1", subBlockStatistics.DimensionBounds.AsString());
        }

        [Fact]
        public void TryToCreateHttpInputStreamObjectWithPropertyBagAtInteropLevelTest()
        {
            IntPtr handleInputStream;
            try
            {
                handleInputStream = LibCziApiInterop.Instance.CreateInputStream(
                    "curl_http_inputstream",
                    "{\"CurlHttp_UserAgent\":\"libCZI\"}",
                    "https://media.githubusercontent.com/media/ptahmose/libCZI_testdata/main/CZICheck_Testdata/sparse_planes.czi");
            }
            catch (Exception ex)
            {
                this.output.WriteLine(
                    $"Creating the input-stream failed to exception: {ex.Message}. Assuming this is due to problems like 'resource not available', no internet or the like, we skip this test.");
                return; // Exit the test early
            }

            LibCziApiInterop.Instance.ReleaseInputStream(handleInputStream);
        }

        [Fact]
        public void TryToCreateHttpInputStreamObjectWithPropertyBagTest()
        {
            IInputStream inputStream;
            try
            {
                inputStream = Factory.CreateInputStream(
                    "curl_http_inputstream",
                    "https://media.githubusercontent.com/media/ptahmose/libCZI_testdata/main/CZICheck_Testdata/sparse_planes.czi",
                    new Dictionary<string, object>()
                    {
                        { StreamClassPropertyKeys.CurlHttpUserAgent, "libCZI"},
                    });
            }
            catch (Exception ex)
            {
                this.output.WriteLine(
                    $"Creating the input-stream failed to exception: {ex.Message}. Assuming this is due to problems like 'resource not available', no internet or the like, we skip this test.");
                return; // Exit the test early
            }

            using var reader = Factory.CreateReader();
            reader.Open(inputStream);
            var subBlockStatistics = reader.GetSimpleStatistics();
            Assert.Equal(2, subBlockStatistics.SubBlockCount);
            Assert.Equal(1, subBlockStatistics.MinimumMIndex);
            Assert.Equal(2, subBlockStatistics.MaximumMIndex);
            Assert.Equal("Z0:3C0:1T0:1S0:1", subBlockStatistics.DimensionBounds.AsString());
        }

        [Fact]
        public void ExternalInputStreamCheckIfExceptionWithReadIsHandledProperlyTest()
        {
            // We create an external input stream using an object that throws an exception upon invocation.
            // We then check that an exception is thrown when we try to read from the stream. Since the error handling
            // in this case straddles the managed/unmanaged boundary, it is non-trivial that the exception is properly
            // thrown in the managed code below.
            // TODO(JBL): when the error handling is refined, we should adapt this test to check for the specific exception type.
            using var inputStream = Factory.CreateInputStreamFromExternalStream(new ExternalInputStreamThrowingException());
            using var reader = Factory.CreateReader();
            Assert.ThrowsAny<Exception>(() =>
            {
                reader.Open(inputStream);
            });
        }

        [Fact]
        public void ExternalOutputStreamCheckIfExceptionWithReadIsHandledProperlyTest()
        {
            // We create an external output stream using an object that throws an exception upon invocation.
            // We then check that an exception is thrown when we try to write to the stream.
            // TODO(JBL): when the error handling is refined, we should adapt this test to check for the specific exception type.
            using var outputStream = Factory.CreateOutputStreamFromExternalStream(new ExternalOutputStreamThrowingException());
            using var writer = Factory.CreateWriter();
            Assert.ThrowsAny<Exception>(() =>
            {
                writer.Open(outputStream);
            });
        }
    }

    /// <content>
    /// Internally used classes for the tests are found here.
    /// </content>
    public partial class StreamObjectTests
    {
        /// <summary>
        /// Implementation of the 'IExternalInputStream' which throws an exception upon
        /// invocation.
        /// </summary>
        private class ExternalInputStreamThrowingException : IExternalInputStream
        {
            public void Read(long offset, IntPtr data, long size, out long bytesRead)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                // nothing to do
            }
        }

        /// <summary>
        /// Implementation of the 'IExternalOutputStream' which throws an exception upon
        /// invocation.
        /// </summary>
        private class ExternalOutputStreamThrowingException : IExternalOutputStream
        {
            public void Write(long offset, IntPtr data, long size, out long bytesWritten)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                // nothing to do
            }
        }
    }
}