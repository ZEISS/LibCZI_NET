// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;

    /// <summary>
    /// This structure gathers information about the CZI file-header. This struct is immutable.
    /// </summary>
    public readonly struct FileHeaderInfo
    {
        private readonly Guid guid;
        private readonly int majorVersion;
        private readonly int minorVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHeaderInfo" /> struct.
        /// </summary>
        /// <param name="guid">The Guid of the file-header.</param>
        /// <param name="majorVersion">The major version level.</param>
        /// <param name="minorVersion">The minor version level.</param>
        public FileHeaderInfo(in Guid guid, int majorVersion, int minorVersion)
        {
            this.guid = guid;
            this.majorVersion = majorVersion;
            this.minorVersion = minorVersion;
        }

        /// <summary>
        /// Gets the Guid of the file-header.
        /// </summary>
        public Guid Guid => this.guid;

        /// <summary> Gets the major version.</summary>
        /// <value> The major version.</value>
        public int MajorVersion => this.majorVersion;

        /// <summary> Gets the minor version.</summary>
        /// <value> The minor version.</value>s
        public int MinorVersion => this.minorVersion;
    }
}
