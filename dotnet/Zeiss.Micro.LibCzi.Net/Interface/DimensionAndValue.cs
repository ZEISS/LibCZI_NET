// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    /// <summary>This structure gathers a dimension and a coordinate (for this dimension).</summary>
    public struct DimensionAndValue
    {
        /// <summary>Initializes a new instance of the <see cref="DimensionAndValue" /> struct.</summary>
        /// <param name="dimensionIndex">The dimension.</param>
        /// <param name="value">The value.</param>
        public DimensionAndValue(DimensionIndex dimensionIndex, int value)
        {
            this.DimensionIndex = dimensionIndex;
            this.Value = value;
        }

        /// <summary> Gets the dimension.</summary>
        /// <value> The dimension.</value>
        public DimensionIndex DimensionIndex { get; }

        /// <summary> Gets the coordinate value.</summary>
        /// <value> The coordinate value.</value>
        public int Value { get; }
    }
}