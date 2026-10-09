using System.Runtime.InteropServices;

namespace Sxnnyside.Pollux.Internal;

/// <summary>
/// Low-level dynamic P/Invoke bridge for the Pollux Core C ABI.
/// </summary>
internal sealed class NativeBridge : IDisposable
{
    private readonly IntPtr _libraryHandle;
    private readonly PolluxAbiVersionFn _fnAbiVersion;
    private readonly PolluxCoreVersionFn _fnCoreVersion;
    private readonly PolluxEngineCreateFn _fnEngineCreate;
    private readonly PolluxEngineEvaluateFn _fnEngineEvaluate;
    private readonly PolluxStringFreeFn _fnStringFree;
    private readonly PolluxEngineDestroyFn _fnEngineDestroy;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeBridge"/> class.
    /// </summary>
    /// <param name="customLibraryPath">Optional explicit path to the native binary.</param>
    public NativeBridge(string? customLibraryPath = null)
    {
        _libraryHandle = LibraryLoader.LoadLibrary(customLibraryPath);

        T LoadSymbol<T>(string name)
            where T : Delegate
        {
            var symbol = NativeLibrary.GetExport(_libraryHandle, name);
            return Marshal.GetDelegateForFunctionPointer<T>(symbol);
        }

        _fnAbiVersion = LoadSymbol<PolluxAbiVersionFn>("pollux_abi_version");
        _fnCoreVersion = LoadSymbol<PolluxCoreVersionFn>("pollux_core_version");
        _fnEngineCreate = LoadSymbol<PolluxEngineCreateFn>("pollux_engine_create");
        _fnEngineEvaluate = LoadSymbol<PolluxEngineEvaluateFn>("pollux_engine_evaluate");
        _fnStringFree = LoadSymbol<PolluxStringFreeFn>("pollux_string_free");
        _fnEngineDestroy = LoadSymbol<PolluxEngineDestroyFn>("pollux_engine_destroy");
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr PolluxAbiVersionFn();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr PolluxCoreVersionFn();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int PolluxEngineCreateFn(IntPtr manifestPtr, nuint manifestLen, out IntPtr outEngine);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int PolluxEngineEvaluateFn(IntPtr engine, IntPtr opJsonPtr, nuint opJsonLen, out IntPtr outTrace);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void PolluxStringFreeFn(IntPtr ptr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void PolluxEngineDestroyFn(IntPtr engine);

    /// <summary>
    /// Gets the native ABI contract version.
    /// </summary>
    /// <returns>The ABI version string.</returns>
    public string GetAbiVersion()
    {
        var ptr = _fnAbiVersion();
        return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
    }

    /// <summary>
    /// Gets the native evaluation model version.
    /// </summary>
    /// <returns>The Core version string.</returns>
    public string GetCoreVersion()
    {
        var ptr = _fnCoreVersion();
        return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
    }

    /// <summary>
    /// Instantiates an AuthorityEngine from raw UTF-8 manifest bytes.
    /// </summary>
    /// <param name="manifestBytes">UTF-8 encoded YAML manifest.</param>
    /// <returns>Pointer to the native engine instance.</returns>
    public unsafe IntPtr CreateEngine(byte[] manifestBytes)
    {
        fixed (byte* p = manifestBytes)
        {
            var status = _fnEngineCreate((IntPtr)p, (nuint)manifestBytes.Length, out var engineHandle);
            PolluxException.CheckStatus(status, "pollux_engine_create");
            return engineHandle;
        }
    }

    /// <summary>
    /// Evaluates an operation candidate against the native engine instance.
    /// </summary>
    /// <param name="engine">Pointer to the native engine instance.</param>
    /// <param name="operationBytes">UTF-8 encoded JSON operation.</param>
    /// <returns>The raw JSON evaluation trace returned by Pollux Core.</returns>
    public unsafe string Evaluate(IntPtr engine, byte[] operationBytes)
    {
        fixed (byte* p = operationBytes)
        {
            var status = _fnEngineEvaluate(engine, (IntPtr)p, (nuint)operationBytes.Length, out var tracePtr);
            PolluxException.CheckStatus(status, "pollux_engine_evaluate");

            if (tracePtr == IntPtr.Zero)
            {
                throw new PolluxException("Native engine returned a null trace pointer");
            }

            try
            {
                return Marshal.PtrToStringUTF8(tracePtr) ?? string.Empty;
            }
            finally
            {
                _fnStringFree(tracePtr);
            }
        }
    }

    /// <summary>
    /// Destroys a native engine handle.
    /// </summary>
    /// <param name="engine">Pointer to the native engine instance.</param>
    public void DestroyEngine(IntPtr engine)
    {
        if (engine != IntPtr.Zero)
        {
            _fnEngineDestroy(engine);
        }
    }

    /// <summary>
    /// Frees the loaded native library handle.
    /// </summary>
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            NativeLibrary.Free(_libraryHandle);
        }
    }
}
