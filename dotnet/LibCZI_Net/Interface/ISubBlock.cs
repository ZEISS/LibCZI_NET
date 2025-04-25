// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a sub-block containing bitmap data and (optionally) sub-block metadata.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ISubBlock : IDisposable
    {
        /// <summary> Gets information describing the sub-block.</summary>
        /// <value> Information describing the sub-block.</value>
        SubBlockInfo SubBlockInfo { get; }

        /// <summary>
        /// Retrieves the bitmap associated with this sub-block.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IBitmap"/> that represents the bitmap data
        /// contained within this sub-block.
        /// </returns>
        IBitmap GetBitmap();

        /// <summary> Gets raw pixel data. In case of compression utilized, this is the compressed data.</summary>
        /// <returns> The raw pixel data.</returns>
        Span<byte> GetRawData();

        /// <summary>
        /// Gets raw metadata. This is containing an UTF8-encoded XML-document. Note that the content
        /// has not been validated (as part of this method call), and that it may be invalid. Also
        /// note the UTF8-encoding string might not be null-terminated.
        /// If there is no sub-block-metadata available, an empty span is returned.
        /// </summary>
        /// <returns> The raw sub-block metadata.</returns>
        Span<byte> GetRawMetadataData();

        /// <summary>
        /// Gets the sub-block metadata as an XDocument. If the sub-block metadata is not available, null is returned.
        /// </summary>
        /// <returns>The parsed sub-block metadata as an XDocument if available; null otherwise.</returns>
        XDocument GetMetadataAsXDocument();
    }
}