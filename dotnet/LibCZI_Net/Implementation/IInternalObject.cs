// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Implementation
{
    using System;

    /// <summary>
    /// Private interface that exposes a handle to an unmanaged resource.
    /// </summary>
    internal interface IInternalObject
    {
        /// <summary>
        /// Gets the representation of an internal object that exposes a handle to an unmanaged resource.
        /// </summary>
        IntPtr NativeObjectHandle { get; }
    }
}