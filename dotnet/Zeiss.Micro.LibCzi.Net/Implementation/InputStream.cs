// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    using Zeiss.Micro.LibCzi.Net.Interface;
    using Zeiss.Micro.LibCzi.Net.Interop;

    /// <summary>
    /// Represents an input stream that wraps a native input stream handle.
    /// </summary>
    /// <seealso cref="System.Runtime.InteropServices.SafeHandle" />
    /// <seealso cref="IInputStream" />
    /// <seealso cref="IInternalObject" />
    internal class InputStream : SafeHandle, IInputStream, IInternalObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputStream"/> class.
        /// </summary>
        /// <param name="inputStreamObjectHandle">The input stream object handle.</param>
        public InputStream(IntPtr inputStreamObjectHandle)
            : base(inputStreamObjectHandle, true)
        {
        }

        /// <summary>
        /// Gets the native object handle.
        /// </summary>
        /// <value>
        /// The native object handle.
        /// </value>
        public IntPtr NativeObjectHandle => this.handle;

        /// <inheritdoc/>
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <inheritdoc/>
        protected override bool ReleaseHandle()
        {
            LibCziApiInterop.Instance.ReleaseInputStream(this.handle);
            return true;
        }
    }
}