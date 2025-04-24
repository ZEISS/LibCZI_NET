// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents extended statistics for sub-blocks, inheriting from <see cref="SubBlockStatistics"/>.
    /// This section includes additional information about bounding boxes for each scene.
    /// </summary>>
    /// <seealso cref="LibCZI_Net.Interface.SubBlockStatistics" />
    public class SubBlockStatisticsEx : SubBlockStatistics
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubBlockStatisticsEx"/> class.
        /// </summary>
        /// <param name="boundingBoxesPerScene">The bounding boxes per scene.</param>
        public SubBlockStatisticsEx(IEnumerable<ValueTuple<int, BoundingBoxPerScene>> boundingBoxesPerScene)
        {
            Dictionary<int, BoundingBoxPerScene> perScenesBoundingBoxes = new Dictionary<int, BoundingBoxPerScene>();

            if (boundingBoxesPerScene != null)
            {
                foreach (var item in boundingBoxesPerScene)
                {
                    perScenesBoundingBoxes[item.Item1] = item.Item2;
                }
            }

            this.PerScenesBoundingBoxes = perScenesBoundingBoxes;
        }

        /// <summary>
        /// Gets the per scenes bounding boxes.
        /// </summary>
        /// <value>
        /// The per scenes bounding boxes.
        /// </value>
        public IReadOnlyDictionary<int, BoundingBoxPerScene> PerScenesBoundingBoxes { get; }
    }
}