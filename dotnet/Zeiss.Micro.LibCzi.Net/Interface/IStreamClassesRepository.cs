// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a repository that manages input stream classes.
    /// </summary>
    public interface IStreamClassesRepository
    {
        /// <summary>
        /// Enumerates the available input stream classes.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{InputStreamClassInfo}"/> containing information about the input stream classes.</returns>
        IEnumerable<InputStreamClassInfo> EnumerateStreamClasses();
    }
}