// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary>
    /// Represents statistics related to a sub-block, including counts, index ranges,
    /// bounding boxes, and dimension bounds.
    /// </summary>
    public class SubBlockStatistics
    {
        /// <summary>
        /// Gets or sets the total number of sub-blocks in the CZI-document.
        /// We are counting here all sub-block (no matter on which pyramid-layer).
        /// </summary>
        /// <value> The total number of sub-blocks in the CZI-document.</value>
        public int SubBlockCount { get; set; }

        /// <summary>
        /// Gets or sets the minimum M index. This property is nullable.
        /// M stands for mosaic. It determines the set order of tiles in a mosaic.
        /// </summary>
        /// <value> The minimum M index.</value>
        public int? MinimumMIndex { get; set; }

        /// <summary>
        /// Gets or sets the maximum M index. This property is nullable.
        /// M stands for mosaic. It determines the set order of tiles in a mosaic.
        /// </summary>
        /// <value> The maximum M index.</value>
        public int? MaximumMIndex { get; set; }

        /// <summary>
        /// Gets or sets the minimal axis-aligned-bounding-box that covers the sub-blocks.
        /// </summary>
        /// <value> The minimal axis-aligned-bounding-box that covers the sub-blocks.</value>
        public IntRect BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets the minimal axis-aligned-bounding box determined only from the logical coordinates of the sub-blocks on pyramid-layer0 in the
        /// document. The top-left corner of this bounding-box gives the coordinate of the origin of the 'CZI-Pixel-Coordinate-System' in
        /// the coordinate system used by libCZI (which is refered to as 'raw-subblock-coordinate-system'). See @ref coordinatesystems for
        /// additional information.
        /// </summary>
        /// <value>
        /// The minimal axis-aligned-bounding box determined only from the logical coordinates of the sub-blocks on pyramid-layer0 in the document.
        /// </value>
        public IntRect BoundingBoxLayer0 { get; set; }

        /// <summary>
        /// Gets or sets the dimension bounds. The minimum and maximum dimension index determined
        /// from all sub-blocks in the CZI-document.
        /// </summary>
        /// <value>
        /// The dimension bounds. The minimum and maximum dimension index determined
        /// from all sub-blocks in the CZI-document.
        /// </value>
        public IDimensionBounds DimensionBounds { get; set; }
    }
}