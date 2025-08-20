// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Implementation
{
    using Interface;
    using Interop;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Implementation of the IWriter-interface.
    /// </summary>
    internal partial class Writer : SafeHandle, IWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Writer"/> class.
        /// </summary>
        /// <param name="writerObjectHandle">The writer handle.</param>
        public Writer(IntPtr writerObjectHandle)
            : base(writerObjectHandle, true)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the handle is invalid.
        /// </summary>
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <summary>
        /// Override ReleaseHandle to release the native resource.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the handle is released successfully. Otherwise, in the event of a failure, <c>false</c>.
        /// </returns>
        protected override bool ReleaseHandle()
        {
            LibCziApiInterop.Instance.ReleaseWriter(this.handle);
            return true;
        }
    }

    /// <content>
    /// Implementation of the interface IWriter is found here.
    /// </content>
    internal partial class Writer
    {
        /// <inheritdoc/>
        public void Open(IOutputStream outputStream, IReadOnlyDictionary<string, object> parametersPropertyBag = null)
        {
            IInternalObject internalObject = (IInternalObject)outputStream;
            LibCziApiInterop.Instance.WriterInitialize(this.handle, internalObject.NativeObjectHandle, InternalUtilities.FormatPropertyBagAsJson(parametersPropertyBag));
        }

        /// <inheritdoc/>
        void IWriter.Close()
        {
            // need to use explicit interface implementation to avoid name clash with SafeHandle.Close
            LibCziApiInterop.Instance.WriterClose(this.handle);
        }

        /// <inheritdoc/>
        public void AddSubBlockUncompressed(in AddSubBlockInfoUncompressed addSubBlockInfoUncompressed, in AddSubBlockData addSubBlockData)
        {
            LibCziApiInterop.Instance.WriterAddSubBlockUncompressed(this.handle, in addSubBlockInfoUncompressed, in addSubBlockData);
        }

        /// <inheritdoc/>
        public void AddSubBlockCompressed(in AddSubBlockInfoCompressed addSubBlockInfoCompressed, in AddSubBlockData addSubBlockData)
        {
            LibCziApiInterop.Instance.WriterAddSubBlockCompressed(this.handle, in addSubBlockInfoCompressed, in addSubBlockData);
        }

        /// <inheritdoc/>
        public void AddAttachment(in AddAttachmentInfo addAttachmentInfo, Span<byte> attachmentData)
        {
            LibCziApiInterop.Instance.WriterAddAttachment(this.handle, in addAttachmentInfo, attachmentData);
        }

        /// <inheritdoc/>
        public void WriteMetadata(Span<byte> metadata)
        {
            LibCziApiInterop.Instance.WriterWriteMetadata(this.handle, metadata);
        }
    }
}
