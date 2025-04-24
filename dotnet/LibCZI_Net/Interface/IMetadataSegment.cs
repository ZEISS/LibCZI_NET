// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Xml.Linq;

    /// <summary> Interface for accessing information from metadata segment.</summary>
    public interface IMetadataSegment : IDisposable
    {
        /// <summary>
        /// Gets the metadata - the data is parsed and returned as an XDocument object.
        /// </summary>
        /// <returns> The metadata XML-document.</returns>
        XDocument GetMetadataAsXDocument();

        /// <summary> Gets a CZI-document-info object containing information derived from metadata.</summary>
        /// <returns> The CZI-document-info object.</returns>
        ICziDocumentInfo GetCziDocumentInfo();
    }
}