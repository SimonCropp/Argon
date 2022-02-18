﻿#region License
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

public class JsonSchemaNodeTests : TestFixtureBase
{
    [Fact]
    public void AddSchema()
    {
        var first = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""properties"":
  {
    ""firstproperty"":{""type"":""string"",""maxLength"":10},
    ""secondproperty"":{
      ""type"":""object"",
      ""properties"":
      {
        ""secondproperty_firstproperty"":{""type"":""string"",""maxLength"":10,""minLength"":7}
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
    ""firstproperty"":{""type"":""string""},
    ""secondproperty"":{
      ""extends"":{
        ""properties"":
        {
          ""secondproperty_firstproperty"":{""maxLength"":9,""minLength"":6}
        }
      },
      ""type"":""object"",
      ""properties"":
      {
        ""secondproperty_firstproperty"":{}
      }
    },
    ""thirdproperty"":{""type"":""string""}
  },
  ""additionalProperties"":false
}";

        var resolver = new JsonSchemaResolver();
        var firstSchema = JsonSchema.Parse(first, resolver);
        var secondSchema = JsonSchema.Parse(second, resolver);

        var modelBuilder = new JsonSchemaModelBuilder();

        var node = modelBuilder.AddSchema(null, secondSchema);

        Assert.Equal(2, node.Schemas.Count);
        Assert.Equal(2, node.Properties["firstproperty"].Schemas.Count);
        Assert.Equal(3, node.Properties["secondproperty"].Schemas.Count);
        Assert.Equal(3, node.Properties["secondproperty"].Properties["secondproperty_firstproperty"].Schemas.Count);
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

        var node = modelBuilder.AddSchema(null, schema);

        Assert.Single(node.Schemas);

        Assert.Equal(node, node.Items[0]);
    }
}

#pragma warning restore 618