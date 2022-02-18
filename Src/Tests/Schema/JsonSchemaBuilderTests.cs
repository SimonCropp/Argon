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

#pragma warning disable 618
using Xunit;

namespace Argon.Tests.Schema;

public class JsonSchemaBuilderTests : TestFixtureBase
{
    [Fact]
    public void Simple()
    {
        var json = @"
{
  ""description"": ""A person"",
  ""type"": ""object"",
  ""properties"":
  {
    ""name"": {""type"":""string""},
    ""hobbies"": {
      ""type"": ""array"",
      ""items"": {""type"":""string""}
    }
  }
}
";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("A person", schema.Description);
        Xunit.Assert.Equal(JsonSchemaType.Object, schema.Type);

        Xunit.Assert.Equal(2, schema.Properties.Count);

        Xunit.Assert.Equal(JsonSchemaType.String, schema.Properties["name"].Type);
        Xunit.Assert.Equal(JsonSchemaType.Array, schema.Properties["hobbies"].Type);
        Xunit.Assert.Equal(JsonSchemaType.String, schema.Properties["hobbies"].Items[0].Type);
    }

    [Fact]
    public void MultipleTypes()
    {
        var json = @"{
  ""description"":""Age"",
  ""type"":[""string"", ""integer""]
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("Age", schema.Description);
        Xunit.Assert.Equal(JsonSchemaType.String | JsonSchemaType.Integer, schema.Type);
    }

    [Fact]
    public void MultipleItems()
    {
        var json = @"{
  ""description"":""MultipleItems"",
  ""type"":""array"",
  ""items"": [{""type"":""string""},{""type"":""array""}]
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("MultipleItems", schema.Description);
        Xunit.Assert.Equal(JsonSchemaType.String, schema.Items[0].Type);
        Xunit.Assert.Equal(JsonSchemaType.Array, schema.Items[1].Type);
    }

    [Fact]
    public void AdditionalProperties()
    {
        var json = @"{
  ""description"":""AdditionalProperties"",
  ""type"":[""string"", ""integer""],
  ""additionalProperties"":{""type"":[""object"", ""boolean""]}
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("AdditionalProperties", schema.Description);
        Xunit.Assert.Equal(JsonSchemaType.Object | JsonSchemaType.Boolean, schema.AdditionalProperties.Type);
    }

    [Fact]
    public void Required()
    {
        var json = @"{
  ""description"":""Required"",
  ""required"":true
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("Required", schema.Description);
        XUnitAssert.True(schema.Required);
    }

    [Fact]
    public void ExclusiveMinimum_ExclusiveMaximum()
    {
        var json = @"{
  ""exclusiveMinimum"":true,
  ""exclusiveMaximum"":true
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        XUnitAssert.True(schema.ExclusiveMinimum);
        XUnitAssert.True(schema.ExclusiveMaximum);
    }

    [Fact]
    public void ReadOnly()
    {
        var json = @"{
  ""description"":""ReadOnly"",
  ""readonly"":true
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("ReadOnly", schema.Description);
        XUnitAssert.True(schema.ReadOnly);
    }

    [Fact]
    public void Hidden()
    {
        var json = @"{
  ""description"":""Hidden"",
  ""hidden"":true
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("Hidden", schema.Description);
        XUnitAssert.True(schema.Hidden);
    }

    [Fact]
    public void Id()
    {
        var json = @"{
  ""description"":""Id"",
  ""id"":""testid""
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("Id", schema.Description);
        Xunit.Assert.Equal("testid", schema.Id);
    }

    [Fact]
    public void Title()
    {
        var json = @"{
  ""description"":""Title"",
  ""title"":""testtitle""
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("Title", schema.Description);
        Xunit.Assert.Equal("testtitle", schema.Title);
    }

    [Fact]
    public void Pattern()
    {
        var json = @"{
  ""description"":""Pattern"",
  ""pattern"":""testpattern""
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("Pattern", schema.Description);
        Xunit.Assert.Equal("testpattern", schema.Pattern);
    }

    [Fact]
    public void Format()
    {
        var json = @"{
  ""description"":""Format"",
  ""format"":""testformat""
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("Format", schema.Description);
        Xunit.Assert.Equal("testformat", schema.Format);
    }

    [Fact]
    public void Requires()
    {
        var json = @"{
  ""description"":""Requires"",
  ""requires"":""PurpleMonkeyDishwasher""
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("Requires", schema.Description);
        Xunit.Assert.Equal("PurpleMonkeyDishwasher", schema.Requires);
    }

