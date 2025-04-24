// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.UnitTests
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using LibCZI_Net.Interface;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    internal static class TestUtilities
    {
        public static Image CreateImageSharpImageFromBitmap(IBitmap bitmap)
        {
            switch (bitmap.BitmapInfo.PixelType)
            {
                case PixelType.Gray16:
                    return TestUtilities.CreateImageSharp<L16>(bitmap);
                case PixelType.Gray8:
                    return TestUtilities.CreateImageSharp<L8>(bitmap);
                case PixelType.Bgr24:
                    return TestUtilities.CreateImageSharp<Bgr24>(bitmap);
                case PixelType.Bgr48:
                    // TODO: We'd need to deal with BGR-to-RGB conversion here, ImageSharp does not natively support Bgr48 unfortunately
                    return TestUtilities.CreateImageSharp<Rgb48>(bitmap);
            }

            throw new NotImplementedException("Unsupported pixel type encountered.");
        }

        /// <summary>
        /// Calculates a SHA256 hash for the image by processing its pixels line by line.
        /// This method works on the image's native pixel type.
        /// </summary>
        /// <param name="image">The input non‑generic Image from ImageSharp.</param>
        /// <returns>A hexadecimal string representing the hash.</returns>
        public static string CalculateImageHashGetAsString(Image image)
        {
            return BitConverter.ToString(TestUtilities.CalculateImageHash(image)).Replace("-", "").ToLowerInvariant();
        }

        /// <summary> Calculates a SHA256 hash for the image by processing its pixels line by line.</summary>
        /// <param name="image"> The input non‑generic Image from ImageSharp.</param>
        /// <returns> A byte array containing the hash code.</returns>
        public static byte[] CalculateImageHash(Image image)
        {
            // Use dynamic dispatch to call the generic version.
            return TestUtilities.CalculateImageHashInternal((dynamic)image);
        }

        // This helper works for any Image<TPixel> where TPixel is unmanaged and implements IPixel<TPixel>.
        private static byte[] CalculateImageHashInternal<TPixel>(Image<TPixel> image)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using var incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            image.ProcessPixelRows(
                pixelAccessor =>
                {
                    unsafe
                    {
                        for (int y = 0; y < pixelAccessor.Height; y++)
                        {
                            Span<TPixel> pixelRow = pixelAccessor.GetRowSpan(y);
                            incrementalHash.AppendData(MemoryMarshal.AsBytes(pixelRow));
                        }
                    }
                });

            // Finalize and return the hash
            byte[] hashBytes = incrementalHash.GetHashAndReset();
            return hashBytes;
        }

        private static Image<TPixel> CreateImageSharp<TPixel>(IBitmap bitmap) where TPixel : unmanaged, IPixel<TPixel>
        {
            var bitmapInfo = bitmap.BitmapInfo;
            var image = new Image<TPixel>(bitmapInfo.Width, bitmapInfo.Height);

            int lengthOfSourceRow = bitmap.GetLengthOfLine();
            var bitmapLockInfo = bitmap.Lock();
            try
            {
                image.ProcessPixelRows(
                    pixelAccessor =>
                    {
                        unsafe
                        {
                            IntPtr sourceRowPointer = bitmapLockInfo.BitmapData;
                            for (int y = 0; y < pixelAccessor.Height; y++)
                            {
                                Span<TPixel> pixelRow = pixelAccessor.GetRowSpan(y);
                                var sourceRowSpan = MemoryMarshal.Cast<byte, TPixel>(new Span<byte>(sourceRowPointer.ToPointer(), lengthOfSourceRow));
                                sourceRowPointer += bitmapLockInfo.Stride;
                                sourceRowSpan.CopyTo(pixelRow);
                            }
                        }
                    });
            }
            finally
            {
                bitmap.Unlock();
            }

            return image;
        }
    }
}
