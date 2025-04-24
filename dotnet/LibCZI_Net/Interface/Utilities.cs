// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Text;

    /// <summary>
    /// Proposes utility functions for the DimensionBound class.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Converts Dimensions (Enum) to <c>char</c>>.
        /// </summary>
        /// <param name="dimensionIndex">Index of the dimension <see cref="DimensionIndex"/>.</param>
        /// <returns>
        /// A <c>char</c> according to specified <see cref="DimensionIndex"/> index.
        /// </returns>
        public static char DimensionToChar(DimensionIndex dimensionIndex)
        {
            switch (dimensionIndex)
            {
                case DimensionIndex.Z: return 'Z';
                case DimensionIndex.C: return 'C';
                case DimensionIndex.T: return 'T';
                case DimensionIndex.R: return 'R';
                case DimensionIndex.S: return 'S';
                case DimensionIndex.I: return 'I';
                case DimensionIndex.H: return 'H';
                case DimensionIndex.V: return 'V';
                case DimensionIndex.B: return 'B';
            }

            return '?';
        }

        /// <summary>
        /// Converts DimensionsBounds to an informal string.
        /// </summary>
        /// <param name="bounds">The bounds <see cref="IDimensionBounds"/>.</param>
        /// <returns>
        /// A string according to specified bounds <see cref="IDimensionBounds"/>.
        /// </returns>
        public static string DimensionBoundsToString(IDimensionBounds bounds)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var dim in bounds.Enumerate())
            {
                sb.Append(DimensionToChar(dim.DimensionIndex));
                sb.Append(dim.Start);
                sb.Append(':');
                sb.Append(dim.Size);
            }

            return sb.ToString();
        }

        /// <summary>Coordinates to string.</summary>
        /// <param name="coordinate">The coordinate.</param>
        /// <returns>A string according to the specified coordinate <see cref="ICoordinate"/>.</returns>
        public static string CoordinateToString(ICoordinate coordinate)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DimensionAndValue dim in coordinate.Enumerate())
            {
                sb.Append(DimensionToChar(dim.DimensionIndex));
                sb.Append(dim.Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the number of bytes per pixel for the specified pixel type.
        /// </summary>
        /// <param name="pixelType">The pixel type <see cref="PixelType"/>.</param>
        /// <returns>The number of bytes per pixel for the specified pixel type.</returns>
        /// <exception cref="ArgumentException">Thrown when the pixel type is unsupported.</exception>
        public static int GetBytesPerPel(PixelType pixelType)
        {
            switch (pixelType)
            {
                case PixelType.Gray8:
                    return 1;
                case PixelType.Gray16:
                    return 2;
                case PixelType.Bgr24:
                    return 3;
                case PixelType.Bgra32:
                    return 4;
                case PixelType.Bgr48:
                    return 6;
                case PixelType.Gray32:
                    return 4;
                case PixelType.Gray32Float:
                    return 4;
                case PixelType.Bgr96Float:
                    return 3 * 4;
            }

            throw new ArgumentException("Unsupported pixeltype");
        }
    }
}