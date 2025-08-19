// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using System.Collections.Generic;

    /// <summary> This interface is used to represent a coordinate.</summary>
    public interface ICoordinate
    {
        /// <summary> Attempts to get the coordinate value for the specified dimension.</summary>
        /// <param name="dimensionIndex"> The dimension.</param>
        /// <param name="coordinate">     If successful, the coordinate value is put here.</param>
        /// <returns> True if it succeeds; false otherwise.</returns>
        bool TryGetCoordinate(DimensionIndex dimensionIndex, out int coordinate);

        /// <summary> Enumerates the coordinate values for the valid dimensions.</summary>
        /// <returns> An enumerator that allows foreach to be used to process the coordinates.</returns>
        IEnumerable<DimensionAndValue> Enumerate();
    }
}