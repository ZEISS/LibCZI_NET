// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;

    /// <summary>
    /// Defines a read function for external input streams.
    /// This interface extends <see cref="IDisposable"/> to ensure proper resource management.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IExternalInputStream : IDisposable
    {
        /// <summary>
        /// Reads data at the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="data">The data.</param>
        /// <param name="size">The size.</param>
        /// <param name="bytesRead">The number of bytes actually read.</param>
        void Read(long offset, IntPtr data, long size, out long bytesRead);
    }
}