// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using LibCZI_Net.Interface;
    using LibCZI_Net.Interop;

    /// <content>
    /// Internal reader class that wraps the native reader.
    /// This section contains variables, constructor and override methods.
    /// </content>
    /// <seealso cref="System.Runtime.InteropServices.SafeHandle" />
    /// <seealso cref="LibCZI_Net.Interface.IReader" />
    internal partial class Reader : SafeHandle, IReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        /// <param name="readerObjectHandle">The reader handle.</param>
        public Reader(IntPtr readerObjectHandle)
            : base(readerObjectHandle, true)
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
            LibCziApiInterop.Instance.ReleaseReader(this.handle);
            return true;
        }
    }

    /// <content>
    /// Internal reader class that wraps the native reader.
    /// This section contains non-override methods.
    /// </content>
    internal partial class Reader
    {
        /// <inheritdoc/>
        public SubBlockStatistics GetSimpleStatistics()
        {
            return LibCziApiInterop.Instance.ReaderGetSubBlockStatistics(this.handle);
        }

        /// <inheritdoc/>
        public SubBlockStatisticsEx GetExtendedStatistics()
        {
            return LibCziApiInterop.Instance.ReaderGetSubBlockStatisticsEx(this.handle);
        }

        /// <inheritdoc/>
        public IPyramidStatistics GetPyramidStatistics()
        {
            return this.InternalGetPyramidStatistics();
        }

        /// <inheritdoc/>
        public void Open(IInputStream inputStream)
        {
            IInternalObject internalObject = (IInternalObject)inputStream;
            LibCziApiInterop.Instance.ReaderOpen(this.handle, internalObject.NativeObjectHandle);
        }

        /// <inheritdoc/>
        public ISubBlock ReadSubBlock(int index)
        {
            IntPtr subBlockHandle = LibCziApiInterop.Instance.ReaderReadSubBlock(this.handle, index);
            return new SubBlock(subBlockHandle);
        }

        /// <inheritdoc/>
        public bool TryGetSubBlockInfoForIndex(int index, out SubBlockInfo subBlockInfo)
        {
            return LibCziApiInterop.Instance.ReaderTryGetSubBlockInfoForIndex(this.handle, index, out subBlockInfo);
        }

        /// <inheritdoc/>
        public IMetadataSegment GetMetadataSegment()
        {
            IntPtr metadataSegmentHandle = LibCziApiInterop.Instance.ReaderGetMetadataSegment(this.handle);
            return new MetadataSegment(metadataSegmentHandle);
        }

        /// <inheritdoc/>
        public int GetAttachmentsCount()
        {
            return LibCziApiInterop.Instance.ReaderGetAttachmentCount(this.handle);
        }

        /// <inheritdoc/>
        public bool TryGetAttachmentInfoForIndex(int index, out AttachmentInfo attachment)
        {
            return LibCziApiInterop.Instance.ReaderTryGetAttachmentFromDirectoryInfo(this.handle, index, out attachment);
        }

        /// <inheritdoc/>
        public IAttachment ReadAttachment(int index)
        {
            IntPtr attachmentHandle = LibCziApiInterop.Instance.ReaderReadAttachment(this.handle, index);
            return new Attachment(attachmentHandle);
        }

        /// <inheritdoc/>
        public FileHeaderInfo GetFileHeaderInfo()
        {
            return LibCziApiInterop.Instance.ReaderGetFileHeaderInfo(this.handle);
        }

        /// <inheritdoc/>
        public ISingleChannelTileAccessor CreateSingleChannelTileAccessor()
        {
            return new SingleChannelTileAccessor(LibCziApiInterop.Instance.CreateSingleChannelTileAccessor(this.handle));
        }

        private IPyramidStatistics InternalGetPyramidStatistics()
        {
            string pyramidStatisticsAsJson = LibCziApiInterop.Instance.ReaderGetPyramidStatisticsAsJson(this.handle);
            return new Implementation.PyramidStatistics(pyramidStatisticsAsJson);
        }
    }
}