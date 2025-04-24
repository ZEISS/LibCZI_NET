// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents the bounding box information for a scene.
    /// </summary>
    public readonly struct BoundingBoxPerScene
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundingBoxPerScene"/> struct.
        /// </summary>
        /// <param name="boundingBox">The bounding box.</param>
        /// <param name="boundingBoxLayer0">The bounding box layer0.</param>
        public BoundingBoxPerScene(in IntRect boundingBox, in IntRect boundingBoxLayer0)
        {
            this.BoundingBox = boundingBox;
            this.BoundingBoxLayer0 = boundingBoxLayer0;
        }

        // public int sceneIndex { get; }

        /// <summary>
        /// Gets the bounding box.
        /// </summary>
        /// <value>
        /// The bounding box.
        /// </value>
        public IntRect BoundingBox { get; }

        /// <summary>
        /// Gets the bounding box layer0.
        /// </summary>
        /// <value>
        /// The first layer of the bounding box layer 0.
        /// </value>
        public IntRect BoundingBoxLayer0 { get; }
    }
}