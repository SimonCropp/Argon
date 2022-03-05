// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

public class ReflectionAttributeProviderTests : TestFixtureBase
{
    public class ReflectionTestObject
    {
        [DefaultValue("1")] [JsonProperty] public int TestProperty { get; set; }

        [DefaultValue("1")] [JsonProperty] public int TestField;

        public ReflectionTestObject(
            [DefaultValue("1")] [JsonProperty] int testParameter)
        {
            TestProperty = testParameter;
            TestField = testParameter;
        }
    }

    [Fact]
    public void GetAttributes_Property()
    {
        var property = typeof(ReflectionTestObject).GetProperty("TestProperty");

        var provider = new ReflectionAttributeProvider(property);

        var attributes = provider.GetAttributes(typeof(DefaultValueAttribute), false);
        Assert.Equal(1, attributes.Count);

        attributes = provider.GetAttributes(false);
        Assert.Equal(2, attributes.Count);
    }

    [Fact]
    public void GetAttributes_Field()
    {
        var field = typeof(ReflectionTestObject).GetField("TestField");

        var provider = new ReflectionAttributeProvider(field);

        var attributes = provider.GetAttributes(typeof(DefaultValueAttribute), false);
        Assert.Equal(1, attributes.Count);

        attributes = provider.GetAttributes(false);
        Assert.Equal(2, attributes.Count);
    }

    [Fact]
    public void GetAttributes_Parameter()
    {
        var parameters = typeof(ReflectionTestObject).GetConstructor(new[] {typeof(int)}).GetParameters();

        var parameter = parameters[0];

        var provider = new ReflectionAttributeProvider(parameter);

        var attributes = provider.GetAttributes(typeof(DefaultValueAttribute), false);
        Assert.Equal(1, attributes.Count);

        attributes = provider.GetAttributes(false);
        Assert.Equal(2, attributes.Count);
    }
}