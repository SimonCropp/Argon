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

using System.Runtime.Serialization.Formatters;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
using Xunit;

namespace Argon.Tests.Serialization;

public class MetadataPropertyHandlingTests : TestFixtureBase
{
    public class User
    {
        public string Name { get; set; }
    }

    [Fact]
    public void Demo()
    {
        var json = @"{
	            'Name': 'James',
	            'Password': 'Password1',
	            '$type': 'Argon.Tests.Serialization.MetadataPropertyHandlingTests+User, Tests'
            }";

        var o = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            // no longer needs to be first
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        });

        var u = (User)o;

        Assert.Equal(u.Name, "James");
    }

    [Fact]
    public void DeserializeArraysWithPreserveObjectReferences()
    {
        var json = @"{
  ""$id"": ""1"",
  ""$values"": [
    null,
    {
      ""$id"": ""2"",
      ""$values"": [
        null
      ]
    },
    {
      ""$id"": ""3"",
      ""$values"": [
        {
          ""$id"": ""4"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      ]
    }
  ]
}";

        XUnitAssert.Throws<JsonSerializationException>(() =>
        {
            JsonConvert.DeserializeObject<string[][]>(json,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
                });
        }, @"Cannot preserve reference to array or readonly list, or list created from a non-default constructor: System.String[][]. Path '$values', line 3, position 14.");
    }

    [Fact]
    public void SerializeDeserialize_DictionaryContextContainsGuid_DeserializesItemAsGuid()
    {
        const string contextKey = "k1";
        var someValue = new Guid("5dd2dba0-20c0-49f8-a054-1fa3b0a8d774");

        var inputContext = new Dictionary<string, Guid> {{contextKey, someValue}};

        var jsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.All,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        };
        var serializedString = JsonConvert.SerializeObject(inputContext, jsonSerializerSettings);

        XUnitAssert.AreEqualNormalized(@"{
  ""$type"": """ + ReflectionUtils.GetTypeName(typeof(Dictionary<string, Guid>), 0, DefaultSerializationBinder.Instance) + @""",
  ""k1"": ""5dd2dba0-20c0-49f8-a054-1fa3b0a8d774""
}", serializedString);

        var deserializedObject = (Dictionary<string, Guid>)JsonConvert.DeserializeObject(serializedString, jsonSerializerSettings);

        Assert.Equal(someValue, deserializedObject[contextKey]);
    }

    [Fact]
    public void DeserializeGuid()
    {
        var expected = new Item
        {
            SourceTypeID = new Guid("d8220a4b-75b1-4b7a-8112-b7bdae956a45"),
            BrokerID = new Guid("951663c4-924e-4c86-a57a-7ed737501dbd"),
            Latitude = 33.657145,
            Longitude = -117.766684,
            TimeStamp = new DateTime(2000, 3, 1, 23, 59, 59, DateTimeKind.Utc),
            Payload = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }
        };

        var jsonString = JsonConvert.SerializeObject(expected, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"{
  ""SourceTypeID"": ""d8220a4b-75b1-4b7a-8112-b7bdae956a45"",
  ""BrokerID"": ""951663c4-924e-4c86-a57a-7ed737501dbd"",
  ""Latitude"": 33.657145,
  ""Longitude"": -117.766684,
  ""TimeStamp"": ""2000-03-01T23:59:59Z"",
  ""Payload"": {
    ""$type"": """ + ReflectionUtils.GetTypeName(typeof(byte[]), 0, DefaultSerializationBinder.Instance) + @""",
    ""$value"": ""AAECAwQFBgcICQ==""
  }
}", jsonString);

        var actual = JsonConvert.DeserializeObject<Item>(jsonString, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        });

        Assert.Equal(new Guid("d8220a4b-75b1-4b7a-8112-b7bdae956a45"), actual.SourceTypeID);
        Assert.Equal(new Guid("951663c4-924e-4c86-a57a-7ed737501dbd"), actual.BrokerID);
        var bytes = (byte[])actual.Payload;
        Assert.Equal(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, bytes);
    }

    [Fact]
    public void DeserializeListsWithPreserveObjectReferences()
    {
        var json = @"{
  ""$id"": ""1"",
  ""$values"": [
    null,
    {
      ""$id"": ""2"",
      ""$values"": [
        null
      ]
    },
    {
      ""$id"": ""3"",
      ""$values"": [
        {
          ""$id"": ""4"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      ]
    }
  ]
}";

        var circularList = JsonConvert.DeserializeObject<PreserveReferencesHandlingTests.CircularList>(json,
            new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });

        Assert.Equal(3, circularList.Count);
        Assert.Equal(null, circularList[0]);
        Assert.Equal(1, circularList[1].Count);
        Assert.Equal(1, circularList[2].Count);
        Assert.Equal(1, circularList[2][0].Count);
        Assert.True(ReferenceEquals(circularList, circularList[2][0][0]));
    }

    [Fact]
    public void DeserializeTypeNameOnly()
    {
        var json = @"{
  ""$id"": ""1"",
  ""$type"": ""Argon.Tests.TestObjects.Employee"",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

        XUnitAssert.Throws<JsonSerializationException>(() =>
        {
            JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });
        }, "Type specified in JSON 'Argon.Tests.TestObjects.Employee' was not resolved. Path '$type', line 3, position 45.");
    }

    [Fact]
    public void SerializeRefNull()
    {
        var reference = new Dictionary<string, object>
        {
            {"blah", "blah!"},
            {"$ref", null},
            {"$id", null}
        };

        var child = new Dictionary<string, object>
        {
            {"_id", 2},
            {"Name", "Isabell"},
            {"Father", reference}
        };

        var json = JsonConvert.SerializeObject(child, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"{
  ""_id"": 2,
  ""Name"": ""Isabell"",
  ""Father"": {
    ""blah"": ""blah!"",
    ""$ref"": null,
    ""$id"": null
  }
}", json);

        var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        });

        Assert.Equal(3, result.Count);
        Assert.Equal(1, ((JObject)result["Father"]).Count);
        Assert.Equal("blah!", (string)((JObject)result["Father"])["blah"]);
    }

    [Fact]
    public void DeserializeEmployeeReference()
    {
        var json = @"[
  {
    ""Name"": ""Mike Manager"",
    ""$id"": ""1"",
    ""Manager"": null
  },
  {
    ""Name"": ""Joe User"",
    ""$id"": ""2"",
    ""Manager"": {
      ""$ref"": ""1""
    }
  }
]";

        var employees = JsonConvert.DeserializeObject<List<EmployeeReference>>(json, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        });

        Assert.Equal(2, employees.Count);
        Assert.Equal("Mike Manager", employees[0].Name);
        Assert.Equal("Joe User", employees[1].Name);
        Assert.Equal(employees[0], employees[1].Manager);
    }

    [Fact]
    public void DeserializeFromJToken()
    {
        var json = @"[
  {
    ""Name"": ""Mike Manager"",
    ""$id"": ""1"",
    ""Manager"": null
  },
  {
    ""Name"": ""Joe User"",
    ""$id"": ""2"",
    ""Manager"": {
      ""$ref"": ""1""
    }
  }
]";

        var t1 = JToken.Parse(json);
        var t2 = t1.CloneToken();

        var serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        });
        var employees = t1.ToObject<List<EmployeeReference>>(serializer);

        Assert.Equal(2, employees.Count);
        Assert.Equal("Mike Manager", employees[0].Name);
        Assert.Equal("Joe User", employees[1].Name);
        Assert.Equal(employees[0], employees[1].Manager);

        Assert.True(JToken.DeepEquals(t1, t2));
    }

    [Fact]
    public void DeserializeGenericObjectListWithTypeName()
    {
        var employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;
        var personRef = typeof(Person).AssemblyQualifiedName;

        var json = @"[
  {
    ""Name"": ""Bob"",
    ""$id"": ""1"",
    ""$type"": """ + employeeRef + @""",
    ""Manager"": {
      ""$id"": ""2"",
      ""$type"": """ + employeeRef + @""",
      ""Name"": ""Frank"",
      ""Manager"": null
    }
  },
  {
    ""Name"": null,
    ""$type"": """ + personRef + @""",
    ""BirthDate"": ""\/Date(978134400000)\/"",
    ""LastModified"": ""\/Date(978134400000)\/""
  },
  ""String!"",
  -2147483648
]";

        var values = (List<object>)JsonConvert.DeserializeObject(json, typeof(List<object>), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
#pragma warning disable 618
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
#pragma warning restore 618
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        });

        Assert.Equal(4, values.Count);

        var e = (EmployeeReference)values[0];
        var p = (Person)values[1];

        Assert.Equal("Bob", e.Name);
        Assert.Equal("Frank", e.Manager.Name);

        Assert.Equal(null, p.Name);
        Assert.Equal(new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc), p.BirthDate);
        Assert.Equal(new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc), p.LastModified);

        Assert.Equal("String!", values[2]);
        Assert.Equal((long)int.MinValue, values[3]);
    }

    [Fact]
    public void WriteListTypeNameForProperty()
    {
        var listRef = ReflectionUtils.GetTypeName(typeof(List<int>), TypeNameAssemblyFormatHandling.Simple, null);

        var typeNameProperty = new TypeNameHandlingTests.TypeNameProperty
        {
            Name = "Name!",
            Value = new List<int> { 1, 2, 3, 4, 5 }
        };

        var json = JsonConvert.SerializeObject(typeNameProperty, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"{
  ""Name"": ""Name!"",
  ""Value"": {
    ""$type"": """ + listRef + @""",
    ""$values"": [
      1,
      2,
      3,
      4,
      5
    ]
  }
}", json);

        var deserialized = JsonConvert.DeserializeObject<TypeNameHandlingTests.TypeNameProperty>(json, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
        });
        Assert.Equal("Name!", deserialized.Name);
        Assert.IsType(typeof(List<int>), deserialized.Value);

        var nested = (List<int>)deserialized.Value;
        Assert.Equal(5, nested.Count);
        Assert.Equal(1, nested[0]);
        Assert.Equal(2, nested[1]);
        Assert.Equal(3, nested[2]);
        Assert.Equal(4, nested[3]);
        Assert.Equal(5, nested[4]);
    }

    public class MetadataPropertyDisabledTestClass
    {
        [JsonProperty("$id")]
        public string Id { get; set; }

        [JsonProperty("$ref")]
        public string Ref { get; set; }

        [JsonProperty("$value")]
        public string Value { get; set; }

        [JsonProperty("$values")]
        public string Values { get; set; }

        [JsonProperty("$type")]
        public string Type { get; set; }
    }

    [Fact]
    public void MetadataPropertyHandlingIgnore()
    {
        var c1 = new MetadataPropertyDisabledTestClass
        {
            Id = "Id!",
            Ref = "Ref!",
            Type = "Type!",
            Value = "Value!",
            Values = "Values!"
        };

        var json = JsonConvert.SerializeObject(c1, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"{
  ""$id"": ""Id!"",
  ""$ref"": ""Ref!"",
  ""$value"": ""Value!"",
  ""$values"": ""Values!"",
  ""$type"": ""Type!""
}", json);

        var c2 = JsonConvert.DeserializeObject<MetadataPropertyDisabledTestClass>(json, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore
        });

        Assert.Equal("Id!", c2.Id);
        Assert.Equal("Ref!", c2.Ref);
        Assert.Equal("Type!", c2.Type);
        Assert.Equal("Value!", c2.Value);
        Assert.Equal("Values!", c2.Values);
    }

    [Fact]
    public void MetadataPropertyHandlingIgnore_EmptyObject()
    {
        var json = @"{}";

        var c = JsonConvert.DeserializeObject<MetadataPropertyDisabledTestClass>(json, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore
        });

        Assert.Equal(null, c.Id);
    }

    [Fact]
    public void PrimitiveType_MetadataPropertyIgnore()
    {
        var actual = JsonConvert.DeserializeObject<Item>(@"{
  ""SourceTypeID"": ""d8220a4b-75b1-4b7a-8112-b7bdae956a45"",
  ""BrokerID"": ""951663c4-924e-4c86-a57a-7ed737501dbd"",
  ""Latitude"": 33.657145,
  ""Longitude"": -117.766684,
  ""TimeStamp"": ""2000-03-01T23:59:59Z"",
  ""Payload"": {
    ""$type"": ""System.Byte[], mscorlib"",
    ""$value"": ""AAECAwQFBgcICQ==""
  }
}",
            new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            });

        Assert.Equal(new Guid("d8220a4b-75b1-4b7a-8112-b7bdae956a45"), actual.SourceTypeID);
        Assert.Equal(new Guid("951663c4-924e-4c86-a57a-7ed737501dbd"), actual.BrokerID);
        var o = (JObject)actual.Payload;
        Assert.Equal("System.Byte[], mscorlib", (string)o["$type"]);
        Assert.Equal("AAECAwQFBgcICQ==", (string)o["$value"]);
        Assert.Equal(null, o.Parent);
    }

    [Fact]
    public void ReadAhead_JObject_NoParent()
    {
        var actual = JsonConvert.DeserializeObject<ItemWithUntypedPayload>(@"{
  ""Payload"": {}
}",
            new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });

        var o = (JObject)actual.Payload;
        Assert.Equal(null, o.Parent);
    }

    public class ItemWithJTokens
    {
        public JValue Payload1 { get; set; }
        public JObject Payload2 { get; set; }
        public JArray Payload3 { get; set; }
    }

    [Fact]
    public void ReadAhead_TypedJValue_NoParent()
    {
        var actual = (ItemWithJTokens)JsonConvert.DeserializeObject(@"{
  ""Payload1"": 1,
  ""Payload2"": {'prop1':1,'prop2':[2]},
  ""Payload3"": [1],
  ""$type"": ""Argon.Tests.Serialization.MetadataPropertyHandlingTests+ItemWithJTokens, Tests""
}",
            new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                TypeNameHandling = TypeNameHandling.All
            });

        Assert.Equal(JTokenType.Integer, actual.Payload1.Type);
        Assert.Equal(1, (int)actual.Payload1);
        Assert.Equal(null, actual.Payload1.Parent);

        Assert.Equal(JTokenType.Object, actual.Payload2.Type);
        Assert.Equal(1, (int)actual.Payload2["prop1"]);
        Assert.Equal(2, (int)actual.Payload2["prop2"][0]);
        Assert.Equal(null, actual.Payload2.Parent);

        Assert.Equal(1, (int)actual.Payload3[0]);
        Assert.Equal(null, actual.Payload3.Parent);
    }

    [Fact]
    public void ReadAhead_JArray_NoParent()
    {
        var actual = JsonConvert.DeserializeObject<ItemWithUntypedPayload>(@"{
  ""Payload"": [1]
}",
            new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            });

        var o = (JArray)actual.Payload;
        Assert.Equal(null, o.Parent);
    }

    public class ItemWithUntypedPayload
    {
        public object Payload { get; set; }
    }

    [Fact]
    public void PrimitiveType_MetadataPropertyIgnore_WithNoType()
    {
        var actual = JsonConvert.DeserializeObject<ItemWithUntypedPayload>(@"{
  ""Payload"": {
    ""$type"": ""System.Single, mscorlib"",
    ""$value"": ""5""
  }
}",
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

        Assert.Equal(5f, actual.Payload);

        actual = JsonConvert.DeserializeObject<ItemWithUntypedPayload>(@"{
  ""Payload"": {
    ""$type"": ""System.Single, mscorlib"",
    ""$value"": ""5""
  }
}",
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            });

        Assert.True(actual.Payload is JObject);
    }

    [Fact]
    public void DeserializeCircularReferencesWithConverter()
    {
        var json = @"{
  ""$id"": ""1"",
  ""$type"": ""CircularReferenceClass""
}";

        var c = new MetadataPropertyDisabledTestClass();

        JsonConvert.PopulateObject(json, c, new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore
        });

        Assert.Equal("1", c.Id);
        Assert.Equal("CircularReferenceClass", c.Type);
    }
}