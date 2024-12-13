#if NETFRAMEWORK

public class ImmutableVersionTests
{
    // work around https://github.com/orgs/VerifyTests/discussions/1366
    [Fact]
    public void AssertVersion()
    {
        var assemblyName = typeof(ImmutableDictionary).Assembly.GetName();
        Assert.Equal(new Version(8, 0, 0, 0), assemblyName.Version);
    }
}

#endif