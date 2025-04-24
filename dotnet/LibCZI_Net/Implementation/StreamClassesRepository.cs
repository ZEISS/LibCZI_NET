// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    using System;
    using System.Collections.Generic;
    using LibCZI_Net.Interop;

    /// <summary>
    /// Implementation of the IStreamClassesRepository.
    /// </summary>
    /// <seealso cref="LibCZI_Net.Interface.IStreamClassesRepository" />
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