    [Fact]
    public void MinimumMaximum()
    {
        var json = @"{
  ""description"":""MinimumMaximum"",
  ""minimum"":1.1,
  ""maximum"":1.2,
  ""minItems"":1,
  ""maxItems"":2,
  ""minLength"":5,
  ""maxLength"":50,
  ""divisibleBy"":3,
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("MinimumMaximum", schema.Description);
        Xunit.Assert.Equal(1.1, schema.Minimum);
        Xunit.Assert.Equal(1.2, schema.Maximum);
        Xunit.Assert.Equal(1, schema.MinimumItems);
        Xunit.Assert.Equal(2, schema.MaximumItems);
        Xunit.Assert.Equal(5, schema.MinimumLength);
        Xunit.Assert.Equal(50, schema.MaximumLength);
        Xunit.Assert.Equal(3, schema.DivisibleBy);
    }

    [Fact]
    public void DisallowSingleType()
    {
        var json = @"{
  ""description"":""DisallowSingleType"",
  ""disallow"":""string""
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("DisallowSingleType", schema.Description);
        Xunit.Assert.Equal(JsonSchemaType.String, schema.Disallow);
    }

    [Fact]
    public void DisallowMultipleTypes()
    {
        var json = @"{
  ""description"":""DisallowMultipleTypes"",
  ""disallow"":[""string"",""number""]
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("DisallowMultipleTypes", schema.Description);
        Xunit.Assert.Equal(JsonSchemaType.String | JsonSchemaType.Float, schema.Disallow);
    }

    [Fact]
    public void DefaultPrimitiveType()
    {
        var json = @"{
  ""description"":""DefaultPrimitiveType"",
  ""default"":1.1
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("DefaultPrimitiveType", schema.Description);
        Xunit.Assert.Equal(1.1, (double)schema.Default);
    }

    [Fact]
    public void DefaultComplexType()
    {
        var json = @"{
  ""description"":""DefaultComplexType"",
  ""default"":{""pie"":true}
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("DefaultComplexType", schema.Description);
        Xunit.Assert.True(JToken.DeepEquals(JObject.Parse(@"{""pie"":true}"), schema.Default));
    }

    [Fact]
    public void Enum()
    {
        var json = @"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""enum"":[""string"",""object"",""array"",""boolean"",""number"",""integer"",""null"",""any""]
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("Type", schema.Description);
        Xunit.Assert.Equal(JsonSchemaType.String | JsonSchemaType.Array, schema.Type);

        Xunit.Assert.Equal(8, schema.Enum.Count);
        Xunit.Assert.Equal("string", (string)schema.Enum[0]);
        Xunit.Assert.Equal("any", (string)schema.Enum[schema.Enum.Count - 1]);
    }

    [Fact]
    public void CircularReference()
    {
        var json = @"{
  ""id"":""CircularReferenceArray"",
  ""description"":""CircularReference"",
  ""type"":[""array""],
  ""items"":{""$ref"":""CircularReferenceArray""}
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("CircularReference", schema.Description);
        Xunit.Assert.Equal("CircularReferenceArray", schema.Id);
        Xunit.Assert.Equal(JsonSchemaType.Array, schema.Type);

        Xunit.Assert.Equal(schema, schema.Items[0]);
    }

