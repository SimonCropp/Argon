// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if NET6_0_OR_GREATER

public class Issue1404 : TestFixtureBase
{
    [Fact]
    public void Test()
    {
        var type = typeof(FileSystemInfo);

        Assert.True(type.ImplementInterface(typeof(ISerializable)));

        var resolver = new DefaultContractResolver();

        var contract = resolver.ResolveContract(type);

        Assert.Equal(JsonContractType.Object, contract.ContractType);
    }
}
#endif