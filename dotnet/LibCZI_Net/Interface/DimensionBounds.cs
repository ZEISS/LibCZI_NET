// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Implementation of the "dimension bounds" structs. It defines a range of values for a set of dimensions.
    /// Note that this implementation is not very elegant, instead it aims to be fast and reduce memory usage.
    /// </summary>
    public class DimensionBounds : IDimensionBounds
    {
        private DimensionAndRange[] dimBoundsData;

        /// <summary>
        /// Initializes a new instance of the <see cref="DimensionBounds"/> class.
        /// </summary>
        public DimensionBounds()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DimensionBounds"/> class.
        /// </summary>
        /// <param name="bounds">Each item in the collection represents a dimension and its associated range <see cref="DimensionAndRange"/>.</param>
        public DimensionBounds(IEnumerable<DimensionAndRange> bounds)
        {
            int count = bounds.Count();
            this.dimBoundsData = new DimensionAndRange[count];

            int index = 0;
            foreach (var item in bounds)
            {
                for (int i = 0; i < index; i++)
                {
                    if (this.dimBoundsData[i].DimensionIndex == item.DimensionIndex)
                    {
                        throw new ArgumentException($"Duplicate dimension index: {item}");
                    }
                }

                this.dimBoundsData[index] = item;

                index++;
            }
        }

        /// <summary>
        /// Sets the dimension.
        /// </summary>
        /// <param name="dimensionIndex">Index of the dimension.</param>
        /// <param name="start">The start.</param>
        /// <param name="size">The size.</param>
        public void SetDimension(DimensionIndex dimensionIndex, int start, int size)
        {
            if (this.dimBoundsData == null)
            {
                this.dimBoundsData = new DimensionAndRange[1] { new DimensionAndRange(dimensionIndex, start, size) };
            }
            else
            {
                bool found = false;
                int currentLength = this.dimBoundsData.Length;
                for (int i = 0; i < currentLength; i++)
                {
                    if (this.dimBoundsData[i].DimensionIndex == dimensionIndex)
                    {
                        this.dimBoundsData[i] = new DimensionAndRange(dimensionIndex, start, size);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Array.Resize(ref this.dimBoundsData, currentLength + 1);
                    this.dimBoundsData[currentLength] = new DimensionAndRange(dimensionIndex, start, size);
                }
            }
        }

        /// <summary>
        /// Removes the dimension for specified index.
        /// </summary>
        /// <param name="dimensionIndex">Index of the dimension.</param>
        /// <returns>
        /// <c>true</c> if the specified dimension is successfully removed. Otherwise <c>false</c>.
        /// </returns>
        public bool RemoveDimension(DimensionIndex dimensionIndex)
        {
            if (this.dimBoundsData == null)
            {
                return false;
            }

            int index = -1;
            for (int i = 0; i < this.dimBoundsData.Length; i++)
            {
                if (this.dimBoundsData[i].DimensionIndex == dimensionIndex)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return false;
            }

            if (this.dimBoundsData.Length == 1)
            {
                this.dimBoundsData = null;
            }
            else
            {
                DimensionAndRange[] newArray = new DimensionAndRange[this.dimBoundsData.Length - 1];
                if (index > 0)
                {
                    Array.Copy(this.dimBoundsData, 0, newArray, 0, index);
                }

                if (index < this.dimBoundsData.Length - 1)
                {
                    Array.Copy(this.dimBoundsData, index + 1, newArray, index, this.dimBoundsData.Length - index - 1);
                }

                this.dimBoundsData = newArray;
            }

            return true;
        }

        /// <summary>
        /// This function gets the dimension information if the specified index is not null.
        /// </summary>
        /// <param name="dimensionIndex">Index of the dimension.</param>
        /// <param name="start">The start.</param>
        /// <param name="size">The size.</param>
        /// <returns>
        /// <c>true</c> if the get operation is successfully. Otherwise <c>false</c>.
        /// </returns>
        public bool TryGetDimension(DimensionIndex dimensionIndex, out int start, out int size)
        {
            if (this.dimBoundsData != null)
            {
                for (int i = 0; i < this.dimBoundsData.Length; i++)
                {
                    if (this.dimBoundsData[i].DimensionIndex == dimensionIndex)
                    {
                        start = this.dimBoundsData[i].Start;
                        size = this.dimBoundsData[i].Size;
                        return true;
                    }
                }
            }

            start = int.MinValue;
            size = int.MinValue;
            return false;
        }

        /// <summary>
        /// Iterates through the dimBoundsData <see cref="DimensionAndRange"/> array.
        /// </summary>
        /// <returns>Items of dimBoundsData.</returns>
        public IEnumerable<DimensionAndRange> Enumerate()
        {
            if (this.dimBoundsData != null)
            {
                foreach (var item in this.dimBoundsData)
                {
                    yield return item;
                }
            }
        }

        /// <inheritdoc/>
        /// <value> Returns a string that represents the current object. </value>
        public override string ToString()
        {
            return Utilities.DimensionBoundsToString(this);
        }
    }
}