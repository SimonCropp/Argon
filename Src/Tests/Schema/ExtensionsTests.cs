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
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Argon.Schema;
using Argon.Linq;
using System.IO;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
#if !NET5_0_OR_GREATER
using System.Data;

#endif

namespace Argon.Tests.Schema
{
    [TestFixture]
    public class ExtensionsTests : TestFixtureBase
    {
        [Fact]
        public void IsValid()
        {
            var schema = JsonSchema.Parse("{'type':'integer'}");
            var stringToken = JToken.FromObject("pie");
            var integerToken = JToken.FromObject(1);

            Assert.AreEqual(true, integerToken.IsValid(schema));
            Assert.AreEqual(true, integerToken.IsValid(schema, out var errorMessages));
            Assert.AreEqual(0, errorMessages.Count);

            Assert.AreEqual(false, stringToken.IsValid(schema));
            Assert.AreEqual(false, stringToken.IsValid(schema, out errorMessages));
            Assert.AreEqual(1, errorMessages.Count);
            Assert.AreEqual("Invalid type. Expected Integer but got String.", errorMessages[0]);
        }

        [Fact]
        public void ValidateWithEventHandler()
        {
            var schema = JsonSchema.Parse("{'pattern':'lol'}");
            var stringToken = JToken.FromObject("pie lol");

            var errors = new List<string>();
            stringToken.Validate(schema, (_, args) => errors.Add(args.Message));
            Assert.AreEqual(0, errors.Count);

            stringToken = JToken.FromObject("pie");

            stringToken.Validate(schema, (_, args) => errors.Add(args.Message));
            Assert.AreEqual(1, errors.Count);

            Assert.AreEqual("String 'pie' does not match regex pattern 'lol'.", errors[0]);
        }

        [Fact]
        public void ValidateWithOutEventHandlerFailure()
        {
            ExceptionAssert.Throws<JsonSchemaException>(() =>
            {
                var schema = JsonSchema.Parse("{'pattern':'lol'}");
                var stringToken = JToken.FromObject("pie");
                stringToken.Validate(schema);
            }, @"String 'pie' does not match regex pattern 'lol'.");
        }

        [Fact]
        public void ValidateWithOutEventHandlerSuccess()
        {
            var schema = JsonSchema.Parse("{'pattern':'lol'}");
            var stringToken = JToken.FromObject("pie lol");
            stringToken.Validate(schema);
        }

        [Fact]
        public void ValidateFailureWithOutLineInfoBecauseOfEndToken()
        {
            // changed in 6.0.6 to now include line info!
            var schema = JsonSchema.Parse("{'properties':{'lol':{'required':true}}}");
            var o = JObject.Parse("{}");

            var errors = new List<string>();
            o.Validate(schema, (_, args) => errors.Add(args.Message));

            Assert.AreEqual("Required properties are missing from object: lol. Line 1, position 1.", errors[0]);
            Assert.AreEqual(1, errors.Count);
        }

        [Fact]
        public void ValidateRequiredFieldsWithLineInfo()
        {
            var schema = JsonSchema.Parse("{'properties':{'lol':{'type':'string'}}}");
            var o = JObject.Parse("{'lol':1}");

            var errors = new List<string>();
            o.Validate(schema, (_, args) => errors.Add(args.Path + " - " + args.Message));

            Assert.AreEqual("lol - Invalid type. Expected String but got Integer. Line 1, position 8.", errors[0]);
            Assert.AreEqual("1", o.SelectToken("lol").ToString());
            Assert.AreEqual(1, errors.Count);
        }

