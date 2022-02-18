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
using ValidationEventArgs = Argon.Schema.ValidationEventArgs;

namespace Argon.Tests;

public class JsonValidatingReaderTests : TestFixtureBase
{
    [Fact]
    public void CheckInnerReader()
    {
        var json = "{'name':'James','hobbies':['pie','cake']}";
        JsonReader reader = new JsonTextReader(new StringReader(json));

        var validatingReader = new JsonValidatingReader(reader);
        Xunit.Assert.Equal(reader, validatingReader.Reader);
    }

    [Fact]
    public void ValidateTypes()
    {
        var schemaJson = @"{
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
}";

        var json = @"{'name':""James"",'hobbies':[""pie"",'cake']}";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        var schema = JsonSchema.Parse(schemaJson);
        reader.Schema = schema;
        Xunit.Assert.Equal(schema, reader.Schema);
        Xunit.Assert.Equal(0, reader.Depth);
        Xunit.Assert.Equal("", reader.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);
        Xunit.Assert.Equal("", reader.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("name", reader.Value.ToString());
        Xunit.Assert.Equal("name", reader.Path);
        Xunit.Assert.Equal(1, reader.Depth);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("James", reader.Value.ToString());
        Xunit.Assert.Equal(typeof(string), reader.ValueType);
        Xunit.Assert.Equal('"', reader.QuoteChar);
        Xunit.Assert.Equal("name", reader.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("hobbies", reader.Value.ToString());
        Xunit.Assert.Equal('\'', reader.QuoteChar);
        Xunit.Assert.Equal("hobbies", reader.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);
        Xunit.Assert.Equal("hobbies", reader.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("pie", reader.Value.ToString());
        Xunit.Assert.Equal('"', reader.QuoteChar);
        Xunit.Assert.Equal("hobbies[0]", reader.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("cake", reader.Value.ToString());
        Xunit.Assert.Equal("hobbies[1]", reader.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
        Xunit.Assert.Equal("hobbies", reader.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
        Xunit.Assert.Equal("", reader.Path);

        Xunit.Assert.False(reader.Read());

        Xunit.Assert.Null(validationEventArgs);
    }

    [Fact]
    public void ValidateUnrestrictedArray()
    {
        var schemaJson = @"{
  ""type"":""array""
}";

        var json = "['pie','cake',['nested1','nested2'],{'nestedproperty1':1.1,'nestedproperty2':[null]}]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("pie", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("cake", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("nested1", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("nested2", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("nestedproperty1", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(1.1, reader.Value);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("nestedproperty2", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.Null(validationEventArgs);
    }

    [Fact]
    public void StringLessThanMinimumLength()
    {
        var schemaJson = @"{
  ""type"":""string"",
  ""minLength"":5,
  ""maxLength"":50,
}";

        var json = "'pie'";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("String 'pie' is less than minimum length of 5. Line 1, position 5.", validationEventArgs.Message);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void StringGreaterThanMaximumLength()
    {
        var schemaJson = @"{
  ""type"":""string"",
  ""minLength"":5,
  ""maxLength"":10
}";

        var json = "'The quick brown fox jumps over the lazy dog.'";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("String 'The quick brown fox jumps over the lazy dog.' exceeds maximum length of 10. Line 1, position 46.", validationEventArgs.Message);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void StringIsNotInEnum()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""string"",
    ""enum"":[""one"",""two""]
  },
  ""maxItems"":3
}";

        var json = "['one','two','THREE']";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal(@"Value ""THREE"" is not defined in enum. Line 1, position 20.", validationEventArgs.Message);
        Xunit.Assert.Equal("[2]", validationEventArgs.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void StringDoesNotMatchPattern()
    {
        var schemaJson = @"{
  ""type"":""string"",
  ""pattern"":""foo""
}";

        var json = "'The quick brown fox jumps over the lazy dog.'";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("String 'The quick brown fox jumps over the lazy dog.' does not match regex pattern 'foo'. Line 1, position 46.", validationEventArgs.Message);
        Xunit.Assert.Equal("", validationEventArgs.Path);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void IntegerGreaterThanMaximumValue()
    {
        var schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

        var json = "10";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal("Integer 10 exceeds maximum value of 5. Line 1, position 2.", validationEventArgs.Message);
        Xunit.Assert.Equal("", validationEventArgs.Path);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void IntegerGreaterThanMaximumValue_BigInteger()
    {
        var schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

        var json = "99999999999999999999999999999999999999999999999999999999999999999999";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal("Integer 99999999999999999999999999999999999999999999999999999999999999999999 exceeds maximum value of 5. Line 1, position 68.", validationEventArgs.Message);
        Xunit.Assert.Equal("", validationEventArgs.Path);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void IntegerLessThanMaximumValue_BigInteger()
    {
        var schemaJson = @"{
  ""type"":""integer"",
  ""minimum"":5
}";

        var v = new JValue(new BigInteger(1));

        ValidationEventArgs validationEventArgs = null;

        v.Validate(JsonSchema.Parse(schemaJson), (_, args) => { validationEventArgs = args; });

        Xunit.Assert.NotNull(validationEventArgs);
        Xunit.Assert.Equal("Integer 1 is less than minimum value of 5.", validationEventArgs.Message);
        Xunit.Assert.Equal("", validationEventArgs.Path);
    }

    [Fact]
    public void ThrowExceptionWhenNoValidationEventHandler()
    {
        ExceptionAssert.Throws<JsonSchemaException>(() =>
        {
            var schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader("10")));
            reader.Schema = JsonSchema.Parse(schemaJson);

            Xunit.Assert.True(reader.Read());
        }, "Integer 10 exceeds maximum value of 5. Line 1, position 2.");
    }

    [Fact]
    public void IntegerLessThanMinimumValue()
    {
        var schemaJson = @"{
  ""type"":""integer"",
  ""minimum"":5
}";

        var json = "1";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal("Integer 1 is less than minimum value of 5. Line 1, position 1.", validationEventArgs.Message);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void IntegerIsNotInEnum()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""integer"",
    ""enum"":[1,2]
  },
  ""maxItems"":3
}";

