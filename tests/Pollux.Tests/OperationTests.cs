using Xunit;

namespace Sxnnyside.Pollux.Tests;

public class OperationTests
{
    [Fact]
    public void CreatesFilesystemReadOperation()
    {
        var op = Operation.FileRead("/var/log/system.log");
        Assert.Equal("read", op.Capability);
        Assert.Equal("filesystem", op.ResourceDomain);
        Assert.Equal("/var/log/system.log", op.ResourceValue);

        var json = op.ToJson();
        Assert.Contains("\"capability\":\"read\"", json);
        Assert.Contains("\"resource_domain\":\"filesystem\"", json);
        Assert.Contains("\"resource_value\":\"/var/log/system.log\"", json);
    }

    [Fact]
    public void CreatesFilesystemWriteOperation()
    {
        var op = Operation.FileWrite("./data.bin");
        Assert.Equal("write", op.Capability);
        Assert.Equal("filesystem", op.ResourceDomain);
        Assert.Equal("./data.bin", op.ResourceValue);
    }

    [Fact]
    public void CreatesNetworkConnectOperation()
    {
        var op = Operation.NetConnect("api.sxnnysideproject.com:443");
        Assert.Equal("connect", op.Capability);
        Assert.Equal("network", op.ResourceDomain);
        Assert.Equal("api.sxnnysideproject.com:443", op.ResourceValue);
    }

    [Fact]
    public void CreatesProcessSpawnOperation()
    {
        var op = Operation.ProcessSpawn("cmd.exe");
        Assert.Equal("spawn", op.Capability);
        Assert.Equal("process", op.ResourceDomain);
        Assert.Equal("cmd.exe", op.ResourceValue);
    }
}
