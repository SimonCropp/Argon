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
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Argon.Linq;
using Argon.Schema;
using ValidationEventArgs = Argon.Schema.ValidationEventArgs;

namespace Argon.Tests
{
    [TestFixture]
    public class JsonValidatingReaderTests : TestFixtureBase
    {
        [Fact]
        public void CheckInnerReader()
        {
            var json = "{'name':'James','hobbies':['pie','cake']}";
            JsonReader reader = new JsonTextReader(new StringReader(json));

            var validatingReader = new JsonValidatingReader(reader);
            Assert.AreEqual(reader, validatingReader.Reader);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            var schema = JsonSchema.Parse(schemaJson);
            reader.Schema = schema;
            Assert.AreEqual(schema, reader.Schema);
            Assert.AreEqual(0, reader.Depth);
            Assert.AreEqual("", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
            Assert.AreEqual("", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("name", reader.Value.ToString());
            Assert.AreEqual("name", reader.Path);
            Assert.AreEqual(1, reader.Depth);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("James", reader.Value.ToString());
            Assert.AreEqual(typeof(string), reader.ValueType);
            Assert.AreEqual('"', reader.QuoteChar);
            Assert.AreEqual("name", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("hobbies", reader.Value.ToString());
            Assert.AreEqual('\'', reader.QuoteChar);
            Assert.AreEqual("hobbies", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);
            Assert.AreEqual("hobbies", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("pie", reader.Value.ToString());
            Assert.AreEqual('"', reader.QuoteChar);
            Assert.AreEqual("hobbies[0]", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("cake", reader.Value.ToString());
            Assert.AreEqual("hobbies[1]", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
            Assert.AreEqual("hobbies", reader.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
            Assert.AreEqual("", reader.Path);

            Assert.IsFalse(reader.Read());

            Assert.IsNull(validationEventArgs);
        }

        [Fact]
        public void ValidateUnrestrictedArray()
        {
            var schemaJson = @"{
  ""type"":""array""
}";

            var json = "['pie','cake',['nested1','nested2'],{'nestedproperty1':1.1,'nestedproperty2':[null]}]";

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("pie", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("cake", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("nested1", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("nested2", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("nestedproperty1", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(1.1, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("nestedproperty2", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNull(validationEventArgs);
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

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("String 'pie' is less than minimum length of 5. Line 1, position 5.", validationEventArgs.Message);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("String 'The quick brown fox jumps over the lazy dog.' exceeds maximum length of 10. Line 1, position 46.", validationEventArgs.Message);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(@"Value ""THREE"" is not defined in enum. Line 1, position 20.", validationEventArgs.Message);
            Assert.AreEqual("[2]", validationEventArgs.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNotNull(validationEventArgs);
        }

        [Fact]
        public void StringDoesNotMatchPattern()
        {
            var schemaJson = @"{
  ""type"":""string"",
  ""pattern"":""foo""
}";

            var json = "'The quick brown fox jumps over the lazy dog.'";

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("String 'The quick brown fox jumps over the lazy dog.' does not match regex pattern 'foo'. Line 1, position 46.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);

            Assert.IsNotNull(validationEventArgs);
        }

        [Fact]
        public void IntegerGreaterThanMaximumValue()
        {
            var schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

            var json = "10";

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual("Integer 10 exceeds maximum value of 5. Line 1, position 2.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);

            Assert.IsNotNull(validationEventArgs);
        }

        [Fact]
        public void IntegerGreaterThanMaximumValue_BigInteger()
        {
            var schemaJson = @"{
  ""type"":""integer"",
  ""maximum"":5
}";

            var json = "99999999999999999999999999999999999999999999999999999999999999999999";

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual("Integer 99999999999999999999999999999999999999999999999999999999999999999999 exceeds maximum value of 5. Line 1, position 68.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);

            Assert.IsNotNull(validationEventArgs);
        }

        [Fact]
        public void IntegerLessThanMaximumValue_BigInteger()
        {
            var schemaJson = @"{
  ""type"":""integer"",
  ""minimum"":5
}";

            var v = new JValue(new BigInteger(1));

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            v.Validate(JsonSchema.Parse(schemaJson), (_, args) => { validationEventArgs = args; });

            Assert.IsNotNull(validationEventArgs);
            Assert.AreEqual("Integer 1 is less than minimum value of 5.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);
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

                Assert.IsTrue(reader.Read());
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual("Integer 1 is less than minimum value of 5. Line 1, position 1.", validationEventArgs.Message);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(@"Value 3 is not defined in enum. Line 1, position 6.", validationEventArgs.Message);
            Assert.AreEqual("[2]", validationEventArgs.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNotNull(validationEventArgs);
        }

        [Fact]
        public void FloatGreaterThanMaximumValue()
        {
            var schemaJson = @"{
  ""type"":""number"",
  ""maximum"":5
}";

            var json = "10.0";

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual("Float 10.0 exceeds maximum value of 5. Line 1, position 4.", validationEventArgs.Message);

            Assert.IsNotNull(validationEventArgs);
        }

        [Fact]
        public void FloatLessThanMinimumValue()
        {
            var schemaJson = @"{
  ""type"":""number"",
  ""minimum"":5
}";

            var json = "1.1";

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual("Float 1.1 is less than minimum value of 5. Line 1, position 3.", validationEventArgs.Message);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(@"Value 3.0 is not defined in enum. Line 1, position 12.", validationEventArgs.Message);
            Assert.AreEqual("[2]", validationEventArgs.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(@"Float 4.001 is not evenly divisible by 0.1. Line 1, position 14.", validationEventArgs.Message);
            Assert.AreEqual("[2]", validationEventArgs.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.IsNull(validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(@"Integer 999999999999999999999999999999999999999999999999999999999 is not evenly divisible by 2. Line 1, position 58.", validationEventArgs.Message);
            Assert.AreEqual("[0]", validationEventArgs.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.IsNotNull(validationEventArgs);
            Assert.AreEqual(@"Integer 999999999999999999999999999999999999999999999999999999999 is not evenly divisible by 1.1. Line 1, position 58.", validationEventArgs.Message);
            Assert.AreEqual("[0]", validationEventArgs.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
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

            Assert.IsNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.AreEqual(@"Value null is not defined in enum. Line 1, position 5.", validationEventArgs.Message);
            Assert.AreEqual("[0]", validationEventArgs.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);
            Assert.AreEqual(@"Value false is not defined in enum. Line 1, position 11.", validationEventArgs.Message);
            Assert.AreEqual("[1]", validationEventArgs.Path);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
            Assert.AreEqual("Array item count 4 exceeds maximum count of 3. Line 1, position 21.", validationEventArgs.Message);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
            Assert.AreEqual("Array item count 1 is less than minimum count of 2. Line 1, position 6.", validationEventArgs.Message);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);
            Assert.AreEqual(@"Invalid type. Expected String but got Array. Line 1, position 1.", validationEventArgs.Message);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Float, reader.TokenType);
            Assert.AreEqual(@"Type Float is disallowed. Line 1, position 10.", validationEventArgs.Message);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("name", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("James", reader.Value.ToString());
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
            Assert.AreEqual("Required properties are missing from object: hobbies, age. Line 1, position 16.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("name", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("James", reader.Value.ToString());
            Assert.IsNull(validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("name", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("James", reader.Value.ToString());
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("additionalProperty1", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.AreEqual(null, reader.Value);
            Assert.AreEqual("Property 'additionalProperty1' has not been defined and the schema does not allow additional properties. Line 1, position 38.", validationEventArgs.Message);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("additionalProperty2", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.AreEqual(null, reader.Value);
            Assert.AreEqual("Property 'additionalProperty2' has not been defined and the schema does not allow additional properties. Line 1, position 65.", validationEventArgs.Message);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsNotNull(validationEventArgs);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) =>
            {
                validationEventArgs = args;
                errors.Add(validationEventArgs.Message);
            };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("String 'The quick brown fox jumps over the lazy dog.' exceeds maximum length of 9. Line 1, position 46.", errors[0]);

            Assert.IsNotNull(validationEventArgs);
        }

        private JsonSchema GetExtendedSchema()
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = GetExtendedSchema();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("firstproperty", reader.Value.ToString());
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("blah", reader.Value.ToString());
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("secondproperty", reader.Value.ToString());
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("blah2", reader.Value.ToString());
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("additional", reader.Value.ToString());
            Assert.AreEqual("Property 'additional' has not been defined and the schema does not allow additional properties. Line 1, position 62.", validationEventArgs.Message);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("blah3", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("additional2", reader.Value.ToString());
            Assert.AreEqual("Property 'additional2' has not been defined and the schema does not allow additional properties. Line 1, position 84.", validationEventArgs.Message);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("blah4", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
        }

        [Fact]
        public void ExtendsMissingRequiredProperties()
        {
            var json = "{}";

            var errors = new List<string>();

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { errors.Add(args.Message); };
            reader.Schema = GetExtendedSchema();

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Required properties are missing from object: secondproperty, firstproperty. Line 1, position 2.", errors[0]);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual("Invalid type. Expected String but got Integer. Line 1, position 2.", validationEventArgs.Message);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("Invalid type. Expected Integer but got String. Line 1, position 7.", validationEventArgs.Message);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.AreEqual("Index 3 has not been defined and the schema does not allow additional items. Line 1, position 13.", validationEventArgs.Message);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual("Invalid type. Expected String but got Integer. Line 4, position 9.", validationEventArgs.Message);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("Property 'hey' has not been defined and the schema does not allow additional properties. Line 5, position 8.", validationEventArgs.Message);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;
            var errors = new List<string>();

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) =>
            {
                validationEventArgs = args;
                errors.Add(validationEventArgs.Path + " - " + validationEventArgs.Message);
            };
            reader.Schema = secondSchema;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("firstproperty", reader.Value.ToString());
            Assert.AreEqual(null, validationEventArgs);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("blahblahblahblahblahblah", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("secondproperty", reader.Value.ToString());

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("secasecasecasecaseca", reader.Value.ToString());
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("secondproperty - String 'secasecasecasecaseca' exceeds maximum length of 10. Line 3, position 41.", errors[0]);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("thirdproperty", reader.Value.ToString());
            Assert.AreEqual(1, errors.Count);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
            Assert.AreEqual(1, errors.Count);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("thirdproperty_firstproperty", reader.Value.ToString());
            Assert.AreEqual(1, errors.Count);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("aaa", reader.Value.ToString());
            Assert.AreEqual(4, errors.Count);
            Assert.AreEqual("thirdproperty.thirdproperty_firstproperty - String 'aaa' is less than minimum length of 7. Line 5, position 39.", errors[1]);
            Assert.AreEqual("thirdproperty.thirdproperty_firstproperty - String 'aaa' does not match regex pattern 'hi'. Line 5, position 39.", errors[2]);
            Assert.AreEqual("thirdproperty.thirdproperty_firstproperty - String 'aaa' does not match regex pattern 'hi2u'. Line 5, position 39.", errors[3]);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.AreEqual("additional", reader.Value.ToString());
            Assert.AreEqual(4, errors.Count);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("three", reader.Value.ToString());
            Assert.AreEqual(5, errors.Count);
            Assert.AreEqual("thirdproperty.additional - String 'three' is less than minimum length of 6. Line 6, position 24.", errors[4]);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
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

            Assert.AreEqual(1, validationEventArgs.Count);
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

            CollectionAssert.AreEquivalent(data, bytes);
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

            Assert.AreEqual(1, i);
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

            Assert.AreEqual(1.5m, d);
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

            Assert.AreEqual(3, values.Length);
            Assert.AreEqual(1, values[0]);
            Assert.AreEqual(2, values[1]);
            Assert.AreEqual(3, values[2]);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            reader.Read();
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            reader.ReadAsInt32();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            reader.ReadAsInt32();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            reader.ReadAsInt32();
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);
            Assert.AreEqual("Array item count 2 exceeds maximum count of 1. Line 1, position 5.", validationEventArgs.Message);
            Assert.AreEqual("", validationEventArgs.Path);
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

            Argon.Schema.ValidationEventArgs validationEventArgs = null;

            var reader = new JsonValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (_, args) => { validationEventArgs = args; };
            reader.Schema = JsonSchema.Parse(schemaJson);

            reader.Read();
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            reader.ReadAsInt32();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            reader.ReadAsInt32();
            Assert.AreEqual(JsonToken.Integer, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);

            reader.ReadAsInt32();
            Assert.AreEqual(JsonToken.None, reader.TokenType);
            Assert.AreEqual(null, validationEventArgs);
        }

        [Fact]
        public void CloseAlsoClosesUnderlyingReader()
        {
            var underlyingReader = new TestObjects.JsonReaderStubWithIsClosed();
            var validatingReader = new JsonValidatingReader(underlyingReader) { CloseInput = true };

            validatingReader.Close();

            Assert.IsTrue(underlyingReader.IsClosed);
        }
    }
}

#pragma warning restore 618