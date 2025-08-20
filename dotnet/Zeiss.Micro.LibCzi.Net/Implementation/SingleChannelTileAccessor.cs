// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Implementation
{
    using Interface;
    using Interop;
    using System;
    using System.Runtime.InteropServices;

    /// <summary> Implementation of the ISingleChannelTileAccessor interface, based on the native functionality.</summary>
    internal partial class SingleChannelTileAccessor : SafeHandle, ISingleChannelTileAccessor
    {
        /// <summary> Initializes a new instance of the <see cref="SingleChannelTileAccessor"/> class.</summary>
        /// <param name="singleChannelTileAccessorHandle"> Handle of the single channel tile accessor.</param>
        public SingleChannelTileAccessor(IntPtr singleChannelTileAccessorHandle)
            : base(singleChannelTileAccessorHandle, true)
        {
        }

        /// <summary>
        /// Gets a value indicating whether determine if the handle is invalid.
        /// </summary>
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <summary>
        /// Override ReleaseHandle to release the native resource.
        /// </summary>
        /// <returns>Always returns <c>true</c> to indicate that the handle has been successfully released.</returns>
        protected override bool ReleaseHandle()
        {
            LibCziApiInterop.Instance.ReleaseCreateSingleChannelTileAccessor(this.handle);
            return true;
        }
    }

    /// <content>
    /// This partial section contains the implementation of the ISingleChannelTileAccessor interface.
    /// </content>
    internal partial class SingleChannelTileAccessor
    {
        /// <inheritdoc/>
        public IntSize CalcSize(in IntRect roi, float zoomFactor)
        {
            return LibCziApiInterop.Instance.SingleChannelTileAccessorCalcSize(this.handle, in roi, zoomFactor);
        }

        /// <inheritdoc/>
        public IBitmap Get(ICoordinate coordinate, in IntRect roi, float zoomFactor, in AccessorOptions options)
        {
            IntPtr bitmapHandle = LibCziApiInterop.Instance.SingleChannelTileAccessorGet(this.handle, coordinate, in roi, zoomFactor, in options);
            return new Bitmap(bitmapHandle);
        }
    }
}
