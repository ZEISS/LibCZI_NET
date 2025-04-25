// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using FluentAssertions;
    using LibCZI_Net.Interface;
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
                /*inputStream = Factory.CreateInputStream(
                    "curl_http_inputstream",
                    "https://zenodo.org/records/14968770/files/2025_01_27__0007_offline_Zen_3_9_5.czi?download=1",
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

            int numberOfAttachments = reader.GetAttachmentsCount();
            Assert.Equal(6, numberOfAttachments);
        }

        [Fact]
        public void CallEnumerateAttachmentsAndCheckResult()
        {
            IInputStream inputStream;
            try
            {
                /*inputStream = Factory.CreateInputStream(
                    "curl_http_inputstream",
                    "https://zenodo.org/records/14968770/files/2025_01_27__0007_offline_Zen_3_9_5.czi?download=1",
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
                /*inputStream = Factory.CreateInputStream(
                    "curl_http_inputstream",
                    "https://zenodo.org/records/14968770/files/2025_01_27__0007_offline_Zen_3_9_5.czi?download=1",
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
    }
}
