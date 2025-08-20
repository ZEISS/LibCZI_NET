// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary> This class gives the coordinates (of a sub-block) for a set of dimension.</summary>
    public class Coordinate : ICoordinate
    {
        private readonly DimensionAndValue[] coordinates;

        /// <summary>Initializes a new instance of the <see cref="Coordinate"/> class.</summary>
        /// <param name="dimensionAndValues">An enumeration which specifies that valid coordinates and their value.</param>
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
        ///                                    illegal values.</exception>
        public Coordinate(IEnumerable<DimensionAndValue> dimensionAndValues)
        {
            int count = dimensionAndValues.Count();
            this.coordinates = new DimensionAndValue[count];
            int index = 0;
            foreach (var item in dimensionAndValues)
            {
                for (int i = 0; i < index; i++)
                {
                    if (this.coordinates[i].DimensionIndex == item.DimensionIndex)
                    {
                        throw new ArgumentException($"Duplicate dimension index: {item}");
                    }
                }

                this.coordinates[index] = item;
                index++;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<DimensionAndValue> Enumerate()
        {
            if (this.coordinates != null)
            {
                foreach (var item in this.coordinates)
                {
                    yield return item;
                }
            }
        }

        /// <inheritdoc/>
        public bool TryGetCoordinate(DimensionIndex dimensionIndex, out int coordinate)
        {
            if (this.coordinates != null)
            {
                for (int i = 0; i < this.coordinates.Length; i++)
                {
                    if (this.coordinates[i].DimensionIndex == dimensionIndex)
                    {
                        coordinate = this.coordinates[i].Value;
                        return true;
                    }
                }
            }

            coordinate = int.MinValue;
            return false;
        }
    }
}