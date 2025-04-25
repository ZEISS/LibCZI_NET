﻿// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Implementation
{
    using System;
    using System.Runtime.InteropServices;
    using LibCZI_Net.Interface;
    using LibCZI_Net.Interop;

    /// <summary>
    /// Represents an output stream that wraps a native output stream handle.
    /// </summary>
    /// <seealso cref="System.Runtime.InteropServices.SafeHandle" />
    /// <seealso cref="LibCZI_Net.Interface.IOutputStream" />
    /// <seealso cref="LibCZI_Net.Implementation.IInternalObject" />
    internal class OutputStream : SafeHandle, IOutputStream, IInternalObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputStream"/> class.
        /// </summary>
        /// <param name="outputStreamObjectHandle">The output stream object handle.</param>
        public OutputStream(IntPtr outputStreamObjectHandle)
            : base(outputStreamObjectHandle, true)
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
            LibCziApiInterop.Instance.ReleaseOutputStream(this.handle);
            return true;
        }
    }
}