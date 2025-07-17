// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interop
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using LibCZI_Net.Implementation;
    using LibCZI_Net.Interface;

    /// <summary>
    /// Provides interop methods for interacting with the CZI native files (.dll, .so, etc.)
    /// This class implements the singleton pattern to ensure that only one
    /// instance of <see cref="LibCziApiInterop"/> is created and used throughout
    /// the application.
    /// <para/>
    /// The following patterns have been applied so far:
    /// <list type="bullet">
    /// <item><description>
    /// For objects (created and living on the unmanaged side) we use the concept of a 'handle'. A handle has the type 'IntPtr'.
    /// </description></item>
    /// <item><description>
    /// For each type of object (represented by a handle) there is a corresponding 'Release'-method, which is used to release the object.
    /// </description></item>
    /// <item><description>
    /// For functions (operating on objects) a naming pattern is used - the method has the name '&lt;class-name&gt;&lt;method-name&gt;'.
    /// </description></item>
    /// <item><description>
    /// The naming pattern for the 'Release'-methods is 'Release&lt;class-name&gt;'.
    /// </description></item>
    /// </list>
    /// </summary>>
    internal partial class LibCziApiInterop
    {
        private static readonly Lazy<LibCziApiInterop> LibCziApiInteropInstance =
            new Lazy<LibCziApiInterop>(
                () => new LibCziApiInterop(),
                System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private readonly IntPtr dllHandle;

        /// <summary>
        /// In case there was a problem loading the native library (and the class therefore being in-operational),
        /// then this field contains some debug information about the error.
        /// </summary>
        private string nativeLibraryLoadingErrorLog;

        /// <summary>
        /// Prevents a default instance of the <see cref="LibCziApiInterop"/> class from being created.
        /// </summary>
        /// <remarks>
        /// The strategy of this implementation for dealing with errors (i.e. native DLL could not be loaded or some function-exports
        /// could not be found) is:
        /// <list>
        /// <item><description>
        /// The constructor does NOT throw an exception (i.e. the singleton is always available).
        /// </description></item>
        /// <item><description>
        /// But, in case of an error-condition, every method will immediately throw an InvalidOperationException exception.
        /// </description></item>
        /// </list>
        /// </remarks>
        private LibCziApiInterop()
        {
            var possibleDllFilenames = this.GetFullyQualifiedDllPaths();

            IntPtr dllHandle = IntPtr.Zero;
            StringBuilder errorLog = new StringBuilder();

            // we try to load all filenames we got, and if "Load" succeeded, we keep this DllLoader-instance
            foreach (var filename in possibleDllFilenames)
            {
                try
                {
                    errorLog.AppendLine($"Trying to load library from '{filename}'");
                    dllHandle = NativeLibraryUtils.LoadLibrary(filename);
                    if (dllHandle != IntPtr.Zero)
                    {
                        errorLog.AppendLine($" - Success. ");
                        break;
                    }

                    errorLog.AppendLine($" - Failure. ");
                }
                catch
                {
                }
            }

            // it there is no operational DllLoader-instance at this point, we get out of here (and leave this instance in a non-operational state)
            if (dllHandle == IntPtr.Zero)
            {
                this.nativeLibraryLoadingErrorLog = errorLog.ToString();
                return;
            }

            try
            {
                this.libCziFree = NativeLibraryUtils.GetProcAddressThrowIfNotFound<LibCziFreeDelegate>(dllHandle, "libCZI_Free");
                this.libCziAllocateMemory = NativeLibraryUtils.GetProcAddressThrowIfNotFound<LibCziAllocateMemoryDelegate>(dllHandle, "libCZI_AllocateMemory");
                this.getLibCziVersionInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<GetLibCziVersionInfoDelegate>(dllHandle, "libCZI_GetLibCZIVersionInfo");
                this.getLibCziBuildInformation = NativeLibraryUtils.GetProcAddressThrowIfNotFound<GetLibCziBuildInformationDelegate>(dllHandle, "libCZI_GetLibCZIBuildInformation");
                this.createInputStreamFromFileWide = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CreateInputStreamFromFileWideDelegate>(dllHandle, "libCZI_CreateInputStreamFromFileWide");
                this.createInputStreamFromFileUtf8 = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CreateInputStreamFromFileUtf8Delegate>(dllHandle, "libCZI_CreateInputStreamFromFileUTF8");
                this.createInputStream = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CreateInputStreamDelegate>(dllHandle, "libCZI_CreateInputStream");
                this.createInputStreamFromExternal = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CreateInputStreamFromExternalDelegate>(dllHandle, "libCZI_CreateInputStreamFromExternal");
                this.createReader = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CreateReaderDelegate>(dllHandle, "libCZI_CreateReader");
                this.readerOpen = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderOpenDelegate>(dllHandle, "libCZI_ReaderOpen");
                this.readerGetFileHeaderInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderGetFileHeaderInfoDelegate>(dllHandle, "libCZI_ReaderGetFileHeaderInfo");
                this.readerGetStatisticsSimple = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderGetStatisticsSimpleDelegate>(dllHandle, "libCZI_ReaderGetStatisticsSimple");
                this.readerGetStatisticsEx = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderGetStatisticsExDelegate>(dllHandle, "libCZI_ReaderGetStatisticsEx");
                this.readerGetPyramidStatistics = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderGetPyramidStatisticsDelegate>(dllHandle, "libCZI_ReaderGetPyramidStatistics");
                this.getStreamClassesCount = NativeLibraryUtils.GetProcAddressThrowIfNotFound<GetStreamClassesCountDelegate>(dllHandle, "libCZI_GetStreamClassesCount");
                this.getStreamClassInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<GetStreamClassInfoDelegate>(dllHandle, "libCZI_GetStreamClassInfo");

                this.readerReadSubBlock = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderReadSubBlockDelegate>(dllHandle, "libCZI_ReaderReadSubBlock");
                this.readerTryGetSubBlockInfoForIndex = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderTryGetSubBlockInfoForIndexDelegate>(dllHandle, "libCZI_TryGetSubBlockInfoForIndex");
                this.readerGetMetadataSegment = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderGetMetadataSegmentDelegate>(dllHandle, "libCZI_ReaderGetMetadataSegment");
                this.readerGetAttachmentCount = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderGetAttachmentCountDelegate>(dllHandle, "libCZI_ReaderGetAttachmentCount");
                this.readerGetAttachmentFromDirectoryInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderGetAttachmentInfoFromDirectoryDelegate>(dllHandle, "libCZI_ReaderGetAttachmentInfoFromDirectory");
                this.readerReadAttachment = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderReaderReadAttachmentDelegate>(dllHandle, "libCZI_ReaderReadAttachment");
                this.readerRelease = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReaderReleaseDelegate>(dllHandle, "libCZI_ReleaseReader");
                this.releaseInputStream = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReleaseInputStreamDelegate>(dllHandle, "libCZI_ReleaseInputStream");
                this.subBlockCreateBitmap = NativeLibraryUtils.GetProcAddressThrowIfNotFound<SubBlockCreateBitmapDelegate>(dllHandle, "libCZI_SubBlockCreateBitmap");
                this.subBlockGetInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<SubBlockGetInfoDelegate>(dllHandle, "libCZI_SubBlockGetInfo");
                this.subBlockGetRawData = NativeLibraryUtils.GetProcAddressThrowIfNotFound<SubBlockGetRawDataDelegate>(dllHandle, "libCZI_SubBlockGetRawData");
                this.releaseSubBlock = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReleaseSubBlockDelegate>(dllHandle, "libCZI_ReleaseSubBlock");
                this.bitmapGetInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<BitmapGetInfoDelegate>(dllHandle, "libCZI_BitmapGetInfo");
                this.bitmapLock = NativeLibraryUtils.GetProcAddressThrowIfNotFound<BitmapLockDelegate>(dllHandle, "libCZI_BitmapLock");
                this.bitmapUnlock = NativeLibraryUtils.GetProcAddressThrowIfNotFound<BitmapUnlockDelegate>(dllHandle, "libCZI_BitmapUnlock");
                this.bitmapCopyTo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<BitmapCopyToDelegate>(dllHandle, "libCZI_BitmapCopyTo");
                this.releaseBitmap = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReleaseBitmapDelegate>(dllHandle, "libCZI_ReleaseBitmap");
                this.metadataSegmentGetMetadataAsXml = NativeLibraryUtils.GetProcAddressThrowIfNotFound<MetadataSegmentGetMetadataAsXmlDelegate>(dllHandle, "libCZI_MetadataSegmentGetMetadataAsXml");
                this.metadataSegmentGetCziDocumentInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<MetadataSegmentGetCziDocumentInfoDelegate>(dllHandle, "libCZI_MetadataSegmentGetCziDocumentInfo");
                this.metadataSegmentRelease = NativeLibraryUtils.GetProcAddressThrowIfNotFound<MetadataSegmentReleaseDelegate>(dllHandle, "libCZI_ReleaseMetadataSegment");
                this.attachmentGetInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<AttachmentGetInfoDelegate>(dllHandle, "libCZI_AttachmentGetInfo");
                this.attachmentGetRawData = NativeLibraryUtils.GetProcAddressThrowIfNotFound<AttachmentGetRawDataDelegate>(dllHandle, "libCZI_AttachmentGetRawData");
                this.releaseAttachment = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReleaseAttachmentDelegate>(dllHandle, "libCZI_ReleaseAttachment");

                this.createOutputStreamForFileWide = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CreateOutputStreamForFileWideDelegate>(dllHandle, "libCZI_CreateOutputStreamForFileWide");
                this.createOutputStreamForFileUtf8 = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CreateOutputStreamForFileUtf8Delegate>(dllHandle, "libCZI_CreateOutputStreamForFileUTF8");
                this.createOutputStreamForExternal = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CreateOutputStreamForExternalDelegate>(dllHandle, "libCZI_CreateOutputStreamFromExternal");
                this.releaseOutputStream = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReleaseOutputStreamDelegate>(dllHandle, "libCZI_ReleaseOutputStream");
                this.createWriter = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CreateWriterDelegate>(dllHandle, "libCZI_CreateWriter");
                this.writerCreate = NativeLibraryUtils.GetProcAddressThrowIfNotFound<WriterCreateDelegate>(dllHandle, "libCZI_WriterCreate");
                this.writerClose = NativeLibraryUtils.GetProcAddressThrowIfNotFound<WriterCloseDelegate>(dllHandle, "libCZI_WriterClose");
                this.releaseWriter = NativeLibraryUtils.GetProcAddressThrowIfNotFound<WriterReleaseDelegate>(dllHandle, "libCZI_ReleaseWriter");
                this.writerAddSubBlock = NativeLibraryUtils.GetProcAddressThrowIfNotFound<WriterAddSubBlockDelegate>(dllHandle, "libCZI_WriterAddSubBlock");
                this.writerAddAttachment = NativeLibraryUtils.GetProcAddressThrowIfNotFound<WriterAddAttachmentDelegate>(dllHandle, "libCZI_WriterAddAttachment");
                this.writerWriteMetadata = NativeLibraryUtils.GetProcAddressThrowIfNotFound<WriterWriteMetadataDelegate>(dllHandle, "libCZI_WriterWriteMetadata");

                this.createSingleChannelTileAccessor = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CreateSingleChannelTileAccessorDelegate>(dllHandle, "libCZI_CreateSingleChannelTileAccessor");
                this.singleChannelTileAccessorCalcSize = NativeLibraryUtils.GetProcAddressThrowIfNotFound<SingleChannelTileAccessorCalcSizeDelegate>(dllHandle, "libCZI_SingleChannelTileAccessorCalcSize");
                this.singleChannelTileAccessorGet = NativeLibraryUtils.GetProcAddressThrowIfNotFound<SingleChannelTileAccessorGetDelegate>(dllHandle, "libCZI_SingleChannelTileAccessorGet");
                this.releaseCreateSingleChannelTileAccessor = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReleaseCreateSingleChannelTileAccessorDelegate>(dllHandle, "libCZI_ReleaseCreateSingleChannelTileAccessor");

                // --- Compositor functions ---
                this.compositorFillOutCompositionChannelInfoInterop = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CompositorFillOutCompositionChannelInfoInteropDelegate>(dllHandle, "libCZI_CompositorFillOutCompositionChannelInfoInterop");
                this.compositorDoMultiChannelComposition = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CompositorDoMultiChannelCompositionDelegate>(dllHandle, "libCZI_CompositorDoMultiChannelComposition");

                // --- CziDocumentInfo functions ---
                this.cziDocumentInfoGetScalingInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CziDocumentInfoGetScalingInfoDelegate>(dllHandle, "libCZI_CziDocumentInfoGetScalingInfo");
                this.cziDocumentInfoGetGeneralDocumentInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CziDocumentInfoGetGeneralDocumentInfoDelegate>(dllHandle, "libCZI_CziDocumentInfoGetGeneralDocumentInfo");
                this.cziDocumentInfoGetAvailableDimension = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CziDocumentInfoGetAvailableDimensionDelegate>(dllHandle, "libCZI_CziDocumentInfoGetAvailableDimension");
                this.cziDocumentInfoGetDisplaySettings = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CziDocumentInfoGetDisplaySettingsDelegate>(dllHandle, "libCZI_CziDocumentInfoGetDisplaySettings");
                this.cziDocumentInfoGetDimensionInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<CziDocumentInfoGetDimensionInfoDelegate>(dllHandle, "libCZI_CziDocumentInfoGetDimensionInfo");
                this.releaseCziDocumentInfo = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReleaseCziDocumentInfoDelegate>(dllHandle, "libCZI_ReleaseCziDocumentInfo");

                // --- DisplaySettings functions ---
                this.releaseDisplaySettings = NativeLibraryUtils.GetProcAddressThrowIfNotFound<ReleaseDisplaySettingsDelegate>(dllHandle, "libCZI_ReleaseDisplaySettings");

                // Note: make sure that the delegates used below are not (i.e. NEVER) garbage-collected, otherwise the native code will crash
                this.functionPointerInputStreamReadFunction =
                    Marshal.GetFunctionPointerForDelegate<InputStreamReadFunctionDelegate>(LibCziApiInterop.InputStreamReadFunctionDelegateObject);
                this.functionPointerInputStreamCloseFunction =
                    Marshal.GetFunctionPointerForDelegate<InputStreamCloseFunctionDelegate>(LibCziApiInterop.InputStreamCloseFunctionDelegateObject);
                this.functionPointerOutputStreamWriteFunction =
                    Marshal.GetFunctionPointerForDelegate<OutputStreamWriteFunctionDelegate>(LibCziApiInterop.OutputStreamWriteFunctionDelegateObject);
                this.functionPointerOutputStreamCloseFunction =
                    Marshal.GetFunctionPointerForDelegate<OutputStreamCloseFunctionDelegate>(LibCziApiInterop.OutputStreamCloseFunctionDelegateObject);

                this.dllHandle = dllHandle;
            }
            catch
            {
                // if we have an exception here, we are in a non-operational state
            }
        }

        /// <summary>
        /// Gets the (one and only) instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static LibCziApiInterop Instance => LibCziApiInterop.LibCziApiInteropInstance.Value;

        private IEnumerable<string> GetFullyQualifiedDllPaths()
        {
            // Note: We are looking for the native DLL in the following locations:
            // - in the same directory as the executing assembly
            // - in the "runtimes/xxx/native" subdirectory of the executing assembly, where "xxx" is the runtime identifier (c.f. https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)
            // - in the folder "../../runtimes/xxx/native", relative to the executing assembly - this was found to be necessary for .NET-interactive
            //
            // Currently, we only support linux-x64, win-x64 and win-arm64.
            // The Nuget-package contains the native DLLs in the "runtimes" directory. Searching in the same directory is used for development purposes.
            //
            // TODO(JBL): I'd like to have CPU-architecture-specific suffixes for the filenames ("x86", "x64", "arm32" etc.) and probably a "d" for debug-builds or so
            const string baseDllName = "libCZIAPI";
            string pathOfExecutable = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            string dllName = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                dllName = baseDllName + ".dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                dllName = "lib" + baseDllName + ".so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                dllName = "lib" + baseDllName + ".dylib";
            }

            if (!string.IsNullOrEmpty(dllName))
            {
                yield return Path.Combine(pathOfExecutable, dllName);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    yield return Path.Combine(pathOfExecutable, "runtimes/win-x64/native/" + dllName);
                    yield return Path.Combine(pathOfExecutable, "../../runtimes/win-x64/native/" + dllName);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    yield return Path.Combine(pathOfExecutable, "runtimes/win-arm64/native/" + dllName);
                    yield return Path.Combine(pathOfExecutable, "../../runtimes/win-arm64/native/" + dllName);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    yield return Path.Combine(pathOfExecutable, "runtimes/osx-arm64/native/" + dllName);
                    yield return Path.Combine(pathOfExecutable, "../../runtimes/osx-arm64/native/" + dllName);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                {
                    if (string.Compare(NativeLibraryUtils.GetLinuxDistro(), "alpine", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        yield return Path.Combine(pathOfExecutable, "runtimes/linux-musl-arm64/native/" + dllName);
                        yield return Path.Combine(pathOfExecutable, "../../runtimes/linux-musl-arm64/native/" + dllName);
                    }
                    else
                    {
                        yield return Path.Combine(pathOfExecutable, "runtimes/linux-arm64/native/" + dllName);
                        yield return Path.Combine(pathOfExecutable, "../../runtimes/linux-arm64/native/" + dllName);
                    }
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    if (string.Compare(NativeLibraryUtils.GetLinuxDistro(), "alpine", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        yield return Path.Combine(pathOfExecutable, "runtimes/linux-musl-x64/native/" + dllName);
                        yield return Path.Combine(pathOfExecutable, "../../runtimes/linux-musl-x64/native/" + dllName);
                    }
                    else
                    {
                        yield return Path.Combine(pathOfExecutable, "runtimes/linux-x64/native/" + dllName);

                        // in case of .NET-interactive, the assembly is loaded directly from the Nuget-package, so we need to look in the parent directory
                        yield return Path.Combine(pathOfExecutable, "../../runtimes/linux-x64/native/" + dllName);
                    }
                }
            }
        }
    }

    /// <content>
    /// Provides interop methods for interacting with the CZI library.
    /// </content>
    internal partial class LibCziApiInterop
    {
        /// <summary>
        /// Frees the memory.
        /// </summary>
        /// <param name="memory">The memory that needs to be freed.</param>
        public void FreeMemory(IntPtr memory)
        {
            this.ThrowIfNotInitialized();
            this.libCziFree(memory);
        }

        /// <summary>
        /// Allocates memory.
        /// </summary>
        /// <param name="size">The size to allocate in bytes.</param>
        /// <returns>A pointer to the allocated memory.</returns>
        public IntPtr AllocateMemory(ulong size)
        {
            this.ThrowIfNotInitialized();
            IntPtr memory = IntPtr.Zero;
            unsafe
            {
                int returnCode = this.libCziAllocateMemory(size, &memory);
                this.ThrowIfError(returnCode);
            }

            return memory;
        }

        /// <summary>
        /// Gets the version information.
        /// </summary>
        /// <returns>Struct that <see cref="VersionInfo"/> includes version information (major, minor, patch, tweak.)</returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1129:Do not use default value type constructor", Justification = "Interop-function.")]
        public VersionInfo GetVersionInfo()
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                LibCziVersionInfoInterop versionInfoInterop = new LibCziVersionInfoInterop();
                int returnCode = this.getLibCziVersionInfo(&versionInfoInterop);
                this.ThrowIfError(returnCode);
                return new VersionInfo(versionInfoInterop.major, versionInfoInterop.minor, versionInfoInterop.patch, versionInfoInterop.tweak);
            }
        }

        /// <summary>
        ///  Gets the build information.
        /// </summary>
        /// <returns>
        /// <see cref="BuildInfo"/> includes build information (compilerIdentification, repositoryUrl, repositoryBranch, repositoryTag.)
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1129:Do not use default value type constructor", Justification = "Interop-function.")]
        public BuildInfo GetBuildInformation()
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                LibCziBuildInformationInterop buildInformationInterop = new LibCziBuildInformationInterop();
                int returnCode = this.getLibCziBuildInformation(&buildInformationInterop);
                this.ThrowIfError(returnCode);
                try
                {
                    var result = new BuildInfo(
                        NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminatedAllowNull(buildInformationInterop.compilerIdentification),
                        NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminatedAllowNull(buildInformationInterop.repositoryUrl),
                        NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminatedAllowNull(buildInformationInterop.repositoryBranch),
                        NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminatedAllowNull(buildInformationInterop.repositoryTag));

                    return result;
                }
                finally
                {
                    this.libCziFree(buildInformationInterop.compilerIdentification);
                    this.libCziFree(buildInformationInterop.repositoryUrl);
                    this.libCziFree(buildInformationInterop.repositoryBranch);
                    this.libCziFree(buildInformationInterop.repositoryTag);
                }
            }
        }

        /// <summary>
        /// Gets the stream classes count.
        /// </summary>
        /// <returns>The number of stream classes as <c>int</c>.</returns>
        public int GetStreamClassesCount()
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                int count;
                int returnCode = this.getStreamClassesCount(&count);
                this.ThrowIfError(returnCode);
                return count;
            }
        }

        /// <summary>
        /// Gets the stream class information.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>An instance of <see cref="InputStreamClassInfo"/> that contains information about a given stream class.</returns>
        public InputStreamClassInfo GetStreamClassInfo(int index)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                InputStreamClassInfoInterop inputStreamClassInfoInterop = default(InputStreamClassInfoInterop);
                int returnCode = this.getStreamClassInfo(index, &inputStreamClassInfoInterop);
                this.ThrowIfError(returnCode);
                try
                {
                    return new InputStreamClassInfo(
                        NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminatedAllowNull(inputStreamClassInfoInterop.name),
                        NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminatedAllowNull(inputStreamClassInfoInterop.description));
                }
                finally
                {
                    this.libCziFree(inputStreamClassInfoInterop.name);
                    this.libCziFree(inputStreamClassInfoInterop.description);
                }
            }
        }

        /// <summary>
        /// Creates the input stream from file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>A handle to the created input stream object.</returns>
        public IntPtr CreateInputStreamFromFile(string filename)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntPtr streamObjectHandle;
                int returnCode;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // On Windows, we use the wide version of the function (which takes a UCS2-encoded string)
                    fixed (char* filenamePtr = filename)
                    {
                        returnCode = this.createInputStreamFromFileWide(new IntPtr(filenamePtr), &streamObjectHandle);
                    }
                }
                else
                {
                    // On Linux and MacOS, we use the UTF-8 version of the function (which takes a UTF-8-encoded string)
                    byte[] filenameUtf8 = NativeLibraryUtils.ConvertToUtf8ByteArray(filename);
                    fixed (byte* filenameUtf8Ptr = filenameUtf8)
                    {
                        returnCode = this.createInputStreamFromFileUtf8(new IntPtr(filenameUtf8Ptr), &streamObjectHandle);
                    }
                }

                this.ThrowIfError(returnCode);
                return streamObjectHandle;
            }
        }

        /// <summary>
        /// Creates the input stream from external stream (for example .Net filestream).
        /// </summary>
        /// <param name="externalInputStream">The external input stream.</param>
        /// <returns>
        /// A handle to the input stream represented as an <see cref = "IntPtr" />.
        /// This handle can be used to read data from the input stream.
        /// </returns>
        public IntPtr CreateInputStreamFromExternal(IExternalInputStream externalInputStream)
        {
            this.ThrowIfNotInitialized();
            var data = new InputStreamData() { ExternalInputStream = externalInputStream };
            GCHandle gcHandle = GCHandle.Alloc(data);

            ExternalInputStreamStructInterop externalInputStreamStructInterop = new ExternalInputStreamStructInterop
            {
                opaque_handle1 = GCHandle.ToIntPtr(gcHandle),
                opaque_handle2 = IntPtr.Zero,
                read_function_pointer = this.functionPointerInputStreamReadFunction,
                close_function_pointer = this.functionPointerInputStreamCloseFunction,
            };

            unsafe
            {
                IntPtr streamObjectHandle;
                this.createInputStreamFromExternal(&externalInputStreamStructInterop, &streamObjectHandle);
                return streamObjectHandle;
            }
        }

        /// <summary>
        /// Creates an input stream using the specified class name, properties, and URI.
        /// </summary>
        /// <param name="className">The name of the class to create the input stream from. Cannot be null.</param>
        /// <param name="properties">Optional properties for the input stream. If null, an empty string is used.</param>
        /// <param name="uri">Optional URI for the input stream. If null, an empty string is used.</param>
        /// <returns>A handle to the created input stream object.</returns>
        public IntPtr CreateInputStream(string className, string properties, string uri)
        {
            this.ThrowIfNotInitialized();
            if (className == null)
            {
                throw new ArgumentNullException(nameof(className));
            }

            if (properties == null)
            {
                properties = string.Empty;
            }

            if (uri == null)
            {
                uri = string.Empty;
            }

            byte[] classNameUf8 = NativeLibraryUtils.ConvertToUtf8ByteArray(className);
            byte[] propertiesUtf8 = NativeLibraryUtils.ConvertToUtf8ByteArray(properties);
            byte[] uriUtf8 = NativeLibraryUtils.ConvertToUtf8ByteArray(uri);

            unsafe
            {
                fixed (byte* classNamePtr = classNameUf8)
                {
                    fixed (byte* propertiesPtr = propertiesUtf8)
                    {
                        fixed (byte* uriPtr = uriUtf8)
                        {
                            IntPtr streamObjectHandle;
                            int returnCode = this.createInputStream(new IntPtr(classNamePtr), new IntPtr(propertiesPtr), new IntPtr(uriPtr), &streamObjectHandle);
                            this.ThrowIfError(returnCode);
                            return streamObjectHandle;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Releases the input stream.
        /// </summary>
        /// <param name="inputStreamHandle">The input stream handle.</param>
        public void ReleaseInputStream(IntPtr inputStreamHandle)
        {
            this.ThrowIfNotInitialized();
            this.releaseInputStream(inputStreamHandle);
        }

        /// <summary>
        /// Creates a reader handler.
        /// </summary>
        /// <returns>
        /// A reader handler as an<see cref = "IntPtr" />.
        /// </returns>
        public IntPtr CreateReader()
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntPtr readerHandle;
                int returnCode = this.createReader(&readerHandle);
                this.ThrowIfError(returnCode);
                return readerHandle;
            }
        }

        /// <summary>
        /// Releases the reader.
        /// </summary>
        /// <param name="readerHandle">The reader handle.</param>
        public void ReleaseReader(IntPtr readerHandle)
        {
            this.ThrowIfNotInitialized();
            this.readerRelease(readerHandle);
        }

        /// <summary>
        /// Opens a reader for the specified input stream.
        /// </summary>
        /// <param name="readerHandle">A handle to the reader that will be opened.</param>
        /// <param name="inputStreamHandle">A handle to the input stream that the reader will read from.</param>
        public void ReaderOpen(IntPtr readerHandle, IntPtr inputStreamHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                ReaderOpenInfoInterop readerOpenInfoInterop = new ReaderOpenInfoInterop { streamObject = inputStreamHandle };
                int returnCode = this.readerOpen(readerHandle, &readerOpenInfoInterop);
                this.ThrowIfError(returnCode);
            }
        }

        /// <summary>
        /// Reads the file-header.
        /// </summary>
        /// <param name="readerObjectHandle"> The reader handle.</param>
        /// <returns>
        /// A FileHeaderInfo structure that includes file-header information.
        /// </returns>
        public FileHeaderInfo ReaderGetFileHeaderInfo(IntPtr readerObjectHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                FileHeaderInfoInterop fileHeaderInfoInterop = default(FileHeaderInfoInterop);
                int returnCode = this.readerGetFileHeaderInfo(readerObjectHandle, &fileHeaderInfoInterop);
                this.ThrowIfError(returnCode);
                return ToFileHeaderInfo(in fileHeaderInfoInterop);
            }
        }

        /// <summary>
        /// Gets a sub-block statistics from specified reader.
        /// </summary>
        /// <param name="readerHandle">A handle to the reader that will read the sub-block statistics.</param>
        /// <returns>
        /// Sub-block statistics <see cref = "SubBlockStatistics" />.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1129:Do not use default value type constructor", Justification = "Interop-function.")]
        public SubBlockStatistics ReaderGetSubBlockStatistics(IntPtr readerHandle)
        {
            this.ThrowIfNotInitialized();
            SubBlockStatisticsInterop statisticsInterop = new SubBlockStatisticsInterop();

            unsafe
            {
                int returnCode = this.readerGetStatisticsSimple(readerHandle, &statisticsInterop);
                this.ThrowIfError(returnCode);
            }

            SubBlockStatistics subBlockStatistics = new SubBlockStatistics()
            {
                SubBlockCount = statisticsInterop.subBlockCount,
                MinimumMIndex = statisticsInterop.minimum_m_index,
                MaximumMIndex = statisticsInterop.maximum_m_index,
                BoundingBox = new IntRect(statisticsInterop.boundingBox.x, statisticsInterop.boundingBox.y, statisticsInterop.boundingBox.w, statisticsInterop.boundingBox.h),
                BoundingBoxLayer0 =
                    new IntRect(statisticsInterop.boundingBoxLayer0.x, statisticsInterop.boundingBoxLayer0.y, statisticsInterop.boundingBoxLayer0.w, statisticsInterop.boundingBoxLayer0.h),
                DimensionBounds = LibCziApiInterop.DimBoundsInteropToDimensionBounds(in statisticsInterop.dimBounds),
            };

            return subBlockStatistics;
        }

        /// <summary>
        /// Gets an extended sub-block statistics from the specified reader.
        /// </summary>
        /// <param name="readerHandle">A handle to the reader that will read the extended sub-block statistics.</param>
        /// <returns>
        /// Extended sub-block statistics <see cref = "SubBlockStatisticsEx" />.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">The number of scenes changed between the first and second call to libCZI_ReaderGetStatisticsEx. This is unexpected and a fatal error.</exception>
        public SubBlockStatisticsEx ReaderGetSubBlockStatisticsEx(IntPtr readerHandle)
        {
            this.ThrowIfNotInitialized();
            const int maxNumberOfScenesInitially = 8;

            int sizeOfSubBlockStatisticsInteropExInitially = GetSizeOfSubBlockStatisticsInteropExForNScenes(maxNumberOfScenesInitially);
            unsafe
            {
                // Allocate memory on the stack for the structs
                Span<byte> memory = stackalloc byte[sizeOfSubBlockStatisticsInteropExInitially];

                // Get a pointer to the first element
                SubBlockStatisticsInteropEx* pointer = (SubBlockStatisticsInteropEx*)Unsafe.AsPointer(ref memory[0]);

                int numberOfScenesAllocated = maxNumberOfScenesInitially;
                this.readerGetStatisticsEx(readerHandle, pointer, &numberOfScenesAllocated);

                if (numberOfScenesAllocated < maxNumberOfScenesInitially)
                {
                    // If the initial buffer was large enough, we can return the result directly
                    return new SubBlockStatisticsEx(GetBoundingBoxes(pointer))
                    {
                        SubBlockCount = pointer->subBlockCount,
                        MinimumMIndex = pointer->minimum_m_index,
                        MaximumMIndex = pointer->maximum_m_index,
                        BoundingBox = new IntRect(pointer->boundingBox.x, pointer->boundingBox.y, pointer->boundingBox.w, pointer->boundingBox.h),
                        BoundingBoxLayer0 = new IntRect(pointer->boundingBoxLayer0.x, pointer->boundingBoxLayer0.y, pointer->boundingBoxLayer0.w, pointer->boundingBoxLayer0.h),
                        DimensionBounds = LibCziApiInterop.DimBoundsInteropToDimensionBounds(in pointer->dimBounds),
                    };
                }

                // so, we need to allocate a larger buffer and try again (this time we know the required size, and we allocate on the heap)
                int numberOfScenesRequired = numberOfScenesAllocated;
                memory = new byte[GetSizeOfSubBlockStatisticsInteropExForNScenes(numberOfScenesRequired)];
                pointer = (SubBlockStatisticsInteropEx*)Unsafe.AsPointer(ref memory[0]);

                // and, try again, this time we can be sure that the buffer is large enough
                this.readerGetStatisticsEx(readerHandle, pointer, &numberOfScenesAllocated);

                // Check if the number of scenes is still the same as before - if not, we have a problem
                if (numberOfScenesAllocated != numberOfScenesRequired)
                {
                    throw new InvalidOperationException(
                        "The number of scenes changed between the first and second call to libCZI_ReaderGetStatisticsEx. This is unexpected and a fatal error.");
                }

                return new SubBlockStatisticsEx(GetBoundingBoxes(pointer))
                {
                    SubBlockCount = pointer->subBlockCount,
                    MinimumMIndex = pointer->minimum_m_index,
                    MaximumMIndex = pointer->maximum_m_index,
                    BoundingBox = new IntRect(pointer->boundingBox.x, pointer->boundingBox.y, pointer->boundingBox.w, pointer->boundingBox.h),
                    BoundingBoxLayer0 = new IntRect(pointer->boundingBoxLayer0.x, pointer->boundingBoxLayer0.y, pointer->boundingBoxLayer0.w, pointer->boundingBoxLayer0.h),
                    DimensionBounds = LibCziApiInterop.DimBoundsInteropToDimensionBounds(in pointer->dimBounds),
                };
            }
        }

        /// <summary>
        /// Gets a pyramid statistics as json from the specified reader.
        /// </summary>
        /// <param name="readerHandle">A handle to the reader that will read the pyramid statistics.</param>
        /// <returns>
        /// Pyramid statistics as string.
        /// </returns>
        public string ReaderGetPyramidStatisticsAsJson(IntPtr readerHandle)
        {
            this.ThrowIfNotInitialized();
            IntPtr pyramidStatisticsAsJson = IntPtr.Zero;
            try
            {
                unsafe
                {
                    this.readerGetPyramidStatistics(readerHandle, &pyramidStatisticsAsJson);
                    return NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminated(pyramidStatisticsAsJson);
                }
            }
            finally
            {
                this.libCziFree(pyramidStatisticsAsJson);
            }
        }

        /// <summary>
        /// Reads a sub-block from the specified reader at the given index.
        /// </summary>
        /// <param name="readerHandle">The reader handle.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        /// A handle to the sub-block read from the reader.
        /// This handle can be used for further operations on the sub-block.
        /// </returns>
        public IntPtr ReaderReadSubBlock(IntPtr readerHandle, int index)
        {
            this.ThrowIfNotInitialized();
            IntPtr subBlockHandle;
            unsafe
            {
                int returnCode = this.readerReadSubBlock(readerHandle, index, &subBlockHandle);
                this.ThrowIfError(returnCode);
            }

            return subBlockHandle;
        }

        /// <summary>
        /// Tries to get information about the sub-block with the specified index.
        /// </summary>
        /// <param name="readerHandle">The reader handle.</param>
        /// <param name="index">The index.</param>
        /// <param name="subBlockInfo"> [out] Information describing the sub-block.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool ReaderTryGetSubBlockInfoForIndex(IntPtr readerHandle, int index, out SubBlockInfo subBlockInfo)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                SubBlockInfoInterop subBlockInfoInterop = default(SubBlockInfoInterop);
                int returnCode = this.readerTryGetSubBlockInfoForIndex(readerHandle, index, &subBlockInfoInterop);
                if (returnCode == LibCziIndexOutOfRange)
                {
                    subBlockInfo = default;
                    return false;
                }

                this.ThrowIfError(returnCode);
                subBlockInfo = ToSubBlockInfo(in subBlockInfoInterop);

                return true;
            }
        }

        /// <summary>
        /// Gets the metadata segment.
        /// </summary>
        /// <param name="readerHandle">The reader handle.</param>
        /// <returns>A handle to the metadata-segment read from the reader.</returns>
        public IntPtr ReaderGetMetadataSegment(IntPtr readerHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntPtr metadataSegmentHandle;
                int returnCode = this.readerGetMetadataSegment(readerHandle, &metadataSegmentHandle);
                this.ThrowIfError(returnCode);
                return metadataSegmentHandle;
            }
        }

        /// <summary>
        /// Get the number of attachments available.
        /// </summary>
        /// <param name="readerHandle">The reader handle.</param>
        /// <returns>The number of attachments available.</returns>
        public int ReaderGetAttachmentCount(IntPtr readerHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                int attachmentCount = 0;
                int returnCode = this.readerGetAttachmentCount(readerHandle, &attachmentCount);
                this.ThrowIfError(returnCode);
                return attachmentCount;
            }
        }

        /// <summary> Tries to get information about the attachment with the specified index.</summary>
        /// <param name="readerHandle">   The reader handle.</param>
        /// <param name="index">          The index.</param>
        /// <param name="attachmentInfo"> [out] Information describing the attachment.</param>
        /// <returns> True if it succeeds, false if it fails.</returns>
        public bool ReaderTryGetAttachmentFromDirectoryInfo(IntPtr readerHandle, int index, out AttachmentInfo attachmentInfo)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                AttachmentInfoInterop attachmentInfoInterop = default(AttachmentInfoInterop);
                try
                {
                    int returnCode = this.readerGetAttachmentFromDirectoryInfo(readerHandle, index, &attachmentInfoInterop);
                    if (returnCode == LibCziIndexOutOfRange)
                    {
                        attachmentInfo = default;
                        return false;
                    }

                    this.ThrowIfError(returnCode);
                    attachmentInfo = ToAttachmentInfo(in attachmentInfoInterop);
                    return true;
                }
                finally
                {
                    if (attachmentInfoInterop.name_overflow && attachmentInfoInterop.name_in_case_of_overflow != IntPtr.Zero)
                    {
                        this.libCziFree(attachmentInfoInterop.name_in_case_of_overflow);
                    }
                }
            }
        }

        /// <summary> Read the specified attachment.</summary>
        /// <param name="readerHandle"> The reader handle.</param>
        /// <param name="index">        The index.</param>
        /// <returns> A handle for the newly created attachment object.</returns>
        public IntPtr ReaderReadAttachment(IntPtr readerHandle, int index)
        {
            this.ThrowIfNotInitialized();
            IntPtr attachmentHandle;
            unsafe
            {
                int returnCode = this.readerReadAttachment(readerHandle, index, &attachmentHandle);
                this.ThrowIfError(returnCode);
            }

            return attachmentHandle;
        }

        /// <summary>
        /// Releases the attachment object.
        /// </summary>
        /// <param name="attachmentHandle">The attachment handle.</param>
        public void ReleaseAttachment(IntPtr attachmentHandle)
        {
            this.ThrowIfNotInitialized();
            int status = this.releaseAttachment(attachmentHandle);
            this.ThrowIfError(status);
        }

        /// <summary> Get information from the specified attachment object.</summary>
        /// <param name="attachmentHandle"> The attachment handle.</param>
        /// <returns> The AttachmentInfo.</returns>
        public AttachmentInfo AttachmentGetInfo(IntPtr attachmentHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                AttachmentInfoInterop attachmentInfoInterop = default(AttachmentInfoInterop);
                try
                {
                    int returnCode = this.attachmentGetInfo(attachmentHandle, &attachmentInfoInterop);
                    this.ThrowIfError(returnCode);
                    return ToAttachmentInfo(in attachmentInfoInterop);
                }
                finally
                {
                    if (attachmentInfoInterop.name_overflow && attachmentInfoInterop.name_in_case_of_overflow != IntPtr.Zero)
                    {
                        this.libCziFree(attachmentInfoInterop.name_in_case_of_overflow);
                    }
                }
            }
        }

        /// <summary> Get the data payload from the specified attachment.</summary>
        /// <param name="attachmentHandle"> The attachment handle.</param>
        /// <returns> A Span&lt;byte&gt; containing the requested data. </returns>
        public Span<byte> AttachmentGetRawData(IntPtr attachmentHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                ulong size = 0;
                int returnCode = this.attachmentGetRawData(attachmentHandle, &size, null);
                this.ThrowIfError(returnCode);
                if (size == 0)
                {
                    return Span<byte>.Empty;
                }

                // TODO(JBL): check size whether it is too large for "new byte[]" allocation
                var buffer = new byte[size];
                fixed (byte* bufferPtr = buffer)
                {
                    returnCode = this.attachmentGetRawData(attachmentHandle, &size, bufferPtr);
                }

                this.ThrowIfError(returnCode);
                return buffer;
            }
        }

        /// <summary>
        /// Gets metadata-segment as XML.
        /// </summary>
        /// <param name="metadataSegmentHandle">A handle to the metadata segment from which to retrieve the XML metadata.</param>
        /// <returns>A <see cref="SafeBuffer"/> containing the XML representation of the metadata.</returns>
        public SafeBuffer MetadataSegmentGetMetadataAsXml(IntPtr metadataSegmentHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                MetadataAsXmlInterop metadataAsXmlInterop = default(MetadataAsXmlInterop);
                int returnCode = this.metadataSegmentGetMetadataAsXml(metadataSegmentHandle, &metadataAsXmlInterop);
                this.ThrowIfError(returnCode);
                return new SafeBufferOnNativeMemory(metadataAsXmlInterop.xml, metadataAsXmlInterop.size);
            }
        }

        /// <summary> Create CZI-document-info object.</summary>
        /// <param name="metadataSegmentHandle"> A handle to the metadata segment for which to create
        ///                                      CZI-document-info object.</param>
        /// <returns> A handle representing the newly created CZI-document-info object.</returns>
        public IntPtr MetadataSegmentGetCziDocumentInfo(IntPtr metadataSegmentHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntPtr cziDocumentInfoHandle;
                int returnCode = this.metadataSegmentGetCziDocumentInfo(metadataSegmentHandle, &cziDocumentInfoHandle);
                this.ThrowIfError(returnCode);
                return cziDocumentInfoHandle;
            }
        }

        /// <summary>
        /// Releases the metadata segment.
        /// </summary>
        /// <param name="metadataSegmentHandle">The metadata segment handle.</param>
        public void ReleaseMetadataSegment(IntPtr metadataSegmentHandle)
        {
            this.ThrowIfNotInitialized();
            int returnCode = this.metadataSegmentRelease(metadataSegmentHandle);
            this.ThrowIfError(returnCode);
        }

        /// <summary>
        /// Creates a bitmap handler from the specified sub-block.
        /// </summary>
        /// <param name="subBlockHandle">The sub block handle.</param>
        /// <returns>
        /// A handle to the bitmap created from sub-block.
        /// This handle can be used for further operations on the bitmap.
        /// </returns>
        public IntPtr SubBlockCreateBitmap(IntPtr subBlockHandle)
        {
            this.ThrowIfNotInitialized();
            IntPtr bitmapHandle;
            unsafe
            {
                int returnCode = this.subBlockCreateBitmap(subBlockHandle, &bitmapHandle);
                this.ThrowIfError(returnCode);
            }

            return bitmapHandle;
        }

        /// <summary> Get the sub-block-information for the specified sub-block.</summary>
        /// <param name="subBlockHandle"> The sub block handle.</param>
        /// <returns> The SubBlockInfo for the specified sub-block.</returns>
        public SubBlockInfo SubBlockGetInfo(IntPtr subBlockHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                SubBlockInfoInterop subBlockInfoInterop = default(SubBlockInfoInterop);
                int returnCode = this.subBlockGetInfo(subBlockHandle, &subBlockInfoInterop);
                this.ThrowIfError(returnCode);
                return new SubBlockInfo(
                    subBlockInfoInterop.compression_mode_raw,
                    (PixelType)subBlockInfoInterop.pixel_type,
                    CoordinateInteropToCoordinate(in subBlockInfoInterop.coordinate),
                    new IntRect(subBlockInfoInterop.logical_rect.x, subBlockInfoInterop.logical_rect.y, subBlockInfoInterop.logical_rect.w, subBlockInfoInterop.logical_rect.h),
                    new IntSize(subBlockInfoInterop.physical_size.w, subBlockInfoInterop.physical_size.h),
                    subBlockInfoInterop.m_index);
            }
        }

        /// <summary>
        /// Gets data for the specified sub-block. There are two types of data that can be retrieved: the bitmap-data and
        /// the sub-block metadata.
        /// If the requested type of data is not available, then an empty span is returned.
        /// </summary>
        /// <param name="subBlockHandle"> The sub block handle.</param>
        /// <param name="type">           The type of data to retrieve - 0 for "pixel-data", 1 for "sub-block metadata".</param>
        /// <returns> A Span&lt;byte&gt; containing the requested data.</returns>
        public Span<byte> SubBlockGetRawData(IntPtr subBlockHandle, int type)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                ulong size = 0;
                int returnCode = this.subBlockGetRawData(subBlockHandle, type, &size, null);
                this.ThrowIfError(returnCode);
                if (size == 0)
                {
                    return Span<byte>.Empty;
                }

                var buffer = new byte[size];
                fixed (byte* bufferPtr = buffer)
                {
                    returnCode = this.subBlockGetRawData(subBlockHandle, type, &size, bufferPtr);
                }

                this.ThrowIfError(returnCode);
                return buffer;
            }
        }

        /// <summary>
        /// Releases the sub block.
        /// </summary>
        /// <param name="subBlockHandle">The sub block handle.</param>
        public void ReleaseSubBlock(IntPtr subBlockHandle)
        {
            this.ThrowIfNotInitialized();
            int status = this.releaseSubBlock(subBlockHandle);
            this.ThrowIfError(status);
        }

        /// <summary>
        /// Gets the bitmap information from specified bitmap handler.
        /// </summary>
        /// <param name="bitmapHandle">The bitmap handle.</param>
        /// <returns>
        /// Width, height and pixel type as BitmapInfo struct <see cref = "BitmapInfo" />.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1129:Do not use default value type constructor", Justification = "Interop-function.")]
        public BitmapInfo BitmapGetInfo(IntPtr bitmapHandle)
        {
            this.ThrowIfNotInitialized();
            BitmapInfoInterop bitmapInfoInterop = new BitmapInfoInterop();
            unsafe
            {
                this.bitmapGetInfo(bitmapHandle, &bitmapInfoInterop);
            }

            return new BitmapInfo(
                (int)bitmapInfoInterop.width,
                (int)bitmapInfoInterop.height,
                (PixelType)bitmapInfoInterop.pixelType);
        }

        /// <summary>
        /// Locks the specified bitmap and retrieves information about its data region and stride.
        /// </summary>
        /// <param name="bitmapHandle">The bitmap handle.</param>
        /// <returns>
        /// A <see cref="BitmapLockInfo"/> object containing information about the locked bitmap.
        /// </returns>
        public BitmapLockInfo BitmapLock(IntPtr bitmapHandle)
        {
            this.ThrowIfNotInitialized();
            var bitmapLockInfoInterop = default(BitmapLockInfoInterop);
            unsafe
            {
                this.bitmapLock(bitmapHandle, &bitmapLockInfoInterop);
            }

            return new BitmapLockInfo(bitmapLockInfoInterop.dataRoi, (int)bitmapLockInfoInterop.stride);
        }

        /// <summary>
        /// Unlocks the specified bitmap.
        /// </summary>
        /// <param name="bitmapHandle">The bitmap handle.</param>
        public void BitmapUnlock(IntPtr bitmapHandle)
        {
            this.ThrowIfNotInitialized();
            this.bitmapUnlock(bitmapHandle);
        }

        /// <summary>
        /// Copies the specified bitmap to the specified ptr parameter.
        /// </summary>
        /// <param name="bitmapHandle">The bitmap handle.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="pixelType">Type of the pixel.</param>
        /// <param name="stride">The stride.</param>
        /// <param name="ptr">The PTR.</param>
        public void BitmapCopyTo(IntPtr bitmapHandle, int width, int height, PixelType pixelType, int stride, IntPtr ptr)
        {
            this.ThrowIfNotInitialized();
            this.bitmapCopyTo(bitmapHandle, (uint)width, (uint)height, (int)pixelType, (uint)stride, ptr);
        }

        /// <summary>
        /// Releases the specified bitmap.
        /// </summary>
        /// <param name="bitmapHandle">The bitmap handle.</param>
        public void ReleaseBitmap(IntPtr bitmapHandle)
        {
            this.ThrowIfNotInitialized();
            this.releaseBitmap(bitmapHandle);
        }

        /// <summary> Creates an output stream for the specified filename.</summary>
        /// <param name="filename">          The filename.</param>
        /// <param name="overwriteExisting"> True if to attempt to overwrite an existing file, false otherwise.</param>
        /// <returns> A handle to the created output stream object.</returns>
        public IntPtr CreateOutputStreamForFile(string filename, bool overwriteExisting)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntPtr streamObjectHandle;
                int returnCode;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // On Windows, we use the wide version of the function (which takes a UCS2-encoded string)
                    fixed (char* filenamePtr = filename)
                    {
                        returnCode = this.createOutputStreamForFileWide(new IntPtr(filenamePtr), overwriteExisting, &streamObjectHandle);
                    }
                }
                else
                {
                    // On Linux and MacOS, we use the UTF-8 version of the function (which takes a UTF-8-encoded string)
                    byte[] filenameUtf8 = NativeLibraryUtils.ConvertToUtf8ByteArray(filename);
                    fixed (byte* filenameUtf8Ptr = filenameUtf8)
                    {
                        returnCode = this.createOutputStreamForFileUtf8(new IntPtr(filenameUtf8Ptr), overwriteExisting, &streamObjectHandle);
                    }
                }

                this.ThrowIfError(returnCode);
                return streamObjectHandle;
            }
        }

        /// <summary>
        /// Releases the output stream object.
        /// </summary>
        /// <param name="outputStreamHandle">The output stream handle.</param>
        public void ReleaseOutputStream(IntPtr outputStreamHandle)
        {
            this.ThrowIfNotInitialized();
            int returnCode = this.releaseOutputStream(outputStreamHandle);
            this.ThrowIfError(returnCode);
        }

        /// <summary>
        /// Creates the output stream from external stream (for example .Net filestream).
        /// </summary>
        /// <param name="externalOutputStream">The external output stream.</param>
        /// <returns>
        /// A handle to the output stream represented as an <see cref = "IntPtr" />.
        /// This handle can be used to write data to the output stream.
        /// </returns>
        public IntPtr CreateOutputStreamFromExternal(IExternalOutputStream externalOutputStream)
        {
            this.ThrowIfNotInitialized();
            var data = new OutputStreamData() { ExternalOutputStream = externalOutputStream };
            GCHandle gcHandle = GCHandle.Alloc(data);

            ExternalOutputStreamStructInterop externalOutputStreamStructInterop = new ExternalOutputStreamStructInterop
            {
                opaque_handle1 = GCHandle.ToIntPtr(gcHandle),
                opaque_handle2 = IntPtr.Zero,
                write_function_pointer = this.functionPointerOutputStreamWriteFunction,
                close_function_pointer = this.functionPointerOutputStreamCloseFunction,
            };

            unsafe
            {
                IntPtr outputStreamObjectHandle;
                int returnCode = this.createOutputStreamForExternal(&externalOutputStreamStructInterop, &outputStreamObjectHandle);
                this.ThrowIfError(returnCode);
                return outputStreamObjectHandle;
            }
        }

        /// <summary> Creates an output stream object for the specified filename.</summary>
        /// <param name="filename">             The filename.</param>
        /// <param name="tryOverwriteExisting"> True if an existing file should be overwritten.</param>
        /// <returns> The newly created output stream object.</returns>
        public IntPtr CreateOutputStreamForFilename(string filename, bool tryOverwriteExisting)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntPtr outputStreamObjectHandle;
                int returnCode;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // On Windows, we use the wide version of the function (which takes a UCS2-encoded string)
                    fixed (char* filenamePtr = filename)
                    {
                        returnCode = this.createOutputStreamForFileWide(new IntPtr(filenamePtr), tryOverwriteExisting, &outputStreamObjectHandle);
                    }
                }
                else
                {
                    // On Linux and MacOS, we use the UTF-8 version of the function (which takes a UTF-8-encoded string)
                    byte[] filenameUtf8 = NativeLibraryUtils.ConvertToUtf8ByteArray(filename);
                    fixed (byte* filenameUtf8Ptr = filenameUtf8)
                    {
                        returnCode = this.createOutputStreamForFileUtf8(new IntPtr(filenameUtf8Ptr), tryOverwriteExisting, &outputStreamObjectHandle);
                    }
                }

                this.ThrowIfError(returnCode);
                return outputStreamObjectHandle;
            }
        }

        /// <summary>Creates a writer object.</summary>
        /// <returns>
        /// Handle to the writer object as <see cref = "IntPtr" />.
        /// </returns>
        public IntPtr CreateWriter()
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntPtr writerHandle;
                int returnCode = this.createWriter(&writerHandle, IntPtr.Zero);
                this.ThrowIfError(returnCode);
                return writerHandle;
            }
        }

        /// <summary>
        /// Releases the Writer object.
        /// </summary>
        /// <param name="writerHandle">The writer handle.</param>
        public void ReleaseWriter(IntPtr writerHandle)
        {
            this.ThrowIfNotInitialized();
            int returnCode = this.releaseWriter(writerHandle);
            this.ThrowIfError(returnCode);
        }

        /// <summary>
        /// Closes the CZI-file associated with the specified handle.
        /// </summary>
        /// <param name="writerHandle">The writer handle.</param>
        public void WriterClose(IntPtr writerHandle)
        {
            this.ThrowIfNotInitialized();
            int returnCode = this.writerClose(writerHandle);
            this.ThrowIfError(returnCode);
        }

        /// <summary>
        /// Initializes the given writer-object.
        /// </summary>
        /// <param name="writerHandle">A handle to the writer that will be initialized.</param>
        /// <param name="outputStreamHandle">A handle to the output-stream that the writer will write to.</param>
        /// <param name="properties">Optional properties for the writer. Those are given in JSON format. May be null.</param>
        public void WriterInitialize(IntPtr writerHandle, IntPtr outputStreamHandle, string properties)
        {
            this.ThrowIfNotInitialized();

            if (properties == null)
            {
                properties = string.Empty;
            }

            byte[] propertiesUtf8 = NativeLibraryUtils.ConvertToUtf8ByteArray(properties);
            unsafe
            {
                fixed (byte* propertiesPtr = propertiesUtf8)
                {
                    int returnCode = this.writerCreate(writerHandle, outputStreamHandle, new IntPtr(propertiesPtr));
                    this.ThrowIfError(returnCode);
                }
            }
        }

        /// <summary>
        /// Adds the specified uncompressed sub-block to the CZI-file.
        /// </summary>
        /// <param name="writerHandle">A handle to the writer that will add the sub-block into CZI-file.</param>
        /// <param name="addSubBlockInfoUncompressed">Uncompressed sub-block information that is going to be added.</param>
        /// <param name="addSubBlockData">The sub-block data that is going to be added.</param>
        public void WriterAddSubBlockUncompressed(IntPtr writerHandle, in AddSubBlockInfoUncompressed addSubBlockInfoUncompressed, in AddSubBlockData addSubBlockData)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                AddSubBlockInfoInterop addSubBlockInfoInterop = default(AddSubBlockInfoInterop);
                addSubBlockInfoInterop.coordinate = LibCziApiInterop.ToCoordinateInterop(addSubBlockInfoUncompressed.AddSubBlockInfo.Coordinate);
                if (addSubBlockInfoUncompressed.AddSubBlockInfo.MindexValid)
                {
                    addSubBlockInfoInterop.m_index_valid = 1;
                    addSubBlockInfoInterop.m_index = addSubBlockInfoUncompressed.AddSubBlockInfo.Mindex;
                }
                else
                {
                    addSubBlockInfoInterop.m_index_valid = 0;
                }

                addSubBlockInfoInterop.x = addSubBlockInfoUncompressed.AddSubBlockInfo.X;
                addSubBlockInfoInterop.y = addSubBlockInfoUncompressed.AddSubBlockInfo.Y;
                addSubBlockInfoInterop.logical_width = addSubBlockInfoUncompressed.AddSubBlockInfo.LogicalWidth;
                addSubBlockInfoInterop.logical_height = addSubBlockInfoUncompressed.AddSubBlockInfo.LogicalHeight;
                addSubBlockInfoInterop.physical_width = addSubBlockInfoUncompressed.AddSubBlockInfo.PhysicalWidth;
                addSubBlockInfoInterop.physical_height = addSubBlockInfoUncompressed.AddSubBlockInfo.PhysicalHeight;
                addSubBlockInfoInterop.pixel_type = (int)addSubBlockInfoUncompressed.AddSubBlockInfo.PixelType;
                addSubBlockInfoInterop.compression_mode_raw = 0;    // TOD(JBL): need to define compression-modes - somehow, somewhere

                int returnCode;
                fixed (byte* dataPtr = addSubBlockData.BitmapData)
                {
                    fixed (byte* metadataPtr = addSubBlockData.Metadata)
                    {
                        fixed (byte* attachmentPtr = addSubBlockData.Attachment)
                        {
                            addSubBlockInfoInterop.data = new IntPtr(dataPtr);
                            addSubBlockInfoInterop.size_data = (uint)addSubBlockData.BitmapData.Length;
                            addSubBlockInfoInterop.stride = addSubBlockInfoUncompressed.Stride;
                            addSubBlockInfoInterop.metadata = new IntPtr(metadataPtr);
                            addSubBlockInfoInterop.size_metadata = (uint)addSubBlockData.Metadata.Length;
                            addSubBlockInfoInterop.attachment = new IntPtr(attachmentPtr);
                            addSubBlockInfoInterop.size_attachment = (uint)addSubBlockData.Attachment.Length;
                            returnCode = this.writerAddSubBlock(writerHandle, &addSubBlockInfoInterop);
                        }
                    }
                }

                this.ThrowIfError(returnCode);
            }
        }

        /// <summary>
        /// Adds the specified compressed sub-block to the CZI-file.
        /// </summary>
        /// <param name="writerHandle">A handle to the writer that will add the sub-block into CZI-file.</param>
        /// <param name="addSubBlockInfo">Compressed sub-block information that is going to be added.</param>
        /// <param name="addSubBlockData">The sub-block data that is going to be added.</param>
        public void WriterAddSubBlockCompressed(IntPtr writerHandle, in AddSubBlockInfoCompressed addSubBlockInfo, in AddSubBlockData addSubBlockData)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                AddSubBlockInfoInterop addSubBlockInfoInterop = default(AddSubBlockInfoInterop);
                addSubBlockInfoInterop.coordinate = LibCziApiInterop.ToCoordinateInterop(addSubBlockInfo.AddSubBlockInfo.Coordinate);
                if (addSubBlockInfo.AddSubBlockInfo.MindexValid)
                {
                    addSubBlockInfoInterop.m_index_valid = 1;
                    addSubBlockInfoInterop.m_index = addSubBlockInfo.AddSubBlockInfo.Mindex;
                }
                else
                {
                    addSubBlockInfoInterop.m_index_valid = 0;
                }

                addSubBlockInfoInterop.x = addSubBlockInfo.AddSubBlockInfo.X;
                addSubBlockInfoInterop.y = addSubBlockInfo.AddSubBlockInfo.Y;
                addSubBlockInfoInterop.logical_width = addSubBlockInfo.AddSubBlockInfo.LogicalWidth;
                addSubBlockInfoInterop.logical_height = addSubBlockInfo.AddSubBlockInfo.LogicalHeight;
                addSubBlockInfoInterop.physical_width = addSubBlockInfo.AddSubBlockInfo.PhysicalWidth;
                addSubBlockInfoInterop.physical_height = addSubBlockInfo.AddSubBlockInfo.PhysicalHeight;
                addSubBlockInfoInterop.pixel_type = (int)addSubBlockInfo.AddSubBlockInfo.PixelType;
                addSubBlockInfoInterop.compression_mode_raw = addSubBlockInfo.CompressionMode;

                int returnCode;
                fixed (byte* dataPtr = addSubBlockData.BitmapData)
                {
                    fixed (byte* metadataPtr = addSubBlockData.Metadata)
                    {
                        fixed (byte* attachmentPtr = addSubBlockData.Attachment)
                        {
                            addSubBlockInfoInterop.data = new IntPtr(dataPtr);
                            addSubBlockInfoInterop.size_data = (uint)addSubBlockData.BitmapData.Length;
                            addSubBlockInfoInterop.stride = 0;
                            addSubBlockInfoInterop.metadata = new IntPtr(metadataPtr);
                            addSubBlockInfoInterop.size_metadata = (uint)addSubBlockData.Metadata.Length;
                            addSubBlockInfoInterop.attachment = new IntPtr(attachmentPtr);
                            addSubBlockInfoInterop.size_attachment = (uint)addSubBlockData.Attachment.Length;
                            returnCode = this.writerAddSubBlock(writerHandle, &addSubBlockInfoInterop);
                        }
                    }
                }

                this.ThrowIfError(returnCode);
            }
        }

        /// <summary>
        /// Adds the specified attachment to the CZI-file.
        /// </summary>
        /// <param name="writerHandle">A handle to the writer that will add the attachment into CZI-file.</param>
        /// <param name="addAttachmentInfo">Attachment information that is going to be added.</param>
        /// <param name="attachmentData">The attachment data that is going to be added.</param>
        public void WriterAddAttachment(IntPtr writerHandle, in AddAttachmentInfo addAttachmentInfo, Span<byte> attachmentData)
        {
            this.ThrowIfNotInitialized();
            AddAttachmentInfoInterop addAttachmentInfoInterop = default(AddAttachmentInfoInterop);
            unsafe
            {
                var guidBytes = addAttachmentInfo.Guid.ToByteArray();
                for (int i = 0; i < 16; i++)
                {
                    addAttachmentInfoInterop.guid[i] = guidBytes[i];
                }

                // Convert to ASCII bytes (non-ASCII characters will be replaced with '?')
                Encoding asciiEncoding = Encoding.ASCII;
                byte[] asciiBytes = asciiEncoding.GetBytes(addAttachmentInfo.ContentFileType ?? string.Empty);
                for (int i = 0; i < Math.Min(8, asciiBytes.Length); i++)
                {
                    addAttachmentInfoInterop.content_file_type[i] = asciiBytes[i];
                }

                asciiBytes = asciiEncoding.GetBytes(addAttachmentInfo.Name ?? string.Empty);
                for (int i = 0; i < Math.Min(80, asciiBytes.Length); i++)
                {
                    addAttachmentInfoInterop.name[i] = asciiBytes[i];
                }

                fixed (byte* attachmentDataPtr = attachmentData)
                {
                    addAttachmentInfoInterop.attachment_data = new IntPtr(attachmentDataPtr);
                    addAttachmentInfoInterop.size_attachment_data = (uint)attachmentData.Length;
                    int returnCode = this.writerAddAttachment(writerHandle, &addAttachmentInfoInterop);
                    this.ThrowIfError(returnCode);
                }
            }
        }

        /// <summary>
        /// Adds the specified metadata to the CZI-file.
        /// </summary>
        /// <param name="writerHandle">A handle to the writer that will write the metadata into CZI-file.</param>
        /// <param name="metadata">The metadata that is going to be added.</param>
        public void WriterWriteMetadata(IntPtr writerHandle, Span<byte> metadata)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                fixed (byte* metadataPtr = metadata)
                {
                    WriteMetadataInfoInterop writerWriteMetadataInfoInterop = default(WriteMetadataInfoInterop);
                    writerWriteMetadataInfoInterop.size_metadata = (uint)metadata.Length;
                    writerWriteMetadataInfoInterop.metadata = new IntPtr(metadataPtr);
                    int returnCode = this.writerWriteMetadata(writerHandle, &writerWriteMetadataInfoInterop);
                    this.ThrowIfError(returnCode);
                }
            }
        }

        /// <summary> Creates single-channel-tile-scaling-tile accessor.</summary>
        /// <param name="readerObject"> The reader object.</param>
        /// <returns> The new single-channel-tile-scaling-tile accessor.</returns>
        public IntPtr CreateSingleChannelTileAccessor(IntPtr readerObject)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntPtr accessorHandle = IntPtr.Zero;
                int returnCode = this.createSingleChannelTileAccessor(readerObject, &accessorHandle);
                this.ThrowIfError(returnCode);
                return accessorHandle;
            }
        }

        /// <summary> Determine the size of the bitmap which will be created for the specified parameters.</summary>
        /// <param name="accessorHandle"> Handle of the accessor.</param>
        /// <param name="roi">            The roi.</param>
        /// <param name="zoom">           The zoom.</param>
        /// <returns> The size of the bitmap that would be created for the specified ROI and zoom.</returns>
        public IntSize SingleChannelTileAccessorCalcSize(IntPtr accessorHandle, in IntRect roi, float zoom)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntRectInterop roiInterop = new IntRectInterop() { x = roi.X, y = roi.Y, w = roi.Width, h = roi.Height };
                IntSizeInterop sizeInterop = default;
                int returnCode = this.singleChannelTileAccessorCalcSize(accessorHandle, &roiInterop, zoom, &sizeInterop);
                this.ThrowIfError(returnCode);
                return new IntSize(sizeInterop.w, sizeInterop.h);
            }
        }

        /// <summary> Releases the single-channel-tile-accessor object.</summary>
        /// <param name="accessorHandle"> Handle of the accessor to be released.</param>
        public void ReleaseCreateSingleChannelTileAccessor(IntPtr accessorHandle)
        {
            this.ThrowIfNotInitialized();
            int returnCode = this.releaseCreateSingleChannelTileAccessor(accessorHandle);
            this.ThrowIfError(returnCode);
        }

        /// <summary> Get the tile-composite for the specified parameters.</summary>
        /// <param name="accessorHandle">  Handle of the accessor.</param>
        /// <param name="coordinate">      The coordinate.</param>
        /// <param name="roi">             The roi.</param>
        /// <param name="zoomFactor">      The zoom factor.</param>
        /// <param name="accessorOptions"> Options for controlling the operation.</param>
        /// <returns> Handle representing the resulting bitmap object.</returns>
        public IntPtr SingleChannelTileAccessorGet(IntPtr accessorHandle, ICoordinate coordinate, in IntRect roi, float zoomFactor, in AccessorOptions accessorOptions)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntPtr bitmapHandle = IntPtr.Zero;
                CoordinateInterop coordinateInterop = LibCziApiInterop.ToCoordinateInterop(coordinate);
                IntRectInterop roiInterop = new IntRectInterop() { x = roi.X, y = roi.Y, w = roi.Width, h = roi.Height };
                AccessorOptionsInterop accessorOptionsInterop = new AccessorOptionsInterop()
                {
                    back_ground_color_r = accessorOptions.BackGroundColorR,
                    back_ground_color_g = accessorOptions.BackGroundColorG,
                    back_ground_color_b = accessorOptions.BackGroundColorB,
                    sort_by_m = accessorOptions.SortByM,
                    use_visibility_check_optimization = accessorOptions.UseVisibilityCheckOptimization,
                    additional_parameters = IntPtr.Zero,
                };

                int returnCode = this.singleChannelTileAccessorGet(accessorHandle, &coordinateInterop, &roiInterop, zoomFactor, &accessorOptionsInterop, &bitmapHandle);
                this.ThrowIfError(returnCode);
                return bitmapHandle;
            }
        }

        /// <summary> Construct a multi-channel-composition-channel-info structure.</summary>
        /// <param name="displaySettingsHandle"> The display-settings object from which to derive the information.</param>
        /// <param name="channelIndex">          Zero-based index of the channel.</param>
        /// <param name="sixteenOrEightBitLut">  True to sixteen or eight bit LUT.</param>
        /// <returns> The multi-channel-composition-channel-info structure (for the multi-channel-composition).</returns>
        public MultiChannelCompositionChannelInfo CompositorFillOutCompositionChannelInfo(IntPtr displaySettingsHandle, int channelIndex, bool sixteenOrEightBitLut)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                CompositionChannelInfoInterop channelInfoInterop = default(CompositionChannelInfoInterop);
                try
                {
                    int returnCode = this.compositorFillOutCompositionChannelInfoInterop(
                        displaySettingsHandle,
                        channelIndex,
                        sixteenOrEightBitLut,
                        &channelInfoInterop);
                    this.ThrowIfError(returnCode);
                    byte[] lutData = null;
                    if (channelInfoInterop.look_up_table_element_count > 0 && channelInfoInterop.ptr_look_up_table != IntPtr.Zero)
                    {
                        lutData = new byte[channelInfoInterop.look_up_table_element_count];
                        Marshal.Copy(channelInfoInterop.ptr_look_up_table, lutData, 0, channelInfoInterop.look_up_table_element_count);
                    }

                    return new MultiChannelCompositionChannelInfo()
                    {
                        Weight = channelInfoInterop.weight,
                        BlackPoint = channelInfoInterop.black_point,
                        WhitePoint = channelInfoInterop.white_point,
                        EnableTinting = channelInfoInterop.enable_tinting,
                        TintingColor = channelInfoInterop.enable_tinting
                            ? new Rgb24Color(channelInfoInterop.tinting_color_r, channelInfoInterop.tinting_color_g, channelInfoInterop.tinting_color_b)
                            : Rgb24Color.Black,
                        LookupTable = lutData,
                    };
                }
                finally
                {
                    if (channelInfoInterop.look_up_table_element_count > 0 && channelInfoInterop.ptr_look_up_table != IntPtr.Zero)
                    {
                        this.libCziFree(channelInfoInterop.ptr_look_up_table);
                    }
                }
            }
        }

        /// <summary>
        /// Perform the multichannel composition with the specified bitmaps and the specified channel-information. A handle
        /// for a newly created bitmap containing the result is returned.
        /// At most 'channelCount' elements are taken from the 'channelInfo' collection.
        /// </summary>
        /// <param name="channelCount"> The number of channels, or the number of elements to take from 'channelInfo'.</param>
        /// <param name="channelInfo">  Information describing the channel.</param>
        /// <returns> Handle to a newly created bitmap object containing the result.</returns>
        public IntPtr CompositorComposeMultiChannelImage(int channelCount, IEnumerable<(IntPtr, MultiChannelCompositionChannelInfo)> channelInfo)
        {
            this.ThrowIfNotInitialized();
            if (channelCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(channelCount), "The number of channels must be greater than zero.");
            }

            if (channelInfo == null)
            {
                throw new ArgumentNullException(nameof(channelInfo));
            }

            IntPtr[] bitmapHandles = new IntPtr[channelCount];
            CompositionChannelInfoInterop[] channelInfos = new CompositionChannelInfoInterop[channelCount];
            List<MemoryHandle> pinnedLuts = new List<MemoryHandle>(channelCount);

            try
            {
                unsafe
                {
                    int actualNumberOfChannels = 0;
                    foreach (var item in channelInfo)
                    {
                        bitmapHandles[actualNumberOfChannels] = item.Item1;
                        channelInfos[actualNumberOfChannels] = new CompositionChannelInfoInterop()
                        {
                            weight = item.Item2.Weight,
                            enable_tinting = item.Item2.EnableTinting,
                            tinting_color_r = item.Item2.TintingColor.Red,
                            tinting_color_g = item.Item2.TintingColor.Green,
                            tinting_color_b = item.Item2.TintingColor.Blue,
                            black_point = item.Item2.BlackPoint,
                            white_point = item.Item2.WhitePoint,
                        };

                        // If there is a LUT, then we pin it here. We pin it manually, using Memory<T>.Pin(), put the MemoryHandle into a list
                        //  with we free at the end of the method (in the finally-clause).
                        if (!item.Item2.LookupTable.IsEmpty)
                        {
                            MemoryHandle lutMemoryHandle = item.Item2.LookupTable.Pin();
                            channelInfos[actualNumberOfChannels].ptr_look_up_table = new IntPtr(lutMemoryHandle.Pointer);
                            channelInfos[actualNumberOfChannels].look_up_table_element_count = item.Item2.LookupTable.Length;
                            pinnedLuts.Add(lutMemoryHandle);
                        }

                        ++actualNumberOfChannels;
                        if (actualNumberOfChannels >= channelCount)
                        {
                            break;
                        }
                    }

                    if (actualNumberOfChannels == 0)
                    {
                        throw new ArgumentException("The collection of channel-information must contain at least one element.", nameof(channelInfo));
                    }

                    fixed (IntPtr* bitmapHandlesPtr = bitmapHandles)
                    {
                        fixed (CompositionChannelInfoInterop* compositionChannelsInteropPtr = channelInfos)
                        {
                            IntPtr resultBitmapHandle = IntPtr.Zero;
                            int returnValue = this.compositorDoMultiChannelComposition(
                                actualNumberOfChannels,
                                bitmapHandlesPtr,
                                compositionChannelsInteropPtr,
                                &resultBitmapHandle);
                            this.ThrowIfError(returnValue);
                            return resultBitmapHandle;
                        }
                    }
                }
            }
            finally
            {
                foreach (MemoryHandle lutMemoryHandle in pinnedLuts)
                {
                    lutMemoryHandle.Dispose();
                }
            }
        }

        /// <summary>
        /// Releases the CZI-document-info object.
        /// </summary>
        /// <param name="cziDocumentHandle">The CZI-document-info object.</param>
        public void ReleaseCziDocumentInfo(IntPtr cziDocumentHandle)
        {
            this.ThrowIfNotInitialized();
            int returnCode = this.releaseCziDocumentInfo(cziDocumentHandle);
            this.ThrowIfError(returnCode);
        }

        /// <summary> Retrieve the scaling information.</summary>
        /// <param name="cziDocumentHandle"> The CZI-document-info object.</param>
        /// <returns> The scaling info.</returns>
        public ScalingInfo CziDocumentInfoGetScalingInfo(IntPtr cziDocumentHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                ScalingInfoInterop scalingInfoInterop = default(ScalingInfoInterop);
                int returnCode = this.cziDocumentInfoGetScalingInfo(cziDocumentHandle, &scalingInfoInterop);
                this.ThrowIfError(returnCode);
                return new ScalingInfo(scalingInfoInterop.scale_x, scalingInfoInterop.scale_y, scalingInfoInterop.scale_z);
            }
        }

        /// <summary> Get "general info", formatted  as JSON.</summary>
        /// <param name="cziDocumentInfoHandle"> The CZI-document-info object.</param>
        /// <returns> A JSON-formatted string containing the "general info".</returns>
        public string CziDocumentInfoGetGeneralDocumentInfoAsJson(IntPtr cziDocumentInfoHandle)
        {
            this.ThrowIfNotInitialized();
            IntPtr generalDocumentInfoAsJson = IntPtr.Zero;
            try
            {
                unsafe
                {
                    int returnCode = this.cziDocumentInfoGetGeneralDocumentInfo(cziDocumentInfoHandle, &generalDocumentInfoAsJson);
                    this.ThrowIfError(returnCode);
                    return NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminated(generalDocumentInfoAsJson);
                }
            }
            finally
            {
                this.libCziFree(generalDocumentInfoAsJson);
            }
        }

        /// <summary> Get the set of dimensions for which there is dimension-info available.</summary>
        /// <param name="cziDocumentInfoHandle"> The CZI-document-info object.</param>
        /// <returns> An array containing the dimensions for which there is "dimension-info" is available.</returns>
        public DimensionIndex[] CziDocumentInfoGetAvailableDimensions(IntPtr cziDocumentInfoHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                // Allocate an array on the stack with DimensionIndex.MaxDimensionIndex + 1 elements.
                // (Note that the array we allocate here is large enough to hold all possible cases, so
                //  we do not deal with the case of the buffer being too small)
                int count = (int)DimensionIndex.MaxDimensionIndex + 1;
                uint* dimensionIndexArray = stackalloc uint[count];
                int returnCode = this.cziDocumentInfoGetAvailableDimension(cziDocumentInfoHandle, (uint)count, dimensionIndexArray);
                this.ThrowIfError(returnCode);

                int countOfDimensions = 0;
                for (; countOfDimensions < count; countOfDimensions++)
                {
                    if (dimensionIndexArray[countOfDimensions] == (uint)DimensionIndex.Invalid)
                    {
                        break;
                    }
                }

                if (countOfDimensions == 0)
                {
                    return Array.Empty<DimensionIndex>();
                }

                // Allocate an array on the heap and copy the elements from the stack-allocated array
                DimensionIndex[] dimensionIndices = new DimensionIndex[countOfDimensions];
                for (int i = 0; i < countOfDimensions; i++)
                {
                    dimensionIndices[i] = (DimensionIndex)dimensionIndexArray[i];
                }

                return dimensionIndices;
            }
        }

        /// <summary> Create a display-settings-object (and return a handle for it).</summary>
        /// <param name="cziDocumentHandle"> The CZI-document-info object.</param>
        /// <returns> The handle of the newly created display-settings object.</returns>
        public IntPtr CziDocumentInfoGetDisplaySettings(IntPtr cziDocumentHandle)
        {
            this.ThrowIfNotInitialized();
            unsafe
            {
                IntPtr displaySettingsHandle;
                int returnCode = this.cziDocumentInfoGetDisplaySettings(cziDocumentHandle, &displaySettingsHandle);
                this.ThrowIfError(returnCode);
                return displaySettingsHandle;
            }
        }

        /// <summary>
        /// Get the dimension-info information for the specified dimension. This gives a JSON-formatted
        /// text with the dimension-information.
        /// </summary>
        /// <param name="cziDocumentHandle"> The CZI-document-info object.</param>
        /// <param name="dimensionIndex">    The dimension for which to retrieve the dimension-info information.</param>
        ///
        /// <returns> JSON-formatted text, containing the requested information.</returns>
        public string CziDocumentInfoGetDimensionInfo(IntPtr cziDocumentHandle, DimensionIndex dimensionIndex)
        {
            this.ThrowIfNotInitialized();
            IntPtr dimensionInfoAsJson = IntPtr.Zero;
            try
            {
                unsafe
                {
                    int returnCode = this.cziDocumentInfoGetDimensionInfo(cziDocumentHandle, (uint)dimensionIndex, &dimensionInfoAsJson);
                    this.ThrowIfError(returnCode);
                    return NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminated(dimensionInfoAsJson);
                }
            }
            finally
            {
                this.libCziFree(dimensionInfoAsJson);
            }
        }

        /// <summary>
        /// Releases the display-settings object.
        /// </summary>
        /// <param name="displaySettingsHandle">The display-settings object.</param>
        public void ReleaseDisplaySettings(IntPtr displaySettingsHandle)
        {
            this.ThrowIfNotInitialized();
            int returnCode = this.releaseDisplaySettings(displaySettingsHandle);
            this.ThrowIfError(returnCode);
        }
    }

    /// <content>
    /// Provides interop methods for interacting with the CZI native.
    /// This class contains delegates and methods necessary for managing
    /// input streams and freeing resources associated with the CZI API.
    /// </content>
    internal partial class LibCziApiInterop
    {
        /// <summary>
        /// Delegate to the (static) BlobOutputSetSizeFunction-forwarder-function. It is important that this delegate does NOT get
        /// GCed (which is ensured in case of a static variable of course).
        /// </summary>
        private static readonly unsafe InputStreamReadFunctionDelegate InputStreamReadFunctionDelegateObject = LibCziApiInterop.ReadFunctionCallback;
        private static readonly unsafe OutputStreamWriteFunctionDelegate OutputStreamWriteFunctionDelegateObject = LibCziApiInterop.WriteFunctionCallback;

        private static readonly InputStreamCloseFunctionDelegate InputStreamCloseFunctionDelegateObject = LibCziApiInterop.CloseInputStreamFunctionCallback;
        private static readonly OutputStreamCloseFunctionDelegate OutputStreamCloseFunctionDelegateObject = LibCziApiInterop.CloseOutputStreamFunctionCallback;

        private readonly LibCziFreeDelegate libCziFree;
        private readonly LibCziAllocateMemoryDelegate libCziAllocateMemory;
        private readonly GetLibCziVersionInfoDelegate getLibCziVersionInfo;
        private readonly GetLibCziBuildInformationDelegate getLibCziBuildInformation;
        private readonly CreateReaderDelegate createReader;
        private readonly ReaderOpenDelegate readerOpen;
        private readonly ReaderGetFileHeaderInfoDelegate readerGetFileHeaderInfo;
        private readonly ReaderGetStatisticsSimpleDelegate readerGetStatisticsSimple;
        private readonly ReaderGetStatisticsExDelegate readerGetStatisticsEx;
        private readonly ReaderGetPyramidStatisticsDelegate readerGetPyramidStatistics;
        private readonly ReaderReadSubBlockDelegate readerReadSubBlock;
        private readonly ReaderTryGetSubBlockInfoForIndexDelegate readerTryGetSubBlockInfoForIndex;
        private readonly ReaderGetAttachmentCountDelegate readerGetAttachmentCount;
        private readonly ReaderGetAttachmentInfoFromDirectoryDelegate readerGetAttachmentFromDirectoryInfo;
        private readonly ReaderReaderReadAttachmentDelegate readerReadAttachment;
        private readonly ReaderReleaseDelegate readerRelease;
        private readonly CreateInputStreamFromFileWideDelegate createInputStreamFromFileWide;
        private readonly CreateInputStreamFromFileUtf8Delegate createInputStreamFromFileUtf8;
        private readonly CreateInputStreamDelegate createInputStream;
        private readonly CreateInputStreamFromExternalDelegate createInputStreamFromExternal;
        private readonly ReleaseInputStreamDelegate releaseInputStream;
        private readonly SubBlockCreateBitmapDelegate subBlockCreateBitmap;
        private readonly SubBlockGetInfoDelegate subBlockGetInfo;
        private readonly SubBlockGetRawDataDelegate subBlockGetRawData;
        private readonly ReleaseSubBlockDelegate releaseSubBlock;
        private readonly BitmapGetInfoDelegate bitmapGetInfo;
        private readonly BitmapLockDelegate bitmapLock;
        private readonly BitmapUnlockDelegate bitmapUnlock;
        private readonly BitmapCopyToDelegate bitmapCopyTo;
        private readonly ReleaseBitmapDelegate releaseBitmap;
        private readonly GetStreamClassesCountDelegate getStreamClassesCount;
        private readonly GetStreamClassInfoDelegate getStreamClassInfo;

        private readonly ReaderGetMetadataSegmentDelegate readerGetMetadataSegment;
        private readonly MetadataSegmentGetMetadataAsXmlDelegate metadataSegmentGetMetadataAsXml;
        private readonly MetadataSegmentGetCziDocumentInfoDelegate metadataSegmentGetCziDocumentInfo;
        private readonly MetadataSegmentReleaseDelegate metadataSegmentRelease;

        private readonly AttachmentGetInfoDelegate attachmentGetInfo;
        private readonly AttachmentGetRawDataDelegate attachmentGetRawData;
        private readonly ReleaseAttachmentDelegate releaseAttachment;

        private readonly CreateOutputStreamForFileWideDelegate createOutputStreamForFileWide;
        private readonly CreateOutputStreamForFileUtf8Delegate createOutputStreamForFileUtf8;
        private readonly CreateOutputStreamForExternalDelegate createOutputStreamForExternal;
        private readonly ReleaseOutputStreamDelegate releaseOutputStream;

        private readonly CreateWriterDelegate createWriter;
        private readonly WriterCreateDelegate writerCreate;
        private readonly WriterCloseDelegate writerClose;
        private readonly WriterReleaseDelegate releaseWriter;
        private readonly WriterAddSubBlockDelegate writerAddSubBlock;
        private readonly WriterAddAttachmentDelegate writerAddAttachment;
        private readonly WriterWriteMetadataDelegate writerWriteMetadata;

        private readonly CreateSingleChannelTileAccessorDelegate createSingleChannelTileAccessor;
        private readonly SingleChannelTileAccessorCalcSizeDelegate singleChannelTileAccessorCalcSize;
        private readonly SingleChannelTileAccessorGetDelegate singleChannelTileAccessorGet;
        private readonly ReleaseCreateSingleChannelTileAccessorDelegate releaseCreateSingleChannelTileAccessor;

        // --- Compositor functions ---
        private readonly CompositorFillOutCompositionChannelInfoInteropDelegate compositorFillOutCompositionChannelInfoInterop;
        private readonly CompositorDoMultiChannelCompositionDelegate compositorDoMultiChannelComposition;

        // --- CziDocumentInfo functions ---
        private readonly CziDocumentInfoGetGeneralDocumentInfoDelegate cziDocumentInfoGetGeneralDocumentInfo;
        private readonly CziDocumentInfoGetScalingInfoDelegate cziDocumentInfoGetScalingInfo;
        private readonly CziDocumentInfoGetAvailableDimensionDelegate cziDocumentInfoGetAvailableDimension;
        private readonly CziDocumentInfoGetDisplaySettingsDelegate cziDocumentInfoGetDisplaySettings;
        private readonly CziDocumentInfoGetDimensionInfoDelegate cziDocumentInfoGetDimensionInfo;
        private readonly ReleaseCziDocumentInfoDelegate releaseCziDocumentInfo;

        // --- DisplaySettings functions ---
        /*private readonly DisplaySettingsGetChannelDisplaySettingsDelegate displaySettingsGetChannelDisplaySettings;*/
        private readonly ReleaseDisplaySettingsDelegate releaseDisplaySettings;

        /// <summary> This is a native function pointer to the Input-Stream-Read-Function.</summary>
        private readonly IntPtr functionPointerInputStreamReadFunction;

        /// <summary> This is a native function pointer to the Output-Stream-Write-Function.</summary>
        private readonly IntPtr functionPointerOutputStreamWriteFunction;

        /// <summary> This is a native function pointer to the Input-Stream-Close-Function.</summary>
        private readonly IntPtr functionPointerInputStreamCloseFunction;

        /// <summary> This is a native function pointer to the Output-Stream-Close-Function.</summary>
        private readonly IntPtr functionPointerOutputStreamCloseFunction;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void LibCziFreeDelegate(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int LibCziAllocateMemoryDelegate(ulong size, IntPtr* ptr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int GetLibCziVersionInfoDelegate(LibCziVersionInfoInterop* libCziVersionInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int GetLibCziBuildInformationDelegate(LibCziBuildInformationInterop* buildInformationInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CreateReaderDelegate(IntPtr* readerObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderOpenDelegate(IntPtr readerObjectHandle, ReaderOpenInfoInterop* readerOpenInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderGetFileHeaderInfoDelegate(IntPtr readerObjectHandle, FileHeaderInfoInterop* fileHeaderInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderGetStatisticsSimpleDelegate(IntPtr readerObjectHandle, SubBlockStatisticsInterop* statistics);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderGetStatisticsExDelegate(IntPtr readerObjectHandle, SubBlockStatisticsInteropEx* statistics, int* numberOfScenes);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderGetPyramidStatisticsDelegate(IntPtr readerObjectHandle, IntPtr* pyramidStatisticsAsJson);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderReadSubBlockDelegate(IntPtr readerObjectHandle, int index, IntPtr* subBlockObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderTryGetSubBlockInfoForIndexDelegate(IntPtr readerObjectHandle, int index, SubBlockInfoInterop* subBlocksInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CreateInputStreamFromFileWideDelegate(IntPtr filenameWide, IntPtr* streamObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CreateInputStreamFromFileUtf8Delegate(IntPtr filenameUtf8, IntPtr* streamObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CreateInputStreamFromExternalDelegate(ExternalInputStreamStructInterop* externalInputStreamStructInterop, IntPtr* streamObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ReaderReleaseDelegate(IntPtr readerObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ReleaseInputStreamDelegate(IntPtr inputStreamObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int InputStreamReadFunctionDelegate(IntPtr opaqueHandle1, IntPtr opaqueHandle2, long offset, IntPtr data, long size, long* bytesRead, ExternalStreamErrorInfoInterop* errorInfo);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int OutputStreamWriteFunctionDelegate(IntPtr opaqueHandle1, IntPtr opaqueHandle2, long offset, IntPtr data, long size, long* bytesWritten, ExternalStreamErrorInfoInterop* errorInfo);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void InputStreamCloseFunctionDelegate(IntPtr opaqueHandle1, IntPtr opaqueHandle2);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void OutputStreamCloseFunctionDelegate(IntPtr opaqueHandle1, IntPtr opaqueHandle2);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int SubBlockCreateBitmapDelegate(IntPtr subBlockObjectHandle, IntPtr* bitmapObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int SubBlockGetInfoDelegate(IntPtr subBlockObjectHandle, SubBlockInfoInterop* subBlockInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int SubBlockGetRawDataDelegate(IntPtr subBlockObjectHandle, int type, ulong* size, void* data);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ReleaseSubBlockDelegate(IntPtr subBlockObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int BitmapGetInfoDelegate(IntPtr bitmapObjectHandle, BitmapInfoInterop* bitmapInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int BitmapLockDelegate(IntPtr bitmapObjectHandle, BitmapLockInfoInterop* bitmapLockInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int BitmapUnlockDelegate(IntPtr bitmapObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int BitmapCopyToDelegate(IntPtr bitmapObjectHandle, uint width, uint height, int pixelType, uint stride, IntPtr data);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ReleaseBitmapDelegate(IntPtr bitmapObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int GetStreamClassesCountDelegate(int* count);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int GetStreamClassInfoDelegate(int index, InputStreamClassInfoInterop* inputStreamClassInfo);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CreateInputStreamDelegate(IntPtr streamClassName, IntPtr creationPropertyBag, IntPtr streamIdentified, IntPtr* streamObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderGetMetadataSegmentDelegate(IntPtr readerObjectHandle, IntPtr* metadataSegmentObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int MetadataSegmentGetMetadataAsXmlDelegate(IntPtr metadataSegmentObjectHandle, MetadataAsXmlInterop* metadataAsXmlInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int MetadataSegmentGetCziDocumentInfoDelegate(IntPtr metadataSegmentObjectHandle, IntPtr* cziDocumentInfoHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int MetadataSegmentReleaseDelegate(IntPtr metadataSegmentObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderGetAttachmentCountDelegate(IntPtr readerObjectHandle, int* count);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderGetAttachmentInfoFromDirectoryDelegate(IntPtr readerObjectHandle, int index, AttachmentInfoInterop* attachmentInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReaderReaderReadAttachmentDelegate(IntPtr readerObjectHandle, int index, IntPtr* attachmentObject);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int AttachmentGetInfoDelegate(IntPtr attachmentObjectHandle, AttachmentInfoInterop* attachmentInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int AttachmentGetRawDataDelegate(IntPtr attachmentObjectHandle, ulong* size, void* data);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ReleaseAttachmentDelegate(IntPtr attachmentObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CreateOutputStreamForFileWideDelegate(IntPtr filenameWide, bool overwrite, IntPtr* outputStreamObject);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CreateOutputStreamForFileUtf8Delegate(IntPtr filenameUtf8, bool overwrite, IntPtr* outputStreamObject);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CreateOutputStreamForExternalDelegate(ExternalOutputStreamStructInterop* externalOutputStreamStructInterop, IntPtr* outputStreamObject);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ReleaseOutputStreamDelegate(IntPtr outputStreamObject);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CreateWriterDelegate(IntPtr* writerObjectHandle, IntPtr options);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int WriterCreateDelegate(IntPtr writerObjectHandle, IntPtr outputStreamObject, IntPtr parameters);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int WriterCloseDelegate(IntPtr writerObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int WriterReleaseDelegate(IntPtr writerObjectHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int WriterAddSubBlockDelegate(IntPtr writerObjectHandle, AddSubBlockInfoInterop* addSubBlockInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int WriterAddAttachmentDelegate(IntPtr writerObjectHandle, AddAttachmentInfoInterop* addAttachmentInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int WriterWriteMetadataDelegate(IntPtr writerObjectHandle, WriteMetadataInfoInterop* writeMetadataInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CreateSingleChannelTileAccessorDelegate(IntPtr readerObject, IntPtr* singleChannelScalingTileAccessor);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int SingleChannelTileAccessorCalcSizeDelegate(IntPtr singleChannelScalingTileAccessor, IntRectInterop* roi, float zoom, IntSizeInterop* size);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int SingleChannelTileAccessorGetDelegate(IntPtr singleChannelScalingTileAccessor, CoordinateInterop* coordinate, IntRectInterop* roi, float zoom, AccessorOptionsInterop* options, IntPtr* bitmapObject);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ReleaseCreateSingleChannelTileAccessorDelegate(IntPtr singleChannelScalingTileAccessor);

        // --- Compositor functions ---
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CompositorFillOutCompositionChannelInfoInteropDelegate(IntPtr displaySettingsObject, int channelIndex, bool sixteenOrEightBitsLut, CompositionChannelInfoInterop* channelInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CompositorDoMultiChannelCompositionDelegate(int channelCount, IntPtr* sourceBitmaps, CompositionChannelInfoInterop* channelInfoInterop, IntPtr* bitmapObject);

        // --- CziDocumentInfo functions ---
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CziDocumentInfoGetScalingInfoDelegate(IntPtr cziDocumentInfoHandle, ScalingInfoInterop* scalingInfoInterop);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ReleaseCziDocumentInfoDelegate(IntPtr cziDocumentInfoHandle);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CziDocumentInfoGetGeneralDocumentInfoDelegate(IntPtr cziDocumentInfoHandle, IntPtr* jsonText);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CziDocumentInfoGetAvailableDimensionDelegate(IntPtr cziDocumentInfoHandle, uint availableDimensionsCount, uint* availableDimensions);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CziDocumentInfoGetDisplaySettingsDelegate(IntPtr cziDocumentInfoHandle, IntPtr* displaySettingsObject);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int CziDocumentInfoGetDimensionInfoDelegate(IntPtr cziDocumentInfoHandle, uint dimensionIndex, IntPtr* jsonText);

        // --- DisplaySettings functions ---
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ReleaseDisplaySettingsDelegate(IntPtr displaySettingsObject);
    }

    /// <content>
    /// This part contains definitions of the structures used to interact with the native CZI.
    /// </content>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Interop-struct.")]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore", Justification = "Interop-struct.")]
    internal partial class LibCziApiInterop
    {
        /// <summary>The number of "valid dimensions" (as defined in the enum 'DimensionIndex').</summary>
        private const int MaxDimensionCount = (int)DimensionIndex.MaxDimensionIndex;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct LibCziVersionInfoInterop
        {
            public int major;
            public int minor;
            public int patch;
            public int tweak;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct LibCziBuildInformationInterop
        {
            /// <summary>
            /// If non-null, the compiler identification. This is a free-form string.
            /// It is a zero-terminated string in UTF-8 encoding.
            /// This string must be freed by the caller (using libCZI_Free).
            /// </summary>
            public IntPtr compilerIdentification;

            /// <summary>
            /// If non-null, the URL of the repository.
            /// It is a zero-terminated string in UTF-8 encoding.
            /// This string must be freed by the caller (using libCZI_Free).
            /// </summary>
            public IntPtr repositoryUrl;

            public IntPtr repositoryBranch;
            public IntPtr repositoryTag;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct ReaderOpenInfoInterop
        {
            public IntPtr streamObject;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct ExternalInputStreamStructInterop
        {
            /// <summary> An opaque user parameter which is passed to the callback function.</summary>
            public IntPtr opaque_handle1;

            /// <summary> An opaque user parameter which is passed to the callback function.</summary>
            public IntPtr opaque_handle2;

            /// <summary>Function pointer used to read data from the stream.</summary>
            public IntPtr read_function_pointer;

            /// <summary> Function pointer used to close the stream.</summary>
            public IntPtr close_function_pointer;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct ExternalOutputStreamStructInterop
        {
            /// <summary> An opaque user parameter which is passed to the callback function.</summary>
            public IntPtr opaque_handle1;

            /// <summary> An opaque user parameter which is passed to the callback function.</summary>
            public IntPtr opaque_handle2;

            /// <summary>Function pointer used to write data into the stream.</summary>
            public IntPtr write_function_pointer;

            /// <summary> Function pointer used to close the stream.</summary>
            public IntPtr close_function_pointer;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct ExternalStreamErrorInfoInterop
        {
            /// <summary> The error code - possible value are the constants kStreamErrorCode_XXX.</summary>
            public int error_code;

            /// <summary> The error message (zero-terminated UTF8-encoded string). This string must be
            ///           allocated with 'libCZI_AllocateMemory'.</summary>
            public IntPtr error_message;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct IntRectInterop
        {
            public int x;
            public int y;
            public int w;
            public int h;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct IntSizeInterop
        {
            public int w;
            public int h;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private unsafe struct DimBoundsInterop
        {
            public uint dimensionsValid;
            public fixed int start[MaxDimensionCount];
            public fixed int size[MaxDimensionCount];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct SubBlockStatisticsInterop
        {
            public int subBlockCount;
            public int minimum_m_index;
            public int maximum_m_index;
            public IntRectInterop boundingBox;
            public IntRectInterop boundingBoxLayer0;
            public DimBoundsInterop dimBounds;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct BoundingBoxesInterop
        {
            public int sceneIndex;
            public IntRectInterop bounding_box;
            public IntRectInterop bounding_box_layer0_only;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct SubBlockStatisticsInteropEx
        {
            public int subBlockCount;
            public int minimum_m_index;
            public int maximum_m_index;
            public IntRectInterop boundingBox;
            public IntRectInterop boundingBoxLayer0;
            public DimBoundsInterop dimBounds;
            public int numberOfScenes;
            public BoundingBoxesInterop perScenesBoundingBoxes0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct BitmapInfoInterop
        {
            public uint width;
            public uint height;
            public int pixelType;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct BitmapLockInfoInterop
        {
            public IntPtr data;
            public IntPtr dataRoi;
            public uint stride;
            public ulong size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct InputStreamClassInfoInterop
        {
            /// <summary> The name of the input stream class. This is a free-form string. This string must be freed by the caller (using libCZI_Free).</summary>
            public IntPtr name;

            /// <summary> The description of the input stream class. This is a free-form string. This string must be freed by the caller (using libCZI_Free).</summary>
            public IntPtr description;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct MetadataAsXmlInterop
        {
            public IntPtr xml;
            public ulong size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private unsafe struct CoordinateInterop
        {
            public uint dimensionsValid;
            public fixed int value[MaxDimensionCount];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct SubBlockInfoInterop
        {
            public int compression_mode_raw;
            public int pixel_type;
            public CoordinateInterop coordinate;
            public IntRectInterop logical_rect;
            public IntSizeInterop physical_size;

            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:VariableNamesMustNotBePrefixed", Justification = "Interop structure.")]
            public int m_index;
        }

        /// <summary>
        /// This structure contains the information about an attachment.
        /// Note that performance reasons we use a fixed-size array for the name. In the rare case that the name is too long to fit into the
        /// fixed-size array, the 'overflow' field is set to true. In this case, the name is truncated and the 'overflow' field is set to true,
        /// and the full name is stored in the 'name_in_case_of_overflow' field. This memory must be freed using 'libCZI_Free'.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private unsafe struct AttachmentInfoInterop
        {
            /// <summary> The GUID of the attachment.</summary>
            public fixed byte guid[16];

            /// <summary> A null-terminated character array identifying the content of the attachment.</summary>
            public fixed byte content_file_type[9];

            /// <summary> A zero-terminated string (in UTF8-encoding) identifying the content of the attachment.</summary>
            public fixed byte name[255];

            /// <summary>
            /// True if the name is too long to fit into the 'name' field.
            /// </summary>
            public bool name_overflow;

            /// <summary>
            /// If 'name_overflow' is true, then this field contains the name (in UTF8-encoding and zero terminated) of the attachment.
            /// This memory must be freed using 'libCZI_Free'.
            /// </summary>
            public IntPtr name_in_case_of_overflow;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private unsafe struct FileHeaderInfoInterop
        {
            /// <summary> The GUID of the file-header.</summary>
            public fixed byte guid[16];

            /// <summary> The major version level.</summary>
            public int major_version;

            /// <summary> The minor version level.</summary>
            public int minor_version;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct AddSubBlockInfoInterop
        {
            public CoordinateInterop coordinate;
            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:VariableNamesMustNotBePrefixed", Justification = "Interop structure.")]
            public byte m_index_valid;
            [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:VariableNamesMustNotBePrefixed", Justification = "Interop structure.")]
            public int m_index;
            public int x;
            public int y;
            public int logical_width;
            public int logical_height;
            public int physical_width;
            public int physical_height;
            public int pixel_type;
            public int compression_mode_raw;
            public uint size_data;
            public IntPtr data;

            /// <summary>
            /// If the compression mode is set to 'Uncompressed', then this is valid and the stride of the bitmap.
            /// In this case, the line-size of the bitmap is determined by the pixel-type and the physical_width.
            /// And size_data must be large enough to hold the bitmap-data, and is validated.
            /// In other cases (compression-mode is not 'Uncompressed'), this field is ignored.
            /// </summary>
            public uint stride;

            public uint size_metadata;
            public IntPtr metadata;

            public uint size_attachment;
            public IntPtr attachment;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private unsafe struct AddAttachmentInfoInterop
        {
            public fixed byte guid[16];
            public fixed byte content_file_type[8];
            public fixed byte name[80];
            public uint size_attachment_data;
            public IntPtr attachment_data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct WriteMetadataInfoInterop
        {
            public uint size_metadata;
            public IntPtr metadata;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct AccessorOptionsInterop
        {
            public float back_ground_color_r;
            public float back_ground_color_g;
            public float back_ground_color_b;

            public bool sort_by_m;
            public bool use_visibility_check_optimization;

            public IntPtr additional_parameters;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct CompositionChannelInfoInterop
        {
            /// <summary> The weight of the channel in the composition.</summary>
            public float weight;

            /// <summary> True if tinting is enabled for this channel (in which case the tinting member is to
            ///           be examined), false if no tinting is to be applied (the tinting member is then
            ///           not used).</summary>
            public bool enable_tinting;

            /// <summary> The red component of the tinting color (only examined if enableTinting is true).</summary>
            public byte tinting_color_r;

            /// <summary> The green component of the tinting color (only examined if enableTinting is true).</summary>
            public byte tinting_color_g;

            /// <summary> The blue component of the tinting color (only examined if enableTinting is true).</summary>
            public byte tinting_color_b;

            /// <summary> The black point - it is a float between 0 and 1, where 0 corresponds to the lowest
            ///           pixel value (of the pixel type for the channel) and 1 to the highest pixel value
            ///           (of the pixel type of this channel). All pixel values below the black point are
            ///           mapped to 0.</summary>
            public float black_point;

            /// <summary> The white point - it is a float between 0 and 1, where 0 corresponds to the lowest
            ///           pixel value (of the pixel type for the channel) and 1 to the highest pixel value
            ///           (of the pixel type of this channel). All pixel value above the white pointer are
            ///           mapped to the highest pixel value.</summary>
            public float white_point;

            /// <summary> Number of elements in the look-up table. If 0, then the look-up table is not used. If
            ///           this channelInfo applies to a Gray8/Bgr24-channel, then the size of the look-up
            ///           table must be 256. In case of a Gray16/Bgr48-channel, the size must be
            ///           65536.
            ///           \remark If a look-up table is provided, then <c>blackPoint</c> and <c>whitePoint</c>
            ///           are not used anymore .</summary>
            public int look_up_table_element_count;

            /// <summary> (Immutable) The pointer to the look-up table. If look_up_table_element_count is &lt;
            ///           &gt; 0, then this pointer must be valid.</summary>
            public IntPtr ptr_look_up_table;
        }

        /// <summary>
        /// This structure contains the scaling information of a CZI document.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct ScalingInfoInterop
        {
            /// <summary>
            /// The length of a pixel in x-direction in the unit meters. If unknown/invalid, this value is double.NaN.
            /// </summary>
            public double scale_x;

            /// <summary>
            /// The length of a pixel in y-direction in the unit meters. If unknown/invalid, this value is double.NaN.
            /// </summary>
            public double scale_y;

            /// <summary>
            /// The length of a pixel in z-direction in the unit meters. If unknown/invalid, this value is double.NaN.
            /// </summary>
            public double scale_z;
        }
    }

    /// <content>
    /// Here we define the error codes used by the libCZIAPI.
    /// </content>
    internal partial class LibCziApiInterop
    {
        /// <summary>The operation completed successfully.</summary>
        private const int LibCziApiSuccess = 0;

        /// <summary>An invalid argument was supplied to the function.</summary>
        private const int LibCziApiInvalidArgument = 1;

        /// <summary>An invalid handle was supplied to the function (i.e. a handle which is either a bogus value or a handle which has already been destroyed).</summary>
        private const int LibCziApiInvalidHandle = 2;

        /// <summary>The operation failed due to an out-of-memory condition.</summary>
        private const int LibCziApiOutOfMemory = 3;

        /// <summary>A specified index is out of range.</summary>
        private const int LibCziIndexOutOfRange = 4;

        /// <summary>An unspecified error occurred.</summary>
        private const int LibCziApiUnspecifiedError = 50;
    }

    /// <content>
    /// This part contains private utilities.
    /// </content>
    internal partial class LibCziApiInterop
    {
        private static DimensionBounds DimBoundsInteropToDimensionBounds(in DimBoundsInterop dimBoundsInterop)
        {
            List<DimensionAndRange> bounds = new List<DimensionAndRange>(MaxDimensionCount);
            unsafe
            {
                int dimBoundsIndex = 0;
                for (int i = 0; i < MaxDimensionCount; i++)
                {
                    if ((dimBoundsInterop.dimensionsValid & (1 << i)) != 0)
                    {
                        bounds.Add(new DimensionAndRange(
                            (DimensionIndex)(1 + i),
                            dimBoundsInterop.start[dimBoundsIndex],
                            dimBoundsInterop.size[dimBoundsIndex]));
                        ++dimBoundsIndex;
                    }
                }
            }

            return new DimensionBounds(bounds);
        }

        private static CoordinateInterop ToCoordinateInterop(ICoordinate coordinate)
        {
            CoordinateInterop coordinateInterop = default(CoordinateInterop);
            int currentIndex = 0;
            unsafe
            {
                for (int i = 0; i < MaxDimensionCount; i++)
                {
                    if (coordinate.TryGetCoordinate((DimensionIndex)(1 + i), out int value))
                    {
                        coordinateInterop.dimensionsValid |= 1u << i;
                        coordinateInterop.value[currentIndex] = value;
                        ++currentIndex;
                    }
                }
            }

            return coordinateInterop;
        }

        private static Coordinate CoordinateInteropToCoordinate(in CoordinateInterop coordinateInterop)
        {
            List<DimensionAndValue> values = new List<DimensionAndValue>(MaxDimensionCount);
            unsafe
            {
                int valueIndex = 0;
                for (int i = 0; i < MaxDimensionCount; i++)
                {
                    if ((coordinateInterop.dimensionsValid & (1 << i)) != 0)
                    {
                        values.Add(new DimensionAndValue((DimensionIndex)(1 + i), coordinateInterop.value[valueIndex]));
                        ++valueIndex;
                    }
                }
            }

            return new Coordinate(values);
        }

        private static AttachmentInfo ToAttachmentInfo(in AttachmentInfoInterop attachmentInfoInterop)
        {
            Guid guid;
            string contentFileType;
            string name;
            unsafe
            {
                fixed (byte* guidPtr = attachmentInfoInterop.guid)
                {
                    guid = new Guid(new ReadOnlySpan<byte>(guidPtr, 16).ToArray());
                }

                fixed (byte* contentFileTypePtr = attachmentInfoInterop.content_file_type)
                {
                    // TODO(JBL): get rid of those magic values here
                    contentFileType = NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminatedWithMaxLength(new IntPtr(contentFileTypePtr), 9);
                }

                if (!attachmentInfoInterop.name_overflow)
                {
                    // if there was no overflow, we can use the fixed size string
                    fixed (byte* namePtr = attachmentInfoInterop.name)
                    {
                        name = NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminatedWithMaxLength(new IntPtr(namePtr), 255);
                    }
                }
                else
                {
                    // otherwise we have to use the overflow field
                    name = NativeLibraryUtils.ConvertFromUtf8IntPtrZeroTerminated(attachmentInfoInterop.name_in_case_of_overflow);
                }
            }

            return new AttachmentInfo(guid, contentFileType, name);
        }

        private static SubBlockInfo ToSubBlockInfo(in SubBlockInfoInterop subBlockInfoInterop)
        {
            int compressionModeRaw;
            PixelType pixelType;
            Coordinate coordinate;
            IntRect logicalRect;
            IntSize physicalSize;
            int mIndex;
            unsafe
            {
                compressionModeRaw = subBlockInfoInterop.compression_mode_raw;
                pixelType = (PixelType)subBlockInfoInterop.pixel_type;
                coordinate = CoordinateInteropToCoordinate(in subBlockInfoInterop.coordinate);
                logicalRect = new IntRect(subBlockInfoInterop.logical_rect.x, subBlockInfoInterop.logical_rect.y, subBlockInfoInterop.logical_rect.w, subBlockInfoInterop.logical_rect.h);
                physicalSize = new IntSize(subBlockInfoInterop.physical_size.w, subBlockInfoInterop.physical_size.h);
                mIndex = subBlockInfoInterop.m_index;
            }

            return new SubBlockInfo(compressionModeRaw, pixelType, coordinate, logicalRect, physicalSize, mIndex);
        }

        private static FileHeaderInfo ToFileHeaderInfo(in FileHeaderInfoInterop fileHeaderInfoInterop)
        {
            const int byteLengthOfGuid = 16;
            Guid guid;
            int majorVersion;
            int minorVersion;
            unsafe
            {
                fixed (byte* guidPtr = fileHeaderInfoInterop.guid)
                {
                    guid = new Guid(new ReadOnlySpan<byte>(guidPtr, byteLengthOfGuid).ToArray());
                }

                majorVersion = fileHeaderInfoInterop.major_version;
                minorVersion = fileHeaderInfoInterop.minor_version;
            }

            return new FileHeaderInfo(guid, majorVersion, minorVersion);
        }
    }

    /// <content>
    ///   Provides internal InputStream from external input stream.
    /// </content>
    internal partial class LibCziApiInterop
    {
        private void ThrowIfNotInitialized()
        {
            if (this.dllHandle == IntPtr.Zero)
            {
                if (string.IsNullOrEmpty(this.nativeLibraryLoadingErrorLog))
                {
                    throw new InvalidOperationException("LibCziApiInterop is not operational");
                }

                throw new InvalidOperationException($"LibCziApiInterop is not operational => {this.nativeLibraryLoadingErrorLog}");
            }
        }

        private void ThrowIfError(int returnCode)
        {
            if (returnCode != LibCziApiSuccess)
            {
                LibCziException.ExceptionCode exceptionCode;
                switch (returnCode)
                {
                    case LibCziApiInvalidArgument:
                        exceptionCode = LibCziException.ExceptionCode.InvalidArgument;
                        break;
                    case LibCziApiInvalidHandle:
                        exceptionCode = LibCziException.ExceptionCode.InvalidHandle;
                        break;
                    case LibCziApiOutOfMemory:
                        exceptionCode = LibCziException.ExceptionCode.OutOfMemory;
                        break;
                    case LibCziApiUnspecifiedError:
                    default:
                        exceptionCode = LibCziException.ExceptionCode.UnspecifiedError;
                        break;
                }

                throw new LibCziException(exceptionCode);
            }
        }

        private class InputStreamData
        {
            public IExternalInputStream ExternalInputStream { get; set; }
        }

        private class OutputStreamData
        {
            public IExternalOutputStream ExternalOutputStream { get; set; }
        }
    }

    /// <content>
    ///   Here we gather internally used utilities.
    /// </content>
    internal partial class LibCziApiInterop
    {
        private static int GetSizeOfSubBlockStatisticsInteropExForNScenes(int numberOfScenes)
        {
            return Marshal.SizeOf<SubBlockStatisticsInteropEx>() + (Math.Max(0, numberOfScenes - 1) * Marshal.SizeOf<BoundingBoxesInterop>());
        }

        private static unsafe BoundingBoxesInterop* GetScenesBoundingBoxOfSubBlockStatisticsInteropExStruct(SubBlockStatisticsInteropEx* statisticsInteropEx, int index)
        {
            return &statisticsInteropEx->perScenesBoundingBoxes0 + index;
        }

        private static IntPtr ConvertToZeroTerminatedUtf8LibCziAllocated(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return IntPtr.Zero;
            }

            byte[] utf8Bytes = NativeLibraryUtils.ConvertToUtf8ByteArray(s);
            IntPtr utf8StringLibCziAllocated = LibCziApiInterop.Instance.AllocateMemory(1 + (ulong)utf8Bytes.Length);
            Marshal.Copy(utf8Bytes, 0, utf8StringLibCziAllocated, utf8Bytes.Length);
            Marshal.WriteByte(utf8StringLibCziAllocated, utf8Bytes.Length, 0); // zero-terminate
            return utf8StringLibCziAllocated;
        }

        private static ExternalStreamErrorInfoInterop CreateExternalStreamErrorInfoInterop(int errorCode, string errorMessage)
        {
            ExternalStreamErrorInfoInterop errorInfo = default(ExternalStreamErrorInfoInterop);
            errorInfo.error_code = errorCode;
            errorInfo.error_message = LibCziApiInterop.ConvertToZeroTerminatedUtf8LibCziAllocated(errorMessage);
            return errorInfo;
        }

        private static unsafe ValueTuple<int, BoundingBoxPerScene>[] GetBoundingBoxes(SubBlockStatisticsInteropEx* statisticsInteropEx)
        {
            var result = new List<ValueTuple<int, BoundingBoxPerScene>>(statisticsInteropEx->numberOfScenes);
            for (int i = 0; i < statisticsInteropEx->numberOfScenes; i++)
            {
                BoundingBoxesInterop* boundingBoxesInterop = GetScenesBoundingBoxOfSubBlockStatisticsInteropExStruct(statisticsInteropEx, i);
                result.Add(new ValueTuple<int, BoundingBoxPerScene>(
                    boundingBoxesInterop->sceneIndex,
                    new BoundingBoxPerScene(
                        new IntRect(boundingBoxesInterop->bounding_box.x, boundingBoxesInterop->bounding_box.y, boundingBoxesInterop->bounding_box.w, boundingBoxesInterop->bounding_box.h),
                        new IntRect(boundingBoxesInterop->bounding_box_layer0_only.x, boundingBoxesInterop->bounding_box_layer0_only.y, boundingBoxesInterop->bounding_box_layer0_only.w, boundingBoxesInterop->bounding_box_layer0_only.h))));
            }

            return result.ToArray();
        }

        private static unsafe int ReadFunctionCallback(IntPtr opaqueHandle1, IntPtr opaqueHandle2, long offset, IntPtr data, long size, long* bytesRead, ExternalStreamErrorInfoInterop* errorInfo)
        {
            InputStreamData inputStreamData = (InputStreamData)GCHandle.FromIntPtr(opaqueHandle1).Target;

            try
            {
                inputStreamData.ExternalInputStream.Read(offset, data, size, out *bytesRead);
                *errorInfo = default(ExternalStreamErrorInfoInterop);
                return 0;
            }
            catch (Exception e)
            {
                *errorInfo = CreateExternalStreamErrorInfoInterop(1, e.Message);
                return 1;   // this is kStreamErrorCode_UnspecifiedError
            }
        }

        private static unsafe int WriteFunctionCallback(IntPtr opaqueHandle1, IntPtr opaqueHandle2, long offset, IntPtr data, long size, long* bytesWritten, ExternalStreamErrorInfoInterop* errorInfo)
        {
            OutputStreamData outputStreamData = (OutputStreamData)GCHandle.FromIntPtr(opaqueHandle1).Target;

            try
            {
                outputStreamData.ExternalOutputStream.Write(offset, data, size, out *bytesWritten);
                *errorInfo = default(ExternalStreamErrorInfoInterop);
                return 0;
            }
            catch (Exception e)
            {
                *errorInfo = CreateExternalStreamErrorInfoInterop(1, e.Message);
                return 1;   // this is kStreamErrorCode_UnspecifiedError
            }
        }

        private static void CloseInputStreamFunctionCallback(IntPtr opaqueHandle1, IntPtr opaqueHandle2)
        {
            var gcHandle = GCHandle.FromIntPtr(opaqueHandle1);
            InputStreamData inputStreamData = (InputStreamData)gcHandle.Target;
            inputStreamData.ExternalInputStream.Dispose();
            gcHandle.Free();
        }

        private static void CloseOutputStreamFunctionCallback(IntPtr opaqueHandle1, IntPtr opaqueHandle2)
        {
            var gcHandle = GCHandle.FromIntPtr(opaqueHandle1);
            OutputStreamData outputStreamData = (OutputStreamData)gcHandle.Target;
            outputStreamData.ExternalOutputStream.Dispose();
            gcHandle.Free();
        }
    }
}