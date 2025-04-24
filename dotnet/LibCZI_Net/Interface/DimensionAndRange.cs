// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary>
    /// This struct defines a range of values for a set of dimensions.
    /// </summary>
    public struct DimensionAndRange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DimensionAndRange"/> struct.
        /// </summary>
        /// <param name="dimensionIndex">Index of the dimension.</param>
        /// <param name="start">The start.</param>
        /// <param name="size">The size.</param>
        public DimensionAndRange(DimensionIndex dimensionIndex, int start, int size)
        {
            this.DimensionIndex = dimensionIndex;
            this.Start = start;
            this.Size = size;
        }

        /// <summary>
        /// Gets the index of the dimension.
        /// </summary>
        /// <value>
        /// The index of the dimension.
        /// </value>
        public DimensionIndex DimensionIndex { get; }

        /// <summary>
        /// Gets the start.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public int Start { get; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size { get; }
    }
}