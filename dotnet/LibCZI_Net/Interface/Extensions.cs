// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods for the libCZI_Net-interfaces are gathered here.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Copies the specified bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="pixelType">Type of the pixel.</param>
        /// <param name="stride">The stride.</param>
        /// <param name="data">The data.</param>
        public static void Copy(this IBitmap bitmap, int width, int height, PixelType pixelType, int stride, Span<byte> data)
        {
            unsafe
            {
                fixed (byte* p = data)
                {
                    IntPtr pointer = new IntPtr(p);
                    bitmap.Copy(width, height, pixelType, stride, pointer);
                }
            }
        }

        /// <summary>
        /// Converts dimension bound to an informal string.
        /// </summary>
        /// <param name="dimensionBounds">The dimension bounds.</param>
        /// <returns>A string according to specified bounds <see cref="IDimensionBounds"/>.</returns>
        public static string AsString(this IDimensionBounds dimensionBounds)
        {
            return Utilities.DimensionBoundsToString(dimensionBounds);
        }

        /// <summary> Converts a coordinate to an informal string.</summary>
        /// <param name="coordinate"> The coordinate to act on.</param>
        /// <returns> A string according to specified coordinate <see cref="ICoordinate"/>.</returns>
        public static string AsString(this ICoordinate coordinate)
        {
            return Utilities.CoordinateToString(coordinate);
        }

        /// <summary>
        /// Enumerates the attachments available in the document.
        /// </summary>
        /// <param name="reader"> The reader object to act on.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the attachments in this
        /// collection.
        /// </returns>
        public static IEnumerable<AttachmentInfo> EnumerateAttachments(this IReader reader)
        {
            int attachmentCount = reader.GetAttachmentsCount();
            for (int i = 0; i < attachmentCount; i++)
            {
                if (reader.TryGetAttachmentInfoForIndex(i, out AttachmentInfo attachment))
                {
                    yield return attachment;
                }
            }
        }

        /// <summary>
        /// This utility function allows to process the read-only memory of a bitmap in a safe way. The bitmap is locked,
        /// and passed to the specified delegate, wrapped in a Memory-of-T object. The memory is only valid within the
        /// delegate. The bitmap is unlocked after the delegate has been executed.
        /// The first parameter of the action is the read-only memory object,
        /// which is only valid within the action. The second parameter is the stride in bytes.
        /// </summary>
        /// <param name="bitmap">The bitmap to process.</param>
        /// <param name="action">
        /// The action to perform on the read-only memory. The first parameter of the action is the read-only memory object,
        /// which is only valid within the action. The second parameter is the stride in bytes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the bitmap or action is null.
        /// </exception>
        public static void ProcessReadOnlyLockedMemory(this IBitmap bitmap, Action<ReadOnlyMemory<byte>, int> action)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // Lock the bitmap to get access to its memory
            var lockInfo = bitmap.Lock();
            try
            {
                // Calculate the total size of the bitmap data
                int totalSize = (lockInfo.Stride * (bitmap.BitmapInfo.Height - 1)) + bitmap.GetLengthOfLine();

                // Use a custom memory manager to wrap the unmanaged memory
                var memoryManager = new UnmanagedMemoryManager(lockInfo.BitmapData, totalSize);
                ReadOnlyMemory<byte> memory = memoryManager.Memory;

                // Call the provided action
                action(memory, lockInfo.Stride);
            }
            finally
            {
                // Ensure the bitmap is unlocked
                bitmap.Unlock();
            }
        }

        /// <summary>
        /// Gets the length of a single line of the bitmap in bytes.
        /// </summary>
        /// <param name="bitmap">The bitmap for which to get the length of a line.</param>
        /// <returns>The length of a single line of the bitmap in bytes.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the bitmap is null.</exception>
        public static int GetLengthOfLine(this IBitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            return bitmap.BitmapInfo.Width * Utilities.GetBytesPerPel(bitmap.BitmapInfo.PixelType);
        }

        /// <summary>
        /// Gets the scaled tile composite of the specified plane and the specified ROI with the specified zoom factor.
        /// This method uses the default options for the accessor options.
        /// </summary>
        /// <param name="accessor">   The accessor to act on.</param>
        /// <param name="coordinate"> The plane coordinate.</param>
        /// <param name="roi">        The roi.</param>
        /// <param name="zoomFactor"> The zoom factor.</param>
        /// <returns> A bitmap containing the specified roi at the specified zoom.</returns>
        public static IBitmap Get(this ISingleChannelTileAccessor accessor, ICoordinate coordinate, in IntRect roi, float zoomFactor)
        {
            return accessor.Get(coordinate, roi, zoomFactor, AccessorOptions.Default);
        }

        /// <summary>
        /// Create a multi-channel-composition image from an array of bitmaps and their corresponding channel information.
        /// This extension accepts two arrays of equal length, one containing the bitmaps and the other containing the channel information.
        /// </summary>
        /// <param name="multiChannelCompositor">The multi-channel compositor instance.</param>
        /// <param name="bitmapsArray">An array of bitmaps to be composed.</param>
        /// <param name="compositionChannelInfoArray">An array of channel information corresponding to each bitmap.</param>
        /// <returns>An <see cref="IBitmap"/> containing the composed multi-channel image.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bitmapsArray"/> or <paramref name="compositionChannelInfoArray"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the length of <paramref name="bitmapsArray"/> does not match the length of <paramref name="compositionChannelInfoArray"/>, or when the length of <paramref name="bitmapsArray"/> is zero.</exception>
        public static IBitmap ComposeMultiChannelImage(this IMultiChannelCompositor multiChannelCompositor, IBitmap[] bitmapsArray, MultiChannelCompositionChannelInfo[] compositionChannelInfoArray)
        {
            if (bitmapsArray == null)
            {
                throw new ArgumentNullException(nameof(bitmapsArray));
            }

            if (compositionChannelInfoArray == null)
            {
                throw new ArgumentNullException(nameof(compositionChannelInfoArray));
            }

            if (bitmapsArray.Length != compositionChannelInfoArray.Length)
            {
                throw new ArgumentException("The number of bitmaps and composition channel infos must be equal.");
            }

            if (bitmapsArray.Length == 0)
            {
                throw new ArgumentException("The number of bitmaps must be greater than zero.");
            }

            return multiChannelCompositor.ComposeMultiChannelImage(
                bitmapsArray.Length,
                bitmapsArray.Zip(compositionChannelInfoArray, (bitmap, info) => (bitmap, info)));
        }
    }

    /// <content>
    /// Internally used utility functions and definitions are found here.
    /// </content>
    public static partial class Extensions
    {
        private sealed class UnmanagedMemoryManager : MemoryManager<byte>
        {
            private readonly IntPtr pointer;
            private readonly int length;

            public UnmanagedMemoryManager(IntPtr pointer, int length)
            {
                this.pointer = pointer;
                this.length = length;
            }

            public override Span<byte> GetSpan()
            {
                unsafe
                {
                    return new Span<byte>(this.pointer.ToPointer(), this.length);
                }
            }

            public override MemoryHandle Pin(int elementIndex = 0)
            {
                // Since the memory is already pinned (unmanaged), we can return a handle to it
                unsafe
                {
                    return new MemoryHandle(this.pointer.ToPointer());
                }
            }

            public override void Unpin()
            {
                // No-op: The memory is unmanaged and doesn't need to be unpinned
            }

            protected override void Dispose(bool disposing)
            {
                // No-op: The memory is unmanaged and doesn't need to be disposed
            }
        }
    }
}