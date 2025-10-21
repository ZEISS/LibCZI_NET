// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using Zeiss.Micro.LibCzi.Net.Interface;
    using Zeiss.Micro.LibCzi.Net.Interop;

    /// <summary> Implementation of the ICziDocumentInfo interface, based on the native functionality.</summary>
    internal partial class CziDocumentInfo : SafeHandle, ICziDocumentInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CziDocumentInfo"/> class.
        /// </summary>
        /// <param name="cziDocumentInfoHandle">A pointer to the unmanaged resource that represents the CZI-document-info object.</param>
        public CziDocumentInfo(IntPtr cziDocumentInfoHandle)
            : base(cziDocumentInfoHandle, true)
        {
        }

        /// <summary>
        /// Gets a value indicating whether determine if the handle is invalid.
        /// </summary>
        public override bool IsInvalid => this.handle == IntPtr.Zero;

        /// <summary>
        /// Override ReleaseHandle to release the native resource.
        /// </summary>
        /// <returns>Always returns <c>true</c> to indicate that the handle has been successfully released.</returns>
        protected override bool ReleaseHandle()
        {
            LibCziApiInterop.Instance.ReleaseCziDocumentInfo(this.handle);
            return true;
        }
    }

    /// <content>
    /// Implementation of the interface 'ICziDocumentInfo' is found here.
    /// </content>
    internal partial class CziDocumentInfo
    {
        /// <inheritdoc/>
        public ScalingInfo ScalingInfo
        {
            get
            {
                return LibCziApiInterop.Instance.CziDocumentInfoGetScalingInfo(this.handle);
            }
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, object> GetGeneralDocumentInfo()
        {
            string json = LibCziApiInterop.Instance.CziDocumentInfoGetGeneralDocumentInfoAsJson(this.handle);
            var dictionary = new Dictionary<string, object>();

            // Parse the JSON string, and store the key-value pairs in the dictionary.
            // We assume here that the root element must be an object.
            JObject root = JObject.Parse(json);

            foreach (var property in root.Properties())
            {
                if (property.Name == CziDocumentPropertyKeys.GeneralDocumentInfoCreationDateTime)
                {
                    // Parse the "creation_data_time" node as a DateTime
                    dictionary[property.Name] = CziDocumentInfo.ParseCreationDateTime(property.Value);
                }
                else
                {
                    dictionary[property.Name] = CziDocumentInfo.GetValue(property.Value);
                }
            }

            return dictionary;
        }

        /// <inheritdoc/>
        public DimensionIndex[] GetAvailableDimensions()
        {
            return LibCziApiInterop.Instance.CziDocumentInfoGetAvailableDimensions(this.handle);
        }

        /// <inheritdoc/>
        public JObject GetDimensionInfo(DimensionIndex dimensionIndex)
        {
            string jsonText = LibCziApiInterop.Instance.CziDocumentInfoGetDimensionInfo(this.handle, dimensionIndex);
            return JObject.Parse(jsonText);
        }

        /// <inheritdoc/>
        public IDisplaySettings GetDisplaySettings()
        {
            IntPtr displaySettingsHandle = LibCziApiInterop.Instance.CziDocumentInfoGetDisplaySettings(this.handle);
            return new DisplaySettings(displaySettingsHandle);
        }

        private static DateTime ParseCreationDateTime(JToken token)
        {
            if (token.Type == JTokenType.String)
            {
                string dateTimeString = token.Value<string>();
                if (DateTime.TryParseExact(dateTimeString, "O", null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                {
                    return dt;
                }

                if (DateTime.TryParse(dateTimeString, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out dt))
                {
                    return dt;
                }

                if (DateTime.TryParse(dateTimeString, System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.RoundtripKind, out dt))
                {
                    return dt;
                }

                throw new FormatException($"Invalid date time format for 'creation_data_time': '{dateTimeString}'.");
            }

            if (token.Type == JTokenType.Date)
            {
                // Use the already-parsed DateTime value
                return token.Value<DateTime>();
            }

            throw new FormatException("Invalid date time format for 'creation_data_time'.");
        }

        private static object GetValue(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    return token.Value<string>();
                case JTokenType.Integer:
                    // Try to fit into int, long, or just return as Int64
                    long longValue = token.Value<long>();
                    if (longValue >= int.MinValue && longValue <= int.MaxValue)
                    {
                        return (int)longValue;
                    }

                    return longValue;
                case JTokenType.Float:
                    return token.Value<double>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Null:
                    return null;
                default:
                    // Default case: return the token as a string
                    return token.ToString();
            }
        }
    }
}
