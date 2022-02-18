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
#pragma warning disable 1062
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Schema;

[TestFixture]
public class JsonSchemaTests : TestFixtureBase
{
  [Fact]
  public void Extends()
  {
      var resolver = new JsonSchemaResolver();

    var json = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""additionalProperties"":{}
}";

    var first = JsonSchema.Parse(json, resolver);

    json =
      @"{
  ""id"":""second"",
  ""type"":""object"",
  ""extends"":{""$ref"":""first""},
  ""additionalProperties"":{""type"":""string""}
}";

    var second = JsonSchema.Parse(json, resolver);
    Assert.AreEqual(first, second.Extends[0]);

    json =
      @"{
  ""id"":""third"",
  ""type"":""object"",
  ""extends"":{""$ref"":""second""},
  ""additionalProperties"":false
}";

    var third = JsonSchema.Parse(json, resolver);
    Assert.AreEqual(second, third.Extends[0]);
    Assert.AreEqual(first, third.Extends[0].Extends[0]);

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    third.WriteTo(jsonWriter, resolver);

    var writtenJson = writer.ToString();
    StringAssert.AreEqual(@"{
  ""id"": ""third"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""extends"": {
    ""$ref"": ""second""
  }
}", writtenJson);

    var writer1 = new StringWriter();
    var jsonWriter1 = new JsonTextWriter(writer1);
    jsonWriter1.Formatting = Formatting.Indented;

    third.WriteTo(jsonWriter1);

    writtenJson = writer1.ToString();
    StringAssert.AreEqual(@"{
  ""id"": ""third"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""extends"": {
    ""id"": ""second"",
    ""type"": ""object"",
    ""additionalProperties"": {
      ""type"": ""string""
    },
    ""extends"": {
      ""id"": ""first"",
      ""type"": ""object"",
      ""additionalProperties"": {}
    }
  }
}", writtenJson);
  }

  [Fact]
  public void Extends_Multiple()
  {
    var json = @"{
  ""type"":""object"",
  ""extends"":{""type"":""string""},
  ""additionalProperties"":{""type"":""string""}
}";

    var s = JsonSchema.Parse(json);

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    var newJson = s.ToString();

    StringAssert.AreEqual(@"{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""string""
  },
  ""extends"": {
    ""type"": ""string""
  }
}", newJson);

    json = @"{
  ""type"":""object"",
  ""extends"":[{""type"":""string""}],
  ""additionalProperties"":{""type"":""string""}
}";

    s = JsonSchema.Parse(json);

    writer = new StringWriter();
    jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    newJson = s.ToString();

    StringAssert.AreEqual(@"{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""string""
  },
  ""extends"": {
    ""type"": ""string""
  }
}", newJson);

    json = @"{
  ""type"":""object"",
  ""extends"":[{""type"":""string""},{""type"":""object""}],
  ""additionalProperties"":{""type"":""string""}
}";

    s = JsonSchema.Parse(json);

    writer = new StringWriter();
    jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    newJson = s.ToString();

    StringAssert.AreEqual(@"{
  ""type"": ""object"",
  ""additionalProperties"": {
    ""type"": ""string""
  },
  ""extends"": [
    {
      ""type"": ""string""
    },
    {
      ""type"": ""object""
    }
  ]
}", newJson);
  }

  [Fact]
  public void WriteTo_AdditionalProperties()
  {
    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    var schema = JsonSchema.Parse(@"{
  ""description"":""AdditionalProperties"",
  ""type"":[""string"", ""integer""],
  ""additionalProperties"":{""type"":[""object"", ""boolean""]}
}");

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""description"": ""AdditionalProperties"",
  ""type"": [
    ""string"",
    ""integer""
  ],
  ""additionalProperties"": {
    ""type"": [
      ""boolean"",
      ""object""
    ]
  }
}", json);
  }

  [Fact]
  public void WriteTo_Properties()
  {
    var schema = JsonSchema.Parse(@"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":
    {
      ""type"":""array"",
      ""items"": {""type"":""string""}
    }
  }
}");

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""description"": ""A person"",
  ""type"": ""object"",
  ""properties"": {
    ""name"": {
      ""type"": ""string""
    },
    ""hobbies"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""string""
      }
    }
  }
}", json);
  }

  [Fact]
  public void WriteTo_Enum()
  {
    var schema = JsonSchema.Parse(@"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""items"":{},
  ""enum"":[""string"",""object"",""array"",""boolean"",""number"",""integer"",""null"",""any""]
}");

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""description"": ""Type"",
  ""type"": [
    ""string"",
    ""array""
  ],
  ""items"": {},
  ""enum"": [
    ""string"",
    ""object"",
    ""array"",
    ""boolean"",
    ""number"",
    ""integer"",
    ""null"",
    ""any""
  ]
}", json);
  }

  [Fact]
  public void WriteTo_CircularReference()
  {
    var json = @"{
  ""id"":""CircularReferenceArray"",
  ""description"":""CircularReference"",
  ""type"":[""array""],
  ""items"":{""$ref"":""CircularReferenceArray""}
}";

    var schema = JsonSchema.Parse(json);

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var writtenJson = writer.ToString();

    StringAssert.AreEqual(@"{
  ""id"": ""CircularReferenceArray"",
  ""description"": ""CircularReference"",
  ""type"": ""array"",
  ""items"": {
    ""$ref"": ""CircularReferenceArray""
  }
}", writtenJson);
  }

  [Fact]
  public void WriteTo_DisallowMultiple()
  {
    var schema = JsonSchema.Parse(@"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""items"":{},
  ""disallow"":[""string"",""object"",""array""]
}");

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""description"": ""Type"",
  ""type"": [
    ""string"",
    ""array""
  ],
  ""items"": {},
  ""disallow"": [
    ""string"",
    ""object"",
    ""array""
  ]
}", json);
  }

  [Fact]
  public void WriteTo_DisallowSingle()
  {
    var schema = JsonSchema.Parse(@"{
  ""description"":""Type"",
  ""type"":[""string"",""array""],
  ""items"":{},
  ""disallow"":""any""
}");

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""description"": ""Type"",
  ""type"": [
    ""string"",
    ""array""
  ],
  ""items"": {},
  ""disallow"": ""any""
}", json);
  }

  [Fact]
  public void WriteTo_MultipleItems()
  {
    var schema = JsonSchema.Parse(@"{
  ""items"":[{},{}]
}");

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""items"": [
    {},
    {}
  ]
}", json);
  }

  [Fact]
  public void WriteTo_ExclusiveMinimum_ExclusiveMaximum()
  {
    var schema = new JsonSchema
    {
      ExclusiveMinimum = true,
      ExclusiveMaximum = true
    };

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""exclusiveMinimum"": true,
  ""exclusiveMaximum"": true
}", json);
  }

  [Fact]
  public void WriteTo_PatternProperties()
  {
    var schema = new JsonSchema
    {
      PatternProperties = new Dictionary<string, JsonSchema>
      {
        { "[abc]", new JsonSchema() }
      }
    };

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""patternProperties"": {
    ""[abc]"": {}
  }
}", json);
  }

  [Fact]
  public void ToString_AdditionalItems()
  {
    var schema = JsonSchema.Parse(@"{
    ""additionalItems"": {""type"": ""integer""}
}");

    var json = schema.ToString();

    StringAssert.AreEqual(@"{
  ""additionalItems"": {
    ""type"": ""integer""
  }
}", json);
  }

  [Fact]
  public void WriteTo_PositionalItemsValidation_True()
  {
    var schema = new JsonSchema
    {
      PositionalItemsValidation = true
    };

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""items"": []
}", json);
  }

  [Fact]
  public void WriteTo_PositionalItemsValidation_TrueWithItemsSchema()
  {
    var schema = new JsonSchema
    {
      PositionalItemsValidation = true,
      Items = new List<JsonSchema> { new() { Type = JsonSchemaType.String } }
    };

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""items"": [
    {
      ""type"": ""string""
    }
  ]
}", json);
  }

  [Fact]
  public void WriteTo_PositionalItemsValidation_FalseWithItemsSchema()
  {
    var schema = new JsonSchema
    {
      Items = new List<JsonSchema> { new() { Type = JsonSchemaType.String } }
    };

    var writer = new StringWriter();
    var jsonWriter = new JsonTextWriter(writer);
    jsonWriter.Formatting = Formatting.Indented;

    schema.WriteTo(jsonWriter);

    var json = writer.ToString();

    StringAssert.AreEqual(@"{
  ""items"": {
    ""type"": ""string""
  }
}", json);
  }

  [Fact]
  public void IntegerValidatesAgainstFloatFlags()
  {
    var schema = JsonSchema.Parse(@"{
  ""type"": ""object"",
  ""$schema"": ""http://json-schema.org/draft-03/schema"",
  ""required"": false,
  ""properties"": {
  ""NumberProperty"": {
    ""required"": false,
    ""type"": [
        ""number"",
        ""null""
      ]
    }
  }
}");

    var json = JObject.Parse(@"{
        ""NumberProperty"": 23
      }");

    Assert.IsTrue(json.IsValid(schema));
  }
}