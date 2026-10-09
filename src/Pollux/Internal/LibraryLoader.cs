using System.Runtime.InteropServices;

namespace Sxnnyside.Pollux.Internal;

/// <summary>
/// Locates and dynamically loads the native libpollux_ffi binary across Windows, Linux, and macOS.
/// </summary>
internal static class LibraryLoader
{
    /// <summary>
    /// Dynamically loads the native library using NativeLibrary.Load.
    /// </summary>
    /// <param name="customPath">Optional explicit path to the native binary.</param>
    /// <returns>An unmanaged handle to the loaded native library.</returns>
    public static IntPtr LoadLibrary(string? customPath = null)
    {
        var path = ResolvePath(customPath);
        return NativeLibrary.Load(path);
    }

    /// <summary>
    /// Resolves the absolute path to the native binary.
    /// </summary>
    /// <param name="customPath">Optional explicit path to the native binary.</param>
    /// <returns>The resolved file path to the native binary.</returns>
    public static string ResolvePath(string? customPath = null)
    {
        var searchedPaths = new List<string>();
        var fileName = GetLibraryFileName();

        if (!string.IsNullOrEmpty(customPath))
        {
            if (File.Exists(customPath))
            {
                return Path.GetFullPath(customPath);
            }

            searchedPaths.Add(customPath);
        }

        var envCore = Environment.GetEnvironmentVariable("POLLUX_CORE_LIB");
        if (!string.IsNullOrEmpty(envCore))
        {
            if (File.Exists(envCore))
            {
                return Path.GetFullPath(envCore);
            }

            searchedPaths.Add(envCore);
        }

        var envFfi = Environment.GetEnvironmentVariable("POLLUX_FFI_PATH");
        if (!string.IsNullOrEmpty(envFfi))
        {
            if (File.Exists(envFfi))
            {
                return Path.GetFullPath(envFfi);
            }

            searchedPaths.Add(envFfi);
        }

        var baseDir = AppContext.BaseDirectory;
        var rid = RuntimeInformation.RuntimeIdentifier; // e.g. win-x64, osx-arm64, linux-x64
        var candidates = new List<string>
        {
            Path.Combine(baseDir, fileName),
            Path.Combine(baseDir, "runtimes", rid, "native", fileName),
            Path.Combine(baseDir, "lib", fileName),
            Path.Combine(baseDir, "..", "..", "..", "..", "lib", fileName),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "lib", fileName),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "Pollux", "target", "release", fileName),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "Pollux", "target", "debug", fileName),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "Native", "pollux-polyglot-native-bridge", "target", "release", fileName),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "Native", "pollux-polyglot-native-bridge", "target", "debug", fileName)
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (!string.IsNullOrEmpty(programFiles))
            {
                candidates.Add(Path.Combine(programFiles, "Pollux", "bin", fileName));
            }

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrEmpty(localAppData))
            {
                candidates.Add(Path.Combine(localAppData, "Pollux", "bin", fileName));
            }

            var systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            if (!string.IsNullOrEmpty(systemDir))
            {
                candidates.Add(Path.Combine(systemDir, fileName));
            }

            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    candidates.Add(Path.Combine(dir, fileName));
                }
            }
        }
        else
        {
            candidates.Add(Path.Combine("/opt/homebrew/lib", fileName));
            candidates.Add(Path.Combine("/usr/local/lib", fileName));
            candidates.Add(Path.Combine("/usr/lib", fileName));
        }

        foreach (var candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }

            searchedPaths.Add(candidate);
        }

        // Final attempt: let the operating system loader find it if on PATH or system library loader
        try
        {
            if (NativeLibrary.TryLoad(fileName, out _))
            {
                return fileName;
            }
        }
        catch
        {
            // Ignore
        }

        throw new PolluxException.LibraryNotFoundException(searchedPaths);
    }

    private static string GetLibraryFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "pollux_ffi.dll";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "libpollux_ffi.dylib";
        }

        return "libpollux_ffi.so";
    }
}
