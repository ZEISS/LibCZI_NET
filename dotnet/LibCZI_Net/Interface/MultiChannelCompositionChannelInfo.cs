// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;

    /// <summary>
    /// This structure gathers the information about a channel and is used
    /// for the channel-composition.
    /// </summary>
    public struct MultiChannelCompositionChannelInfo
    {
        /// <value> The weight.</value>
        public float Weight;

        /// <value> True to enable, false to disable tinting.</value>
        public bool EnableTinting;

        /// <value> The tinting color - only valid if 'EnableTinting' is true.</value>
        public Rgb24Color TintingColor;

        /// <value>
        /// The black point - it is a float between 0 and 1, where 0 corresponds to the lowest pixel value
        /// (of the pixel-type for the channel) and 1 to the highest pixel value (of the pixel-type of this channel).
        /// All pixel values below the black point are mapped to 0.
        /// </value>
        public float BlackPoint;

        /// <value>
        /// The white point - it is a float between 0 and 1, where 0 corresponds to the lowest pixel value
        /// (of the pixel-type for the channel) and 1 to the highest pixel value (of the pixel-type of this channel).
        /// All pixel value above the white pointer are mapped to the highest pixel value.
        /// </value>
        public float WhitePoint;

        /// <value> The lookup table.</value>
        public Memory<byte> LookupTable;
    }
}
