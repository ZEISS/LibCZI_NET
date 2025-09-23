// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    using Zeiss.Micro.LibCzi.Net.Interface;

    /// <content>
    /// Represents the statistics related to a pyramid structure.
    /// This partial section includes constructor and scene-based statistics variables.
    /// </content>
    /// <seealso cref="IPyramidStatistics" />
    internal partial class PyramidStatistics : IPyramidStatistics
    {
        private readonly Dictionary<int, IReadOnlyList<IPyramidLayerStatistics>> pyramidLayerStatisticsPerScene;

        /// <summary>
        /// Initializes a new instance of the <see cref="PyramidStatistics"/> class.
        /// Parses the given JSON string to a <see cref="PyramidLayerStatistics"/> object and adds it into the dictionary of <see cref="IPyramidLayerStatistics"/>.
        /// </summary>
        /// <param name="json">The json formatted input for parsing.</param>
        public PyramidStatistics(string json)
        {
            this.pyramidLayerStatisticsPerScene = new Dictionary<int, IReadOnlyList<IPyramidLayerStatistics>>();

            // Parse the JSON string
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                // Navigate to the "scenePyramidStatistics" object
                JsonElement root = document.RootElement;
                JsonElement scenePyramidStatistics = root.GetProperty("scenePyramidStatistics");

                // Iterate through the keys (e.g., "0", "1") and arrays
                foreach (JsonProperty property in scenePyramidStatistics.EnumerateObject())
                {
                    List<IPyramidLayerStatistics> pyramidLayerStatistics = new List<IPyramidLayerStatistics>();

                    string key = property.Name;
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        throw new FormatException("'key' cannot be null or whitespace.");
                    }

                    int sceneIndex = int.Parse(key);

                    JsonElement array = property.Value;

                    // Iterate through each array element
                    foreach (JsonElement element in array.EnumerateArray())
                    {
                        JsonElement layerInfo = element.GetProperty("layerInfo");
                        byte minificationFactor = layerInfo.GetProperty("minificationFactor").GetByte();
                        byte pyramidLayerNo = layerInfo.GetProperty("pyramidLayerNo").GetByte();
                        int count = element.GetProperty("count").GetInt32();

                        PyramidLayerStatistics pyramidLayerStatistic = new PyramidLayerStatistics(
                            count,
                            new PyramidLayerInfo(minificationFactor, pyramidLayerNo));
                        pyramidLayerStatistics.Add(pyramidLayerStatistic);
                    }

                    this.pyramidLayerStatisticsPerScene.Add(sceneIndex, pyramidLayerStatistics);
                }
            }
        }

        /// <summary>
        /// Gets the statistics of pyramid layers for each scene.
        /// </summary>
        public IReadOnlyDictionary<int, IReadOnlyList<IPyramidLayerStatistics>> ScenePyramidStatistics => this.pyramidLayerStatisticsPerScene;
    }

    /// <content>
    /// This partial section includes information about PyramidStatistics, such as <see cref="PyramidLayerInfo"/> and SubBlockCount.
    /// SubBlockCount is the number of sub-blocks that are present in the pyramid layer.
    /// </content>
    internal partial class PyramidStatistics
    {
        private struct PyramidLayerStatistics : IPyramidLayerStatistics
        {
            public PyramidLayerStatistics(int subBlockCount, in PyramidLayerInfo layerInfo)
            {
                this.SubBlockCount = subBlockCount;
                this.LayerInfo = layerInfo;
            }

            public int SubBlockCount { get; }

            public PyramidLayerInfo LayerInfo { get; }
        }
    }
}