        [Fact]
        public void Blog()
        {
            var schemaJson = @"
{
  ""description"": ""A person schema"",
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

            //JsonSchema schema;

            //using (JsonTextReader reader = new JsonTextReader(new StringReader(schemaJson)))
            //{
            //  JsonSchemaBuilder builder = new JsonSchemaBuilder(new JsonSchemaResolver());
            //  schema = builder.Parse(reader);
            //}

            var schema = JsonSchema.Parse(schemaJson);

            var person = JObject.Parse(@"{
        ""name"": ""James"",
        ""hobbies"": ["".NET"", ""Blogging"", ""Reading"", ""Xbox"", ""LOLCATS""]
      }");

            var valid = person.IsValid(schema);
            // true
        }

        private void GenerateSchemaAndSerializeFromType<T>(T value)
        {
            var generator = new JsonSchemaGenerator
            {
                UndefinedSchemaIdHandling = UndefinedSchemaIdHandling.UseAssemblyQualifiedName
            };
            var typeSchema = generator.Generate(typeof(T));
            var schema = typeSchema.ToString();

            var json = JsonConvert.SerializeObject(value, Formatting.Indented);
            var token = JToken.ReadFrom(new JsonTextReader(new StringReader(json)));

            var errors = new List<string>();

            token.Validate(typeSchema, (_, args) => { errors.Add(args.Message); });

            if (errors.Count > 0)
            {
                Assert.Fail("Schema generated for type '{0}' is not valid." + Environment.NewLine + string.Join(Environment.NewLine, errors.ToArray()), typeof(T));
            }
        }

        [Fact]
        public void GenerateSchemaAndSerializeFromTypeTests()
        {
            GenerateSchemaAndSerializeFromType(new List<string> { "1", "Two", "III" });
            GenerateSchemaAndSerializeFromType(new List<int> { 1 });
            GenerateSchemaAndSerializeFromType(new Version("1.2.3.4"));
            GenerateSchemaAndSerializeFromType(new Store());
            GenerateSchemaAndSerializeFromType(new Person());
            GenerateSchemaAndSerializeFromType(new PersonRaw());
            GenerateSchemaAndSerializeFromType(new CircularReferenceClass { Name = "I'm required" });
            GenerateSchemaAndSerializeFromType(new CircularReferenceWithIdClass());
            GenerateSchemaAndSerializeFromType(new ClassWithArray());
            GenerateSchemaAndSerializeFromType(new ClassWithGuid());
            GenerateSchemaAndSerializeFromType(new NullableDateTimeTestClass());
#if !NET5_0_OR_GREATER
            GenerateSchemaAndSerializeFromType(new DataSet());
#endif
            GenerateSchemaAndSerializeFromType(new object());
            GenerateSchemaAndSerializeFromType(1);
            GenerateSchemaAndSerializeFromType("Hi");
            GenerateSchemaAndSerializeFromType(new DateTime(2000, 12, 29, 23, 59, 0, DateTimeKind.Utc));
            GenerateSchemaAndSerializeFromType(TimeSpan.FromTicks(1000000));
#if !NET5_0_OR_GREATER
            GenerateSchemaAndSerializeFromType(DBNull.Value);
#endif
            GenerateSchemaAndSerializeFromType(new JsonPropertyWithHandlingValues());
        }

        [Fact]
        public void UndefinedPropertyOnNoPropertySchema()
        {
            var schema = JsonSchema.Parse(@"{
  ""description"": ""test"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""properties"": {
  }
}");

            var o = JObject.Parse("{'g':1}");

            var errors = new List<string>();
            o.Validate(schema, (_, args) => errors.Add(args.Message));

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Property 'g' has not been defined and the schema does not allow additional properties. Line 1, position 5.", errors[0]);
        }

        [Fact]
        public void ExclusiveMaximum_Int()
        {
            ExceptionAssert.Throws<JsonSchemaException>(() =>
            {
                var schema = new JsonSchema
                {
                    Maximum = 10,
                    ExclusiveMaximum = true
                };

                var v = new JValue(10);
                v.Validate(schema);
            }, "Integer 10 equals maximum value of 10 and exclusive maximum is true.");
        }

        [Fact]
        public void ExclusiveMaximum_Float()
        {
            ExceptionAssert.Throws<JsonSchemaException>(() =>
            {
                var schema = new JsonSchema
                {
                    Maximum = 10.1,
                    ExclusiveMaximum = true
                };

                var v = new JValue(10.1);
                v.Validate(schema);
            }, "Float 10.1 equals maximum value of 10.1 and exclusive maximum is true.");
        }

        [Fact]
        public void ExclusiveMinimum_Int()
        {
            ExceptionAssert.Throws<JsonSchemaException>(() =>
            {
                var schema = new JsonSchema
                {
                    Minimum = 10,
                    ExclusiveMinimum = true
                };

                var v = new JValue(10);
                v.Validate(schema);
            }, "Integer 10 equals minimum value of 10 and exclusive minimum is true.");
        }

        [Fact]
        public void ExclusiveMinimum_Float()
        {
            ExceptionAssert.Throws<JsonSchemaException>(() =>
            {
                var schema = new JsonSchema
                {
                    Minimum = 10.1,
                    ExclusiveMinimum = true
                };

                var v = new JValue(10.1);
                v.Validate(schema);
            }, "Float 10.1 equals minimum value of 10.1 and exclusive minimum is true.");
        }

        [Fact]
        public void DivisibleBy_Int()
        {
            ExceptionAssert.Throws<JsonSchemaException>(() =>
            {
                var schema = new JsonSchema
                {
                    DivisibleBy = 3
                };

                var v = new JValue(10);
                v.Validate(schema);
            }, "Integer 10 is not evenly divisible by 3.");
        }

