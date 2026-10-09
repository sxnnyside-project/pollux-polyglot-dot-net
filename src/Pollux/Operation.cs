using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sxnnyside.Pollux;

/// <summary>
/// Canonical operation candidate evaluated against Authority Manifest rules.
/// Conforms to OperationWire in pollux-protocol/1.
/// </summary>
/// <param name="Capability">Capability identifier requested by the operation.</param>
/// <param name="ResourceDomain">Domain classification of the resource target.</param>
/// <param name="ResourceValue">Resource identifier or specifier.</param>
public record Operation(
    [property: JsonPropertyName("capability")] string Capability,
    [property: JsonPropertyName("resource_domain")] string ResourceDomain,
    [property: JsonPropertyName("resource_value")] string ResourceValue)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    /// <summary>
    /// Creates a filesystem read operation.
    /// </summary>
    /// <param name="path">Path to the target filesystem entity.</param>
    /// <returns>A configured <see cref="Operation"/> instance for filesystem reading.</returns>
    public static Operation FileRead(string path)
    {
        return new("read", "filesystem", path);
    }

    /// <summary>
    /// Creates a filesystem write operation.
    /// </summary>
    /// <param name="path">Path to the target filesystem entity.</param>
    /// <returns>A configured <see cref="Operation"/> instance for filesystem writing.</returns>
    public static Operation FileWrite(string path)
    {
        return new("write", "filesystem", path);
    }

    /// <summary>
    /// Creates a network connection operation.
    /// </summary>
    /// <param name="target">Target host or address specification.</param>
    /// <returns>A configured <see cref="Operation"/> instance for network connection.</returns>
    public static Operation NetConnect(string target)
    {
        return new("connect", "network", target);
    }

    /// <summary>
    /// Creates a process execution/spawn operation.
    /// </summary>
    /// <param name="command">Command or executable path to spawn.</param>
    /// <returns>A configured <see cref="Operation"/> instance for process spawning.</returns>
    public static Operation ProcessSpawn(string command)
    {
        return new("spawn", "process", command);
    }

    /// <summary>
    /// Creates a custom (capability, resourceDomain, resourceValue) operation candidate.
    /// </summary>
    /// <param name="capability">Requested capability name.</param>
    /// <param name="resourceDomain">Resource domain boundary.</param>
    /// <param name="resourceValue">Resource target value.</param>
    /// <returns>A configured <see cref="Operation"/> instance.</returns>
    public static Operation Custom(string capability, string resourceDomain, string resourceValue)
    {
        return new(capability, resourceDomain, resourceValue);
    }

    /// <summary>
    /// Serializes the operation to standard pollux-protocol/1 JSON.
    /// </summary>
    /// <returns>A JSON representation of the operation.</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, JsonOptions);
    }
}