    [Fact]
    public void UnresolvedReference()
    {
        ExceptionAssert.Throws<Exception>(() =>
        {
            var json = @"{
  ""id"":""CircularReferenceArray"",
  ""description"":""CircularReference"",
  ""type"":[""array""],
  ""items"":{""$ref"":""MyUnresolvedReference""}
}";

            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            var schema = builder.Read(new JsonTextReader(new StringReader(json)));
        }, @"Could not resolve schema reference 'MyUnresolvedReference'.");
    }

    [Fact]
    public void PatternProperties()
    {
        var json = @"{
  ""patternProperties"": {
    ""[abc]"": { ""id"":""Blah"" }
  }
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.NotNull(schema.PatternProperties);
        Xunit.Assert.Equal(1, schema.PatternProperties.Count);
        Xunit.Assert.Equal("Blah", schema.PatternProperties["[abc]"].Id);
    }

    [Fact]
    public void AdditionalItems()
    {
        var json = @"{
    ""items"": [],
    ""additionalItems"": {""type"": ""integer""}
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.NotNull(schema.AdditionalItems);
        Xunit.Assert.Equal(JsonSchemaType.Integer, schema.AdditionalItems.Type);
        XUnitAssert.True(schema.AllowAdditionalItems);
    }

    [Fact]
    public void DisallowAdditionalItems()
    {
        var json = @"{
    ""items"": [],
    ""additionalItems"": false
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Null(schema.AdditionalItems);
        XUnitAssert.False(schema.AllowAdditionalItems);
    }

    [Fact]
    public void AllowAdditionalItems()
    {
        var json = @"{
    ""items"": {},
    ""additionalItems"": false
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Null(schema.AdditionalItems);
        XUnitAssert.False(schema.AllowAdditionalItems);
    }

    [Fact]
    public void Location()
    {
        var json = @"{
  ""properties"":{
    ""foo"":{
      ""type"":""array"",
      ""items"":[
        {
          ""type"":""integer""
        },
        {
          ""properties"":{
            ""foo"":{
              ""type"":""integer""
            }
          }
        }
      ]
    }
  }
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal("#", schema.Location);
        Xunit.Assert.Equal("#/properties/foo", schema.Properties["foo"].Location);
        Xunit.Assert.Equal("#/properties/foo/items/1/properties/foo", schema.Properties["foo"].Items[1].Properties["foo"].Location);
    }

    [Fact]
    public void Reference_BackwardsLocation()
    {
        var json = @"{
  ""properties"": {
    ""foo"": {""type"": ""integer""},
    ""bar"": {""$ref"": ""#/properties/foo""}
  }
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal(schema.Properties["foo"], schema.Properties["bar"]);
    }

    [Fact]
    public void Reference_ForwardsLocation()
    {
        var json = @"{
  ""properties"": {
    ""bar"": {""$ref"": ""#/properties/foo""},
    ""foo"": {""type"": ""integer""}
  }
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal(schema.Properties["foo"], schema.Properties["bar"]);
    }

    [Fact]
    public void Reference_NonStandardLocation()
    {
        var json = @"{
  ""properties"": {
    ""bar"": {""$ref"": ""#/common/foo""},
    ""foo"": {""$ref"": ""#/common/foo""}
  },
  ""common"": {
    ""foo"": {""type"": ""integer""}
  }
}";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal(schema.Properties["foo"], schema.Properties["bar"]);
    }

    [Fact]
    public void EscapedReferences()
    {
        var json = @"{
            ""tilda~field"": {""type"": ""integer""},
            ""slash/field"": {""type"": ""object""},
            ""percent%field"": {""type"": ""array""},
            ""properties"": {
                ""tilda"": {""$ref"": ""#/tilda~0field""},
                ""slash"": {""$ref"": ""#/slash~1field""},
                ""percent"": {""$ref"": ""#/percent%25field""}
            }
        }";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal(JsonSchemaType.Integer, schema.Properties["tilda"].Type);
        Xunit.Assert.Equal(JsonSchemaType.Object, schema.Properties["slash"].Type);
        Xunit.Assert.Equal(JsonSchemaType.Array, schema.Properties["percent"].Type);
    }

    [Fact]
    public void References_Array()
    {
        var json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/1/prop""}
            }
        }";

        var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
        var schema = builder.Read(new JsonTextReader(new StringReader(json)));

        Xunit.Assert.Equal(JsonSchemaType.Integer, schema.Properties["array"].Type);
        Xunit.Assert.Equal(JsonSchemaType.Object, schema.Properties["arrayprop"].Type);
    }

    [Fact]
    public void References_IndexTooBig()
    {
        // JsonException : Could not resolve schema reference '#/array/10'.

        var json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/10""}
            }
        }";

        ExceptionAssert.Throws<JsonException>(() =>
        {
            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            builder.Read(new JsonTextReader(new StringReader(json)));
        }, "Could not resolve schema reference '#/array/10'.");
    }

    [Fact]
    public void References_IndexNegative()
    {
        var json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/-1""}
            }
        }";

        ExceptionAssert.Throws<JsonException>(() =>
        {
            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            builder.Read(new JsonTextReader(new StringReader(json)));
        }, "Could not resolve schema reference '#/array/-1'.");
    }

    [Fact]
    public void References_IndexNotInteger()
    {
        var json = @"{
            ""array"": [{""type"": ""integer""},{""prop"":{""type"": ""object""}}],
            ""properties"": {
                ""array"": {""$ref"": ""#/array/0""},
                ""arrayprop"": {""$ref"": ""#/array/one""}
            }
        }";

        ExceptionAssert.Throws<JsonException>(() =>
        {
            var builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            builder.Read(new JsonTextReader(new StringReader(json)));
        }, "Could not resolve schema reference '#/array/one'.");
    }
}

#pragma warning restore 618