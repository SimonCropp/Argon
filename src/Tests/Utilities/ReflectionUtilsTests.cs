// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.ComponentModel;

public class ReflectionUtilsTests : TestFixtureBase
{
    public class ReflectionTestObject
    {
        [DefaultValue("1")]
        [JsonProperty]
        public int TestProperty { get; set; }

        [DefaultValue("1")]
        [JsonProperty]
        public int TestField;

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

        var attributes = ReflectionUtils.GetAttributes(property, typeof(DefaultValueAttribute), false);
        Assert.Equal(1, attributes.Length);

        attributes = ReflectionUtils.GetAttributes(property, null, false);
        Assert.Equal(2, attributes.Length);
    }

    [Fact]
    public void GetAttributes_Field()
    {
        var field = typeof(ReflectionTestObject).GetField("TestField");

        var attributes = ReflectionUtils.GetAttributes(field, typeof(DefaultValueAttribute), false);
        Assert.Equal(1, attributes.Length);

        attributes = ReflectionUtils.GetAttributes(field, null, false);
        Assert.Equal(2, attributes.Length);
    }

    [Fact]
    public void GetAttributes_Parameter()
    {
        var parameters = typeof(ReflectionTestObject).GetConstructor(new[]
        {
            typeof(int)
        }).GetParameters();

        var parameter = parameters[0];

        var attributes = ReflectionUtils.GetAttributes(parameter, typeof(DefaultValueAttribute), false);
        Assert.Equal(1, attributes.Length);

        attributes = ReflectionUtils.GetAttributes(parameter, null, false);
        Assert.Equal(2, attributes.Length);
    }

    [Fact]
    public void GetTypeNameSimpleForGenericTypes()
    {
        var typeName = typeof(IList<Type>).GetTypeName(TypeNameAssemblyFormatHandling.Simple, null);
        Assert.Equal("System.Collections.Generic.IList`1[[System.Type, mscorlib]], mscorlib", typeName);

        typeName = typeof(IDictionary<IList<Type>, IList<Type>>).GetTypeName(TypeNameAssemblyFormatHandling.Simple, null);
        Assert.Equal("System.Collections.Generic.IDictionary`2[[System.Collections.Generic.IList`1[[System.Type, mscorlib]], mscorlib],[System.Collections.Generic.IList`1[[System.Type, mscorlib]], mscorlib]], mscorlib", typeName);

        typeName = typeof(IList<>).GetTypeName(TypeNameAssemblyFormatHandling.Simple, null);
        Assert.Equal("System.Collections.Generic.IList`1, mscorlib", typeName);

        typeName = typeof(IDictionary<,>).GetTypeName(TypeNameAssemblyFormatHandling.Simple, null);
        Assert.Equal("System.Collections.Generic.IDictionary`2, mscorlib", typeName);
    }
}