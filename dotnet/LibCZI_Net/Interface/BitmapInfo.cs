// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary>
    /// This struct contains information about a bitmap.
    /// It is immutable.
    /// </summary>
    public struct BitmapInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapInfo"/> struct.</summary>
        /// <param name="width">     The width.</param>
        /// <param name="height">    The height.</param>
        /// <param name="pixelType"> Type of the pixel.</param>
        public BitmapInfo(int width, int height, PixelType pixelType)
        {
            this.Width = width;
            this.Height = height;
            this.PixelType = pixelType;
        }

        /// <summary> Gets the width.</summary>
        /// <value> The width.</value>
        public int Width { get; }

        /// <summary> Gets the height.</summary>
        /// <value> The height.</value>
        public int Height { get; }

        /// <summary> Gets the type of the pixel.</summary>
        /// <value> The type of the pixel.</value>
        public PixelType PixelType { get; }
    }
}