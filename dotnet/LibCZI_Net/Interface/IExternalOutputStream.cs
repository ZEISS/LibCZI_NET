// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;

    /// <summary>
    /// Defines a write function for external output streams.
    /// This interface extends <see cref="IDisposable"/> to ensure proper resource management.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IExternalOutputStream : IDisposable
    {
        /// <summary>
        /// Write data at the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="data">The data.</param>
        /// <param name="size">The size.</param>
        /// <param name="bytesWritten">The number of bytes actually written.</param>
        void Write(long offset, IntPtr data, long size, out long bytesWritten);
    }
}