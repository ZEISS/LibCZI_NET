// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Implementation
{
    using System;
    using System.Runtime.InteropServices;

    using Zeiss.Micro.LibCzi.Net.Interop;

    /// <summary>
    /// Represents a safe buffer that operates on native memory.
    /// </summary>
    /// <seealso cref="System.Runtime.InteropServices.SafeBuffer" />
    internal sealed class SafeBufferOnNativeMemory : SafeBuffer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeBufferOnNativeMemory"/> class.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="length">The length.</param>
        public SafeBufferOnNativeMemory(IntPtr pointer, ulong length)
            : base(true)
        {
            this.Initialize(length);
            this.SetHandle(pointer);
        }

        /// <inheritdoc/>
        protected override bool ReleaseHandle()
        {
            LibCziApiInterop.Instance.FreeMemory(this.handle);
            return true;
        }
    }
}