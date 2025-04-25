// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary> An enum representing a pixel-type.</summary>
    public enum PixelType : byte
    {
        /// <summary> Invalid pixel type.</summary>
        Invalid = 0xff,

        /// <summary> Grayscale 8-bit unsigned.</summary>
        Gray8 = 0,

        /// <summary> Grayscale 16-bit unsigned.</summary>
        Gray16 = 1,

        /// <summary> Grayscale 4 byte float.</summary>
        Gray32Float = 2,

        /// <summary> BGR-color 8-bytes triples (memory order B, G, R).</summary>
        Bgr24 = 3,

        /// <summary> BGR-color 16-bytes triples (memory order B, G, R).</summary>
        Bgr48 = 4,

        /// <summary> BGR-color 4 byte float triples (memory order B, G, R).</summary>
        Bgr96Float = 8,

        /// <summary> Currently not supported in libCZI.</summary>
        Bgra32 = 9,

        /// <summary> Currently not supported in libCZI.</summary>
        Gray64ComplexFloat = 10,

        /// <summary> Currently not supported in libCZI.</summary>
        Bgr192ComplexFloat = 11,

        /// <summary> Currently not supported in libCZI.</summary>
        Gray32 = 12,

        /// <summary> Currently not supported in libCZI.</summary>
        Gray64Float = 13,
    }
}