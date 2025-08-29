// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface provides statistics about the pyramid structure.
    /// </summary>
    public interface IPyramidStatistics
    {
        /// <summary>
        /// Gets a dictionary containing the pyramid statistics for each scene.
        /// If the document does not contain a scene (i.e. it does not use the S-index), then
        /// there will be a single entry with the key int.MaxValue.
        /// </summary>
        /// <value> Dictionary containing the pyramid statistics for each scene. </value>
        IReadOnlyDictionary<int, IReadOnlyList<IPyramidLayerStatistics>> ScenePyramidStatistics { get; }
    }

    /// <summary>
    /// Interface for pyramid layer statistics. This describes one layer of the pyramid.
    /// </summary>
    public interface IPyramidLayerStatistics
    {
        /// <summary>
        /// Gets information describing the pyramid layer.
        /// </summary>
        /// <value> Information describing the pyramid layer.</value>
        PyramidLayerInfo LayerInfo { get; }

        /// <summary> Gets the number of sub-blocks which are present in the pyramid-layer.</summary>
        /// <value> The number of sub blocks.</value>
        int SubBlockCount { get; }
    }

    /// <summary>
    /// Information about the pyramid-layer.
    /// It consists of two parts: the minification factor and the layer number.
    /// The minification factor specifies by which factor two adjacent pyramid-layers are shrunk. Commonly used in
    /// CZI are 2 or 3.
    /// The layer number starts with 0 with the highest resolution layer.
    /// The lowest level (layer 0) is denoted by pyramidLayerNo == 0 AND minificationFactor==0.
    /// Another special case is pyramidLayerNo == 0xff AND minificationFactor==0xff which means that the
    /// pyramid-layer could not be determined (=the minification factor could not unambiguously be correlated to
    /// a pyramid-layer).
    /// </summary>
    public readonly struct PyramidLayerInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PyramidLayerInfo"/> struct.
        /// </summary>
        /// <param name="minificationFactor">The minification factor.</param>
        /// <param name="pyramidLayerNo">The pyramid layer no.</param>
        public PyramidLayerInfo(byte minificationFactor, byte pyramidLayerNo)
        {
            this.MinificationFactor = minificationFactor;
            this.PyramidLayerNo = pyramidLayerNo;
        }

        /// <summary> Gets the factor by which adjacent pyramid-layers are shrunk. Commonly used in CZI are 2 or 3. </summary>
        /// <value> The minification factor.</value>
        public byte MinificationFactor { get; }

        /// <summary> Gets the pyramid layer number. </summary>
        /// <value> The pyramid layer number.</value>
        public byte PyramidLayerNo { get; }

        /// <summary> Gets a value indicating whether this layer is layer 0 (=no minification). </summary>
        /// <value> True if this layer is layer zero, false if not.</value>
        public bool IsLayerZero => this.MinificationFactor == 0 && this.PyramidLayerNo == 0;

        /// <summary> Gets a value indicating whether this layer represents the set of sub-blocks which cannot be represented as pyramid-layers.</summary>
        /// <value> True if the set of "not representable as pyramid-layer" is represented by this object, false if not.</value>
        public bool IsNotIdentifiedAsPyramidLayer => this.MinificationFactor == 0xff && this.PyramidLayerNo == 0xff;
    }
}