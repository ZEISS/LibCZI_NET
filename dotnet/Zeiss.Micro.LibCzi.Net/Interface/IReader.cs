// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using System;

    /// <summary>
    /// Interface for the reader object.
    /// </summary>
    public interface IReader : IDisposable
    {
        /// <summary> Opens the specified stream and reads the global information from the CZI-document.</summary>
        /// <param name="inputStream"> Stream to read data from.</param>
        void Open(IInputStream inputStream);

        /// <summary>
        /// Retrieves simple statistics about the sub-blocks of data.
        /// </summary>
        /// <returns>An instance of <see cref="SubBlockStatistics"/> that contains statistics for sub-blocks of data.</returns>
        SubBlockStatistics GetSimpleStatistics();

        /// <summary>
        /// Retrieves extended statistics about the sub-blocks of data.
        /// </summary>
        /// <returns>An instance of <see cref="SubBlockStatisticsEx"/> that contains extended statistics for sub-blocks of data.</returns>
        SubBlockStatisticsEx GetExtendedStatistics();

        /// <summary>
        /// Gets the pyramid information.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IPyramidStatistics"/> containing pyramid statistics.
        /// <see cref="IPyramidStatistics"/> holds <see cref="PyramidLayerInfo"/> and SubBlockCount for each layer in the pyramid.
        /// </returns>
        IPyramidStatistics GetPyramidStatistics();

        /// <summary>
        /// Reads a sub-block of data at the specified index.
        /// </summary>
        /// <param name="index">The sub-block index that is going to be read.</param>
        /// <returns>An instance of <see cref="ISubBlock"/> that represents the specified sub-block.</returns>
        ISubBlock ReadSubBlock(int index);

        /// <summary> Reads the metadata segment from the stream.</summary>
        /// <returns> The metadata segment object.</returns>
        IMetadataSegment GetMetadataSegment();

        /// <summary> Gets the count of attachments in the document.</summary>
        /// <returns> The attachments count.</returns>
        int GetAttachmentsCount();

        /// <summary>
        /// Attempts to get the attachment information for the specified index.
        /// </summary>
        /// <param name="index">      The index of the attachment.</param>
        /// <param name="attachment"> [out] The requested information about the attachment.</param>
        /// <returns> True if it succeeds, false if it fails.</returns>
        bool TryGetAttachmentInfoForIndex(int index, out AttachmentInfo attachment);

        /// <summary>
        /// Reads  attachment the attachment with the specified index.
        /// </summary>
        /// <remarks>
        /// TODO(JBL): check and document the behavior if the index is out of range.
        /// </remarks>
        /// <param name="index"> The index of the attachment that is going to be read.</param>
        /// <returns> The attachment.</returns>
        IAttachment ReadAttachment(int index);

        /// <summary>
        /// Gets the file header information from the CZI document.
        /// </summary>
        /// <returns>A struct FileHeaderInfo that contains GUID, major, and minor CZI version levels.</returns>
        FileHeaderInfo GetFileHeaderInfo();

        /// <summary> Creates a new scaling single-channel accessor.</summary>
        /// <returns> The new accessor instance.</returns>
        ISingleChannelTileAccessor CreateSingleChannelTileAccessor();
    }
}
