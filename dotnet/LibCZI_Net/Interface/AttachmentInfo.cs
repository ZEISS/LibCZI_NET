// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;

    /// <summary>
    /// This structure gathers information about an attachment. This struct is immutable.
    /// </summary>
    public readonly struct AttachmentInfo
    {
        private readonly Guid guid;
        private readonly string contentFileType;
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentInfo" /> struct.
        /// </summary>
        /// <param name="guid">The Guid of the attachment.</param>
        /// <param name="contentFileType">The content file type of the attachment.</param>
        /// <param name="name">The name of the attachment.</param>
        public AttachmentInfo(in Guid guid, string contentFileType, string name)
        {
            this.guid = guid;
            this.contentFileType = contentFileType;
            this.name = name;
        }

        /// <summary>
        /// Gets the Guid of the attachment.
        /// </summary>
        /// <value> The Guid of the attachment.</value>
        public Guid Guid => this.guid;

        /// <summary>
        /// Gets the content file type of the attachment.
        /// </summary>
        /// <value>  The content file type.</value>
        public string ContentFileType => this.contentFileType;

        /// <summary>
        /// Gets the name of the attachment.
        /// </summary>
        /// <value> The name of the attachment.</value>
        public string Name => this.name;
    }
}
