// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface contains Dimension-Bounds related function declarations. Implementations can find in DimensionBounds.cs file.
    /// </summary>
    public interface IDimensionBounds
    {
        /// <summary>
        /// This function gets the dimension information if the specified index is not null.
        /// </summary>
        /// <param name="dimensionIndex">Index of the dimension. You can see the dimensions in <see cref="DimensionIndex"/>.</param>
        /// <param name="start">The start.</param>
        /// <param name="size">The size.</param>
        /// <returns>
        /// <c>true</c> if the get operation is successfully. Otherwise <c>false</c>.
        /// </returns>
        bool TryGetDimension(DimensionIndex dimensionIndex, out int start, out int size);

        /// <summary>
        /// Iterates through the dimBoundsData <see cref="DimensionAndRange"/> array.
        /// </summary>
        /// <returns>Items of dimBoundsData.</returns>
        IEnumerable<DimensionAndRange> Enumerate();
    }
}