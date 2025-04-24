// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary> This structure is representing an 8-bit R-G-B color.</summary>
    public struct Rgb24Color
    {
        /// <summary> The black color value.</summary>
        public static Rgb24Color Black = new Rgb24Color(0, 0, 0);

        /// <summary> The white color value.</summary>
        public static Rgb24Color White = new Rgb24Color(255, 255, 255);

        /// <summary>Initializes a new instance of the <see cref="Rgb24Color" /> struct.</summary>
        /// <param name="red">Value for the red component.</param>
        /// <param name="green">Value for the green component.</param>
        /// <param name="blue">Value for the blue component.</param>
        public Rgb24Color(byte red, byte green, byte blue)
        {
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
        }

        /// <summary> Gets the value for the red component.</summary>
        /// <value> The value for the red component.</value>
        public byte Red { get; }

        /// <summary> Gets the value for the green component.</summary>
        /// <value> The value for the green component.</value>
        public byte Green { get; }

        /// <summary> Gets the value for the blue component.</summary>
        /// <value> The value for the blue component.</value>
        public byte Blue { get; }
    }
}
