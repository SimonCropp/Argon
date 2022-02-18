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

public class JsonSchemaModelBuilderTests : TestFixtureBase
{
    [Fact]
    public void ExtendedComplex()
    {
        var first = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""properties"":
  {
    ""firstproperty"":{""type"":""string""},
    ""secondproperty"":{""type"":""string"",""maxLength"":10},
    ""thirdproperty"":{
      ""type"":""object"",
      ""properties"":
      {
        ""thirdproperty_firstproperty"":{""type"":""string"",""maxLength"":10,""minLength"":7}
      }
    }
  },
  ""additionalProperties"":{}
}";

        var second = @"{
  ""id"":""second"",
  ""type"":""object"",
  ""extends"":{""$ref"":""first""},
  ""properties"":
  {
    ""secondproperty"":{""type"":""any""},
    ""thirdproperty"":{
      ""extends"":{
        ""properties"":
        {
          ""thirdproperty_firstproperty"":{""maxLength"":9,""minLength"":6,""pattern"":""hi2u""}
        },
        ""additionalProperties"":{""maxLength"":9,""minLength"":6,""enum"":[""one"",""two""]}
      },
      ""type"":""object"",
      ""properties"":
      {
        ""thirdproperty_firstproperty"":{""pattern"":""hi""}
      },
      ""additionalProperties"":{""type"":""string"",""enum"":[""two"",""three""]}
    },
    ""fourthproperty"":{""type"":""string""}
  },
  ""additionalProperties"":false
}";

        var resolver = new JsonSchemaResolver();
        var firstSchema = JsonSchema.Parse(first, resolver);
        var secondSchema = JsonSchema.Parse(second, resolver);

        var modelBuilder = new JsonSchemaModelBuilder();

        var model = modelBuilder.Build(secondSchema);

        Assert.Equal(4, model.Properties.Count);

        Assert.Equal(JsonSchemaType.String, model.Properties["firstproperty"].Type);

        Assert.Equal(JsonSchemaType.String, model.Properties["secondproperty"].Type);
        Assert.Equal(10, model.Properties["secondproperty"].MaximumLength);
        Assert.Equal(null, model.Properties["secondproperty"].Enum);
        Assert.Equal(null, model.Properties["secondproperty"].Patterns);

        Assert.Equal(JsonSchemaType.Object, model.Properties["thirdproperty"].Type);
        Assert.Equal(3, model.Properties["thirdproperty"].AdditionalProperties.Enum.Count);
        Assert.Equal("two", (string) model.Properties["thirdproperty"].AdditionalProperties.Enum[0]);
        Assert.Equal("three", (string) model.Properties["thirdproperty"].AdditionalProperties.Enum[1]);
        Assert.Equal("one", (string) model.Properties["thirdproperty"].AdditionalProperties.Enum[2]);

        Assert.Equal(JsonSchemaType.String, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Type);
        Assert.Equal(9, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].MaximumLength);
        Assert.Equal(7, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].MinimumLength);
        Assert.Equal(2, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Patterns.Count);
        Assert.Equal("hi", model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Patterns[0]);
        Assert.Equal("hi2u", model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Patterns[1]);
        Assert.Equal(null, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Properties);
        Assert.Equal(null, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].Items);
        Assert.Equal(null, model.Properties["thirdproperty"].Properties["thirdproperty_firstproperty"].AdditionalProperties);
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

        var schema = JsonSchema.Parse(json);

        var modelBuilder = new JsonSchemaModelBuilder();

        var model = modelBuilder.Build(schema);

        Assert.Equal(JsonSchemaType.Array, model.Type);

        Assert.Equal(model, model.Items[0]);
    }

    [Fact]
    public void Required()
    {
        var schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":{""type"":""string"",required:true},
    ""age"":{""type"":""integer"",required:true}
  }
}";

        var schema = JsonSchema.Parse(schemaJson);
        var modelBuilder = new JsonSchemaModelBuilder();
        var model = modelBuilder.Build(schema);

        Assert.Equal(JsonSchemaType.Object, model.Type);
        Assert.Equal(3, model.Properties.Count);
        Assert.False(model.Properties["name"].Required);
        Assert.True(model.Properties["hobbies"].Required);
        Assert.True(model.Properties["age"].Required);
    }
}

#pragma warning restore 618