// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using FluentAssertions;
    using Zeiss.Micro.LibCzi.Net.Interface;
    using Xunit.Abstractions;

    public class AttachmentsTests
    {
        private readonly ITestOutputHelper output;

        public AttachmentsTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CallGetAttachmentsCountAndCheckResult()
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

            int numberOfAttachments = reader.GetAttachmentsCount();
            Assert.Equal(6, numberOfAttachments);
        }

        [Fact]
        public void CallEnumerateAttachmentsAndCheckResult()
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

            var attachmentInfos = reader.EnumerateAttachments().ToList();

            Assert.Equal(6, attachmentInfos.Count);

            var expectedResult = new List<AttachmentInfo>()
            {
                new AttachmentInfo(new Guid("{cfbb16ea-b0b3-40d2-a3ca-6c0b62907628}"), "CZEVL", "EventList"),
                new AttachmentInfo(new Guid("{2ccf99f9-90f3-4ec5-bcfa-2ea5fa14b520}"), "CZTIMS", "TimeStamps"),
                new AttachmentInfo(new Guid("{5fab4654-fab0-4ba5-a92b-219752bf8e1a}"), "JPG", "Thumbnail"),
                new AttachmentInfo(new Guid("{12808fd2-dbeb-453d-a773-4ffe4755a362}"), "CZI", "Label"),
                new AttachmentInfo(new Guid("{e6764efe-4ff2-4198-af60-ccbd37e9b33a}"), "CZI", "SlidePreview"),
                new AttachmentInfo(new Guid("{b9b8bea5-86a3-4f44-b8b7-19590904b5fb}"), "Zip-Comp", "Profile"),
            };

            attachmentInfos.Should().BeEquivalentTo(expectedResult);
        }

        [Fact]
        public void GetThumbnailAttachmentAndGetRawData()
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

            Guid thumbnailGuid = new Guid("{5fab4654-fab0-4ba5-a92b-219752bf8e1a}");
            int thumbnailAttachmentIndex = reader.EnumerateAttachments().Select((info, idx) => new { info, idx }).First(x => x.info.Guid == thumbnailGuid).idx;

            using var thumbnailAttachment = reader.ReadAttachment(thumbnailAttachmentIndex);

            var thumbnailData = thumbnailAttachment.GetRawData();

            thumbnailData.Length.Should().Be(16_965);
            byte[] hash = SHA256.HashData(thumbnailData);
            byte[] expectedHash =
            [
                0xf1, 0x50, 0x9b, 0xc6, 0x98, 0x06, 0x5f, 0x96, 0xe4, 0xf4, 0xd2, 0x2e, 0xb4, 0x5a,
                0xe3, 0x11, 0x31, 0xba, 0x58, 0x29, 0xe0, 0x49, 0xb3, 0x61, 0x54, 0x80, 0x3c, 0xde,
                0x5f, 0xec, 0x7e, 0x20
            ];

            hash.Should().Equal(expectedHash);
        }

        /// <summary>
        /// Writes multiple random attachments into a CZI stream, then reads them back to verify
        /// count, metadata (with truncation constraints), and payload integrity.
        /// </summary>
        [Fact]
        public void Write15RandomAttachmentsThenReadThenAndCompare()
        {
            const int numberOfAttachments = 15;

            // Generate randomized attachment metadata to write into the stream.
            List<AddAttachmentInfo> addAttachmentInfosToWrite = new List<AddAttachmentInfo>();
            var random = new Random(12345);
            const string randomChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            for (int i = 0; i < numberOfAttachments; i++)
            {
                int contentLength = random.Next(1, 21);
                int nameLength = random.Next(1, 100);
                string contentFileType = new string(Enumerable.Range(0, contentLength)
                    .Select(_ => randomChars[random.Next(randomChars.Length)])
                    .ToArray());
                string name = new string(Enumerable.Range(0, nameLength)
                    .Select(_ => randomChars[random.Next(randomChars.Length)])
                    .ToArray());
                addAttachmentInfosToWrite.Add(new AddAttachmentInfo
                {
                    Guid = Guid.NewGuid(),
                    ContentFileType = contentFileType,
                    Name = name,
                });
            }

            // Create randomized payload data for each attachment.
            List<byte[]> attachmentDataToWrite = new List<byte[]>(addAttachmentInfosToWrite.Count);
            for (int i = 0; i < addAttachmentInfosToWrite.Count; i++)
            {
                int dataLength = random.Next(1, 201);
                byte[] data = new byte[dataLength];
                random.NextBytes(data);
                attachmentDataToWrite.Add(data);
            }

            using MemoryStream memoryStream = new MemoryStream();

            {
                // Write attachments into an in-memory CZI stream.
                using IOutputStream? outputStream = Factory.CreateOutputStreamFromExternalStream(new OutputStreamObject(memoryStream, false));
                using IWriter? writer = Factory.CreateWriter();
                writer.Open(outputStream);

                for (int i = 0; i < numberOfAttachments; i++)
                {
                    writer.AddAttachment(addAttachmentInfosToWrite[i], attachmentDataToWrite[i]);
                }

                writer.Close();
            }

            Assert.True(memoryStream.Length > 0);

            // Now, open the CZI (from memory) and check content
            {
                // Note that we are NOT giving away ownership of the memoryStream here (by using the InputStreamObject constructor with takeOwnership = false),
                //  since we still have a using-clause for it in this scope.
                using IInputStream? inputStream = Factory.CreateInputStreamFromExternalStream(new InputStreamObject(memoryStream, false));

                using IReader? reader = Factory.CreateReader();
                reader.Open(inputStream);

                int attachmentsCount = reader.GetAttachmentsCount();
                Assert.Equal(numberOfAttachments, attachmentsCount);

                // Verify attachment info and payload round-trip with truncation constraints.
                for (int i = 0; i < numberOfAttachments; i++)
                {
                    bool foundAttachment = reader.TryGetAttachmentInfoForIndex(i, out AttachmentInfo attachmentInfo);
                    Assert.True(foundAttachment);

                    int attachmentIndex = addAttachmentInfosToWrite.FindIndex(info => info.Guid == attachmentInfo.Guid);
                    Assert.True(attachmentIndex >= 0);

                    AddAttachmentInfo expectedAttachmentInfo = addAttachmentInfosToWrite[attachmentIndex];
                    string expectedContentFileType = Truncate(expectedAttachmentInfo.ContentFileType, 8);
                    string expectedName = Truncate(expectedAttachmentInfo.Name, 79);

                    attachmentInfo.ContentFileType.Should().Be(expectedContentFileType);
                    attachmentInfo.Name.Should().Be(expectedName);

                    using IAttachment? attachment = reader.ReadAttachment(i);
                    attachment.Should().NotBeNull();
                    attachment.AttachmentInfo.ContentFileType.Should().Be(expectedContentFileType);
                    attachment.AttachmentInfo.Name.Should().Be(expectedName);
                    attachment.AttachmentInfo.Guid.Should().Be(expectedAttachmentInfo.Guid);
                    Span<byte> dataOfAttachment = attachment.GetRawData();
                    dataOfAttachment.ToArray().Should().Equal(attachmentDataToWrite[attachmentIndex]);
                }

                static string Truncate(string value, int maxLength)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return string.Empty;
                    }

                    return value.Length <= maxLength ? value : value.Substring(0, maxLength);
                }
            }
        }
    }
}
