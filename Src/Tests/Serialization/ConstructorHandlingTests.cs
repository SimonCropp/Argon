#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.ComponentModel;
using Argon.Tests.TestObjects;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Serialization;

public class ConstructorHandlingTests : TestFixtureBase
{
    [Fact]
    public void UsePrivateConstructorIfThereAreMultipleConstructorsWithParametersAndNothingToFallbackTo()
    {
        var json = @"{Name:""Name!""}";

        var c = JsonConvert.DeserializeObject<PrivateConstructorTestClass>(json);

        Xunit.Assert.Equal("Name!", c.Name);
    }

    [Fact]
    public void SuccessWithPrivateConstructorAndAllowNonPublic()
    {
        var json = @"{Name:""Name!""}";

        var c = JsonConvert.DeserializeObject<PrivateConstructorTestClass>(json,
            new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });
        Xunit.Assert.NotNull(c);
        Xunit.Assert.Equal("Name!", c.Name);
    }

    [Fact]
    public void FailWithPrivateConstructorPlusParameterizedAndDefault()
    {
        ExceptionAssert.Throws<Exception>(() =>
        {
            var json = @"{Name:""Name!""}";

            var c = JsonConvert.DeserializeObject<PrivateConstructorWithPublicParameterizedConstructorTestClass>(json);
        });
    }

    [Fact]
    public void SuccessWithPrivateConstructorPlusParameterizedAndAllowNonPublic()
    {
        var json = @"{Name:""Name!""}";

        var c = JsonConvert.DeserializeObject<PrivateConstructorWithPublicParameterizedConstructorTestClass>(json,
            new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });
        Xunit.Assert.NotNull(c);
        Xunit.Assert.Equal("Name!", c.Name);
        Xunit.Assert.Equal(1, c.Age);
    }

    [Fact]
    public void SuccessWithPublicParameterizedConstructor()
    {
        var json = @"{Name:""Name!""}";

        var c = JsonConvert.DeserializeObject<PublicParameterizedConstructorTestClass>(json);
        Xunit.Assert.NotNull(c);
        Xunit.Assert.Equal("Name!", c.Name);
    }

    [Fact]
    public void SuccessWithPublicParameterizedConstructorWhenParameterIsNotAProperty()
    {
        var json = @"{nameParameter:""Name!""}";

        var c = JsonConvert.DeserializeObject<PublicParameterizedConstructorWithNonPropertyParameterTestClass>(json);
        Xunit.Assert.NotNull(c);
        Xunit.Assert.Equal("Name!", c.Name);
    }

    [Fact]
    public void SuccessWithPublicParameterizedConstructorWhenParameterRequiresAConverter()
    {
        var json = @"{nameParameter:""Name!""}";

        var c = JsonConvert.DeserializeObject<PublicParameterizedConstructorRequiringConverterTestClass>(json, new NameContainerConverter());
        Xunit.Assert.NotNull(c);
        Xunit.Assert.Equal("Name!", c.Name.Value);
    }

    [Fact]
    public void SuccessWithPublicParameterizedConstructorWhenParameterRequiresAConverterWithParameterAttribute()
    {
        var json = @"{nameParameter:""Name!""}";

        var c = JsonConvert.DeserializeObject<PublicParameterizedConstructorRequiringConverterWithParameterAttributeTestClass>(json);
        Xunit.Assert.NotNull(c);
        Xunit.Assert.Equal("Name!", c.Name.Value);
    }

    [Fact]
    public void SuccessWithPublicParameterizedConstructorWhenParameterRequiresAConverterWithPropertyAttribute()
    {
        var json = @"{name:""Name!""}";

        var c = JsonConvert.DeserializeObject<PublicParameterizedConstructorRequiringConverterWithPropertyAttributeTestClass>(json);
        Xunit.Assert.NotNull(c);
        Xunit.Assert.Equal("Name!", c.Name.Value);
    }

    [Fact]
    public void SuccessWithPublicParameterizedConstructorWhenParameterNameConflictsWithPropertyName()
    {
        var json = @"{name:""1""}";

        var c = JsonConvert.DeserializeObject<PublicParameterizedConstructorWithPropertyNameConflict>(json);
        Xunit.Assert.NotNull(c);
        Xunit.Assert.Equal(1, c.Name);
    }

    [Fact]
    public void PublicParameterizedConstructorWithPropertyNameConflictWithAttribute()
    {
        var json = @"{name:""1""}";

        var c = JsonConvert.DeserializeObject<PublicParameterizedConstructorWithPropertyNameConflictWithAttribute>(json);
        Xunit.Assert.NotNull(c);
        Xunit.Assert.Equal(1, c.Name);
    }

    public class ConstructorParametersRespectDefaultValueAttributes
    {
        [DefaultValue("parameter1_default")]
        public string Parameter1 { get; private set; }

        [DefaultValue("parameter2_default")]
        public string Parameter2 { get; private set; }

        [DefaultValue("parameter3_default")]
        public string Parameter3 { get; set; }

        [DefaultValue("parameter4_default")]
        public string Parameter4 { get; set; }

        public ConstructorParametersRespectDefaultValueAttributes(string parameter1, string parameter2, string parameter3)
        {
            Parameter1 = parameter1;
            Parameter2 = parameter2;
            Parameter3 = parameter3;
        }
    }

    [Fact]
    public void ConstructorParametersRespectDefaultValueTest_Attrbutes()
    {
        var testObject = JsonConvert.DeserializeObject<ConstructorParametersRespectDefaultValueAttributes>("{'Parameter2':'value!'}", new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Populate
        });

        Xunit.Assert.Equal("parameter1_default", testObject.Parameter1);
        Xunit.Assert.Equal("value!", testObject.Parameter2);
        Xunit.Assert.Equal("parameter3_default", testObject.Parameter3);
        Xunit.Assert.Equal("parameter4_default", testObject.Parameter4);
    }

    [Fact]
    public void ConstructorParametersRespectDefaultValueTest()
    {
        var testObject = JsonConvert.DeserializeObject<ConstructorParametersRespectDefaultValue>("{}", new JsonSerializerSettings { ContractResolver = ConstructorParameterDefaultStringValueContractResolver.Instance });

        Xunit.Assert.Equal("Default Value", testObject.Parameter1);
        Xunit.Assert.Equal("Default Value", testObject.Parameter2);
    }

    public class ConstructorParametersRespectDefaultValue
    {
        public const string DefaultValue = "Default Value";

        public string Parameter1 { get; private set; }
        public string Parameter2 { get; private set; }

        public ConstructorParametersRespectDefaultValue(string parameter1, string parameter2)
        {
            Parameter1 = parameter1;
            Parameter2 = parameter2;
        }
    }

    public class ConstructorParameterDefaultStringValueContractResolver : DefaultContractResolver
    {
        public static new ConstructorParameterDefaultStringValueContractResolver Instance = new();

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            foreach (var property in properties.Where(p => p.PropertyType == typeof(string)))
            {
                property.DefaultValue = ConstructorParametersRespectDefaultValue.DefaultValue;
                property.DefaultValueHandling = DefaultValueHandling.Populate;
            }

            return properties;
        }
    }
}