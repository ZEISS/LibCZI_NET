// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;

    /// <summary>
    /// Provides information about a locked bitmap, including a pointer to the bitmap data
    /// and the stride.
    /// </summary>
    public struct BitmapLockInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapLockInfo"/> struct.
        /// </summary>
        /// <param name="pointerBitmap">Pointer to the locked bitmap.</param>
        /// <param name="stride">The total number of bytes between the start of one row of the bitmap and the start of the next row.
        /// This includes both actual pixel data and any padding added to align rows.
        /// </param>
        public BitmapLockInfo(IntPtr pointerBitmap, int stride)
        {
            this.BitmapData = pointerBitmap;
            this.Stride = stride;
        }

        /// <summary>
        /// Gets the pointer to the bitmap.
        /// </summary>
        /// <value>
        /// The pointer bitmap.
        /// </value>
        public IntPtr BitmapData { get; }

        /// <summary>
        /// Gets the stride.
        /// </summary>
        /// <value>
        /// The stride.
        /// </value>
        public int Stride { get; }
    }
}