        [Fact]
        public void DivisibleBy_Approx()
        {
            var schema = new JsonSchema
            {
                DivisibleBy = 0.01
            };

            var v = new JValue(20.49);
            v.Validate(schema);
        }

        [Fact]
        public void UniqueItems_SimpleUnique()
        {
            var schema = new JsonSchema
            {
                UniqueItems = true
            };

            var a = new JArray(1, 2, 3);
            Assert.IsTrue(a.IsValid(schema));
        }

        [Fact]
        public void UniqueItems_SimpleDuplicate()
        {
            var schema = new JsonSchema
            {
                UniqueItems = true
            };

            var a = new JArray(1, 2, 3, 2, 2);
            Assert.IsFalse(a.IsValid(schema, out var errorMessages));
            Assert.AreEqual(2, errorMessages.Count);
            Assert.AreEqual("Non-unique array item at index 3.", errorMessages[0]);
            Assert.AreEqual("Non-unique array item at index 4.", errorMessages[1]);
        }

        [Fact]
        public void UniqueItems_ComplexDuplicate()
        {
            var schema = new JsonSchema
            {
                UniqueItems = true
            };

            var a = new JArray(1, new JObject(new JProperty("value", "value!")), 3, 2, new JObject(new JProperty("value", "value!")), 4, 2, new JObject(new JProperty("value", "value!")));
            Assert.IsFalse(a.IsValid(schema, out var errorMessages));
            Assert.AreEqual(3, errorMessages.Count);
            Assert.AreEqual("Non-unique array item at index 4.", errorMessages[0]);
            Assert.AreEqual("Non-unique array item at index 6.", errorMessages[1]);
            Assert.AreEqual("Non-unique array item at index 7.", errorMessages[2]);
        }

        [Fact]
        public void UniqueItems_NestedDuplicate()
        {
            var schema = new JsonSchema
            {
                UniqueItems = true,
                Items = new List<JsonSchema>
                {
                    new JsonSchema
                    {
                        UniqueItems = true
                    }
                },
                PositionalItemsValidation = false
            };

            var a = new JArray(
                new JArray(1, 2),
                new JArray(1, 1),
                new JArray(3, 4),
                new JArray(1, 2),
                new JArray(1, 1)
                );
            Assert.IsFalse(a.IsValid(schema, out var errorMessages));
            Assert.AreEqual(4, errorMessages.Count);
            Assert.AreEqual("Non-unique array item at index 1.", errorMessages[0]);
            Assert.AreEqual("Non-unique array item at index 3.", errorMessages[1]);
            Assert.AreEqual("Non-unique array item at index 1.", errorMessages[2]);
            Assert.AreEqual("Non-unique array item at index 4.", errorMessages[3]);
        }

        [Fact]
        public void Enum_Properties()
        {
            var schema = new JsonSchema
            {
                Properties = new Dictionary<string, JsonSchema>
                {
                    {
                        "bar",
                        new JsonSchema
                        {
                            Enum = new List<JToken>
                            {
                                new JValue(1),
                                new JValue(2)
                            }
                        }
                    }
                }
            };

            var o = new JObject(
                new JProperty("bar", 1)
                );
            Assert.IsTrue(o.IsValid(schema, out var errorMessages));
            Assert.AreEqual(0, errorMessages.Count);

            o = new JObject(
                new JProperty("bar", 3)
                );
            Assert.IsFalse(o.IsValid(schema, out errorMessages));
            Assert.AreEqual(1, errorMessages.Count);
        }

        [Fact]
        public void UniqueItems_Property()
        {
            var schema = new JsonSchema
            {
                Properties = new Dictionary<string, JsonSchema>
                {
                    {
                        "bar",
                        new JsonSchema
                        {
                            UniqueItems = true
                        }
                    }
                }
            };

            var o = new JObject(
                new JProperty("bar", new JArray(1, 2, 3, 3))
                );
            Assert.IsFalse(o.IsValid(schema, out var errorMessages));
            Assert.AreEqual(1, errorMessages.Count);
        }

        [Fact]
        public void Items_Positional()
        {
            var schema = new JsonSchema
            {
                Items = new List<JsonSchema>
                {
                    new JsonSchema { Type = JsonSchemaType.Object },
                    new JsonSchema { Type = JsonSchemaType.Integer }
                },
                PositionalItemsValidation = true
            };

            var a = new JArray(new JObject(), 1);
            Assert.IsTrue(a.IsValid(schema, out var errorMessages));
            Assert.AreEqual(0, errorMessages.Count);
        }
    }
}

#pragma warning restore 618