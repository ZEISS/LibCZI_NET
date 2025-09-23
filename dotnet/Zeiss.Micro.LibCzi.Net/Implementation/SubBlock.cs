// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Implementation
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;

    using Zeiss.Micro.LibCzi.Net.Interface;
    using Zeiss.Micro.LibCzi.Net.Interop;

    /// <content>
    /// This section includes constructor and override methods.
    /// </content>
    /// <seealso cref="System.Runtime.InteropServices.SafeHandle" />
    /// <seealso cref="ISubBlock" />
    internal partial class SubBlock : SafeHandle, ISubBlock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubBlock"/> class.
        /// </summary>
        /// <param name="readerSubBlockHandle">A handle for the unmanaged resource that represents the sub-block.</param>
        public SubBlock(IntPtr readerSubBlockHandle)
            : base(readerSubBlockHandle, true)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the handle is valid.
        /// </summary>
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <summary>
        /// Override ReleaseHandle to release the native resource.
        /// </summary>
        /// <returns>Always returns <c>true</c> to indicate that the handle has been successfully released.</returns>
        protected override bool ReleaseHandle()
        {
            LibCziApiInterop.Instance.ReleaseSubBlock(this.handle);
            return true;
        }
    }

    /// <content>
    /// Implementation of the interface 'ISubBlock'.
    /// </content>
    internal partial class SubBlock
    {
        /// <inheritdoc/>
        public SubBlockInfo SubBlockInfo
        {
            get
            {
                return LibCziApiInterop.Instance.SubBlockGetInfo(this.handle);
            }
        }

        /// <inheritdoc/>
        public IBitmap GetBitmap()
        {
            IntPtr bitmapObjectHandle = LibCziApiInterop.Instance.SubBlockCreateBitmap(this.handle);
            return new Bitmap(bitmapObjectHandle);
        }

        /// <inheritdoc/>
        public Span<byte> GetRawData()
        {
            return LibCziApiInterop.Instance.SubBlockGetRawData(this.handle, 0);
        }

        /// <inheritdoc/>
        public Span<byte> GetRawMetadataData()
        {
            return LibCziApiInterop.Instance.SubBlockGetRawData(this.handle, 1);
        }

        /// <inheritdoc/>
        public XDocument GetMetadataAsXDocument()
        {
            var metadataXml = LibCziApiInterop.Instance.SubBlockGetRawData(this.handle, 1);
            if (metadataXml.IsEmpty)
            {
                return null;
            }

            unsafe
            {
                fixed (byte* pointer = metadataXml)
                {
                    using (UnmanagedMemoryStream stream = new UnmanagedMemoryStream(pointer, metadataXml.Length))
                    {
                        return XDocument.Load(stream);
                    }
                }
            }
        }
    }
}