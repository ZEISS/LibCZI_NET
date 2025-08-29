// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using System;

    /// <summary>
    /// Defines an output stream for reading.
    /// This interface extends <see cref="IDisposable"/> to ensure proper resource management.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IOutputStream : IDisposable
    {
    }
}