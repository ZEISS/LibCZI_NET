// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface for writer object allows to write a CZI document.
    /// The mode of operation is:
    /// * Open the writer with an output stream.
    /// * Then add sub-blocks and attachments, in any order.
    /// * Write the XML-metadata. (TODO: functionality is missing here, namely GetPreparedMetadata and the metadata-builder-object)
    /// * Then close the writer.
    /// It is important to note that the writer is not thread-safe. Also, the call to Close() is mandatory. If Close() is not called,
    /// the CZI will not be finalized and the file will be incomplete.
    /// </summary>
    public interface IWriter : IDisposable
    {
        /// <summary> Initialize the writer by passing in the output-stream-object.</summary>
        /// <param name="outputStream">          The stream object to write data to.</param>
        /// <param name="parametersPropertyBag">
        /// (Optional) A property bag with additional parameters for the operation. For
        /// possible parameters, see the class <see cref="WriterOpenParameters"/>.
        /// </param>
        void Open(IOutputStream outputStream, IReadOnlyDictionary<string, object> parametersPropertyBag = null);

        /// <summary>
        /// Adds the specified (uncompressed) sub block to the CZI-file. This is a synchronous method, meaning that it will return when all
        /// data has been written out to the file AND that it must not be called concurrently with other method-invocations of
        /// this object.
        /// </summary>
        /// <param name="addSubBlockInfoUncompressed"> Information describing the sub block to be added.</param>
        /// <param name="addSubBlockData">             The payload data of the sub block to be added.</param>
        void AddSubBlockUncompressed(in AddSubBlockInfoUncompressed addSubBlockInfoUncompressed, in AddSubBlockData addSubBlockData);

        /// <summary>
        /// Adds the specified (compressed) sub block to the CZI-file. This is a synchronous method, meaning that it will return when all
        /// data has been written out to the file AND that it must not be called concurrently with other method-invocations of
        /// this object.
        /// Note that this method does not perform the compression itself, but only writes the compressed data to the file.
        /// </summary>
        /// <param name="addSubBlockInfoCompressed">   Information describing the sub block to be added.</param>
        /// <param name="addSubBlockData">             The payload data of the sub block to be added.</param>
        void AddSubBlockCompressed(in AddSubBlockInfoCompressed addSubBlockInfoCompressed, in AddSubBlockData addSubBlockData);

        /// <summary> Adds the specified attachment to the CZI-file.</summary>
        /// <param name="addAttachmentInfo"> Information describing attachment to be added.</param>
        /// <param name="attachmentData">    The payload data of the attachment to be added.</param>
        void AddAttachment(in AddAttachmentInfo addAttachmentInfo, Span<byte> attachmentData);

        /// <summary> Writes the metadata segment containing the specified data.</summary>
        /// <param name="metadata"> The metadata (must be given in XML in UTF8-encoding).</param>
        void WriteMetadata(Span<byte> metadata);

        /// <summary>
        /// Finalizes the CZI (i.e. writes out the final directory-segments) and closes the file.
        /// Note that this method must be called explicitly in order to get a valid CZI - calling the destructor alone will
        /// close the file immediately without finalization.
        /// </summary>
        void Close();
    }

    /// <summary>
    /// Information about a sub block that is to be added to the CZI document.
    /// The information gathered here is common to both compressed and uncompressed sub blocks.
    /// </summary>
    public struct AddSubBlockInfo
    {
        /// <value> The sub block's coordinate.</value>
        public ICoordinate Coordinate;

        /// <value> Boolean flag indicating whether the field 'Mindex' is valid.</value>
        public bool MindexValid;

        /// <value> The M-index of the sub block.</value>
        public int Mindex;

        /// <value> The x-coordinate of the sub block.</value>
        public int X;

        /// <value> The y-coordinate of the sub block.</value>
        public int Y;

        /// <value> The logical with of the sub block (in pixels).</value>
        public int LogicalWidth;

        /// <value> The logical with of the sub block (in pixels).</value>
        public int LogicalHeight;

        /// <value> The physical with of the sub block (in pixels).</value>
        public int PhysicalWidth;

        /// <value> The physical height of the sub block (in pixels).</value>
        public int PhysicalHeight;

        /// <value> The pixel type of the sub block.</value>
        public PixelType PixelType;
    }

    /// <summary> This structure gathers all information required for adding an uncompressed sub block.</summary>
    public struct AddSubBlockInfoUncompressed
    {
        /// <value> Information describing the sub block.</value>
        public AddSubBlockInfo AddSubBlockInfo;

        /// <value> The stride of the bitmap being added.</value>
        public uint Stride;
    }

    /// <summary>
    /// This structure gathers all information required for adding a compressed sub block.
    /// </summary>
    public struct AddSubBlockInfoCompressed
    {
        /// <value> Information describing the sub block.</value>
        public AddSubBlockInfo AddSubBlockInfo;

        /// <value> The compression mode.</value>
        public int CompressionMode;
    }

    /// <summary>
    /// This structure gathers all payload data which makes up a sub block.
    /// Note that 'Metadata' and 'Attachment' are optional and can be empty. If the Span
    /// is empty, the data is not written.
    /// </summary>
    public ref struct AddSubBlockData
    {
        /// <value> The bitmap data.</value>
        public Span<byte> BitmapData;

        /// <value> The metadata.</value>
        public Span<byte> Metadata;

        /// <value> The attachment.</value>
        public Span<byte> Attachment;
    }

    /// <summary>
    /// Information describing an attachment that is to be added to the CZI document.
    /// </summary>
    public struct AddAttachmentInfo
    {
        /// <value> Unique identifier for the content.</value>
        public Guid Guid;

        /// <value>
        /// The content file type. The max length of this string is 8 characters,
        /// larger strings will be truncated.
        /// </value>
        public string ContentFileType;

        /// <value>
        /// The attachment's name. The max length of this string is 80 characters.
        /// </value>
        public string Name;
    }

    /// <summary> The keys for use with the property bag used with IWriter.Open are found here.</summary>
    public static class WriterOpenParameters
    {
        /// <value>
        /// Specify the file-GUID of the CZI-document being created. If this is GUID_NULL or not given, a new GUID will be generated.
        /// Type: Guid or string.
        /// </value>
        public const string FileGuid = "file_guid";
    }
}
