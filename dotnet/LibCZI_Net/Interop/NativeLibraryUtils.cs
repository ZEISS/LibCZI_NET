// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace LibCZI_Net.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Provides utility functions to import native libraries.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "<Native-Handling>")]
    internal class NativeLibraryUtils
    {
        // RTLD_NOW constant for Linux and macOS
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore", Justification = "External-Constant")]
        private const int RTLD_NOW = 2; // For Linux and macOS

        /// <summary>
        /// Loads the specified library.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <returns>If the function succeeds, the return value is a handle to the module.</returns>
        public static IntPtr LoadLibrary(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return LoadLibraryWindows(path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LoadLibraryLinux(path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return LoadLibraryMac(path);
            }

            throw new PlatformNotSupportedException("Unsupported platform");
        }

        /// <summary>
        /// Gets the function pointer from imported library.
        /// </summary>
        /// <param name="libraryHandle">The library handle.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>Funtion pointer.</returns>
        /// <exception cref="System.PlatformNotSupportedException">Unsupported platform.</exception>
        public static IntPtr GetFunctionPointer(IntPtr libraryHandle, string functionName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetProcAddressWindows(libraryHandle, functionName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return dlsymLinux(libraryHandle, functionName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return dlsymMac(libraryHandle, functionName);
            }

            throw new PlatformNotSupportedException("Unsupported platform");
        }

        /// <summary>
        /// Tries to get a delegate to a function in the specified library with the specified name.
        /// </summary>
        /// <typeparam name="T">The type of delegate that represents the function signature of the desired function.</typeparam>
        /// <param name="libraryHandle">The library handle.</param>
        /// <param name="functionName">Name of the function.</param>
        /// <returns>
        /// An instance of <typeparamref name="T"/> representing the delegate for the specified function,
        /// or <c>null</c> if the function could not be found.
        /// </returns>
        public static T TryGetProcAddress<T>(IntPtr libraryHandle, string functionName)
            where T : Delegate
        {
            IntPtr addressOfFunctionToCall = GetFunctionPointer(libraryHandle, functionName);
            if (addressOfFunctionToCall == IntPtr.Zero)
            {
                return null;
            }

            return (T)Marshal.GetDelegateForFunctionPointer(addressOfFunctionToCall, typeof(T));
        }

        /// <summary>
        /// Utility to create a .NET-delegate for the specified exported function.
        /// If the exported function is not found, then an 'InvalidOperationException' exception is thrown.
        /// </summary>
        /// <typeparam name="T">    The delegate type definition. </typeparam>
        /// <param name="libraryHandle">Handle of the dynamic library.</param>
        /// <param name="functionName" type="string">   The name of the exported function. </param>
        /// <returns type="T">
        /// The newly constructed delegate.
        /// </returns>
        public static T GetProcAddressThrowIfNotFound<T>(IntPtr libraryHandle, string functionName)
            where T : Delegate
        {
            T del = TryGetProcAddress<T>(libraryHandle, functionName);
            if (del == null)
            {
                throw new InvalidOperationException($"Function \"{functionName}\" was not found.");
            }

            return del;
        }

        /// <summary>
        /// Convert a UTF-8 string (given by a pointer and a length) into a .NET string.
        /// </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null.</exception>
        /// <param name="utf8Pointer"> Pointer to the UTF8-encoded string to be converted.</param>
        /// <param name="length">      The length of the input string in bytes.</param>
        /// <returns>
        /// The converted string.
        /// </returns>
        public static string ConvertFromUtf8IntPtr(IntPtr utf8Pointer, int length)
        {
            if (utf8Pointer == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(utf8Pointer), "Pointer is null.");
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length is negative.");
            }

            if (length == 0)
            {
                return string.Empty;
            }

            // Copy data from unmanaged memory to a managed byte array
            byte[] buffer = new byte[length];
            Marshal.Copy(utf8Pointer, buffer, 0, length);

            // Convert the byte array to a string
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Try to get a string indicating the Linux-distro we are running on. We do this by reading the /etc/os-release file.
        /// If the file is not found (or has an unexpected content), then the string "unknown" is returned.
        /// </summary>
        /// <returns>
        /// If successful, a string indicating the Linux-distro. If unsuccessful, "unknown" is returned.
        /// </returns>
        public static string GetLinuxDistro()
        {
            if (File.Exists("/etc/os-release"))
            {
                foreach (var line in File.ReadAllLines("/etc/os-release"))
                {
                    if (line.StartsWith("ID=", StringComparison.OrdinalIgnoreCase))
                    {
                        return line.Substring(3).Trim('"');
                    }
                }
            }

            return "unknown";
        }

        /// <summary> Convert a zero-terminated UTF-8 string from an IntPtr to a .NET string.</summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null.</exception>
        /// <param name="utf8Pointer"> Pointer to the UTF8-encoded string to be converted.</param>
        /// <returns> The converted string.</returns>
        public static string ConvertFromUtf8IntPtrZeroTerminated(IntPtr utf8Pointer)
        {
            // with .NETCore 2.1 we could use the following code instead:
            //  return Marshal.PtrToStringUTF8(utf8Pointer);
            if (utf8Pointer == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(utf8Pointer), "Pointer is null.");
            }

            // Determine the length of the string
            int length = 0;
            while (Marshal.ReadByte(utf8Pointer, length) != 0)
            {
                length++;
            }

            return NativeLibraryUtils.ConvertFromUtf8IntPtr(utf8Pointer, length);
        }

        /// <summary>
        /// Convert a zero-terminated UTF-8 string from an IntPtr to a .NET string.
        /// If the specified pointer is null, then an empty string is returned.
        /// </summary>
        /// <param name="utf8Pointer"> Pointer to the UTF8-encoded string to be converted. The value IntPtr.Zero gives an empty string.</param>
        /// <returns> The converted string.</returns>
        public static string ConvertFromUtf8IntPtrZeroTerminatedAllowNull(IntPtr utf8Pointer)
        {
            // with .NETCore 2.1 we could use the following code instead:
            //  return Marshal.PtrToStringUTF8(utf8Pointer);
            if (utf8Pointer == IntPtr.Zero)
            {
                return string.Empty;
            }

            // Determine the length of the string
            int length = 0;
            while (Marshal.ReadByte(utf8Pointer, length) != 0)
            {
                length++;
            }

            return NativeLibraryUtils.ConvertFromUtf8IntPtr(utf8Pointer, length);
        }

        /// <summary>
        /// Convert a zero-terminated UTF-8 string from an IntPtr to a .NET string. The string is limited to the specified maximum length.
        /// If there is a \0 character before the maximum length is reached, then the string is truncated at that position.
        /// </summary>
        /// <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null.</exception>
        /// <param name="utf8Pointer"> Pointer to the UTF8-encoded string to be converted.</param>
        /// <param name="maxLength">   The maximum length of the string given by 'utf8Pointer'.</param>
        /// <returns> The converted string.</returns>
        public static string ConvertFromUtf8IntPtrZeroTerminatedWithMaxLength(IntPtr utf8Pointer, int maxLength)
        {
            if (utf8Pointer == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(utf8Pointer), "Pointer is null.");
            }

            // Determine the length of the string
            int length = 0;
            while (maxLength > 0 && Marshal.ReadByte(utf8Pointer, length) != 0)
            {
                length++;
                maxLength--;
            }

            return NativeLibraryUtils.ConvertFromUtf8IntPtr(utf8Pointer, length);
        }

        /// <summary>
        /// Converts string to a UTF8 byte array.
        /// </summary>
        /// <param name="str">The string to convert to a UTF-8 byte array.</param>
        /// <returns>A byte array containing the UTF-8 encoded representation of the input string.</returns>
        public static byte[] ConvertToUtf8ByteArray(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        [DllImport("kernel32", SetLastError = true, EntryPoint = "GetProcAddress")]
        private static extern IntPtr GetProcAddressWindows(IntPtr hModule, string procName);

        [DllImport("libdl", EntryPoint = "dlsym")]
        private static extern IntPtr dlsymMac(IntPtr handle, string symbol);

        [DllImport("libdl.so.2", EntryPoint = "dlsym")]
        private static extern IntPtr dlsymLinux(IntPtr handle, string symbol);

        private static IntPtr LoadLibraryWindows(string path) => LoadLibrary_Native(path);

        private static IntPtr LoadLibraryLinux(string path) => dlopen_NativeLinux(path, RTLD_NOW);

        private static IntPtr LoadLibraryMac(string path) => dlopen_NativeMacOs(path, RTLD_NOW);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "LoadLibrary")]
        private static extern IntPtr LoadLibrary_Native(string lpFileName);

        [DllImport("libdl", SetLastError = true, EntryPoint = "dlopen")]
        private static extern IntPtr dlopen_NativeMacOs(string fileName, int flags);

        [DllImport("libdl.so.2", EntryPoint = "dlopen")]
        private static extern IntPtr dlopen_NativeLinux(string fileName, int flags);
    }
}