// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Implementation
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml.Linq;
    using LibCZI_Net.Interface;
    using LibCZI_Net.Interop;

    /// <summary>
    /// Implementation of the metadata-segment object.
    /// </summary>
    internal class MetadataSegment : SafeHandle, IMetadataSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataSegment"/> class.
        /// </summary>
        /// <param name="metadataSegmentHandle">Handle of the (native) metadata segment object.</param>
        public MetadataSegment(IntPtr metadataSegmentHandle)
            : base(metadataSegmentHandle, true)
        {
        }

        /// <inheritdoc/>
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <inheritdoc/>
        public XDocument GetMetadataAsXDocument()
        {
            var metadataXml = LibCziApiInterop.Instance.MetadataSegmentGetMetadataAsXml(this.handle);

            using (UnmanagedMemoryStream stream = new UnmanagedMemoryStream(metadataXml, 0, (long)metadataXml.ByteLength))
            {
                return XDocument.Load(stream);
            }
        }

        /// <inheritdoc/>
        public ICziDocumentInfo GetCziDocumentInfo()
        {
            return new CziDocumentInfo(LibCziApiInterop.Instance.MetadataSegmentGetCziDocumentInfo(this.handle));
        }

        /// <inheritdoc/>
        protected override bool ReleaseHandle()
        {
            LibCziApiInterop.Instance.ReleaseMetadataSegment(this.handle);
            return true;
        }
    }
}