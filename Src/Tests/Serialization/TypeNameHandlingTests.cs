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

using System.Collections.ObjectModel;
using Argon.Tests.Linq;
using System.Runtime.Serialization.Formatters;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
using Xunit;
using System.Net;

namespace Argon.Tests.Serialization;

public class TypeNameHandlingTests : TestFixtureBase
{
    [Fact]
    public void SerializeMultidimensionalByteArrayWithTypeName()
    {
        var array2dRef = ReflectionUtils.GetTypeName(typeof(byte[,]), TypeNameAssemblyFormatHandling.Simple, null);
        var array3dRef = ReflectionUtils.GetTypeName(typeof(byte[,,]), TypeNameAssemblyFormatHandling.Simple, null);

        var o = new HasMultidimensionalByteArray
        {
            Array2D = new byte[,] { { 1, 2 }, { 2, 4 }, { 3, 6 } },
            Array3D = new byte[,,] { { { 1, 2, 3}, { 4, 5, 6 } } }
        };

        var json = JsonConvert.SerializeObject(o, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        });

        var expectedJson = @"{
  ""$type"": ""Argon.Tests.TestObjects.HasMultidimensionalByteArray, Tests"",
  ""Array2D"": {
    ""$type"": """ + array2dRef + @""",
    ""$values"": [
      [
        1,
        2
      ],
      [
        2,
        4
      ],
      [
        3,
        6
      ]
    ]
  },
  ""Array3D"": {
    ""$type"": """ + array3dRef + @""",
    ""$values"": [
      [
        [
          1,
          2,
          3
        ],
        [
          4,
          5,
          6
        ]
      ]
    ]
  }
}";

        XUnitAssert.AreEqualNormalized(expectedJson, json);
    }


    [Fact]
    public void DeserializeMultidimensionalByteArrayWithTypeName()
    {
        var json = @"{
  ""$type"": ""Argon.Tests.TestObjects.HasMultidimensionalByteArray, Tests"",
  ""Array2D"": {
    ""$type"": ""System.Byte[,], mscorlib"",
    ""$values"": [
      [
        1,
        2
      ],
      [
        2,
        4
      ],
      [
        3,
        6
      ]
    ]
  },
  ""Array3D"": {
    ""$type"": ""System.Byte[,,], mscorlib"",
    ""$values"": [
      [
        [
          1,
          2,
          3
        ],
        [
          4,
          5,
          6
        ]
      ]
    ]
  }
}";
        var value = JsonConvert.DeserializeObject<HasMultidimensionalByteArray>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        Assert.Equal(1, value.Array2D[0, 0]);
        Assert.Equal(2, value.Array2D[0, 1]);
        Assert.Equal(2, value.Array2D[1, 0]);
        Assert.Equal(4, value.Array2D[1, 1]);
        Assert.Equal(3, value.Array2D[2, 0]);
        Assert.Equal(6, value.Array2D[2, 1]);

        Assert.Equal(1, value.Array3D[0, 0, 0]);
        Assert.Equal(2, value.Array3D[0, 0, 1]);
        Assert.Equal(3, value.Array3D[0, 0, 2]);
        Assert.Equal(4, value.Array3D[0, 1, 0]);
        Assert.Equal(5, value.Array3D[0, 1, 1]);
        Assert.Equal(6, value.Array3D[0, 1, 2]);
    }

    [Fact]
    public void DeserializeByteArrayWithTypeName()
    {
        var json = @"{
  ""$type"": ""Argon.Tests.TestObjects.HasByteArray, Tests"",
  ""EncryptedPassword"": {
    ""$type"": ""System.Byte[], mscorlib"",
    ""$value"": ""cGFzc3dvcmQ=""
  }
}";
        var value = JsonConvert.DeserializeObject<HasByteArray>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        Assert.Equal(Convert.FromBase64String("cGFzc3dvcmQ="), value.EncryptedPassword);
    }

    [Fact]
    public void DeserializeByteArrayWithTypeName_BadAdditionalContent()
    {
        var json = @"{
  ""$type"": ""Argon.Tests.TestObjects.HasByteArray, Tests"",
  ""EncryptedPassword"": {
    ""$type"": ""System.Byte[], mscorlib"",
    ""$value"": ""cGFzc3dvcmQ="",
    ""$value"": ""cGFzc3dvcmQ=""
  }
}";

        XUnitAssert.Throws<JsonReaderException>(() =>
        {
            JsonConvert.DeserializeObject<HasByteArray>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
        }, "Error reading bytes. Unexpected token: PropertyName. Path 'EncryptedPassword.$value', line 6, position 13.");
    }

    [Fact]
    public void DeserializeByteArrayWithTypeName_ExtraProperty()
    {
        var json = @"{
  ""$type"": ""Argon.Tests.TestObjects.HasByteArray, Tests"",
  ""EncryptedPassword"": {
    ""$type"": ""System.Byte[], mscorlib"",
    ""$value"": ""cGFzc3dvcmQ=""
  },
  ""Pie"": null
}";
        var value = JsonConvert.DeserializeObject<HasByteArray>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        Assert.NotNull(value.EncryptedPassword);
        Assert.Equal(Convert.FromBase64String("cGFzc3dvcmQ="), value.EncryptedPassword);
    }

    [Fact]
    public void SerializeValueTupleWithTypeName()
    {
        var tupleRef = ReflectionUtils.GetTypeName(typeof(ValueTuple<int, int, string>), TypeNameAssemblyFormatHandling.Simple, null);

        var t = ValueTuple.Create(1, 2, "string");

        var json = JsonConvert.SerializeObject(t, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""$type"": """ + tupleRef + @""",
  ""Item1"": 1,
  ""Item2"": 2,
  ""Item3"": ""string""
}", json);

        var t2 = (ValueTuple<int, int, string>)JsonConvert.DeserializeObject(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });

        Assert.Equal(1, t2.Item1);
        Assert.Equal(2, t2.Item2);
        Assert.Equal("string", t2.Item3);
    }

    public class KnownAutoTypes
    {
        public ICollection<string> Collection { get; set; }
        public IList<string> List { get; set; }
        public IDictionary<string, string> Dictionary { get; set; }
        public ISet<string> Set { get; set; }
        public IReadOnlyCollection<string> ReadOnlyCollection { get; set; }
        public IReadOnlyList<string> ReadOnlyList { get; set; }
        public IReadOnlyDictionary<string, string> ReadOnlyDictionary { get; set; }
    }

    [Fact]
    public void KnownAutoTypesTest()
    {
        var c = new KnownAutoTypes
        {
            Collection = new List<string> { "Collection value!" },
            List = new List<string> { "List value!" },
            Dictionary = new Dictionary<string, string>
            {
                { "Dictionary key!", "Dictionary value!" }
            },
            ReadOnlyCollection = new ReadOnlyCollection<string>(new[] { "Read Only Collection value!" }),
            ReadOnlyList = new ReadOnlyCollection<string>(new[] { "Read Only List value!" }),
            Set = new HashSet<string> { "Set value!" },
            ReadOnlyDictionary = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { "Read Only Dictionary key!", "Read Only Dictionary value!" }
            })
        };

        var json = JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""Collection"": [
    ""Collection value!""
  ],
  ""List"": [
    ""List value!""
  ],
  ""Dictionary"": {
    ""Dictionary key!"": ""Dictionary value!""
  },
  ""Set"": [
    ""Set value!""
  ],
  ""ReadOnlyCollection"": [
    ""Read Only Collection value!""
  ],
  ""ReadOnlyList"": [
    ""Read Only List value!""
  ],
  ""ReadOnlyDictionary"": {
    ""Read Only Dictionary key!"": ""Read Only Dictionary value!""
  }
}", json);
    }

    [Fact]
    public void DictionaryAuto()
    {
        var dic = new Dictionary<string, object>
        {
            { "movie", new Movie { Name = "Die Hard" } }
        };

        var json = JsonConvert.SerializeObject(dic, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""movie"": {
    ""$type"": ""Argon.Tests.TestObjects.Movie, Tests"",
    ""Name"": ""Die Hard"",
    ""Description"": null,
    ""Classification"": null,
    ""Studio"": null,
    ""ReleaseDate"": null,
    ""ReleaseCountries"": null
  }
}", json);
    }

    [Fact]
    public void KeyValuePairAuto()
    {
        var dic = new List<KeyValuePair<string, object>>
        {
            new("movie", new Movie { Name = "Die Hard" })
        };

        var json = JsonConvert.SerializeObject(dic, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

        XUnitAssert.AreEqualNormalized(@"[
  {
    ""Key"": ""movie"",
    ""Value"": {
      ""$type"": ""Argon.Tests.TestObjects.Movie, Tests"",
      ""Name"": ""Die Hard"",
      ""Description"": null,
      ""Classification"": null,
      ""Studio"": null,
      ""ReleaseDate"": null,
      ""ReleaseCountries"": null
    }
  }
]", json);
    }

    [Fact]
    public void NestedValueObjects()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 3; i++)
        {
            sb.Append(@"{""$value"":");
        }

        XUnitAssert.Throws<JsonSerializationException>(() =>
        {
            var reader = new JsonTextReader(new StringReader(sb.ToString()));
            var ser = new JsonSerializer
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Default
            };
            ser.Deserialize<sbyte>(reader);
        }, "Unexpected token when deserializing primitive value: StartObject. Path '$value', line 1, position 11.");
    }

    [Fact]
    public void SerializeRootTypeNameIfDerivedWithAuto()
    {
        var serializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        var sw = new StringWriter();
        serializer.Serialize(new JsonTextWriter(sw) { Formatting = Formatting.Indented }, new WagePerson(), typeof(Person));
        var result = sw.ToString();

        XUnitAssert.AreEqualNormalized(@"{
  ""$type"": ""Argon.Tests.TestObjects.Organization.WagePerson, Tests"",
  ""HourlyWage"": 0.0,
  ""Name"": null,
  ""BirthDate"": ""0001-01-01T00:00:00"",
  ""LastModified"": ""0001-01-01T00:00:00""
}", result);

        Assert.True(result.Contains("WagePerson"));
        using (var rd = new JsonTextReader(new StringReader(result)))
        {
            var person = serializer.Deserialize<Person>(rd);

            Assert.IsType(typeof(WagePerson), person);
        }
    }

    [Fact]
    public void SerializeRootTypeNameAutoWithJsonConvert()
    {
        var json = JsonConvert.SerializeObject(new WagePerson(), typeof(object), Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""$type"": ""Argon.Tests.TestObjects.Organization.WagePerson, Tests"",
  ""HourlyWage"": 0.0,
  ""Name"": null,
  ""BirthDate"": ""0001-01-01T00:00:00"",
  ""LastModified"": ""0001-01-01T00:00:00""
}", json);
    }

    [Fact]
    public void SerializeRootTypeNameAutoWithJsonConvert_Generic()
    {
        var json = JsonConvert.SerializeObject(new WagePerson(), typeof(object), Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""$type"": ""Argon.Tests.TestObjects.Organization.WagePerson, Tests"",
  ""HourlyWage"": 0.0,
  ""Name"": null,
  ""BirthDate"": ""0001-01-01T00:00:00"",
  ""LastModified"": ""0001-01-01T00:00:00""
}", json);
    }

    [Fact]
    public void SerializeRootTypeNameAutoWithJsonConvert_Generic2()
    {
        var json = JsonConvert.SerializeObject(new WagePerson(), typeof(object), new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

        XUnitAssert.AreEqualNormalized(@"{""$type"":""Argon.Tests.TestObjects.Organization.WagePerson, Tests"",""HourlyWage"":0.0,""Name"":null,""BirthDate"":""0001-01-01T00:00:00"",""LastModified"":""0001-01-01T00:00:00""}", json);
    }

    public class Wrapper
    {
        public IList<EmployeeReference> Array { get; set; }
        public IDictionary<string, EmployeeReference> Dictionary { get; set; }
    }

    [Fact]
    public void SerializeWrapper()
    {
        var wrapper = new Wrapper
        {
            Array = new List<EmployeeReference>
            {
                new()
            },
            Dictionary = new Dictionary<string, EmployeeReference>
            {
                { "First", new EmployeeReference() }
            }
        };

        var json = JsonConvert.SerializeObject(wrapper, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""Array"": [
    {
      ""$id"": ""1"",
      ""Name"": null,
      ""Manager"": null
    }
  ],
  ""Dictionary"": {
    ""First"": {
      ""$id"": ""2"",
      ""Name"": null,
      ""Manager"": null
    }
  }
}", json);

        var w2 = JsonConvert.DeserializeObject<Wrapper>(json);
        Assert.IsType(typeof(List<EmployeeReference>), w2.Array);
        Assert.IsType(typeof(Dictionary<string, EmployeeReference>), w2.Dictionary);
    }

    [Fact]
    public void WriteTypeNameForObjects()
    {
        var employeeRef = ReflectionUtils.GetTypeName(typeof(EmployeeReference), TypeNameAssemblyFormatHandling.Simple, null);

        var employee = new EmployeeReference();

        var json = JsonConvert.SerializeObject(employee, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": null,
  ""Manager"": null
}", json);
    }

    [Fact]
    public void DeserializeTypeName()
    {
        var employeeRef = ReflectionUtils.GetTypeName(typeof(EmployeeReference), TypeNameAssemblyFormatHandling.Simple, null);

        var json = @"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

        var employee = JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        Assert.IsType(typeof(EmployeeReference), employee);
        Assert.Equal("Name!", ((EmployeeReference)employee).Name);
    }

    [Fact]
    public void DeserializeTypeNameFromGacAssembly()
    {
        var cookieRef = ReflectionUtils.GetTypeName(typeof(Cookie), TypeNameAssemblyFormatHandling.Simple, null);

        var json = @"{
  ""$id"": ""1"",
  ""$type"": """ + cookieRef + @"""
}";

        var cookie = JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        Assert.IsType(typeof(Cookie), cookie);
    }

    [Fact]
    public void SerializeGenericObjectListWithTypeName()
    {
        var employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;
        var personRef = typeof(Person).AssemblyQualifiedName;

        var values = new List<object>
        {
            new EmployeeReference
            {
                Name = "Bob",
                Manager = new EmployeeReference { Name = "Frank" }
            },
            new Person
            {
                Department = "Department",
                BirthDate = new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2000, 12, 30, 0, 0, 0, DateTimeKind.Utc)
            },
            "String!",
            int.MinValue
        };

        var json = JsonConvert.SerializeObject(values, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
#pragma warning disable 618
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
        });

        XUnitAssert.AreEqualNormalized(@"[
  {
    ""$id"": ""1"",
    ""$type"": """ + employeeRef + @""",
    ""Name"": ""Bob"",
    ""Manager"": {
      ""$id"": ""2"",
      ""$type"": """ + employeeRef + @""",
      ""Name"": ""Frank"",
      ""Manager"": null
    }
  },
  {
    ""$type"": """ + personRef + @""",
    ""Name"": null,
    ""BirthDate"": ""2000-12-30T00:00:00Z"",
    ""LastModified"": ""2000-12-30T00:00:00Z""
  },
  ""String!"",
  -2147483648
]", json);
    }

    [Fact]
    public void DeserializeGenericObjectListWithTypeName()
    {
        var employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;
        var personRef = typeof(Person).AssemblyQualifiedName;

        var json = @"[
  {
    ""$id"": ""1"",
    ""$type"": """ + employeeRef + @""",
    ""Name"": ""Bob"",
    ""Manager"": {
      ""$id"": ""2"",
      ""$type"": """ + employeeRef + @""",
      ""Name"": ""Frank"",
      ""Manager"": null
    }
  },
  {
    ""$type"": """ + personRef + @""",
    ""Name"": null,
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
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
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
    public void DeserializeWithBadTypeName()
    {
        var employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;
        var personRef = typeof(Person).AssemblyQualifiedName;

        var json = @"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

        try
        {
            JsonConvert.DeserializeObject(json, typeof(Person), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
            });
        }
        catch (JsonSerializationException ex)
        {
            Assert.True(ex.Message.StartsWith(@"Type specified in JSON '" + employeeRef + @"' is not compatible with '" + personRef + @"'."));
        }
    }

    [Fact]
    public void DeserializeTypeNameWithNoTypeNameHandling()
    {
        var employeeRef = typeof(EmployeeReference).AssemblyQualifiedName;

        var json = @"{
  ""$id"": ""1"",
  ""$type"": """ + employeeRef + @""",
  ""Name"": ""Name!"",
  ""Manager"": null
}";

        var o = (JObject)JsonConvert.DeserializeObject(json);

        XUnitAssert.AreEqualNormalized(@"{
  ""Name"": ""Name!"",
  ""Manager"": null
}", o.ToString());
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
                TypeNameHandling = TypeNameHandling.Objects
            });
        }, "Type specified in JSON 'Argon.Tests.TestObjects.Employee' was not resolved. Path '$type', line 3, position 45.");
    }

    public interface ICorrelatedMessage
    {
        string CorrelationId { get; set; }
    }

    public class SendHttpRequest : ICorrelatedMessage
    {
        public SendHttpRequest()
        {
            RequestEncoding = "UTF-8";
            Method = "GET";
        }

        public string Method { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Url { get; set; }
        public Dictionary<string, string> RequestData;
        public string RequestBodyText { get; set; }
        public string User { get; set; }
        public string Passwd { get; set; }
        public string RequestEncoding { get; set; }
        public string CorrelationId { get; set; }
    }

    [Fact]
    public void DeserializeGenericTypeName()
    {
        var typeName = typeof(SendHttpRequest).AssemblyQualifiedName;

        var json = @"{
""$type"": """ + typeName + @""",
""RequestData"": {
""$type"": ""System.Collections.Generic.Dictionary`2[[System.String, mscorlib,Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"",
""Id"": ""siedemnaście"",
""X"": ""323""
},
""Method"": ""GET"",
""Url"": ""http://www.onet.pl"",
""RequestEncoding"": ""UTF-8"",
""CorrelationId"": ""xyz""
}";

        var message = JsonConvert.DeserializeObject<ICorrelatedMessage>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
#pragma warning disable 618
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
        });

        Assert.IsType(typeof(SendHttpRequest), message);

        var request = (SendHttpRequest)message;
        Assert.Equal("xyz", request.CorrelationId);
        Assert.Equal(2, request.RequestData.Count);
        Assert.Equal("siedemnaście", request.RequestData["Id"]);
    }

    [Fact]
    public void SerializeObjectWithMultipleGenericLists()
    {
        var containerTypeName = typeof(Container).AssemblyQualifiedName;
        var productListTypeName = typeof(List<Product>).AssemblyQualifiedName;

        var container = new Container
        {
            In = new List<Product>(),
            Out = new List<Product>()
        };

        var json = JsonConvert.SerializeObject(container, Formatting.Indented,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
            });

        XUnitAssert.AreEqualNormalized(@"{
  ""$type"": """ + containerTypeName + @""",
  ""In"": {
    ""$type"": """ + productListTypeName + @""",
    ""$values"": []
  },
  ""Out"": {
    ""$type"": """ + productListTypeName + @""",
    ""$values"": []
  }
}", json);
    }

    public class TypeNameProperty
    {
        public string Name { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object Value { get; set; }
    }

    [Fact]
    public void WriteObjectTypeNameForProperty()
    {
        var typeNamePropertyRef = ReflectionUtils.GetTypeName(typeof(TypeNameProperty), TypeNameAssemblyFormatHandling.Simple, null);

        var typeNameProperty = new TypeNameProperty
        {
            Name = "Name!",
            Value = new TypeNameProperty
            {
                Name = "Nested!"
            }
        };

        var json = JsonConvert.SerializeObject(typeNameProperty, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"{
  ""Name"": ""Name!"",
  ""Value"": {
    ""$type"": """ + typeNamePropertyRef + @""",
    ""Name"": ""Nested!"",
    ""Value"": null
  }
}", json);

        var deserialized = JsonConvert.DeserializeObject<TypeNameProperty>(json);
        Assert.Equal("Name!", deserialized.Name);
        Assert.IsType(typeof(TypeNameProperty), deserialized.Value);

        var nested = (TypeNameProperty)deserialized.Value;
        Assert.Equal("Nested!", nested.Name);
        Assert.Equal(null, nested.Value);
    }

    [Fact]
    public void WriteListTypeNameForProperty()
    {
        var listRef = ReflectionUtils.GetTypeName(typeof(List<int>), TypeNameAssemblyFormatHandling.Simple, null);

        var typeNameProperty = new TypeNameProperty
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

        var deserialized = JsonConvert.DeserializeObject<TypeNameProperty>(json);
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

    [Fact]
    public void DeserializeUsingCustomBinder()
    {
        var json = @"{
  ""$id"": ""1"",
  ""$type"": ""Argon.Tests.TestObjects.Employee"",
  ""Name"": ""Name!""
}";

        var p = JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
#pragma warning disable CS0618 // Type or member is obsolete
            Binder = new CustomSerializationBinder()
#pragma warning restore CS0618 // Type or member is obsolete
        });

        Assert.IsType(typeof(Person), p);

        var person = (Person)p;

        Assert.Equal("Name!", person.Name);
    }

    public class CustomSerializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return typeof(Person);
        }
    }

    [Fact]
    public void SerializeUsingCustomBinder()
    {
        var binder = new TypeNameSerializationBinder("Argon.Tests.Serialization.{0}, Tests");

        IList<object> values = new List<object>
        {
            new Customer
            {
                Name = "Caroline Customer"
            },
            new Purchase
            {
                ProductName = "Elbow Grease",
                Price = 5.99m,
                Quantity = 1
            }
        };

        var json = JsonConvert.SerializeObject(values, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
#pragma warning disable CS0618 // Type or member is obsolete
            Binder = binder
#pragma warning restore CS0618 // Type or member is obsolete
        });

        //[
        //  {
        //    "$type": "Customer",
        //    "Name": "Caroline Customer"
        //  },
        //  {
        //    "$type": "Purchase",
        //    "ProductName": "Elbow Grease",
        //    "Price": 5.99,
        //    "Quantity": 1
        //  }
        //]

        XUnitAssert.AreEqualNormalized(@"[
  {
    ""$type"": ""Customer"",
    ""Name"": ""Caroline Customer""
  },
  {
    ""$type"": ""Purchase"",
    ""ProductName"": ""Elbow Grease"",
    ""Price"": 5.99,
    ""Quantity"": 1
  }
]", json);

        var newValues = JsonConvert.DeserializeObject<IList<object>>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
#pragma warning disable CS0618 // Type or member is obsolete
            Binder = new TypeNameSerializationBinder("Argon.Tests.Serialization.{0}, Tests")
#pragma warning restore CS0618 // Type or member is obsolete
        });

        Assert.IsType(typeof(Customer), newValues[0]);
        var customer = (Customer)newValues[0];
        Assert.Equal("Caroline Customer", customer.Name);

        Assert.IsType(typeof(Purchase), newValues[1]);
        var purchase = (Purchase)newValues[1];
        Assert.Equal("Elbow Grease", purchase.ProductName);
    }

    public class TypeNameSerializationBinder : SerializationBinder
    {
        public string TypeFormat { get; private set; }

        public TypeNameSerializationBinder(string typeFormat)
        {
            TypeFormat = typeFormat;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            var resolvedTypeName = string.Format(TypeFormat, typeName);

            return Type.GetType(resolvedTypeName, true);
        }
    }

    [Fact]
    public void NewSerializeUsingCustomBinder()
    {
        var binder = new NewTypeNameSerializationBinder("Argon.Tests.Serialization.{0}, Tests");

        IList<object> values = new List<object>
        {
            new Customer
            {
                Name = "Caroline Customer"
            },
            new Purchase
            {
                ProductName = "Elbow Grease",
                Price = 5.99m,
                Quantity = 1
            }
        };

        var json = JsonConvert.SerializeObject(values, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = binder
        });

        //[
        //  {
        //    "$type": "Customer",
        //    "Name": "Caroline Customer"
        //  },
        //  {
        //    "$type": "Purchase",
        //    "ProductName": "Elbow Grease",
        //    "Price": 5.99,
        //    "Quantity": 1
        //  }
        //]

        XUnitAssert.AreEqualNormalized(@"[
  {
    ""$type"": ""Customer"",
    ""Name"": ""Caroline Customer""
  },
  {
    ""$type"": ""Purchase"",
    ""ProductName"": ""Elbow Grease"",
    ""Price"": 5.99,
    ""Quantity"": 1
  }
]", json);

        var newValues = JsonConvert.DeserializeObject<IList<object>>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = new NewTypeNameSerializationBinder("Argon.Tests.Serialization.{0}, Tests")
        });

        Assert.IsType(typeof(Customer), newValues[0]);
        var customer = (Customer)newValues[0];
        Assert.Equal("Caroline Customer", customer.Name);

        Assert.IsType(typeof(Purchase), newValues[1]);
        var purchase = (Purchase)newValues[1];
        Assert.Equal("Elbow Grease", purchase.ProductName);
    }

    public class NewTypeNameSerializationBinder : ISerializationBinder
    {
        public string TypeFormat { get; private set; }

        public NewTypeNameSerializationBinder(string typeFormat)
        {
            TypeFormat = typeFormat;
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            var resolvedTypeName = string.Format(TypeFormat, typeName);

            return Type.GetType(resolvedTypeName, true);
        }
    }

    [Fact]
    public void CollectionWithAbstractItems()
    {
        var testObject = new HolderClass
        {
            TestMember = new ContentSubClass("First One"),
            AnotherTestMember = new Dictionary<int, IList<ContentBaseClass>> {{1, new List<ContentBaseClass>()}}
        };
        testObject.AnotherTestMember[1].Add(new ContentSubClass("Second One"));
        testObject.AThirdTestMember = new ContentSubClass("Third One");

        var serializingTester = new JsonSerializer
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var sw = new StringWriter();
        using (var jsonWriter = new JsonTextWriter(sw))
        {
            jsonWriter.Formatting = Formatting.Indented;
            serializingTester.TypeNameHandling = TypeNameHandling.Auto;
            serializingTester.Serialize(jsonWriter, testObject);
        }

        var json = sw.ToString();

        var contentSubClassRef = ReflectionUtils.GetTypeName(typeof(ContentSubClass), TypeNameAssemblyFormatHandling.Simple, null);
        var dictionaryRef = ReflectionUtils.GetTypeName(typeof(Dictionary<int, IList<ContentBaseClass>>), TypeNameAssemblyFormatHandling.Simple, null);
        var listRef = ReflectionUtils.GetTypeName(typeof(List<ContentBaseClass>), TypeNameAssemblyFormatHandling.Simple, null);

        var expected = @"{
  ""TestMember"": {
    ""$type"": """ + contentSubClassRef + @""",
    ""SomeString"": ""First One""
  },
  ""AnotherTestMember"": {
    ""$type"": """ + dictionaryRef + @""",
    ""1"": [
      {
        ""$type"": """ + contentSubClassRef + @""",
        ""SomeString"": ""Second One""
      }
    ]
  },
  ""AThirdTestMember"": {
    ""$type"": """ + contentSubClassRef + @""",
    ""SomeString"": ""Third One""
  }
}";

        XUnitAssert.AreEqualNormalized(expected, json);

        var sr = new StringReader(json);

        var deserializingTester = new JsonSerializer();

        HolderClass anotherTestObject;

        using (var jsonReader = new JsonTextReader(sr))
        {
            deserializingTester.TypeNameHandling = TypeNameHandling.Auto;

            anotherTestObject = deserializingTester.Deserialize<HolderClass>(jsonReader);
        }

        Assert.NotNull(anotherTestObject);
        Assert.IsType(typeof(ContentSubClass), anotherTestObject.TestMember);
        Assert.IsType(typeof(Dictionary<int, IList<ContentBaseClass>>), anotherTestObject.AnotherTestMember);
        Assert.Equal(1, anotherTestObject.AnotherTestMember.Count);

        var list = anotherTestObject.AnotherTestMember[1];

        Assert.IsType(typeof(List<ContentBaseClass>), list);
        Assert.Equal(1, list.Count);
        Assert.IsType(typeof(ContentSubClass), list[0]);
    }

    [Fact]
    public void WriteObjectTypeNameForPropertyDemo()
    {
        var message = new Message
        {
            Address = "http://www.google.com",
            Body = new SearchDetails
            {
                Query = "Json.NET",
                Language = "en-us"
            }
        };

        var json = JsonConvert.SerializeObject(message, Formatting.Indented);
        // {
        //   "Address": "http://www.google.com",
        //   "Body": {
        //     "$type": "Argon.Tests.Serialization.SearchDetails, Tests",
        //     "Query": "Json.NET",
        //     "Language": "en-us"
        //   }
        // }

        var deserialized = JsonConvert.DeserializeObject<Message>(json);

        var searchDetails = (SearchDetails)deserialized.Body;
        // Json.NET
    }

    public class UrlStatus
    {
        public int Status { get; set; }
        public string Url { get; set; }
    }

    [Fact]
    public void GenericDictionaryObject()
    {
        var collection = new Dictionary<string, object>
        {
            { "First", new UrlStatus { Status = 404, Url = @"http://www.bing.com" } },
            { "Second", new UrlStatus { Status = 400, Url = @"http://www.google.com" } },
            {
                "List", new List<UrlStatus>
                {
                    new() { Status = 300, Url = @"http://www.yahoo.com" },
                    new() { Status = 200, Url = @"http://www.askjeeves.com" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(collection, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
#pragma warning restore 618
        });

        var dictionaryTypeName = ReflectionUtils.GetTypeName(typeof(Dictionary<string, object>), TypeNameAssemblyFormatHandling.Simple, null);
        var urlStatusTypeName = ReflectionUtils.GetTypeName(typeof(UrlStatus), TypeNameAssemblyFormatHandling.Simple, null);
        var listTypeName = ReflectionUtils.GetTypeName(typeof(List<UrlStatus>), TypeNameAssemblyFormatHandling.Simple, null);

        XUnitAssert.AreEqualNormalized(@"{
  ""$type"": """ + dictionaryTypeName + @""",
  ""First"": {
    ""$type"": """ + urlStatusTypeName + @""",
    ""Status"": 404,
    ""Url"": ""http://www.bing.com""
  },
  ""Second"": {
    ""$type"": """ + urlStatusTypeName + @""",
    ""Status"": 400,
    ""Url"": ""http://www.google.com""
  },
  ""List"": {
    ""$type"": """ + listTypeName + @""",
    ""$values"": [
      {
        ""$type"": """ + urlStatusTypeName + @""",
        ""Status"": 300,
        ""Url"": ""http://www.yahoo.com""
      },
      {
        ""$type"": """ + urlStatusTypeName + @""",
        ""Status"": 200,
        ""Url"": ""http://www.askjeeves.com""
      }
    ]
  }
}", json);

        var c = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
#pragma warning restore 618
        });

        Assert.IsType(typeof(Dictionary<string, object>), c);

        var newCollection = (Dictionary<string, object>)c;
        Assert.Equal(3, newCollection.Count);
        Assert.Equal(@"http://www.bing.com", ((UrlStatus)newCollection["First"]).Url);

        var statues = (List<UrlStatus>)newCollection["List"];
        Assert.Equal(2, statues.Count);
    }

    [Fact]
    public void SerializingIEnumerableOfTShouldRetainGenericTypeInfo()
    {
        var productClassRef = ReflectionUtils.GetTypeName(typeof(CustomEnumerable<Product>), TypeNameAssemblyFormatHandling.Simple, null);

        var products = new CustomEnumerable<Product>();

        var json = JsonConvert.SerializeObject(products, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

        XUnitAssert.AreEqualNormalized(@"{
  ""$type"": """ + productClassRef + @""",
  ""$values"": []
}", json);
    }

    public class CustomEnumerable<T> : IEnumerable<T>
    {
        //NOTE: a simple linked list
        readonly T value;
        readonly CustomEnumerable<T> next;
        readonly int count;

        CustomEnumerable(T value, CustomEnumerable<T> next)
        {
            this.value = value;
            this.next = next;
            count = this.next.count + 1;
        }

        public CustomEnumerable()
        {
            count = 0;
        }

        public CustomEnumerable<T> AddFirst(T newVal)
        {
            return new CustomEnumerable<T>(newVal, this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (count == 0) // last node
            {
                yield break;
            }
            yield return value;

            var nextInLine = next;
            while (nextInLine != null)
            {
                if (nextInLine.count != 0)
                {
                    yield return nextInLine.value;
                }
                nextInLine = nextInLine.next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Car
    {
        // included in JSON
        public string Model { get; set; }
        public DateTime Year { get; set; }
        public List<string> Features { get; set; }
        public object[] Objects { get; set; }

        // ignored
        [JsonIgnore]
        public DateTime LastModified { get; set; }
    }

    [Fact]
    public void ByteArrays()
    {
        var testerObject = new Car
        {
            Year = new DateTime(2000, 10, 5, 1, 1, 1, DateTimeKind.Utc)
        };
        var data = new byte[] { 75, 65, 82, 73, 82, 65 };
        testerObject.Objects = new object[] { data, "prueba" };

        var jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All
        };

        var output = JsonConvert.SerializeObject(testerObject, Formatting.Indented, jsonSettings);

        var carClassRef = ReflectionUtils.GetTypeName(typeof(Car), TypeNameAssemblyFormatHandling.Simple, null);
        var objectArrayRef = ReflectionUtils.GetTypeName(typeof(object[]), TypeNameAssemblyFormatHandling.Simple, null);
        var byteArrayRef = ReflectionUtils.GetTypeName(typeof(byte[]), TypeNameAssemblyFormatHandling.Simple, null);

        XUnitAssert.AreEqualNormalized(output, @"{
  ""$type"": """ + carClassRef + @""",
  ""Year"": ""2000-10-05T01:01:01Z"",
  ""Objects"": {
    ""$type"": """ + objectArrayRef + @""",
    ""$values"": [
      {
        ""$type"": """ + byteArrayRef + @""",
        ""$value"": ""S0FSSVJB""
      },
      ""prueba""
    ]
  }
}");
        var obj = JsonConvert.DeserializeObject<Car>(output, jsonSettings);

        Assert.NotNull(obj);

        Assert.True(obj.Objects[0] is byte[]);

        var d = (byte[])obj.Objects[0];
        Assert.Equal(data, d);
    }

    [Fact]
    public void ISerializableTypeNameHandlingTest()
    {
        //Create an instance of our example type
        IExample e = new Example("Rob");

        var w = new SerializableWrapper
        {
            Content = e
        };

        //Test Binary Serialization Round Trip
        //This will work find because the Binary Formatter serializes type names
        //this.TestBinarySerializationRoundTrip(e);

        //Test Json Serialization
        //This fails because the JsonSerializer doesn't serialize type names correctly for ISerializable objects
        //Type Names should be serialized for All, Auto and Object modes
        TestJsonSerializationRoundTrip(w, TypeNameHandling.All);
        TestJsonSerializationRoundTrip(w, TypeNameHandling.Auto);
        TestJsonSerializationRoundTrip(w, TypeNameHandling.Objects);
    }

    void TestJsonSerializationRoundTrip(SerializableWrapper e, TypeNameHandling flag)
    {
        var writer = new StringWriter();

        //Create our serializer and set Type Name Handling appropriately
        var serializer = new JsonSerializer
        {
            TypeNameHandling = flag
        };

        //Do the actual serialization and dump to Console for inspection
        serializer.Serialize(new JsonTextWriter(writer), e);

        //Now try to deserialize
        //Json.Net will cause an error here as it will try and instantiate
        //the interface directly because it failed to respect the
        //TypeNameHandling property on serialization
        var f = serializer.Deserialize<SerializableWrapper>(new JsonTextReader(new StringReader(writer.ToString())));

        //Check Round Trip
        Assert.Equal(e, f);
    }

    [Fact]
    public void SerializationBinderWithFullName()
    {
        var message = new Message
        {
            Address = "jamesnk@testtown.com",
            Body = new Version(1, 2, 3, 4)
        };

        var json = JsonConvert.SerializeObject(message, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
#pragma warning disable CS0618 // Type or member is obsolete
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Full,
            Binder = new MetroBinder(),
#pragma warning restore CS0618 // Type or member is obsolete
            ContractResolver = new DefaultContractResolver
            {
                IgnoreSerializableAttribute = true
            }
        });

        var o = JObject.Parse(json);

        Assert.Equal(":::MESSAGE:::, AssemblyName", (string)o["$type"]);
    }

    public class MetroBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return null;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = "AssemblyName";
#if !NET5_0_OR_GREATER
                typeName = ":::" + serializedType.Name.ToUpper(CultureInfo.InvariantCulture) + ":::";
#else
            typeName = ":::" + serializedType.Name.ToUpper() + ":::";
#endif
        }
    }

    [Fact]
    public void TypeNameIntList()
    {
        var l = new TypeNameList<int>
        {
            1,
            2,
            3
        };

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  1,
  2,
  3
]", json);
    }

    [Fact]
    public void TypeNameComponentList()
    {
        var c1 = new TestComponentSimple();

        var l = new TypeNameList<object>
        {
            c1,
            new Employee
            {
                BirthDate = new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc),
                Department = "Department!"
            },
            "String!",
            long.MaxValue
        };

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"[
  {
    ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
    ""MyProperty"": 0
  },
  {
    ""$type"": ""Argon.Tests.TestObjects.Organization.Employee, Tests"",
    ""FirstName"": null,
    ""LastName"": null,
    ""BirthDate"": ""2000-12-12T12:12:12Z"",
    ""Department"": ""Department!"",
    ""JobTitle"": null
  },
  ""String!"",
  9223372036854775807
]", json);

        var l2 = JsonConvert.DeserializeObject<TypeNameList<object>>(json);
        Assert.Equal(4, l2.Count);

        Assert.IsType(typeof(TestComponentSimple), l2[0]);
        Assert.IsType(typeof(Employee), l2[1]);
        Assert.IsType(typeof(string), l2[2]);
        Assert.IsType(typeof(long), l2[3]);
    }

    [Fact]
    public void TypeNameDictionary()
    {
        var l = new TypeNameDictionary<object>
        {
            {"First", new TestComponentSimple {MyProperty = 1}},
            {"Second", "String!"},
            {"Third", long.MaxValue}
        };

        var json = JsonConvert.SerializeObject(l, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""First"": {
    ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
    ""MyProperty"": 1
  },
  ""Second"": ""String!"",
  ""Third"": 9223372036854775807
}", json);

        var l2 = JsonConvert.DeserializeObject<TypeNameDictionary<object>>(json);
        Assert.Equal(3, l2.Count);

        Assert.IsType(typeof(TestComponentSimple), l2["First"]);
        Assert.Equal(1, ((TestComponentSimple)l2["First"]).MyProperty);
        Assert.IsType(typeof(string), l2["Second"]);
        Assert.IsType(typeof(long), l2["Third"]);
    }

    [Fact]
    public void TypeNameObjectItems()
    {
        var o1 = new TypeNameObject
        {
            Object1 = new TestComponentSimple { MyProperty = 1 },
            Object2 = 123,
            ObjectNotHandled = new TestComponentSimple { MyProperty = int.MaxValue },
            String = "String!",
            Integer = int.MaxValue
        };

        var json = JsonConvert.SerializeObject(o1, Formatting.Indented);
        var expected = @"{
  ""Object1"": {
    ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
    ""MyProperty"": 1
  },
  ""Object2"": 123,
  ""ObjectNotHandled"": {
    ""MyProperty"": 2147483647
  },
  ""String"": ""String!"",
  ""Integer"": 2147483647
}";
        XUnitAssert.AreEqualNormalized(expected, json);

        var o2 = JsonConvert.DeserializeObject<TypeNameObject>(json);
        Assert.NotNull(o2);

        Assert.IsType(typeof(TestComponentSimple), o2.Object1);
        Assert.Equal(1, ((TestComponentSimple)o2.Object1).MyProperty);
        Assert.IsType(typeof(long), o2.Object2);
        Assert.IsType(typeof(JObject), o2.ObjectNotHandled);
        XUnitAssert.AreEqualNormalized(@"{
  ""MyProperty"": 2147483647
}", o2.ObjectNotHandled.ToString());
    }

    [Fact]
    public void PropertyItemTypeNameHandling()
    {
        var c1 = new PropertyItemTypeNameHandling
        {
            Data = new List<object>
            {
                1,
                "two",
                new TestComponentSimple { MyProperty = 1 }
            }
        };

        var json = JsonConvert.SerializeObject(c1, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""Data"": [
    1,
    ""two"",
    {
      ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
      ""MyProperty"": 1
    }
  ]
}", json);

        var c2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandling>(json);
        Assert.Equal(3, c2.Data.Count);

        Assert.IsType(typeof(long), c2.Data[0]);
        Assert.IsType(typeof(string), c2.Data[1]);
        Assert.IsType(typeof(TestComponentSimple), c2.Data[2]);
        var c = (TestComponentSimple)c2.Data[2];
        Assert.Equal(1, c.MyProperty);
    }

    [Fact]
    public void PropertyItemTypeNameHandlingNestedCollections()
    {
        var c1 = new PropertyItemTypeNameHandling
        {
            Data = new List<object>
            {
                new TestComponentSimple { MyProperty = 1 },
                new List<object>
                {
                    new List<object>
                    {
                        new List<object>()
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(c1, Formatting.Indented);

        var listTypeName = ReflectionUtils.GetTypeName(typeof(List<object>), TypeNameAssemblyFormatHandling.Simple, null);

        XUnitAssert.AreEqualNormalized(@"{
  ""Data"": [
    {
      ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
      ""MyProperty"": 1
    },
    {
      ""$type"": """+ listTypeName + @""",
      ""$values"": [
        [
          []
        ]
      ]
    }
  ]
}", json);

        var c2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandling>(json);
        Assert.Equal(2, c2.Data.Count);

        Assert.IsType(typeof(TestComponentSimple), c2.Data[0]);
        Assert.IsType(typeof(List<object>), c2.Data[1]);
        var c = (List<object>)c2.Data[1];
        Assert.IsType(typeof(JArray), c[0]);

        json = @"{
  ""Data"": [
    {
      ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
      ""MyProperty"": 1
    },
    {
      ""$type"": """ + listTypeName + @""",
      ""$values"": [
        {
          ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
          ""MyProperty"": 1
        }
      ]
    }
  ]
}";

        c2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandling>(json);
        Assert.Equal(2, c2.Data.Count);

        Assert.IsType(typeof(TestComponentSimple), c2.Data[0]);
        Assert.IsType(typeof(List<object>), c2.Data[1]);
        c = (List<object>)c2.Data[1];
        Assert.IsType(typeof(JObject), c[0]);
        var o = (JObject)c[0];
        Assert.Equal(1, (int)o["MyProperty"]);
    }

    [Fact]
    public void PropertyItemTypeNameHandlingNestedDictionaries()
    {
        var c1 = new PropertyItemTypeNameHandlingDictionary
        {
            Data = new Dictionary<string, object>
            {
                {
                    "one", new TestComponentSimple { MyProperty = 1 }
                },
                {
                    "two", new Dictionary<string, object>
                    {
                        {
                            "one", new Dictionary<string, object>
                            {
                                { "one", 1 }
                            }
                        }
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(c1, Formatting.Indented);

        var dictionaryTypeName = ReflectionUtils.GetTypeName(typeof(Dictionary<string, object>), TypeNameAssemblyFormatHandling.Simple, null);

        XUnitAssert.AreEqualNormalized(@"{
  ""Data"": {
    ""one"": {
      ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
      ""MyProperty"": 1
    },
    ""two"": {
      ""$type"": """ + dictionaryTypeName + @""",
      ""one"": {
        ""one"": 1
      }
    }
  }
}", json);

        var c2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandlingDictionary>(json);
        Assert.Equal(2, c2.Data.Count);

        Assert.IsType(typeof(TestComponentSimple), c2.Data["one"]);
        Assert.IsType(typeof(Dictionary<string, object>), c2.Data["two"]);
        var c = (Dictionary<string, object>)c2.Data["two"];
        Assert.IsType(typeof(JObject), c["one"]);

        json = @"{
  ""Data"": {
    ""one"": {
      ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
      ""MyProperty"": 1
    },
    ""two"": {
      ""$type"": """ + dictionaryTypeName + @""",
      ""one"": {
        ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
        ""MyProperty"": 1
      }
    }
  }
}";

        c2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandlingDictionary>(json);
        Assert.Equal(2, c2.Data.Count);

        Assert.IsType(typeof(TestComponentSimple), c2.Data["one"]);
        Assert.IsType(typeof(Dictionary<string, object>), c2.Data["two"]);
        c = (Dictionary<string, object>)c2.Data["two"];
        Assert.IsType(typeof(JObject), c["one"]);

        var o = (JObject)c["one"];
        Assert.Equal(1, (int)o["MyProperty"]);
    }

    [Fact]
    public void PropertyItemTypeNameHandlingObject()
    {
        var o1 = new PropertyItemTypeNameHandlingObject
        {
            Data = new TypeNameHandlingTestObject
            {
                Prop1 = new List<object>
                {
                    new TestComponentSimple
                    {
                        MyProperty = 1
                    }
                },
                Prop2 = new TestComponentSimple
                {
                    MyProperty = 1
                },
                Prop3 = 3,
                Prop4 = new JObject()
            }
        };

        var json = JsonConvert.SerializeObject(o1, Formatting.Indented);

        var listTypeName = ReflectionUtils.GetTypeName(typeof(List<object>), TypeNameAssemblyFormatHandling.Simple, null);

        XUnitAssert.AreEqualNormalized(@"{
  ""Data"": {
    ""Prop1"": {
      ""$type"": """ + listTypeName + @""",
      ""$values"": [
        {
          ""MyProperty"": 1
        }
      ]
    },
    ""Prop2"": {
      ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
      ""MyProperty"": 1
    },
    ""Prop3"": 3,
    ""Prop4"": {}
  }
}", json);

        var o2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandlingObject>(json);
        Assert.NotNull(o2);
        Assert.NotNull(o2.Data);

        Assert.IsType(typeof(List<object>), o2.Data.Prop1);
        Assert.IsType(typeof(TestComponentSimple), o2.Data.Prop2);
        Assert.IsType(typeof(long), o2.Data.Prop3);
        Assert.IsType(typeof(JObject), o2.Data.Prop4);

        var o = (List<object>)o2.Data.Prop1;
        var j = (JObject)o[0];
        Assert.Equal(1, (int)j["MyProperty"]);
    }

    [Fact]
    public void PropertyItemTypeNameHandlingDynamic()
    {
        var d1 = new PropertyItemTypeNameHandlingDynamic();

        dynamic data = new DynamicDictionary();
        data.one = new TestComponentSimple
        {
            MyProperty = 1
        };

        dynamic data2 = new DynamicDictionary();
        data2.one = new TestComponentSimple
        {
            MyProperty = 2
        };

        data.two = data2;

        d1.Data = (DynamicDictionary)data;

        var json = JsonConvert.SerializeObject(d1, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""Data"": {
    ""one"": {
      ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
      ""MyProperty"": 1
    },
    ""two"": {
      ""$type"": ""Argon.Tests.Linq.DynamicDictionary, Tests"",
      ""one"": {
        ""MyProperty"": 2
      }
    }
  }
}", json);

        var d2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandlingDynamic>(json);
        Assert.NotNull(d2);
        Assert.NotNull(d2.Data);

        dynamic data3 = d2.Data;
        var c = (TestComponentSimple)data3.one;
        Assert.Equal(1, c.MyProperty);

        var data4 = data3.two;
        var o = (JObject)data4.one;
        Assert.Equal(2, (int)o["MyProperty"]);

        json = @"{
  ""Data"": {
    ""one"": {
      ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
      ""MyProperty"": 1
    },
    ""two"": {
      ""$type"": ""Argon.Tests.Linq.DynamicDictionary, Tests"",
      ""one"": {
        ""$type"": ""Argon.Tests.TestObjects.TestComponentSimple, Tests"",
        ""MyProperty"": 2
      }
    }
  }
}";

        d2 = JsonConvert.DeserializeObject<PropertyItemTypeNameHandlingDynamic>(json);
        data3 = d2.Data;
        data4 = data3.two;
        o = (JObject)data4.one;
        Assert.Equal(2, (int)o["MyProperty"]);
    }

    [Fact]
    public void SerializeDeserialize_DictionaryContextContainsGuid_DeserializesItemAsGuid()
    {
        const string contextKey = "k1";
        var someValue = new Guid("a6e986df-fc2c-4906-a1ef-9492388f7833");

        var inputContext = new Dictionary<string, Guid> {{contextKey, someValue}};

        var jsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.All
        };
        var serializedString = JsonConvert.SerializeObject(inputContext, jsonSerializerSettings);

        var dictionaryTypeName = ReflectionUtils.GetTypeName(typeof(Dictionary<string, Guid>), TypeNameAssemblyFormatHandling.Simple, null);

        XUnitAssert.AreEqualNormalized(@"{
  ""$type"": """ + dictionaryTypeName + @""",
  ""k1"": ""a6e986df-fc2c-4906-a1ef-9492388f7833""
}", serializedString);

        var deserializedObject = (Dictionary<string, Guid>)JsonConvert.DeserializeObject(serializedString, jsonSerializerSettings);

        Assert.Equal(someValue, deserializedObject[contextKey]);
    }

    [Fact]
    public void TypeNameHandlingWithISerializableValues()
    {
        var p = new MyParent
        {
            Child = new MyChild
            {
                MyProperty = "string!"
            }
        };

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        var json = JsonConvert.SerializeObject(p, settings);

        XUnitAssert.AreEqualNormalized(@"{
  ""c"": {
    ""$type"": ""Argon.Tests.Serialization.MyChild, Tests"",
    ""p"": ""string!""
  }
}", json);

        var p2 = JsonConvert.DeserializeObject<MyParent>(json, settings);
        Assert.IsType(typeof(MyChild), p2.Child);
        Assert.Equal("string!", ((MyChild)p2.Child).MyProperty);
    }

    [Fact]
    public void TypeNameHandlingWithISerializableValuesAndArray()
    {
        var p = new MyParent
        {
            Child = new MyChildList
            {
                "string!"
            }
        };

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        var json = JsonConvert.SerializeObject(p, settings);

        XUnitAssert.AreEqualNormalized(@"{
  ""c"": {
    ""$type"": ""Argon.Tests.Serialization.MyChildList, Tests"",
    ""$values"": [
      ""string!""
    ]
  }
}", json);

        var p2 = JsonConvert.DeserializeObject<MyParent>(json, settings);
        Assert.IsType(typeof(MyChildList), p2.Child);
        Assert.Equal(1, ((MyChildList)p2.Child).Count);
        Assert.Equal("string!", ((MyChildList)p2.Child)[0]);
    }

    [Fact]
    public void ParentTypeNameHandlingWithISerializableValues()
    {
        var pp = new ParentParent
        {
            ParentProp = new MyParent
            {
                Child = new MyChild
                {
                    MyProperty = "string!"
                }
            }
        };

        var settings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        var json = JsonConvert.SerializeObject(pp, settings);

        XUnitAssert.AreEqualNormalized(@"{
  ""ParentProp"": {
    ""c"": {
      ""$type"": ""Argon.Tests.Serialization.MyChild, Tests"",
      ""p"": ""string!""
    }
  }
}", json);

        var pp2 = JsonConvert.DeserializeObject<ParentParent>(json, settings);
        var p2 = pp2.ParentProp;
        Assert.IsType(typeof(MyChild), p2.Child);
        Assert.Equal("string!", ((MyChild)p2.Child).MyProperty);
    }

    [Fact]
    public void ListOfStackWithFullAssemblyName()
    {
        var input = new List<Stack<string>>
        {
            new(new List<string> {"One", "Two", "Three"}),
            new(new List<string> {"Four", "Five", "Six"}),
            new(new List<string> { "Seven", "Eight", "Nine" })
        };

        var serialized = JsonConvert.SerializeObject(input,
            Formatting.Indented,
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full // TypeNameHandling.Auto will work
#pragma warning restore 618
            });

        var output = JsonConvert.DeserializeObject<List<Stack<string>>>(serialized,
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }
        );

        var strings = output.SelectMany(s => s).ToList();

        Assert.Equal(9, strings.Count);
        Assert.Equal("One", strings[0]);
        Assert.Equal("Nine", strings[8]);
    }

    [Fact]
    public void ExistingBaseValue()
    {
        var json = @"{
    ""itemIdentifier"": {
        ""$type"": ""Argon.Tests.Serialization.ReportItemKeys, Tests"",
        ""dataType"": 0,
        ""wantedUnitID"": 1,
        ""application"": 3,
        ""id"": 101,
        ""name"": ""Machine""
    },
    ""isBusinessEntity"": false,
    ""isKeyItem"": true,
    ""summarizeOnThisItem"": false
}";

        var g = JsonConvert.DeserializeObject<GroupingInfo>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        var item = (ReportItemKeys)g.ItemIdentifier;
        Assert.Equal(1UL, item.WantedUnitID);
    }

    [Fact]
    public void GenericItemTypeCollection()
    {
        var data = new DataType();
        data.Rows.Add("key", new List<MyInterfaceImplementationType> { new() { SomeProperty = "property" } });
        var serialized = JsonConvert.SerializeObject(data, Formatting.Indented);

        var listTypeName = ReflectionUtils.GetTypeName(typeof(List<MyInterfaceImplementationType>), TypeNameAssemblyFormatHandling.Simple, null);

        XUnitAssert.AreEqualNormalized(@"{
  ""Rows"": {
    ""key"": {
      ""$type"": """ + listTypeName + @""",
      ""$values"": [
        {
          ""SomeProperty"": ""property""
        }
      ]
    }
  }
}", serialized);

        var deserialized = JsonConvert.DeserializeObject<DataType>(serialized);

        Assert.Equal("property", deserialized.Rows["key"].First().SomeProperty);
    }

#if !NET5_0_OR_GREATER
        [Fact]
        public void DeserializeComplexGenericDictionary_Simple()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
#pragma warning restore 618
            };

            var dictionary = new Dictionary<int, HashSet<string>>
            {
                { 1, new HashSet<string>(new[] { "test" }) },
            };

            var obtainedJson = JsonConvert.SerializeObject(dictionary, serializerSettings);

            var obtainedDictionary = (Dictionary<int, HashSet<string>>)JsonConvert.DeserializeObject(obtainedJson, serializerSettings);

            Assert.NotNull(obtainedDictionary);
        }

        [Fact]
        public void DeserializeComplexGenericDictionary_Full()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
#pragma warning disable 618
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Full
#pragma warning restore 618
            };

            var dictionary = new Dictionary<int, HashSet<string>>
            {
                { 1, new HashSet<string>(new[] { "test" }) },
            };

            var obtainedJson = JsonConvert.SerializeObject(dictionary, serializerSettings);

            var obtainedDictionary = (Dictionary<int, HashSet<string>>)JsonConvert.DeserializeObject(obtainedJson, serializerSettings);

            Assert.NotNull(obtainedDictionary);
        }

        [Fact]
        public void SerializeNullableStructProperty_Auto()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            var objWithMessage = new ObjectWithOptionalMessage(new Message2("Hello!"));

            var json = JsonConvert.SerializeObject(objWithMessage, serializerSettings);

            XUnitAssert.AreEqualNormalized(@"{
  ""Message"": {
    ""Value"": ""Hello!""
  }
}", json);
        }

        [Fact]
        public void DeserializeNullableStructProperty_Auto()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            var json = @"{
  ""Message"": {
    ""Value"": ""Hello!""
  }
}";
            var objWithMessage = JsonConvert.DeserializeObject<ObjectWithOptionalMessage>(json, serializerSettings);

            XUnitAssert.AreEqualNormalized("Hello!", objWithMessage.Message.Value.Value);
        }
#endif

    [Fact]
    public void SerializerWithDefaultBinder()
    {
        var serializer = JsonSerializer.Create();
#pragma warning disable CS0618
        Assert.NotEqual(null, serializer.Binder);
        Assert.IsType(typeof(DefaultSerializationBinder), serializer.Binder);
#pragma warning restore CS0618 // Type or member is obsolete
        Assert.IsType(typeof(DefaultSerializationBinder), serializer.SerializationBinder);
    }

    [Fact]
    public void ObsoleteBinderThrowsIfISerializationBinderSet()
    {
        var serializer = JsonSerializer.Create(new JsonSerializerSettings { SerializationBinder = new FancyBinder() });
        XUnitAssert.Throws<InvalidOperationException>(() =>
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var serializationBinder = serializer.Binder;
#pragma warning restore CS0618 // Type or member is obsolete
            serializationBinder.ToString();
        }, "Cannot get SerializationBinder because an ISerializationBinder was previously set.");

        Assert.IsType(typeof(FancyBinder), serializer.SerializationBinder);
    }

    [Fact]
    public void SetOldBinderAndSerializationBinderReturnsWrapper()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var serializer = JsonSerializer.Create(new JsonSerializerSettings { Binder = new OldBinder() });
        Assert.IsType(typeof(OldBinder), serializer.Binder);
#pragma warning restore CS0618 // Type or member is obsolete

        var binder = serializer.SerializationBinder;

        Assert.IsType(typeof(SerializationBinderAdapter), binder);
        Assert.Equal(typeof(string), binder.BindToType(null, null));
    }

    public class FancyBinder : ISerializationBinder
    {
        static readonly string Annotate = new(':', 3);

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = $"FancyAssemblyName=>{Assembly.GetAssembly(serializedType)?.GetName().Name}";
            typeName = string.Format("{0}{1}{0}", Annotate, serializedType.Name);
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            return null;
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public class OldBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return typeof(string);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}

public struct Message2
{
    public string Value { get; }

    [JsonConstructor]
    public Message2(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        Value = value;
    }
}

public class ObjectWithOptionalMessage
{
    public Message2? Message { get; }

    public ObjectWithOptionalMessage(Message2? message)
    {
        Message = message;
    }
}

public class DataType
{
    public DataType()
    {
        Rows = new Dictionary<string, IEnumerable<IMyInterfaceType>>();
    }

    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto, TypeNameHandling = TypeNameHandling.Auto)]
    public Dictionary<string, IEnumerable<IMyInterfaceType>> Rows { get; private set; }
}

public interface IMyInterfaceType
{
    string SomeProperty { get; set; }
}

public class MyInterfaceImplementationType : IMyInterfaceType
{
    public string SomeProperty { get; set; }
}

public class ParentParent
{
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
    public MyParent ParentProp { get; set; }
}

[Serializable]
public class MyParent : ISerializable
{
    public ISomeBase Child { get; internal set; }

    public MyParent(SerializationInfo info, StreamingContext context)
    {
        Child = (ISomeBase)info.GetValue("c", typeof(ISomeBase));
    }

    public MyParent()
    {
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("c", Child);
    }
}

public class MyChild : ISomeBase
{
    [JsonProperty("p")]
    public String MyProperty { get; internal set; }
}

public class MyChildList : List<string>, ISomeBase
{
}

public interface ISomeBase
{
}

public class Message
{
    public string Address { get; set; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    public object Body { get; set; }
}

public class SearchDetails
{
    public string Query { get; set; }
    public string Language { get; set; }
}

public class Customer
{
    public string Name { get; set; }
}

public class Purchase
{
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class SerializableWrapper
{
    public object Content { get; set; }

    public override bool Equals(object obj)
    {
        var w = obj as SerializableWrapper;

        if (w == null)
        {
            return false;
        }

        return Equals(w.Content, Content);
    }

    public override int GetHashCode()
    {
        if (Content == null)
        {
            return 0;
        }

        return Content.GetHashCode();
    }
}

public interface IExample
    : ISerializable
{
    String Name { get; }
}

[Serializable]
public class Example
    : IExample
{
    public Example(String name)
    {
        Name = name;
    }

    protected Example(SerializationInfo info, StreamingContext context)
    {
        Name = info.GetString("name");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("name", Name);
    }

    public String Name { get; set; }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (ReferenceEquals(this, obj))
        {
            return true;
        }
        if (obj is IExample)
        {
            return Name.Equals(((IExample)obj).Name);
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        if (Name == null)
        {
            return 0;
        }

        return Name.GetHashCode();
    }
}

public class PropertyItemTypeNameHandlingObject
{
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
    public TypeNameHandlingTestObject Data { get; set; }
}

public class PropertyItemTypeNameHandlingDynamic
{
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
    public DynamicDictionary Data { get; set; }
}

public class TypeNameHandlingTestObject
{
    public object Prop1 { get; set; }
    public object Prop2 { get; set; }
    public object Prop3 { get; set; }
    public object Prop4 { get; set; }
}

public class PropertyItemTypeNameHandlingDictionary
{
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
    public IDictionary<string, object> Data { get; set; }
}

public class PropertyItemTypeNameHandling
{
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
    public IList<object> Data { get; set; }
}

[JsonArray(ItemTypeNameHandling = TypeNameHandling.All)]
public class TypeNameList<T> : List<T>
{
}

[JsonDictionary(ItemTypeNameHandling = TypeNameHandling.All)]
public class TypeNameDictionary<T> : Dictionary<string, T>
{
}

[JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
public class TypeNameObject
{
    public object Object1 { get; set; }
    public object Object2 { get; set; }

    [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
    public object ObjectNotHandled { get; set; }

    public string String { get; set; }
    public int Integer { get; set; }
}

[DataContract]
public class GroupingInfo
{
    [DataMember]
    public ApplicationItemKeys ItemIdentifier { get; set; }

    public GroupingInfo()
    {
        ItemIdentifier = new ApplicationItemKeys();
    }
}

[DataContract]
public class ApplicationItemKeys
{
    [DataMember]
    public int ID { get; set; }

    [DataMember]
    public string Name { get; set; }
}

[DataContract]
public class ReportItemKeys : ApplicationItemKeys
{
    protected ulong _wantedUnit;

    [DataMember]
    public ulong WantedUnitID
    {
        get => _wantedUnit;
        set => _wantedUnit = value;
    }
}