// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;

    /// <summary>
    /// This interface is used to represent a bitmap.
    /// </summary>
    public interface IBitmap : IDisposable
    {
        /// <summary> Gets information describing the bitmap.</summary>
        /// <value> Information describing the bitmap.</value>
        BitmapInfo BitmapInfo { get; }

        /// <summary>
        /// Gets a lock on the bitmap, allowing to access the pixel data.
        /// The memory must only be accessed while the lock is active.
        /// Calls to Lock und Unlock must be balanced. It is a fatal error
        /// if the lock is not unlocked before the bitmap is disposed.
        /// </summary>
        /// <returns> Information containing a pointer to the bitmap data.</returns>
        BitmapLockInfo Lock();

        /// <summary>
        /// Unlocks this instance.
        /// </summary>
        void Unlock();

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="pixelType">Type of the pixel.</param>
        /// <param name="stride">The stride.</param>
        /// <param name="ptr">The PTR.</param>
        void Copy(int width, int height, PixelType pixelType, int stride, IntPtr ptr);
    }
}