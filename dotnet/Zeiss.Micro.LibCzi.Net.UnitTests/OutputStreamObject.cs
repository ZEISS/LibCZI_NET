// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using Net.Interface;

    internal class OutputStreamObject : IExternalOutputStream
    {
        private readonly bool streamObjectOwned;
        private readonly Stream stream;

        public OutputStreamObject(Stream stream, bool ownStreamObject)
        {
            this.stream = stream;
            this.streamObjectOwned = ownStreamObject;
        }

        public void Dispose()
        {
            if (this.streamObjectOwned)
            {
                this.stream.Dispose();
            }
        }

        public void Write(long offset, IntPtr data, long size, out long bytesWritten)
        {
            this.stream.Seek(offset, SeekOrigin.Begin);
            unsafe
            {
                ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(data.ToPointer(), (int)size);
                this.stream.Write(span);
                bytesWritten = size;
            }
        }
    }
}