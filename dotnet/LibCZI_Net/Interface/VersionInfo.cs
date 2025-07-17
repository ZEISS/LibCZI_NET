// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using LibCZI_Net.Interop;

    /// <summary>
    /// This structure represents a version number.
    /// For versioning libCZI, SemVer2 <see href="https://semver.org/"/> is used.
    /// Note that the value of the tweak version number does not have a meaning (as far as SemVer2 is concerned).
    /// </summary>
    public readonly struct VersionInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInfo"/> struct.
        /// </summary>
        /// <param name="major">The major version level.</param>
        /// <param name="minor">The minor version level.</param>
        /// <param name="patch">The patch version level.</param>
        /// <param name="tweak">The tweak version level.</param>
        public VersionInfo(int major, int minor, int patch, int tweak)
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
            this.Tweak = tweak;
        }

        /// <summary> Gets the major version.</summary>
        /// <value> The major version.</value>
        public int Major { get; }

        /// <summary> Gets the minor version.</summary>
        /// <value> The minor version.</value>
        public int Minor { get; }

        /// <summary> Gets the patch version.</summary>
        /// <value> The patch version.</value>
        public int Patch { get; }

        /// <summary> Gets the tweak version.</summary>
        /// <value> The tweak version.</value>
        public int Tweak { get; }

        /// <inheritdoc/>
        /// <value> Returns a string that represents the current object.</value>
        public override string ToString()
        {
            return $"v{this.Major}.{this.Minor}.{this.Patch}.{this.Tweak}";
        }
    }
}