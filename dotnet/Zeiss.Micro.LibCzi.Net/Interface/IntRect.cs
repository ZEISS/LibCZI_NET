// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    /// <summary>
    /// This structure represents a rectangle with integer coordinates.
    /// It is commonly used to represent bounding boxes.
    /// This structure is immutable.
    /// </summary>
    public struct IntRect
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntRect"/> struct.
        /// </summary>
        /// <param name="x">      The x coordinate.</param>
        /// <param name="y">      The y coordinate.</param>
        /// <param name="width">  The width.</param>
        /// <param name="height"> The height.</param>
        public IntRect(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        /// <summary> Gets the x coordinate.</summary>
        /// <value> The x coordinate.</value>
        public int X { get; }

        /// <summary> Gets the y coordinate.</summary>
        /// <value> The y coordinate.</value>
        public int Y { get; }

        /// <summary> Gets the width.</summary>
        /// <value> The width.</value>
        public int Width { get; }

        /// <summary> Gets the height.</summary>
        /// <value> The height.</value>
        public int Height { get; }
    }
}