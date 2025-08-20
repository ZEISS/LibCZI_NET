// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    /// <summary>
    /// An enum specifying the compression method.
    /// </summary>
    public enum CompressionMode : byte
    {
        /// <summary>
        /// Invalid compression type (this merely means that the compression mode is not known to libCZI).
        /// </summary>
        Invalid = 0xff,

        /// <summary>
        /// The data is uncompressed.
        /// </summary>
        UnCompressed = 0,

        /// <summary>
        /// The data is JPG-compressed.
        /// </summary>
        /// <remarks>
        /// This mode is deprecated and should not be used.
        /// </remarks>
        Jpg = 1,

        /// <summary>
        /// The data is JPG-XR-compressed.
        /// </summary>
        JpgXr = 4,

        /// <summary>
        /// The data is compressed with zstd.
        /// </summary>
        /// <remarks>
        /// This mode is deprecated and should not be used.
        /// </remarks>
        Zstd0 = 5,

        /// <summary>
        /// The data contains a header, followed by a zstd-compressed block.
        /// </summary>
        Zstd1 = 6,
    }
}
