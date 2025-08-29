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
    /// This section includes constructor and override methods.
    /// </content>
    /// <seealso cref="System.Runtime.InteropServices.SafeHandle" />
    /// <seealso cref="IAttachment" />
    internal partial class Attachment : SafeHandle, IAttachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment"/> class.
        /// </summary>
        /// <param name="attachmentHandle">A pointer to the unmanaged resource that represents the attachment.</param>
        public Attachment(IntPtr attachmentHandle)
            : base(attachmentHandle, true)
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
            LibCziApiInterop.Instance.ReleaseAttachment(this.handle);
            return true;
        }
    }

    /// <content>
    /// Implementation of the interface 'IAttachment'.
    /// </content>
    internal partial class Attachment
    {
        /// <inheritdoc/>
        public AttachmentInfo AttachmentInfo
        {
            get
            {
                return LibCziApiInterop.Instance.AttachmentGetInfo(this.handle);
            }
        }

        /// <inheritdoc/>
        public Span<byte> GetRawData()
        {
            return LibCziApiInterop.Instance.AttachmentGetRawData(this.handle);
        }
    }
}