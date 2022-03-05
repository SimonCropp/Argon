// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

public class JsonPropertyCollectionTests : TestFixtureBase
{
    [Fact]
    public void AddPropertyIncludesPrivateImplementations()
    {
        var value = new PrivateImplementationBClass
        {
            OverriddenProperty = "OverriddenProperty",
            PropertyA = "PropertyA",
            PropertyB = "PropertyB"
        };

        var resolver = new DefaultContractResolver();
        var contract = (JsonObjectContract) resolver.ResolveContract(value.GetType());

        Assert.Equal(3, contract.Properties.Count);
        Assert.True(contract.Properties.Contains("OverriddenProperty"), "Contract is missing property 'OverriddenProperty'");
        Assert.True(contract.Properties.Contains("PropertyA"), "Contract is missing property 'PropertyA'");
        Assert.True(contract.Properties.Contains("PropertyB"), "Contract is missing property 'PropertyB'");
    }
}