        var json = "[1,2,3]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(@"Value 3 is not defined in enum. Line 1, position 6.", validationEventArgs.Message);
        Xunit.Assert.Equal("[2]", validationEventArgs.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void FloatGreaterThanMaximumValue()
    {
        var schemaJson = @"{
  ""type"":""number"",
  ""maximum"":5
}";

        var json = "10.0";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal("Float 10.0 exceeds maximum value of 5. Line 1, position 4.", validationEventArgs.Message);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void FloatLessThanMinimumValue()
    {
        var schemaJson = @"{
  ""type"":""number"",
  ""minimum"":5
}";

        var json = "1.1";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal("Float 1.1 is less than minimum value of 5. Line 1, position 3.", validationEventArgs.Message);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void FloatIsNotInEnum()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""enum"":[1.1,2.2]
  },
  ""maxItems"":3
}";

        var json = "[1.1,2.2,3.0]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(@"Value 3.0 is not defined in enum. Line 1, position 12.", validationEventArgs.Message);
        Xunit.Assert.Equal("[2]", validationEventArgs.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void FloatDivisibleBy()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""divisibleBy"":0.1
  }
}";

        var json = "[1.1,2.2,4.001]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(@"Float 4.001 is not evenly divisible by 0.1. Line 1, position 14.", validationEventArgs.Message);
        Xunit.Assert.Equal("[2]", validationEventArgs.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void BigIntegerDivisibleBy_Success()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""divisibleBy"":2
  }
}";

        var json = "[999999999999999999999999999999999999999999999999999999998]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Null(validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public void BigIntegerDivisibleBy_Failure()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""divisibleBy"":2
  }
}";

        var json = "[999999999999999999999999999999999999999999999999999999999]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(@"Integer 999999999999999999999999999999999999999999999999999999999 is not evenly divisible by 2. Line 1, position 58.", validationEventArgs.Message);
        Xunit.Assert.Equal("[0]", validationEventArgs.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void BigIntegerDivisibleBy_Fraction()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""divisibleBy"":1.1
  }
}";

        var json = "[999999999999999999999999999999999999999999999999999999999]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.NotNull(validationEventArgs);
        Xunit.Assert.Equal(@"Integer 999999999999999999999999999999999999999999999999999999999 is not evenly divisible by 1.1. Line 1, position 58.", validationEventArgs.Message);
        Xunit.Assert.Equal("[0]", validationEventArgs.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
    }

    [Fact]
    public void BigIntegerDivisibleBy_FractionWithZeroValue()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number"",
    ""divisibleBy"":1.1
  }
}";

        var a = new JArray(new JValue(new BigInteger(0)));

        ValidationEventArgs validationEventArgs = null;

        a.Validate(JsonSchema.Parse(schemaJson), (_, args) => { validationEventArgs = args; });

        Xunit.Assert.Null(validationEventArgs);
    }

    [Fact]
    public void IntValidForNumber()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""number""
  }
}";

        var json = "[1]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.Null(validationEventArgs);
    }

    [Fact]
    public void NullNotInEnum()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""null"",
    ""enum"":[]
  },
  ""maxItems"":3
}";

        var json = "[null]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);
        Xunit.Assert.Equal(@"Value null is not defined in enum. Line 1, position 5.", validationEventArgs.Message);
        Xunit.Assert.Equal("[0]", validationEventArgs.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void BooleanNotInEnum()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""boolean"",
    ""enum"":[true]
  },
  ""maxItems"":3
}";

        var json = "[true,false]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Boolean, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Boolean, reader.TokenType);
        Xunit.Assert.Equal(@"Value false is not defined in enum. Line 1, position 11.", validationEventArgs.Message);
        Xunit.Assert.Equal("[1]", validationEventArgs.Path);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void ArrayCountGreaterThanMaximumItems()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""minItems"":2,
  ""maxItems"":3
}";

        var json = "[null,null,null,null]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);
        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
        Xunit.Assert.Equal("Array item count 4 exceeds maximum count of 3. Line 1, position 21.", validationEventArgs.Message);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void ArrayCountLessThanMinimumItems()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""minItems"":2,
  ""maxItems"":3
}";

        var json = "[null]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
        Xunit.Assert.Equal("Array item count 1 is less than minimum count of 2. Line 1, position 6.", validationEventArgs.Message);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void InvalidDataType()
    {
        var schemaJson = @"{
  ""type"":""string"",
  ""minItems"":2,
  ""maxItems"":3,
  ""items"":{}
}";

        var json = "[null,null,null,null]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);
        Xunit.Assert.Equal(@"Invalid type. Expected String but got Array. Line 1, position 1.", validationEventArgs.Message);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void StringDisallowed()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""disallow"":[""number""]
  },
  ""maxItems"":3
}";

        var json = "['pie',1.1]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Float, reader.TokenType);
        Xunit.Assert.Equal(@"Type Float is disallowed. Line 1, position 10.", validationEventArgs.Message);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void MissingRequiredProperties()
    {
        var schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""},
    ""hobbies"":{""type"":""string"",""required"":true},
    ""age"":{""type"":""integer"",""required"":true}
  }
}";

        var json = "{'name':'James'}";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("name", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("James", reader.Value.ToString());
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);
        Xunit.Assert.Equal("Required properties are missing from object: hobbies, age. Line 1, position 16.", validationEventArgs.Message);
        Xunit.Assert.Equal("", validationEventArgs.Path);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void MissingNonRequiredProperties()
    {
        var schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string"",""required"":true},
    ""hobbies"":{""type"":""string"",""required"":false},
    ""age"":{""type"":""integer""}
  }
}";

        var json = "{'name':'James'}";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("name", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("James", reader.Value.ToString());
        Xunit.Assert.Null(validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.Null(validationEventArgs);
    }

    [Fact]
    public void DisableAdditionalProperties()
    {
        var schemaJson = @"{
  ""description"":""A person"",
  ""type"":""object"",
  ""properties"":
  {
    ""name"":{""type"":""string""}
  },
  ""additionalProperties"":false
}";

        var json = "{'name':'James','additionalProperty1':null,'additionalProperty2':null}";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("name", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("James", reader.Value.ToString());
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("additionalProperty1", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);
        Xunit.Assert.Equal(null, reader.Value);
        Xunit.Assert.Equal("Property 'additionalProperty1' has not been defined and the schema does not allow additional properties. Line 1, position 38.", validationEventArgs.Message);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("additionalProperty2", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);
        Xunit.Assert.Equal(null, reader.Value);
        Xunit.Assert.Equal("Property 'additionalProperty2' has not been defined and the schema does not allow additional properties. Line 1, position 65.", validationEventArgs.Message);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    [Fact]
    public void ExtendsStringGreaterThanMaximumLength()
    {
        var schemaJson = @"{
  ""extends"":{
    ""type"":""string"",
    ""minLength"":5,
    ""maxLength"":10
  },
  ""maxLength"":9
}";

        var errors = new List<string>();
        var json = "'The quick brown fox jumps over the lazy dog.'";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) =>
        {
            validationEventArgs = args;
            errors.Add(validationEventArgs.Message);
        };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal(1, errors.Count);
        Xunit.Assert.Equal("String 'The quick brown fox jumps over the lazy dog.' exceeds maximum length of 9. Line 1, position 46.", errors[0]);

        Xunit.Assert.NotNull(validationEventArgs);
    }

    JsonSchema GetExtendedSchema()
    {
        var first = @"{
  ""id"":""first"",
  ""type"":""object"",
  ""properties"":
  {
    ""firstproperty"":{""type"":""string"",""required"":true}
  },
  ""additionalProperties"":{}
}";

        var second = @"{
  ""id"":""second"",
  ""type"":""object"",
  ""extends"":{""$ref"":""first""},
  ""properties"":
  {
    ""secondproperty"":{""type"":""string"",""required"":true}
  },
  ""additionalProperties"":false
}";

        var resolver = new JsonSchemaResolver();
        var firstSchema = JsonSchema.Parse(first, resolver);
        var secondSchema = JsonSchema.Parse(second, resolver);

        return secondSchema;
    }

    [Fact]
    public void ExtendsDisallowAdditionProperties()
    {
        var json = "{'firstproperty':'blah','secondproperty':'blah2','additional':'blah3','additional2':'blah4'}";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = GetExtendedSchema();

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("firstproperty", reader.Value.ToString());
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("blah", reader.Value.ToString());
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("secondproperty", reader.Value.ToString());
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("blah2", reader.Value.ToString());
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("additional", reader.Value.ToString());
        Xunit.Assert.Equal("Property 'additional' has not been defined and the schema does not allow additional properties. Line 1, position 62.", validationEventArgs.Message);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("blah3", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("additional2", reader.Value.ToString());
        Xunit.Assert.Equal("Property 'additional2' has not been defined and the schema does not allow additional properties. Line 1, position 84.", validationEventArgs.Message);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("blah4", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void ExtendsMissingRequiredProperties()
    {
        var json = "{}";

        var errors = new List<string>();

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { errors.Add(args.Message); };
        reader.Schema = GetExtendedSchema();

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.Equal(1, errors.Count);
        Xunit.Assert.Equal("Required properties are missing from object: secondproperty, firstproperty. Line 1, position 2.", errors[0]);
    }

    [Fact]
    public void NoAdditionalItems()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"": [{""type"":""string""},{""type"":""integer""}],
  ""additionalItems"": false
}";

        var json = @"[1, 'a', null]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal("Invalid type. Expected String but got Integer. Line 1, position 2.", validationEventArgs.Message);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("Invalid type. Expected Integer but got String. Line 1, position 7.", validationEventArgs.Message);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Null, reader.TokenType);
        Xunit.Assert.Equal("Index 3 has not been defined and the schema does not allow additional items. Line 1, position 13.", validationEventArgs.Message);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void PatternPropertiesNoAdditionalProperties()
    {
        var schemaJson = @"{
  ""type"":""object"",
  ""patternProperties"": {
     ""hi"": {""type"":""string""},
     ""ho"": {""type"":""string""}
  },
  ""additionalProperties"": false
}";

        var json = @"{
  ""hi"": ""A string!"",
  ""hide"": ""A string!"",
  ""ho"": 1,
  ""hey"": ""A string!""
}";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal("Invalid type. Expected String but got Integer. Line 4, position 9.", validationEventArgs.Message);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("Property 'hey' has not been defined and the schema does not allow additional properties. Line 5, position 8.", validationEventArgs.Message);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.False(reader.Read());
    }

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

        var json = @"{
  'firstproperty':'blahblahblahblahblahblah',
  'secondproperty':'secasecasecasecaseca',
  'thirdproperty':{
    'thirdproperty_firstproperty':'aaa',
    'additional':'three'
  }
}";

        ValidationEventArgs validationEventArgs = null;
        var errors = new List<string>();

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) =>
        {
            validationEventArgs = args;
            errors.Add(validationEventArgs.Path + " - " + validationEventArgs.Message);
        };
        reader.Schema = secondSchema;

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("firstproperty", reader.Value.ToString());
        Xunit.Assert.Equal(null, validationEventArgs);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("blahblahblahblahblahblah", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("secondproperty", reader.Value.ToString());

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("secasecasecasecaseca", reader.Value.ToString());
        Xunit.Assert.Equal(1, errors.Count);
        Xunit.Assert.Equal("secondproperty - String 'secasecasecasecaseca' exceeds maximum length of 10. Line 3, position 41.", errors[0]);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("thirdproperty", reader.Value.ToString());
        Xunit.Assert.Equal(1, errors.Count);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.StartObject, reader.TokenType);
        Xunit.Assert.Equal(1, errors.Count);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("thirdproperty_firstproperty", reader.Value.ToString());
        Xunit.Assert.Equal(1, errors.Count);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("aaa", reader.Value.ToString());
        Xunit.Assert.Equal(4, errors.Count);
        Xunit.Assert.Equal("thirdproperty.thirdproperty_firstproperty - String 'aaa' is less than minimum length of 7. Line 5, position 39.", errors[1]);
        Xunit.Assert.Equal("thirdproperty.thirdproperty_firstproperty - String 'aaa' does not match regex pattern 'hi'. Line 5, position 39.", errors[2]);
        Xunit.Assert.Equal("thirdproperty.thirdproperty_firstproperty - String 'aaa' does not match regex pattern 'hi2u'. Line 5, position 39.", errors[3]);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.PropertyName, reader.TokenType);
        Xunit.Assert.Equal("additional", reader.Value.ToString());
        Xunit.Assert.Equal(4, errors.Count);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.String, reader.TokenType);
        Xunit.Assert.Equal("three", reader.Value.ToString());
        Xunit.Assert.Equal(5, errors.Count);
        Xunit.Assert.Equal("thirdproperty.additional - String 'three' is less than minimum length of 6. Line 6, position 24.", errors[4]);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.True(reader.Read());
        Xunit.Assert.Equal(JsonToken.EndObject, reader.TokenType);

        Xunit.Assert.False(reader.Read());
    }

    [Fact]
    public void DuplicateErrorsTest()
    {
        var schema = @"{
  ""id"":""ErrorDemo.Database"",
  ""properties"":{
    ""ErrorDemoDatabase"":{
      ""type"":""object"",
      ""required"":true,
      ""properties"":{
        ""URL"":{
          ""type"":""string"",
          ""required"":true
        },
        ""Version"":{
          ""type"":""string"",
          ""required"":true
        },
        ""Date"":{
          ""type"":""string"",
          ""format"":""date-time"",
          ""required"":true
        },
        ""MACLevels"":{
          ""type"":""object"",
          ""required"":true,
          ""properties"":{
            ""MACLevel"":{
              ""type"":""array"",
              ""required"":true,
              ""items"":[
                {
                  ""required"":true,
                  ""properties"":{
                    ""IDName"":{
                      ""type"":""string"",
                      ""required"":true
                    },
                    ""Order"":{
                      ""type"":""string"",
                      ""required"":true
                    },
                    ""IDDesc"":{
                      ""type"":""string"",
                      ""required"":true
                    },
                    ""IsActive"":{
                      ""type"":""string"",
                      ""required"":true
                    }
                  }
                }
              ]
            }
          }
        }
      }
    }
  }
}";

        var json = @"{
  ""ErrorDemoDatabase"":{
    ""URL"":""localhost:3164"",
    ""Version"":""1.0"",
    ""Date"":""6.23.2010, 9:35:18.121"",
    ""MACLevels"":{
      ""MACLevel"":[
        {
          ""@IDName"":""Developer"",
          ""Order"":""0"",
          ""IDDesc"":""DeveloperDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""Technician"",
          ""Order"":""1"",
          ""IDDesc"":""TechnicianDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""Administrator"",
          ""Order"":""2"",
          ""IDDesc"":""AdministratorDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""PowerUser"",
          ""Order"":""3"",
          ""IDDesc"":""PowerUserDesc"",
          ""IsActive"":""True""
        },
        {
          ""IDName"":""Operator"",
          ""Order"":""4"",
          ""IDDesc"":""OperatorDesc"",
          ""IsActive"":""True""
        }
      ]
    }
  }
}";

        IList<ValidationEventArgs> validationEventArgs = new List<ValidationEventArgs>();

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs.Add(args); };
        reader.Schema = JsonSchema.Parse(schema);

        while (reader.Read())
        {
        }

        Xunit.Assert.Equal(1, validationEventArgs.Count);
    }

    [Fact]
    public void ReadAsBytes()
    {
        var s = new JsonSchemaGenerator().Generate(typeof(byte[]));

        var data = Encoding.UTF8.GetBytes("Hello world");

        JsonReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(@"""" + Convert.ToBase64String(data) + @"""")))
        {
            Schema = s
        };
        var bytes = reader.ReadAsBytes();

        Xunit.Assert.Equal(data, bytes);
    }

    [Fact]
    public void ReadAsInt32()
    {
        var s = new JsonSchemaGenerator().Generate(typeof(int));

        JsonReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(@"1")))
        {
            Schema = s
        };
        var i = reader.ReadAsInt32();

        Xunit.Assert.Equal(1, i);
    }

    [Fact]
    public void ReadAsInt32Failure()
    {
        ExceptionAssert.Throws<JsonSchemaException>(() =>
        {
            var s = new JsonSchemaGenerator().Generate(typeof(int));
            s.Maximum = 2;

            JsonReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(@"5")))
            {
                Schema = s
            };
            reader.ReadAsInt32();
        }, "Integer 5 exceeds maximum value of 2. Line 1, position 1.");
    }

    [Fact]
    public void ReadAsDecimal()
    {
        var s = new JsonSchemaGenerator().Generate(typeof(decimal));

        JsonReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(@"1.5")))
        {
            Schema = s
        };
        var d = reader.ReadAsDecimal();

        Xunit.Assert.Equal(1.5m, d);
    }

    [Fact]
    public void ReadAsDecimalFailure()
    {
        ExceptionAssert.Throws<JsonSchemaException>(() =>
        {
            var s = new JsonSchemaGenerator().Generate(typeof(decimal));
            s.DivisibleBy = 1;

            JsonReader reader = new JsonValidatingReader(new JsonTextReader(new StringReader(@"5.5")))
            {
                Schema = s
            };
            reader.ReadAsDecimal();
        }, "Float 5.5 is not evenly divisible by 1. Line 1, position 3.");
    }

    [Fact]
    public void ReadAsInt32FromSerializer()
    {
        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader("[1,2,3]")));
        reader.Schema = new JsonSchemaGenerator().Generate(typeof(int[]));
        var values = new JsonSerializer().Deserialize<int[]>(reader);

        Xunit.Assert.Equal(3, values.Length);
        Xunit.Assert.Equal(1, values[0]);
        Xunit.Assert.Equal(2, values[1]);
        Xunit.Assert.Equal(3, values[2]);
    }

    [Fact]
    public void ReadAsInt32InArray()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""integer""
  },
  ""maxItems"":1
}";

        var json = "[1,2]";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        reader.Read();
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        reader.ReadAsInt32();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        reader.ReadAsInt32();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        reader.ReadAsInt32();
        Xunit.Assert.Equal(JsonToken.EndArray, reader.TokenType);
        Xunit.Assert.Equal("Array item count 2 exceeds maximum count of 1. Line 1, position 5.", validationEventArgs.Message);
        Xunit.Assert.Equal("", validationEventArgs.Path);
    }

    [Fact]
    public void ReadAsInt32InArrayIncomplete()
    {
        var schemaJson = @"{
  ""type"":""array"",
  ""items"":{
    ""type"":""integer""
  },
  ""maxItems"":1
}";

        var json = "[1,2";

        ValidationEventArgs validationEventArgs = null;

        var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
        reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
        reader.Schema = JsonSchema.Parse(schemaJson);

        reader.Read();
        Xunit.Assert.Equal(JsonToken.StartArray, reader.TokenType);

        reader.ReadAsInt32();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        reader.ReadAsInt32();
        Xunit.Assert.Equal(JsonToken.Integer, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);

        reader.ReadAsInt32();
        Xunit.Assert.Equal(JsonToken.None, reader.TokenType);
        Xunit.Assert.Equal(null, validationEventArgs);
    }

    [Fact]
    public void CloseAlsoClosesUnderlyingReader()
    {
        var underlyingReader = new TestObjects.JsonReaderStubWithIsClosed();
        var validatingReader = new JsonValidatingReader(underlyingReader) { CloseInput = true };

        validatingReader.Close();

        Xunit.Assert.True(underlyingReader.IsClosed);
    }
}

#pragma warning restore 618