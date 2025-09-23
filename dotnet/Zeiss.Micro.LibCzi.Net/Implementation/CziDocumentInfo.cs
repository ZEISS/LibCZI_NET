// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text.Json;

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
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                JsonElement root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty property in root.EnumerateObject())
                    {
                        // Perform additional parsing for specific nodes if needed
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
        public JsonDocument GetDimensionInfo(DimensionIndex dimensionIndex)
        {
            string jsonText = LibCziApiInterop.Instance.CziDocumentInfoGetDimensionInfo(this.handle, dimensionIndex);
            return JsonDocument.Parse(jsonText);
        }

        /// <inheritdoc/>
        public IDisplaySettings GetDisplaySettings()
        {
            IntPtr displaySettingsHandle = LibCziApiInterop.Instance.CziDocumentInfoGetDisplaySettings(this.handle);
            return new DisplaySettings(displaySettingsHandle);
        }

        private static DateTime ParseCreationDateTime(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                string dateTimeString = element.GetString();
                if (DateTime.TryParse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dateTime))
                {
                    return dateTime;
                }
            }

            throw new FormatException("Invalid date time format for 'creation_data_time'.");
        }

        private static object GetValue(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int intValue))
                    {
                        return intValue;
                    }

                    if (element.TryGetInt64(out long longValue))
                    {
                        return longValue;
                    }

                    if (element.TryGetDouble(out double doubleValue))
                    {
                        return doubleValue;
                    }

                    break;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                    return null;
            }

            // Default case: return the element as a string
            return element.ToString();
        }
    }
}
