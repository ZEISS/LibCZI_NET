// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary>
    /// This structure gathers information about a sub-block. This struct is immutable.
    /// </summary>
    public readonly struct SubBlockInfo
    {
        private readonly int compressionModeRaw;
        private readonly PixelType pixelType;
        private readonly IntRect logicalRect;
        private readonly IntSize physicalSize;
        private readonly int mIndex;
        private readonly ICoordinate coordinate;

        /// <summary>Initializes a new instance of the <see cref="SubBlockInfo" /> struct.</summary>
        /// <param name="compressionModeRaw">The raw compression mode identification of the sub-block.</param>
        /// <param name="pixelType">The pixel type.</param>
        /// <param name="coordinate">The coordinate.</param>
        /// <param name="logicalRect">The logical rect.</param>
        /// <param name="physicalSize">The physical size.</param>
        /// <param name="mIndex">The M-index (if applicable) - a value of int.MinValue means that this value is not valid.</param>
        public SubBlockInfo(int compressionModeRaw, PixelType pixelType, ICoordinate coordinate, in IntRect logicalRect, in IntSize physicalSize, int mIndex)
        {
            this.compressionModeRaw = compressionModeRaw;
            this.pixelType = pixelType;
            this.coordinate = coordinate;
            this.logicalRect = logicalRect;
            this.physicalSize = physicalSize;
            this.mIndex = mIndex;
        }

        /// <summary> Gets the (raw) compression mode identification of the sub-block. </summary>
        /// <value> This value is not interpreted, use "CompressionMode" variable to have it converted to the CompressionMode-enumeration. </value>
        public int CompressionModeRaw => this.compressionModeRaw;

        /// <summary> Gets the pixel type.</summary>
        /// <value> The pixel type.</value>
        public PixelType PixelType => this.pixelType;

        /// <summary> Gets the logical rectangle.</summary>
        /// <value> The logical rectangle.</value>
        public IntRect LogicalRect => this.logicalRect;

        /// <summary> Gets the physical size.</summary>
        /// <value> The size of the physical.</value>
        public IntSize PhysicalSize => this.physicalSize;

        /// <summary> Gets the M-Index (if applicable).</summary>
        /// <value> The M-Index (if applicable).</value>
        public int? Mindex => this.mIndex != int.MinValue ? (int?)this.mIndex : null;

        /// <summary> Gets the coordinate.</summary>
        /// <value> The coordinate.</value>
        public ICoordinate Coordinate => this.coordinate;

        /// <summary>
        /// Gets the compression mode identification of the sub-block as enum.
        /// </summary>
        public CompressionMode CompressionMode
        {
            get
            {
                // Only the values listed here are known to libCZI - other values are mapped
                //  to "Invalid" to indicate that the value is not known.
                switch (this.compressionModeRaw)
                {
                    case 0:
                        return CompressionMode.UnCompressed;
                    case 1:
                        return CompressionMode.Jpg;
                    case 4:
                        return CompressionMode.JpgXr;
                    case 5:
                        return CompressionMode.Zstd0;
                    case 6:
                        return CompressionMode.Zstd1;
                    default:
                        return CompressionMode.Invalid;
                }
            }
        }
    }
}