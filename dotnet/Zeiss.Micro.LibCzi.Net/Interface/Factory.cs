// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using Implementation;
    using Interop;
    using System.Collections.Generic;

    /// <summary>
    /// A factory class for creating instances of <see cref="IInputStream"/> and <see cref="IReader"/>.
    /// It also provides information about the version and build environment.
    /// Note that both <see cref="IInputStream"/> and <see cref="IReader"/> extends to <see cref="System.IDisposable"/>.
    /// </summary>
    public class Factory
    {
        private static readonly MultiChannelCompositor MultiChannelCompositor = new MultiChannelCompositor();

        /// <summary>
        /// Creates an input stream object from an externally provided object implementing the .IExternalInputStream interface.
        /// </summary>
        /// <param name="externalInputStream">The externally provided output stream object.</param>
        /// <returns>A wrapper for the specified external stream.</returns>
        public static IInputStream CreateInputStreamFromExternalStream(IExternalInputStream externalInputStream)
        {
            return new InputStream(LibCziApiInterop.Instance.CreateInputStreamFromExternal(externalInputStream));
        }

        /// <summary>
        /// Creates an output stream object from an externally provided object implementing the IExternalOutputStream interface.
        /// Note that ownership of the object 'externalOutputStream' is transferred to the library, which means that this
        /// object will be disposed when the output-stream object is destroyed.
        /// </summary>
        /// <param name="externalOutputStream">The externally provided output stream object.</param>
        /// <returns>A wrapper for the specified external stream.</returns>
        public static IOutputStream CreateOutputStreamFromExternalStream(IExternalOutputStream externalOutputStream)
        {
            return new OutputStream(LibCziApiInterop.Instance.CreateOutputStreamFromExternal(externalOutputStream));
        }

        /// <summary>
        /// Creates an input stream object from the specified filename.
        /// </summary>
        /// <param name="filename">CZI file path.</param>
        /// <returns>An input-stream object for the specified file.</returns>
        public static IInputStream CreateInputStreamFromFile(string filename)
        {
            return new InputStream(LibCziApiInterop.Instance.CreateInputStreamFromFile(filename));
        }

        /// <summary> Creates an output stream object for the specified file.</summary>
        /// <param name="filename">          The filename to be created.</param>
        /// <param name="overwriteExisting"> True if to attempt to overwrite an existing file; false otherwise.</param>
        /// <returns> The newly created output stream object for the specified file.</returns>
        public static IOutputStream CreateOutputStreamForFile(string filename, bool overwriteExisting)
        {
            return new OutputStream(LibCziApiInterop.Instance.CreateOutputStreamForFilename(filename, overwriteExisting));
        }

        /// <summary>
        /// Creates and initializes a new instance of the specified stream class.
        /// </summary>
        /// <param name="className">             Name of the class (this uniquely identifies the class).</param>
        /// <param name="uri">                   The filename (or, more generally, a URI of some sort) identifying the file to be opened in UTF8-encoding.</param>
        /// <param name="parametersPropertyBag">
        /// A property-bag with options for creating the stream-object. Valid options are specific to the stream-class.
        /// For possible options, see <see cref="StreamClassPropertyKeys"/>.
        /// </param>
        /// <returns> The newly created input stream.</returns>
        public static IInputStream CreateInputStream(string className, string uri, IReadOnlyDictionary<string, object> parametersPropertyBag = null)
        {
            return new InputStream(LibCziApiInterop.Instance.CreateInputStream(className, InternalUtilities.FormatPropertyBagAsJson(parametersPropertyBag), uri));
        }

        /// <summary>
        /// Creates a reader object.
        /// </summary>
        /// <returns>The newly created reader-object.</returns>
        public static IReader CreateReader()
        {
            return new Reader(LibCziApiInterop.Instance.CreateReader());
        }

        /// <summary>
        /// Creates a writer object.
        /// </summary>
        /// <returns>The newly created writer-object.</returns>
        public static IWriter CreateWriter()
        {
            return new Writer(LibCziApiInterop.Instance.CreateWriter());
        }

        /// <summary>
        /// Creates the stream classes repository.
        /// </summary>
        /// <returns>An instance of <see cref="IStreamClassesRepository"/>.</returns>
        public static IStreamClassesRepository CreateStreamClassesRepository()
        {
            return new StreamClassesRepository();
        }

        /// <summary>
        /// Gets the version information.
        /// </summary>
        /// <returns>Struct that <see cref="VersionInfo"/> includes version information (major, minor, patch, tweak.)</returns>
        public static VersionInfo GetVersionInfo()
        {
            var versionInfo = LibCziApiInterop.Instance.GetVersionInfo();
            return new VersionInfo(versionInfo.Major, versionInfo.Minor, versionInfo.Patch, versionInfo.Tweak);
        }

        /// <summary>
        /// Gets the build information.
        /// </summary>
        /// <returns> <see cref="BuildInfo"/> struct.</returns>
        public static BuildInfo GetBuildInformation()
        {
            var buildInfo = LibCziApiInterop.Instance.GetBuildInformation();
            return new BuildInfo(buildInfo.CompilerIdentification, buildInfo.RepositoryUrl, buildInfo.RepositoryBranch, buildInfo.RepositoryTag);
        }

        /// <summary> Gets an object implementing the 'IMultiChannelCompositor' interface.</summary>
        /// <returns> The object implementing the 'IMultiChannelCompositor' interface.</returns>
        public static IMultiChannelCompositor GetChannelCompositor()
        {
            return Factory.MultiChannelCompositor;
        }
    }
}