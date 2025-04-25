// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interface
{
    /// <summary>
    /// Possible property keys for the stream-class property-bag.
    /// </summary>
    public static class StreamClassPropertyKeys
    {
        /// <summary> For CurlHttpInputStream, type string: gives the proxy to use, c.f. https://curl.se/libcurl/c/CURLOPT_PROXY.html for more information.</summary>
        public const string CurlHttpProxy = "CurlHttp_Proxy";

        /// <summary> For CurlHttpInputStream, type string: gives the user agent to use, c.f. https://curl.se/libcurl/c/CURLOPT_USERAGENT.html for more information.</summary>
        public const string CurlHttpUserAgent = "CurlHttp_UserAgent";

        /// <summary> For CurlHttpInputStream, type int32: gives the timeout in seconds, c.f. https://curl.se/libcurl/c/CURLOPT_TIMEOUT.html for more information.</summary>
        public const string CurlHttpTimeout = "CurlHttp_Timeout";

        /// <summary> For CurlHttpInputStream, type int32: gives the timeout in seconds for the connection phase, c.f. https://curl.se/libcurl/c/CURLOPT_CONNECTTIMEOUT.html for more information.</summary>
        public const string CurlHttpConnectTimeout = "CurlHttp_ConnectTimeout";

        /// <summary> For CurlHttpInputStream, type string: gives an OAuth2.0 access token, c.f. https://curl.se/libcurl/c/CURLOPT_XOAUTH2_BEARER.html for more information.</summary>
        public const string CurlHttpXoauth2Bearer = "CurlHttp_Xoauth2Bearer";

        /// <summary> For CurlHttpInputStream, type string: gives a cookie, c.f. https://curl.se/libcurl/c/CURLOPT_COOKIE.html for more information.</summary>
        public const string CurlHttpCookie = "CurlHttp_Cookie";

        /// <summary> For CurlHttpInputStream, type bool: a boolean indicating whether the SSL-certificate of the remote host is to be verified, c.f. https://curl.se/libcurl/c/CURLOPT_SSL_VERIFYPEER.html for more information.</summary>
        public const string CurlHttpSslVerifyPeer = "CurlHttp_SslVerifyPeer";

        /// <summary> For CurlHttpInputStream, type bool: a boolean indicating whether the SSL-certificate's name is to be verified against host, c.f. https://curl.se/libcurl/c/CURLOPT_SSL_VERIFYHOST.html for more information.</summary>
        public const string CurlHttpSslVerifyHost = "CurlHttp_SslVerifyHost";

        /// <summary> For CurlHttpInputStream, type bool: a boolean indicating whether redirects are to be followed, c.f. https://curl.se/libcurl/c/CURLOPT_FOLLOWLOCATION.html for more information.</summary>
        public const string CurlHttpFollowLocation = "CurlHttp_FollowLocation";

        /// <summary> For CurlHttpInputStream, type int32: gives the maximum number of redirects to follow, c.f. https://curl.se/libcurl/c/CURLOPT_MAXREDIRS.html for more information.</summary>
        public const string CurlHttpMaxRedirs = "CurlHttp_MaxRedirs";

        /// <summary> For CurlHttpInputStream, type string: gives the directory to check for CA certificate bundle , c.f. https://curl.se/libcurl/c/CURLOPT_CAINFO.html for more information.</summary>
        public const string CurlHttpCaInfo = "CurlHttp_CaInfo";

        /// <summary> For CurlHttpInputStream, type string: give PEM encoded content holding one or more certificates to verify the HTTPS server with, c.f. https://curl.se/libcurl/c/CURLOPT_CAINFO_BLOB.html for more information.</summary>
        public const string CurlHttpCaInfoBlob = "CurlHttp_CaInfoBlob";

        /// <summary>
        /// For AzureBlobInputStream, type string: specifies how authentication is to be done (c.f. https://learn.microsoft.com/en-us/azure/storage/blobs/quickstart-blobs-c-plus-plus?tabs=managed-identity%2Croles-azure-portal#authenticate-to-azure-and-authorize-access-to-blob-data).
        /// Possible values are: "DefaultAzureCredential", "EnvironmentCredential", "AzureCliCredential", "ManagedIdentityCredential", "WorkloadIdentityCredential", "ConnectionString".
        /// The default is: "DefaultAzureCredential".
        /// </summary>
        public const string AzureBlobAuthenticationMode = "AzureBlob_AuthenticationMode";
    }
}