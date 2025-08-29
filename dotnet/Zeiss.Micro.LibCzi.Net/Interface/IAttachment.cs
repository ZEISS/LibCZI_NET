// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using System;

    /// <summary>
    /// Represents an attachment - containing a blob of data.
    /// </summary>
    public interface IAttachment : IDisposable
    {
        /// <summary> Gets information describing the attachment.</summary>
        /// <value> Information describing the attachment.</value>
        AttachmentInfo AttachmentInfo { get; }

        /// <summary> Gets the data contained in the attachment as a blob of data.</summary>
        /// <returns> The data contained in the attachment.</returns>
        Span<byte> GetRawData();
    }
}
