using System.Text;
using Sxnnyside.Pollux.Internal;

namespace Sxnnyside.Pollux;

/// <summary>
/// Pollux Deterministic Execution Authority and Sandboxing Engine instance.
/// Wraps a native Core AuthorityEngine handle through memory-safe P/Invoke bindings.
/// </summary>
public sealed class PolluxEngine : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The expected ABI version contract string.
    /// </summary>
    public const string ExpectedAbiVersion = "pollux-abi/1";

    private readonly NativeBridge _bridge;
    private readonly object _lock = new();
    private IntPtr _nativeEngine;
    private bool _isDisposed;

    private PolluxEngine(NativeBridge bridge, IntPtr nativeEngine)
    {
        _bridge = bridge;
        _nativeEngine = nativeEngine;
        AbiVersion = bridge.GetAbiVersion();
        CoreVersion = bridge.GetCoreVersion();

        if (AbiVersion != ExpectedAbiVersion)
        {
            Dispose();
            throw new PolluxException.AbiMismatchException(ExpectedAbiVersion, AbiVersion);
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="PolluxEngine"/> class.
    /// </summary>
    ~PolluxEngine()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets the ABI version reported by the linked Pollux Core binary.
    /// </summary>
    public string AbiVersion { get; }

    /// <summary>
    /// Gets the evaluation model version reported by Pollux Core.
    /// </summary>
    public string CoreVersion { get; }

    /// <summary>
    /// Gets a value indicating whether the engine instance has been disposed.
    /// </summary>
    public bool IsDisposed
    {
        get
        {
            lock (_lock)
            {
                return _isDisposed;
            }
        }
    }

    /// <summary>
    /// Loads an AuthorityEngine from an Authority Manifest (YAML string).
    /// </summary>
    /// <param name="manifestYaml">The YAML content defining the Authority Manifest.</param>
    /// <param name="customLibraryPath">Optional explicit path to the native binary.</param>
    /// <returns>A new initialized <see cref="PolluxEngine"/> instance.</returns>
    public static PolluxEngine Load(string manifestYaml, string? customLibraryPath = null)
    {
        ArgumentNullException.ThrowIfNull(manifestYaml);

        var bridge = new NativeBridge(customLibraryPath);
        var manifestBytes = Encoding.UTF8.GetBytes(manifestYaml);
        var engineHandle = bridge.CreateEngine(manifestBytes);

        return new PolluxEngine(bridge, engineHandle);
    }

    /// <summary>
    /// Loads an AuthorityEngine from an Authority Manifest file path.
    /// </summary>
    /// <param name="manifestFilePath">Path to the manifest file on disk.</param>
    /// <param name="customLibraryPath">Optional explicit path to the native binary.</param>
    /// <returns>A new initialized <see cref="PolluxEngine"/> instance.</returns>
    public static PolluxEngine FromFile(string manifestFilePath, string? customLibraryPath = null)
    {
        ArgumentNullException.ThrowIfNull(manifestFilePath);
        var content = File.ReadAllText(manifestFilePath);
        return Load(content, customLibraryPath);
    }

    /// <summary>
    /// Loads an AuthorityEngine asynchronously from an Authority Manifest file path.
    /// </summary>
    /// <param name="manifestFilePath">Path to the manifest file on disk.</param>
    /// <param name="customLibraryPath">Optional explicit path to the native binary.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous load operation returning a <see cref="PolluxEngine"/>.</returns>
    public static async Task<PolluxEngine> FromFileAsync(string manifestFilePath, string? customLibraryPath = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifestFilePath);
        var content = await File.ReadAllTextAsync(manifestFilePath, cancellationToken).ConfigureAwait(false);
        return Load(content, customLibraryPath);
    }

    /// <summary>
    /// Evaluates an operation candidate against the active Authority Manifest rules.
    /// </summary>
    /// <param name="operation">The operation candidate to evaluate.</param>
    /// <returns>The evaluation result verdict and trace.</returns>
    public EvaluationResult Evaluate(Operation operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var json = operation.ToJson();
        return Evaluate(json);
    }

    /// <summary>
    /// Evaluates a raw pollux-protocol/1 JSON operation candidate.
    /// </summary>
    /// <param name="operationJson">Raw JSON representation of the operation candidate.</param>
    /// <returns>The evaluation result verdict and trace.</returns>
    public EvaluationResult Evaluate(string operationJson)
    {
        ArgumentNullException.ThrowIfNull(operationJson);

        lock (_lock)
        {
            if (_isDisposed || _nativeEngine == IntPtr.Zero)
            {
                throw new PolluxException.DisposedException();
            }

            var opBytes = Encoding.UTF8.GetBytes(operationJson);
            var traceJson = _bridge.Evaluate(_nativeEngine, opBytes);
            return EvaluationResult.FromTraceJson(traceJson);
        }
    }

    /// <summary>
    /// Disposes the engine handle and releases native memory.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously disposes the engine instance.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public ValueTask DisposeAsync()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private void Dispose(bool disposing)
    {
        lock (_lock)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                if (_nativeEngine != IntPtr.Zero)
                {
                    _bridge.DestroyEngine(_nativeEngine);
                    _nativeEngine = IntPtr.Zero;
                }

                if (disposing)
                {
                    _bridge.Dispose();
                }
            }
        }
    }
}
