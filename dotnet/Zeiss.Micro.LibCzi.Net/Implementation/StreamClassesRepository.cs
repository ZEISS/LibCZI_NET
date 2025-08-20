// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using Interop;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Implementation of the IStreamClassesRepository.
    /// </summary>
    /// <seealso cref="IStreamClassesRepository" />
    public class StreamClassesRepository : IStreamClassesRepository
    {
        /// <inheritdoc/>
        public IEnumerable<InputStreamClassInfo> EnumerateStreamClasses()
        {
            int noOfStreamClasses = LibCziApiInterop.Instance.GetStreamClassesCount();
            for (int i = 0; i < noOfStreamClasses; i++)
            {
                yield return LibCziApiInterop.Instance.GetStreamClassInfo(i);
            }
        }
    }
}