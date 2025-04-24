// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary>
    ///  This struct represents a input stream class information.
    ///  It provides name and description of input stream class.
    /// </summary>
    public struct InputStreamClassInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputStreamClassInfo"/> struct.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        /// <param name="description">The description of the class.</param>
        public InputStreamClassInfo(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets the class name.
        /// </summary>
        /// <value>
        /// The class name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the class description.
        /// </summary>
        /// <value>
        /// The class description.
        /// </value>
        public string Description { get; }
    }
}