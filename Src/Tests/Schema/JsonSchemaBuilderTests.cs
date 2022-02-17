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
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Schema;

[TestFixture]
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

        Assert.AreEqual("A person", schema.Description);
        Assert.AreEqual(JsonSchemaType.Object, schema.Type);

        Assert.AreEqual(2, schema.Properties.Count);

        Assert.AreEqual(JsonSchemaType.String, schema.Properties["name"].Type);
        Assert.AreEqual(JsonSchemaType.Array, schema.Properties["hobbies"].Type);
        Assert.AreEqual(JsonSchemaType.String, schema.Properties["hobbies"].Items[0].Type);
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

        Assert.AreEqual("Age", schema.Description);
        Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Integer, schema.Type);
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

        Assert.AreEqual("MultipleItems", schema.Description);
        Assert.AreEqual(JsonSchemaType.String, schema.Items[0].Type);
        Assert.AreEqual(JsonSchemaType.Array, schema.Items[1].Type);
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

        Assert.AreEqual("AdditionalProperties", schema.Description);
        Assert.AreEqual(JsonSchemaType.Object | JsonSchemaType.Boolean, schema.AdditionalProperties.Type);
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

        Assert.AreEqual("Required", schema.Description);
        Assert.AreEqual(true, schema.Required);
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

        Assert.AreEqual(true, schema.ExclusiveMinimum);
        Assert.AreEqual(true, schema.ExclusiveMaximum);
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

        Assert.AreEqual("ReadOnly", schema.Description);
        Assert.AreEqual(true, schema.ReadOnly);
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

        Assert.AreEqual("Hidden", schema.Description);
        Assert.AreEqual(true, schema.Hidden);
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

        Assert.AreEqual("Id", schema.Description);
        Assert.AreEqual("testid", schema.Id);
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

        Assert.AreEqual("Title", schema.Description);
        Assert.AreEqual("testtitle", schema.Title);
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

        Assert.AreEqual("Pattern", schema.Description);
        Assert.AreEqual("testpattern", schema.Pattern);
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

        Assert.AreEqual("Format", schema.Description);
        Assert.AreEqual("testformat", schema.Format);
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

        Assert.AreEqual("Requires", schema.Description);
        Assert.AreEqual("PurpleMonkeyDishwasher", schema.Requires);
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

        Assert.AreEqual("MinimumMaximum", schema.Description);
        Assert.AreEqual(1.1, schema.Minimum);
        Assert.AreEqual(1.2, schema.Maximum);
        Assert.AreEqual(1, schema.MinimumItems);
        Assert.AreEqual(2, schema.MaximumItems);
        Assert.AreEqual(5, schema.MinimumLength);
        Assert.AreEqual(50, schema.MaximumLength);
        Assert.AreEqual(3, schema.DivisibleBy);
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

        Assert.AreEqual("DisallowSingleType", schema.Description);
        Assert.AreEqual(JsonSchemaType.String, schema.Disallow);
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

        Assert.AreEqual("DisallowMultipleTypes", schema.Description);
        Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Float, schema.Disallow);
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

        Assert.AreEqual("DefaultPrimitiveType", schema.Description);
        Assert.AreEqual(1.1, (double)schema.Default);
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

        Assert.AreEqual("DefaultComplexType", schema.Description);
        Assert.IsTrue(JToken.DeepEquals(JObject.Parse(@"{""pie"":true}"), schema.Default));
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

        Assert.AreEqual("Type", schema.Description);
        Assert.AreEqual(JsonSchemaType.String | JsonSchemaType.Array, schema.Type);

        Assert.AreEqual(8, schema.Enum.Count);
        Assert.AreEqual("string", (string)schema.Enum[0]);
        Assert.AreEqual("any", (string)schema.Enum[schema.Enum.Count - 1]);
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

        Assert.AreEqual("CircularReference", schema.Description);
        Assert.AreEqual("CircularReferenceArray", schema.Id);
        Assert.AreEqual(JsonSchemaType.Array, schema.Type);

        Assert.AreEqual(schema, schema.Items[0]);
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

        Assert.IsNotNull(schema.PatternProperties);
        Assert.AreEqual(1, schema.PatternProperties.Count);
        Assert.AreEqual("Blah", schema.PatternProperties["[abc]"].Id);
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

        Assert.IsNotNull(schema.AdditionalItems);
        Assert.AreEqual(JsonSchemaType.Integer, schema.AdditionalItems.Type);
        Assert.AreEqual(true, schema.AllowAdditionalItems);
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

        Assert.IsNull(schema.AdditionalItems);
        Assert.AreEqual(false, schema.AllowAdditionalItems);
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

        Assert.IsNull(schema.AdditionalItems);
        Assert.AreEqual(false, schema.AllowAdditionalItems);
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

        Assert.AreEqual("#", schema.Location);
        Assert.AreEqual("#/properties/foo", schema.Properties["foo"].Location);
        Assert.AreEqual("#/properties/foo/items/1/properties/foo", schema.Properties["foo"].Items[1].Properties["foo"].Location);
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

        Assert.AreEqual(schema.Properties["foo"], schema.Properties["bar"]);
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

        Assert.AreEqual(schema.Properties["foo"], schema.Properties["bar"]);
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

        Assert.AreEqual(schema.Properties["foo"], schema.Properties["bar"]);
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

        Assert.AreEqual(JsonSchemaType.Integer, schema.Properties["tilda"].Type);
        Assert.AreEqual(JsonSchemaType.Object, schema.Properties["slash"].Type);
        Assert.AreEqual(JsonSchemaType.Array, schema.Properties["percent"].Type);
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

        Assert.AreEqual(JsonSchemaType.Integer, schema.Properties["array"].Type);
        Assert.AreEqual(JsonSchemaType.Object, schema.Properties["arrayprop"].Type);
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