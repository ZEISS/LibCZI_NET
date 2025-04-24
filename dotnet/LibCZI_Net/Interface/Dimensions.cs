// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary>
    /// Here we have value for each index.
    /// </summary>
    public enum DimensionIndex : byte
    {
        /// <summary> Invalid dimension.</summary>
        Invalid = 0,

        /// <summary> The Z-dimension.</summary>
        Z = 1,

        /// <summary> The C-dimension ("channel").</summary>
        C = 2,

        /// <summary> The T-dimension ("time").</summary>
        T = 3,

        /// <summary> The R-dimension ("rotation").</summary>
        R = 4,

        /// <summary> The S-dimension ("scene").</summary>
        S = 5,

        /// <summary> The I-dimension ("illumination").</summary>
        I = 6,

        /// <summary> The H-dimension ("phase").</summary>
        H = 7,

        /// <summary> The V-dimension ("view").</summary>
        V = 8,

        /// <summary> The B-dimension ("block") - its use is deprecated.</summary>
        B = 9,

        /// <summary> This entry must have the value of the highest used value.</summary>
        MaxDimensionIndex = 9,
    }
}