// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.UnitTests
{
    using LibCZI_Net.Interface;
    using System.Collections.Generic;

    internal class InputStreamsRepository
    {
        public static IInputStream CreateStreamFor4chLargeTiledDocument()
        {
            return Factory.CreateInputStream(
                "curl_http_inputstream",
                // "https://zenodo.org/records/14968770/files/2025_01_27__0007_offline_Zen_3_9_5.czi?download=1",
                // "https://ptahmose.de/nextcloud/s/ZaFwaZxKpaMrbZo/download/2025_01_27__0007_offline_Zen_3_9_5.czi",
                "https://ptahmose.de/nextcloud/public.php/dav/files/ZaFwaZxKpaMrbZo",
                new Dictionary<string, object>()
                {
                    { StreamClassPropertyKeys.CurlHttpUserAgent, "libCZI" },
                    { StreamClassPropertyKeys.CurlHttpFollowLocation, true },
                });
        }

        public static IInputStream CreateStreamFor3chWithLutDocument()
        {
            return Factory.CreateInputStream(
                "curl_http_inputstream",
                // "https://ptahmose.de/nextcloud/s/NyLBdoKjxDaw4m9/download/DCV_30MB_gamma_spline.czi",
                "https://ptahmose.de/nextcloud/public.php/dav/files/NyLBdoKjxDaw4m9",
                new Dictionary<string, object>()
                {
                    { StreamClassPropertyKeys.CurlHttpUserAgent, "libCZI" },
                    { StreamClassPropertyKeys.CurlHttpFollowLocation, true },
                });
        }
    }
}
