// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    /// <summary>
    /// This interface is used for retrieving parsed and consolidated metadata information from a CZI document.
    /// </summary>
    public interface ICziDocumentInfo : IDisposable
    {
        /// <summary> Gets information describing the scaling.</summary>
        /// <value> Information describing the scaling.</value>
        ScalingInfo ScalingInfo { get; }

        /// <summary>
        /// Gets general document information. This gives a property bag, for documented
        /// content see the strings defined in <see cref="CziDocumentPropertyKeys"/>.
        /// </summary>
        /// <returns> The general document information as a property bag.</returns>
        IReadOnlyDictionary<string, object> GetGeneralDocumentInfo();

        /// <summary> Gets dimensions for which there is "dimension-info" is available.</summary>
        /// <returns> An array of dimension indices (for which "dimension-info" is available).</returns>
        DimensionIndex[] GetAvailableDimensions();

        /// <summary> Gets "dimension-info" for the specified dimension.</summary>
        /// <param name="dimensionIndex"> The dimension to request "dimension info" for.</param>
        /// <returns> The dimension information, formatted as JSON.</returns>
        JsonDocument GetDimensionInfo(DimensionIndex dimensionIndex);

        /// <summary> Gets the display settings.</summary>
        /// <returns> The display settings.</returns>
        IDisplaySettings GetDisplaySettings();
    }

    /// <summary>
    /// This class gathers the well-known keys for the general document information.
    /// </summary>
    public static class CziDocumentPropertyKeys
    {
        /// <summary> Key identifying the document's name. Datatype: string.</summary>
        /// <value> The key identifying the document's name.</value>
        public const string GeneralDocumentInfoName = "name";

        /// <summary> Key identifying the document's title. Datatype: string.</summary>
        /// <value> The key identifying the document's title.</value>
        public const string GeneralDocumentInfoTitle = "title";

        /// <summary> Key identifying the name of the author. Datatype: string.</summary>
        /// <value> The key identifying the name of the author.</value>
        public const string GeneralDocumentInfoUserName = "user_name";

        /// <summary> Key identifying the description of the document. Datatype: string.</summary>
        /// <value> The key identifying the description of the document.</value>
        public const string GeneralDocumentInfoDescription = "description";

        /// <summary> Key identifying the comment field of the document. Datatype: string.</summary>
        /// <value> The key identifying the comment field of the document.</value>
        public const string GeneralDocumentInfoComment = "comment";

        /// <summary> Key identifying the keywords field of the document. Datatype: string.</summary>
        /// <value> The key identifying the keywords field of the document.</value>
        public const string GeneralDocumentInfoKeywords = "keywords";

        /// <summary> Key identifying the creation time of the document. Datatype: DateTime.</summary>
        /// <value> The key identifying the creation time of the document.</value>
        public const string GeneralDocumentInfoCreationDateTime = "creation_date_time";
    }
}
