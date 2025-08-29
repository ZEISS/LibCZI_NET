// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Implementation
{
    using Interface;
    using Interop;
    using System;
    using System.Runtime.InteropServices;

    /// <content>
    ///   Represents a bitmap that wraps a native bitmap handle.
    ///   This partial section contains override methods.
    /// </content>
    /// <seealso cref="System.Runtime.InteropServices.SafeHandle" />
    /// <seealso cref="IBitmap" />
    internal partial class Bitmap : SafeHandle, IBitmap, IInternalObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class.
        /// </summary>
        /// <param name="bitmapObjectHandle">The bitmap object handle.</param>
        public Bitmap(IntPtr bitmapObjectHandle)
            : base(bitmapObjectHandle, true)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the handle is invalid.
        /// </summary>
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <summary>
        /// Gets the native object handle.
        /// </summary>
        /// <value>
        /// The native object handle.
        /// </value>
        public IntPtr NativeObjectHandle => this.handle;

        /// <inheritdoc/>
        protected override bool ReleaseHandle()
        {
            LibCziApiInterop.Instance.ReleaseBitmap(this.handle);
            return true;
        }
    }

    /// <content>
    /// Represents a bitmap that wraps a native bitmap handle.
    /// This partial section contains Bitmap related functions.
    /// </content>
    internal partial class Bitmap
    {
        /// <inheritdoc/>
        public BitmapInfo BitmapInfo
        {
            get { return LibCziApiInterop.Instance.BitmapGetInfo(this.handle); }
        }

        /// <inheritdoc/>
        public BitmapLockInfo Lock()
        {
            return LibCziApiInterop.Instance.BitmapLock(this.handle);
        }

        /// <inheritdoc/>
        public void Unlock()
        {
            LibCziApiInterop.Instance.BitmapUnlock(this.handle);
        }

        /// <inheritdoc/>
        public void Copy(int width, int height, PixelType pixelType, int stride, IntPtr ptr)
        {
            LibCziApiInterop.Instance.BitmapCopyTo(this.handle, width, height, pixelType, stride, ptr);
        }
    }
}