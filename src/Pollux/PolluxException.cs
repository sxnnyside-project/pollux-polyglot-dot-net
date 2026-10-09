namespace Sxnnyside.Pollux;

/// <summary>
/// Base exception for all Pollux SDK operations.
/// </summary>
public class PolluxException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PolluxException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public PolluxException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolluxException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The underlying inner exception.</param>
    public PolluxException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Checks a native status code and throws a specialized PolluxException on failure.
    /// </summary>
    /// <param name="code">The native status code returned by the C ABI.</param>
    /// <param name="context">The contextual operation or function name.</param>
    public static void CheckStatus(int code, string context)
    {
        var status = (PolluxStatus)code;
        switch (status)
        {
            case PolluxStatus.Ok:
                return;
            case PolluxStatus.NullArgument:
                throw new ArgumentNullException(context, $"Null argument encountered in {context}");
            case PolluxStatus.InvalidUtf8:
                throw new FormatException($"Invalid UTF-8 string encountered in {context}");
            case PolluxStatus.ManifestError:
                throw new ManifestException($"Failed to validate or parse Authority Manifest in {context}");
            case PolluxStatus.OperationError:
                throw new OperationException($"Operation failed or rejected in {context}");
            case PolluxStatus.InternalError:
            default:
                throw new PolluxException($"Native engine internal error (status {code}) in {context}");
        }
    }

    /// <summary>
    /// Exception thrown when Authority Manifest parsing or validation fails.
    /// </summary>
    public class ManifestException : PolluxException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManifestException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public ManifestException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an operation schema verification fails.
    /// </summary>
    public class OperationException : PolluxException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public OperationException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception thrown when the native ABI version does not match pollux-abi/1.
    /// </summary>
    public class AbiMismatchException : PolluxException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbiMismatchException"/> class.
        /// </summary>
        /// <param name="expected">The expected ABI contract identifier.</param>
        /// <param name="actual">The actual ABI contract reported by the native core.</param>
        public AbiMismatchException(string expected, string actual)
            : base($"Pollux ABI version mismatch: expected '{expected}', got '{actual}'")
        {
        }
    }

    /// <summary>
    /// Exception thrown when the native libpollux_ffi binary could not be found.
    /// </summary>
    public class LibraryNotFoundException : PolluxException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryNotFoundException"/> class.
        /// </summary>
        /// <param name="paths">The sequence of paths searched for the native library.</param>
        public LibraryNotFoundException(IEnumerable<string> paths)
            : base($"Failed to locate libpollux_ffi native library. Searched in: {string.Join(", ", paths)}")
        {
        }
    }

    /// <summary>
    /// Exception thrown when an operation is attempted on an already disposed engine.
    /// </summary>
    public class DisposedException : PolluxException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisposedException"/> class.
        /// </summary>
        public DisposedException()
            : base("Cannot perform operation: PolluxEngine has already been disposed.")
        {
        }
    }
}
