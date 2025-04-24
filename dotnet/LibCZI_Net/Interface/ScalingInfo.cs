// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary> Information about the scaling.</summary>
    public struct ScalingInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ScalingInfo" /> struct.</summary>
        /// <param name="xScale"> The length of a pixel in x-direction in the unit meters.</param>
        /// <param name="yScale"> The length of a pixel in y-direction in the unit meters.</param>
        public ScalingInfo(double xScale, double yScale)
            : this(xScale, yScale, double.NaN)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ScalingInfo" /> struct.</summary>
        /// <param name="xScale">The length of a pixel in x-direction in the unit meters.</param>
        /// <param name="yScale">The length of a pixel in y-direction in the unit meters.</param>
        /// <param name="zScale">The length of a pixel in z-direction in the unit meters.</param>
        public ScalingInfo(double xScale, double yScale, double zScale)
        {
            this.XScale = xScale;
            this.YScale = yScale;
            this.ZScale = zScale;
        }

        /// <summary>
        /// Gets the length of a pixel in x-direction in the unit meters. If unknown/invalid, this value is double.NaN.
        /// </summary>
        /// <value> The length of a pixel in x-direction in the unit meters.</value>
        public double XScale { get; }

        /// <summary>
        /// Gets the length of a pixel in y-direction in the unit meters. If unknown/invalid, this value is double.NaN.
        /// </summary>
        /// <value> The length of a pixel in y-direction in the unit meters.</value>
        public double YScale { get; }

        /// <summary>
        /// Gets the length of a pixel in z-direction in the unit meters. If unknown/invalid, this value is double.NaN.
        /// </summary>
        /// <value> The length of a pixel in z-direction in the unit meters.</value>
        public double ZScale { get; }
    }
}
