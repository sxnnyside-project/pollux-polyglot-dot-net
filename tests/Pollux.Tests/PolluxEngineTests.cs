using Xunit;

namespace Sxnnyside.Pollux.Tests;

public class PolluxEngineTests
{
    private const string ValidManifest = """
        version: 1
        filesystem:
          read:
            - ./assets
            - /tmp
        """;

    private const string InvalidManifest = """
        version: invalid
        """;

    [Fact]
    public void LoadsEngineAndValidatesAbi()
    {
        using var engine = PolluxEngine.Load(ValidManifest);

        Assert.Equal("pollux-abi/1", engine.AbiVersion);
        Assert.False(string.IsNullOrEmpty(engine.CoreVersion));
        Assert.False(engine.IsDisposed);
    }

    [Fact]
    public void EvaluatesAllowedFilesystemRead()
    {
        using var engine = PolluxEngine.Load(ValidManifest);

        var result = engine.Evaluate(Operation.FileRead("./assets"));
        Assert.True(result.IsAllowed);
        Assert.Equal("allow", result.Outcome, ignoreCase: true);
        Assert.Contains("assets", result.TraceJson);
    }

    [Fact]
    public void EvaluatesUnauthorizedFilesystemWrite()
    {
        using var engine = PolluxEngine.Load(ValidManifest);

        var result = engine.Evaluate(Operation.FileWrite("./assets"));
        Assert.False(result.IsAllowed);
        Assert.Equal("deny", result.Outcome, ignoreCase: true);
    }

    [Fact]
    public void FailsOnMalformedManifest()
    {
        Assert.Throws<PolluxException.ManifestException>(() => PolluxEngine.Load(InvalidManifest));
    }

    [Fact]
    public void PreventsEvaluationAfterDispose()
    {
        var engine = PolluxEngine.Load(ValidManifest);
        engine.Dispose();

        Assert.True(engine.IsDisposed);
        Assert.Throws<PolluxException.DisposedException>(() => engine.Evaluate(Operation.FileRead("./assets")));
    }
}
