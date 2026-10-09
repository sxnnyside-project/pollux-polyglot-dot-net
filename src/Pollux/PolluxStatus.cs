namespace Sxnnyside.Pollux;

/// <summary>
/// Status codes returned across the Pollux C-compatible ABI boundary.
/// Mirrors enum PolluxStatus in pollux.h.
/// </summary>
public enum PolluxStatus : int
{
    /// <summary>
    /// Success status indicating operation completed without errors.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// A null argument was supplied across the ABI boundary.
    /// </summary>
    NullArgument = 1,

    /// <summary>
    /// Input string did not contain valid UTF-8 sequences.
    /// </summary>
    InvalidUtf8 = 2,

    /// <summary>
    /// Manifest parsing or cryptographic validation failed.
    /// </summary>
    ManifestError = 3,

    /// <summary>
    /// Operation schema verification or parsing failed.
    /// </summary>
    OperationError = 4,

    /// <summary>
    /// An unrecoverable internal engine error occurred.
    /// </summary>
    InternalError = 5,

    /// <summary>
    /// An unknown status code was encountered.
    /// </summary>
    Unknown = -1
}
