// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.UnitTests
{
    using Net.Interface;
    using System;

    internal class InputStreamObject : IExternalInputStream
    {
        private readonly bool streamObjectOwned;
        private readonly Stream stream;

        public InputStreamObject(Stream stream, bool ownStreamObject)
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

        public void Read(long offset, IntPtr data, long size, out long bytesRead)
        {
            this.stream.Seek(offset, SeekOrigin.Begin);
            unsafe
            {
                Span<byte> span = new Span<byte>(data.ToPointer(), (int)size);
                int bytesActuallyRead = this.stream.Read(span);
                bytesRead = bytesActuallyRead;
            }
        }
    }
}