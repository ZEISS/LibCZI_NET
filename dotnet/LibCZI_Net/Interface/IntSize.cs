// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary>
    /// This structure represents an extent (width and height) with integer coordinates.
    /// This structure is immutable.
    /// </summary>
    public struct IntSize
    {
        /// <summary>Initializes a new instance of the <see cref="IntSize" /> struct.</summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public IntSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        /// <summary> Gets the width.</summary>
        /// <value> The width.</value>
        public int Width { get; }

        /// <summary> Gets the height.</summary>
        /// <value> The height.</value>
        public int Height { get; }
    }
}