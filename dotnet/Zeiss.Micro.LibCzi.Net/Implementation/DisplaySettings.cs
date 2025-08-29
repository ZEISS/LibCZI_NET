// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Implementation
{
    using Interface;
    using Interop;
    using System;
    using System.Runtime.InteropServices;

    /// <summary> Implementation of the IDisplaySettings interface, based on the unmanaged display settings object.</summary>
    internal class DisplaySettings : SafeHandle, IDisplaySettings, IInternalObject
    {
        /// <summary>Initializes a new instance of the <see cref="DisplaySettings" /> class.</summary>
        /// <param name="displaySettingsHandle">A handle for the unmanaged resource that represents the display settings object.</param>
        public DisplaySettings(IntPtr displaySettingsHandle)
            : base(displaySettingsHandle, true)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the handle is valid.
        /// </summary>
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <summary>
        /// Gets the native object handle.
        /// </summary>
        /// <value>
        /// The native object handle.
        /// </value>
        public IntPtr NativeObjectHandle => this.handle;

        /// <summary>
        /// Override ReleaseHandle to release the native resource.
        /// </summary>
        /// <returns>Always returns <c>true</c> to indicate that the handle has been successfully released.</returns>
        protected override bool ReleaseHandle()
        {
            LibCziApiInterop.Instance.ReleaseDisplaySettings(this.handle);
            return true;
        }
    }
}
