// SPDX-FileCopyrightText: 2025 Carl Zeiss Microscopy GmbH
//
// SPDX-License-Identifier: MIT

namespace Zeiss.Micro.LibCzi.Net.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Represents errors that occur within C# layer of the CZI library.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class LibCziException : Exception
    {
        private readonly ExceptionCode exceptionCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibCziException"/> class.
        /// This constructor sets the exception code to <see cref="ExceptionCode.UnspecifiedError"/>.
        /// </summary>
        public LibCziException()
            : this(ExceptionCode.UnspecifiedError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibCziException"/> class.
        /// </summary>
        /// <param name="exceptionCode">The exception code that describes the error condition.</param>
        public LibCziException(ExceptionCode exceptionCode)
        {
            this.exceptionCode = exceptionCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibCziException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public LibCziException(string message)
            : this(ExceptionCode.UnspecifiedError, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibCziException"/> class.
        /// </summary>
        /// <param name="exceptionCode">The exception code that describes the error condition.</param>
        /// <param name="message">The message that describes the error.</param>
        public LibCziException(ExceptionCode exceptionCode, string message)
            : base(message)
        {
            this.exceptionCode = exceptionCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibCziException"/> class.
        /// This constructor sets the exception code to <see cref="ExceptionCode.UnspecifiedError"/>.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public LibCziException(string message, Exception innerException)
            : this(ExceptionCode.UnspecifiedError, message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibCziException"/> class.
        /// </summary>
        /// <param name="exceptionCode">The exception code.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The inner exception.</param>
        public LibCziException(ExceptionCode exceptionCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.exceptionCode = exceptionCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibCziException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source and destination.</param>
        protected LibCziException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Defines the various exception codes.
        /// </summary>
        public enum ExceptionCode
        {
            /// <summary>
            /// Indicates that an invalid argument was provided.
            /// </summary>
            InvalidArgument,

            /// <summary>
            /// Indicates that an invalid handle was encountered.
            /// </summary>
            InvalidHandle,

            /// <summary>
            /// Indicates that the system has run out of memory.
            /// </summary>
            OutOfMemory,

            /// <summary>
            /// Indicates an unspecified error condition.
            /// </summary>
            UnspecifiedError,
        }

        /// <summary>
        /// Gets the exception code.
        /// </summary>
        /// <value>
        /// The exception code.
        /// </value>
        public ExceptionCode Code => this.exceptionCode;
    }
}