// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This accessor creates a multi-tile composite of a single channel (and a single plane) with a given zoom-factor.
    /// It will use pyramid sub-blocks (if present) in order to create the destination bitmap. In this operation, it will use
    /// the pyramid-layer just above the specified zoom-factor and scale down to the requested size.
    /// The scaling operation employed here is a simple nearest-neighbor algorithm.
    /// </summary>
    public interface ISingleChannelTileAccessor : IDisposable
    {
        /// <summary>
        /// Calculates the size a bitmap will have (when created by this accessor) for the specified ROI and the specified Zoom.
        /// Since the exact size is subject to rounding errors, one should always use this method if the exact size must be known beforehand.
        /// </summary>
        /// <param name="roi">        The roi.</param>
        /// <param name="zoomFactor"> The zoom factor.</param>
        /// <returns> The calculated size.</returns>
        IntSize CalcSize(in IntRect roi, float zoomFactor);

        /// <summary>
        /// Gets the scaled tile composite of the specified plane and the specified ROI with the specified zoom factor.
        /// The pixel type is determined by examining the first sub-block found in the specified plane (which is an arbitrary
        /// sub-block). A newly allocated bitmap is returned.
        /// </summary>
        /// <param name="coordinate"> The plane coordinate.</param>
        /// <param name="roi">        The roi.</param>
        /// <param name="zoomFactor"> The zoom factor.</param>
        /// <param name="options">    Options for controlling the operation.</param>
        /// <returns> A bitmap containing the specified roi at the specified zoom.</returns>
        IBitmap Get(ICoordinate coordinate, in IntRect roi, float zoomFactor, in AccessorOptions options);
    }

    /// <summary>
    /// This struct contains options controlling the Get-operation of the ISingleChannelTileAccessor.
    /// </summary>
    public struct AccessorOptions
    {
        /// <summary>
        /// The red component of the background color.  The background color. If the destination bitmap is a grayscale-type, then the mean from R, G and B is calculated and multiplied
        /// with the maximum pixel value (of the specific pixel type). If it is an RGB-color type, then R, G and B are separately multiplied with
        /// the maximum pixel value.
        /// </summary>
        /// <value> The intensity of the red component in the background color.</value>
        public float BackGroundColorR;

        /// <summary>
        /// The green component of the background color.  The background color. If the destination bitmap is a grayscale-type, then the mean from R, G and B is calculated and multiplied
        /// with the maximum pixel value (of the specific pixel type). If it is an RGB-color type, then R, G and B are separately multiplied with
        /// the maximum pixel value.
        /// </summary>
        /// <value> The intensity of the green component in the background color.</value>
        public float BackGroundColorG;

        /// <summary>
        /// The blue component of the background color.  The background color. If the destination bitmap is a grayscale-type, then the mean from R, G and B is calculated and multiplied
        /// with the maximum pixel value (of the specific pixel type). If it is an RGB-color type, then R, G and B are separately multiplied with
        /// the maximum pixel value.
        /// </summary>
        /// <value> The intensity of the blue component in the background color.</value>
        public float BackGroundColorB;

        /// <summary>
        /// If true, then the tiles are sorted by their M-index (tile with highest M-index will be 'on top').
        /// Otherwise, the Z-order is arbitrary.
        /// </summary>
        /// <value> A boolean flag indicating whether to sort tiles by their M-index.</value>
        public bool SortByM;

        /// <summary>
        /// If true, then the tile-visibility-check-optimization is used. When doing the multi-tile composition,
        /// all relevant tiles are checked whether they are visible in the destination bitmap. If a tile is not visible, then
        /// the corresponding sub-block is not read. This can speed up the operation considerably. The result is the same as
        /// without this optimization - i.e. there should be no reason to turn it off besides potential bugs.
        /// </summary>
        /// <value> The boolean flag indicating whether to enable tile visibility check optimization.</value>
        public bool UseVisibilityCheckOptimization;

        /// <summary> A property bag intended for additional options. </summary>
        /// <value> The property bag.</value>
        public IReadOnlyDictionary<string, object> AdditionalOptionsPropertyBag;

        /// <summary> Gets the default.</summary>
        /// <value> The default.</value>
        public static AccessorOptions Default => new AccessorOptions
        {
            BackGroundColorR = 0,
            BackGroundColorG = 0,
            BackGroundColorB = 0,
            SortByM = true,
            UseVisibilityCheckOptimization = true,
            AdditionalOptionsPropertyBag = null,
        };
    }
}
