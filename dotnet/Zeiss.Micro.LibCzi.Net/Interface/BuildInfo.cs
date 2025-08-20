// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    /// <summary>
    /// Holds information about build information.
    /// Note that this struct is Immutable.
    /// </summary>
    public readonly struct BuildInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildInfo"/> struct.
        /// </summary>
        /// <param name="compilerIdentification">      The compiler identification.</param>
        /// <param name="repositoryUrl">               The repository URL.</param>
        /// <param name="repositoryBranch">            The name of the branch.</param>
        /// <param name="repositoryTag">               The repository tag.</param>
        public BuildInfo(string compilerIdentification, string repositoryUrl, string repositoryBranch, string repositoryTag)
        {
            this.CompilerIdentification = compilerIdentification;
            this.RepositoryUrl = repositoryUrl;
            this.RepositoryBranch = repositoryBranch;
            this.RepositoryTag = repositoryTag;
        }

        /// <summary> Gets the compiler identification.</summary>
        /// <value> The compiler identification.</value>
        public string CompilerIdentification { get; }

        /// <summary> Gets the repository URL.</summary>
        /// <value> The repository URL.</value>
        public string RepositoryUrl { get; }

        /// <summary> Gets the branch.</summary>
        /// <value> The branch.</value>
        public string RepositoryBranch { get; }

        /// <summary> Gets the repository tag.</summary>
        /// <value> The repository tag.</value>
        public string RepositoryTag { get; }

        /// <inheritdoc/>
        /// <value> Returns a string that represents the current object.</value>
        public override string ToString()
        {
            return $"CompilerIdentification: {this.CompilerIdentification} " +
                $"RepositoryUrl: {this.RepositoryUrl} " +
                $"RepositoryBranch: {this.RepositoryBranch} " +
                $"RepositoryTag: {this.RepositoryTag}";
        }
    }
}