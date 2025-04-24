// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Implementation
{
    using System.Collections.Generic;
    using System.Text.Json;

    /// <summary>
    /// Here we collect some internal utilities that are used internally in the implementation of the library.
    /// </summary>
    internal class InternalUtilities
    {
        /// <summary>
        /// Serialize the specified property-bag (as used e.g. with the stream-object creation) as a JSON string.
        /// </summary>
        /// <param name="parametersPropertyBag"> The property bag.</param>
        /// <returns> The property bag as a JSON string.</returns>
        public static string FormatPropertyBagAsJson(IReadOnlyDictionary<string, object> parametersPropertyBag)
        {
            if (parametersPropertyBag == null)
            {
                return string.Empty;
            }

            var jsonString = JsonSerializer.Serialize(parametersPropertyBag);
            return jsonString;
        }
    }
}