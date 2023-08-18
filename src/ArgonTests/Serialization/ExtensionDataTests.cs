// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using TestObjects;

public class ExtensionDataTests : TestFixtureBase
{
    public class CustomDictionary : IDictionary<string, object>
    {
        readonly IDictionary<string, object> inner = new Dictionary<string, object>();

        public void Add(string key, object value) =>
            inner.Add(key, value);

        public bool ContainsKey(string key) =>
            inner.ContainsKey(key);

        public ICollection<string> Keys => inner.Keys;

        public bool Remove(string key) =>
            inner.Remove(key);

        public bool TryGetValue(string key, out object value) =>
            inner.TryGetValue(key, out value);

        public ICollection<object> Values => inner.Values;

        public object this[string key]
        {
            get => inner[key];
            set => inner[key] = value;
        }

        public void Add(KeyValuePair<string, object> item) =>
            inner.Add(item);

        public void Clear() =>
            inner.Clear();

        public bool Contains(KeyValuePair<string, object> item) =>
            inner.Contains(item);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) =>
            inner.CopyTo(array, arrayIndex);

        public int Count => inner.Count;

        public bool IsReadOnly => inner.IsReadOnly;

        public bool Remove(KeyValuePair<string, object> item) =>
            inner.Remove(item);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
            inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            inner.GetEnumerator();
    }

    public class Example
    {
        public Example() =>
            Data = new CustomDictionary();

        [JsonExtensionData] public IDictionary<string, object> Data { get; }
    }

    [Fact]
    public void DataBagDoesNotInheritFromDictionaryClass()
    {
        var e = new Example();
        e.Data.Add("extensionData1", new[] {1, 2, 3});

        var json = JsonConvert.SerializeObject(e, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(
            """
            {
              "extensionData1": [
                1,
                2,
                3
              ]
            }
            """,
            json);

        var e2 = JsonConvert.DeserializeObject<Example>(json);

        var o1 = (JArray) e2.Data["extensionData1"];

        Assert.Equal(JTokenType.Array, o1.Type);
        Assert.Equal(3, o1.Count);
        Assert.Equal(1, (int) o1[0]);
        Assert.Equal(2, (int) o1[1]);
        Assert.Equal(3, (int) o1[2]);
    }

    public class ExtensionDataDeserializeWithNonDefaultConstructor
    {
        public ExtensionDataDeserializeWithNonDefaultConstructor(string name) =>
            Name = name;

        [JsonExtensionData] public IDictionary<string, JToken> _extensionData;

        public string Name { get; set; }
    }

    [Fact]
    public void ExtensionDataDeserializeWithNonDefaultConstructorTest()
    {
        var c = new ExtensionDataDeserializeWithNonDefaultConstructor("Name!")
        {
            _extensionData = new Dictionary<string, JToken>
            {
                {"Key!", "Value!"}
            }
        };

        var json = JsonConvert.SerializeObject(c, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(
            """
            {
              "Name": "Name!",
              "Key!": "Value!"
            }
            """,
            json);

        var c2 = JsonConvert.DeserializeObject<ExtensionDataDeserializeWithNonDefaultConstructor>(json);

        Assert.Equal("Name!", c2.Name);
        Assert.NotNull(c2._extensionData);
        Assert.Equal(1, c2._extensionData.Count);
        Assert.Equal("Value!", (string) c2._extensionData["Key!"]);
    }

    [Fact]
    public void ExtensionDataWithNull()
    {
        var json = """
            {
              'TaxRate': 0.125,
              'a':null
            }
            """;

        var invoice = JsonConvert.DeserializeObject<ExtendedObject>(json);

        Assert.Equal(JTokenType.Null, invoice._additionalData["a"].Type);
        Assert.Equal(typeof(double), ((JValue) invoice._additionalData["TaxRate"]).Value.GetType());

        var result = JsonConvert.SerializeObject(invoice);

        Assert.Equal(@"{""TaxRate"":0.125,""a"":null}", result);
    }

    [Fact]
    public void ExtensionDataFloatParseHandling()
    {
        var json = """
            {
              'TaxRate': 0.125,
              'a':null
            }
            """;

        var invoice = JsonConvert.DeserializeObject<ExtendedObject>(json, new JsonSerializerSettings
        {
            FloatParseHandling = FloatParseHandling.Decimal
        });

        Assert.Equal(typeof(decimal), ((JValue) invoice._additionalData["TaxRate"]).Value.GetType());
    }

#pragma warning disable 649
    class ExtendedObject
    {
        [JsonExtensionData] internal IDictionary<string, JToken> _additionalData;
    }
#pragma warning restore 649

#pragma warning disable 169
    public class CustomerInvoice
    {
        // we're only modifying the tax rate
        public decimal TaxRate { get; set; }

        // everything else gets stored here
        [JsonExtensionData] IDictionary<string, JToken> additionalData;
    }
#pragma warning restore 169

    [Fact]
    public void ExtensionDataExample()
    {
        var json = """
            {
              'HourlyRate': 150,
              'Hours': 40,
              'TaxRate': 0.125
            }
            """;

        var invoice = JsonConvert.DeserializeObject<CustomerInvoice>(json);

        // increase tax to 15%
        invoice.TaxRate = 0.15m;

        var result = JsonConvert.SerializeObject(invoice);
        // {
        //   'TaxRate': 0.15,
        //   'HourlyRate': 150,
        //   'Hours': 40
        // }

        Assert.Equal(@"{""TaxRate"":0.15,""HourlyRate"":150,""Hours"":40}", result);
    }

    public class ExtensionDataTestClass
    {
        public string Name { get; set; }

        [JsonProperty("custom_name")] public string CustomName { get; set; }

        [JsonIgnore] public IList<int> Ignored { get; set; }

        public bool GetPrivate { get; internal set; }

        public bool GetOnly => true;

        public readonly string Readonly = "Readonly";
        public IList<int> Ints { get; set; } = new List<int> {0};

        [JsonExtensionData] internal IDictionary<string, JToken> ExtensionData { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), NamingStrategyParameters = new object[] {true, true, true})]
    public class ExtensionDataWithNamingStrategyTestClass
    {
        public string Name { get; set; }

        [JsonExtensionData] internal IDictionary<string, JToken> ExtensionData { get; set; }
    }

    public class JObjectExtensionDataTestClass
    {
        public string Name { get; set; }

        [JsonExtensionData] public JObject ExtensionData { get; set; }
    }

    [Fact]
    public void RoundTripJObjectExtensionData()
    {
        var c = new JObjectExtensionDataTestClass
        {
            Name = "Name!",
            ExtensionData = new()
            {
                {"one", 1},
                {"two", "II"},
                {"three", new JArray(1, 1, 1)}
            }
        };

        var json = JsonConvert.SerializeObject(c, Formatting.Indented);

        var c2 = JsonConvert.DeserializeObject<JObjectExtensionDataTestClass>(json);

        Assert.Equal("Name!", c2.Name);
        Assert.True(JToken.DeepEquals(c.ExtensionData, c2.ExtensionData));
    }

    [Fact]
    public void ExtensionDataTest()
    {
        var json = """
            {
              "Ints": [1,2,3],
              "Ignored": [1,2,3],
              "Readonly": "Readonly",
              "Name": "Actually set!",
              "CustomName": "Wrong name!",
              "GetPrivate": true,
              "GetOnly": true,
              "NewValueSimple": true,
              "NewValueComplex": [1,2,3]
            }
            """;

        var c = JsonConvert.DeserializeObject<ExtensionDataTestClass>(json);

        Assert.Equal("Actually set!", c.Name);
        Assert.Equal(4, c.Ints.Count);

        Assert.Equal("Readonly", (string) c.ExtensionData["Readonly"]);
        Assert.Equal("Wrong name!", (string) c.ExtensionData["CustomName"]);
        XUnitAssert.True((bool) c.ExtensionData["GetPrivate"]);
        XUnitAssert.True((bool) c.ExtensionData["GetOnly"]);
        XUnitAssert.True((bool) c.ExtensionData["NewValueSimple"]);
        Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), c.ExtensionData["NewValueComplex"]));
        Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), c.ExtensionData["Ignored"]));

        Assert.Equal(7, c.ExtensionData.Count);
    }

    [Fact]
    public void ExtensionDataTest_DeserializeWithNamingStrategy()
    {
        var json = """
            {
              "Ints": [1,2,3],
              "Ignored": [1,2,3],
              "Readonly": "Readonly",
              "Name": "Actually set!",
              "CustomName": "Wrong name!",
              "GetPrivate": true,
              "GetOnly": true,
              "NewValueSimple": true,
              "NewValueComplex": [1,2,3]
            }
            """;

        var c = JsonConvert.DeserializeObject<ExtensionDataTestClass>(json, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessExtensionDataNames = true
                }
            }
        });

        Assert.Equal("Actually set!", c.Name);
        Assert.Equal(4, c.Ints.Count);

        Assert.Equal("Readonly", (string) c.ExtensionData["Readonly"]);
        Assert.Equal("Wrong name!", (string) c.ExtensionData["CustomName"]);
        XUnitAssert.True((bool) c.ExtensionData["GetPrivate"]);
        XUnitAssert.True((bool) c.ExtensionData["GetOnly"]);
        XUnitAssert.True((bool) c.ExtensionData["NewValueSimple"]);
        Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), c.ExtensionData["NewValueComplex"]));
        Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), c.ExtensionData["Ignored"]));

        Assert.Equal(7, c.ExtensionData.Count);
    }

    [Fact]
    public void ExtensionDataTest_SerializeWithNamingStrategy_Enabled()
    {
        var c = new ExtensionDataTestClass
        {
            ExtensionData = new Dictionary<string, JToken>
            {
                ["TestValue1"] = 1,
                ["alreadyCamelCase"] = new JObject
                {
                    ["NotProcessed"] = true
                }
            }
        };

        var json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessExtensionDataNames = true
                }
            },
            Formatting = Formatting.Indented
        });

        XUnitAssert.AreEqualNormalized(
            """
            {
              "readonly": "Readonly",
              "name": null,
              "custom_name": null,
              "getPrivate": false,
              "getOnly": true,
              "ints": [
                0
              ],
              "testValue1": 1,
              "alreadyCamelCase": {
                "NotProcessed": true
              }
            }
            """,
            json);
    }

    [Fact]
    public void ExtensionDataTest_SerializeWithNamingStrategy_Disabled()
    {
        var c = new ExtensionDataTestClass
        {
            ExtensionData = new Dictionary<string, JToken>
            {
                ["TestValue1"] = 1,
                ["alreadyCamelCase"] = new JObject
                {
                    ["NotProcessed"] = true
                }
            }
        };

        var json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        });

        XUnitAssert.AreEqualNormalized(
            """
            {
              "readonly": "Readonly",
              "name": null,
              "custom_name": null,
              "getPrivate": false,
              "getOnly": true,
              "ints": [
                0
              ],
              "TestValue1": 1,
              "alreadyCamelCase": {
                "NotProcessed": true
              }
            }
            """,
            json);
    }

    [Fact]
    public void ExtensionDataTest_SerializeWithNamingStrategyAttribute()
    {
        var c = new ExtensionDataWithNamingStrategyTestClass
        {
            ExtensionData = new Dictionary<string, JToken>
            {
                ["TestValue1"] = 1,
                ["alreadyCamelCase"] = new JObject
                {
                    ["NotProcessed"] = true
                }
            }
        };

        var json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        });

        XUnitAssert.AreEqualNormalized(
            """
            {
              "name": null,
              "testValue1": 1,
              "alreadyCamelCase": {
                "NotProcessed": true
              }
            }
            """,
            json);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class MyClass
    {
        public int NotForJson { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int ForJson { get; set; }

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, JToken> ExtraInfoJson { get; set; }

        public MyClass(MyClass other = null)
        {
            if (other != null)
            {
                // copy construct
            }
        }
    }

    [Fact]
    public void PopulateWithExtensionData()
    {
        var jsonStirng = @"{ ""ForJson"" : 33 , ""extra1"" : 11, ""extra2"" : 22 }";

        var c = new MyClass();

        JsonConvert.PopulateObject(jsonStirng, c);

        Assert.Equal(2, c.ExtraInfoJson.Count);
        Assert.Equal(11, (int) c.ExtraInfoJson["extra1"]);
        Assert.Equal(22, (int) c.ExtraInfoJson["extra2"]);
    }

    public class MultipleExtensionDataAttributesTestClass
    {
        public string Name { get; set; }

        [JsonExtensionData] internal IDictionary<string, JToken> ExtensionData1 { get; set; }

        [JsonExtensionData] internal IDictionary<string, JToken> ExtensionData2 { get; set; }
    }

    public class ExtensionDataAttributesInheritanceTestClass : MultipleExtensionDataAttributesTestClass
    {
        [JsonExtensionData] internal IDictionary<string, JToken> ExtensionData0 { get; set; }
    }

    public class FieldExtensionDataAttributeTestClass
    {
        [JsonExtensionData] internal IDictionary<object, object> ExtensionData;
    }

    public class PublicExtensionDataAttributeTestClass
    {
        public string Name { get; set; }

        [JsonExtensionData] public IDictionary<object, object> ExtensionData;
    }

    public class PublicExtensionDataAttributeTestClassWithNonDefaultConstructor
    {
        public string Name { get; set; }

        public PublicExtensionDataAttributeTestClassWithNonDefaultConstructor(string name) =>
            Name = name;

        [JsonExtensionData] public IDictionary<object, object> ExtensionData;
    }

    public class PublicNoReadExtensionDataAttributeTestClass
    {
        public string Name { get; set; }

        [JsonExtensionData(ReadData = false)] public IDictionary<object, object> ExtensionData;
    }

    public class PublicNoWriteExtensionDataAttributeTestClass
    {
        public string Name { get; set; }

        [JsonExtensionData(WriteData = false)] public IDictionary<object, object> ExtensionData;
    }

    public class PublicJTokenExtensionDataAttributeTestClass
    {
        public string Name { get; set; }

        [JsonExtensionData] public IDictionary<string, JToken> ExtensionData;
    }

    [Fact]
    public void DeserializeDirectoryAccount()
    {
        var json = @"{'DisplayName':'John Smith', 'SAMAccountName':'contoso\\johns'}";

        var account = JsonConvert.DeserializeObject<DirectoryAccount>(json);

        Assert.Equal("John Smith", account.DisplayName);
        Assert.Equal("contoso", account.Domain);
        Assert.Equal("johns", account.UserName);
    }

    [Fact]
    public void SerializePublicExtensionData()
    {
        var json = JsonConvert.SerializeObject(new PublicExtensionDataAttributeTestClass
        {
            Name = "Name!",
            ExtensionData = new Dictionary<object, object>
            {
                {"Test", 1}
            }
        });

        Assert.Equal(@"{""Name"":""Name!"",""Test"":1}", json);
    }

    [Fact]
    public void SerializePublicExtensionDataNull()
    {
        var json = JsonConvert.SerializeObject(new PublicExtensionDataAttributeTestClass
        {
            Name = "Name!"
        });

        Assert.Equal(@"{""Name"":""Name!""}", json);
    }

    [Fact]
    public void SerializePublicNoWriteExtensionData()
    {
        var json = JsonConvert.SerializeObject(new PublicNoWriteExtensionDataAttributeTestClass
        {
            Name = "Name!",
            ExtensionData = new Dictionary<object, object>
            {
                {"Test", 1}
            }
        });

        Assert.Equal(@"{""Name"":""Name!""}", json);
    }

    [Fact]
    public void DeserializeNoReadPublicExtensionData()
    {
        var c = JsonConvert.DeserializeObject<PublicNoReadExtensionDataAttributeTestClass>(@"{""Name"":""Name!"",""Test"":1}");

        Assert.Equal(null, c.ExtensionData);
    }

    [Fact]
    public void SerializePublicExtensionDataCircularReference()
    {
        var c = new PublicExtensionDataAttributeTestClass
        {
            Name = "Name!",
            ExtensionData = new Dictionary<object, object>
            {
                {"Test", 1}
            }
        };
        c.ExtensionData["Self"] = c;

        var json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            Formatting = Formatting.Indented
        });

        XUnitAssert.AreEqualNormalized(
            """
            {
              "$id": "1",
              "Name": "Name!",
              "Test": 1,
              "Self": {
                "$ref": "1"
              }
            }
            """,
            json);

        var c2 = JsonConvert.DeserializeObject<PublicExtensionDataAttributeTestClass>(json, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All
        });

        Assert.Equal("Name!", c2.Name);

        var bizzaroC2 = (PublicExtensionDataAttributeTestClass) c2.ExtensionData["Self"];

        Assert.Equal(c2, bizzaroC2);
        Assert.Equal(1, (long) bizzaroC2.ExtensionData["Test"]);
    }

    [Fact]
    public void DeserializePublicJTokenExtensionDataCircularReference()
    {
        var json = """
            {
              "$id": "1",
              "Name": "Name!",
              "Test": 1,
              "Self": {
                "$ref": "1"
              }
            }
            """;

        var c2 = JsonConvert.DeserializeObject<PublicJTokenExtensionDataAttributeTestClass>(json, new JsonSerializerSettings
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All
        });

        Assert.Equal("Name!", c2.Name);

        var bizzaroC2 = (JObject) c2.ExtensionData["Self"];

        Assert.Equal("1", (string) bizzaroC2["$ref"]);
    }

    [Fact]
    public void DeserializePublicExtensionDataTypeNamdHandling()
    {
        var json = """
            {
              "$id": "1",
              "Name": "Name!",
              "Test": 1,
              "Self": {
                "$type": "TestObjects.WagePerson, ArgonTests",
                "HourlyWage": 2.0,
                "Name": null,
                "BirthDate": "0001-01-01T00:00:00",
                "LastModified": "0001-01-01T00:00:00"
              }
            }
            """;

        var c2 = JsonConvert.DeserializeObject<PublicExtensionDataAttributeTestClass>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        Assert.Equal("Name!", c2.Name);

        var bizzaroC2 = (WagePerson) c2.ExtensionData["Self"];

        Assert.Equal(2m, bizzaroC2.HourlyWage);
    }

    [Fact]
    public void DeserializePublicExtensionDataTypeNameHandlingNonDefaultConstructor()
    {
        var json = """
            {
              "$id": "1",
              "Name": "Name!",
              "Test": 1,
              "Self": {
                "$type": "TestObjects.WagePerson, ArgonTests",
                "HourlyWage": 2.0,
                "Name": null,
                "BirthDate": "0001-01-01T00:00:00",
                "LastModified": "0001-01-01T00:00:00"
              }
            }
            """;

        var c2 = JsonConvert.DeserializeObject<PublicExtensionDataAttributeTestClassWithNonDefaultConstructor>(json, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        });

        Assert.Equal("Name!", c2.Name);

        var bizzaroC2 = (WagePerson) c2.ExtensionData["Self"];

        Assert.Equal(2m, bizzaroC2.HourlyWage);
    }

    [Fact]
    public void SerializePublicExtensionDataTypeNamdHandling()
    {
        var c = new PublicExtensionDataAttributeTestClass
        {
            Name = "Name!",
            ExtensionData = new Dictionary<object, object>
            {
                {
                    "Test", new WagePerson
                    {
                        HourlyWage = 2.1m
                    }
                }
            }
        };

        var json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.Indented
        });

        XUnitAssert.AreEqualNormalized(
            """
            {
              "$type": "ExtensionDataTests+PublicExtensionDataAttributeTestClass, ArgonTests",
              "Name": "Name!",
              "Test": {
                "$type": "TestObjects.WagePerson, ArgonTests",
                "HourlyWage": 2.1,
                "Name": null,
                "BirthDate": "0001-01-01T00:00:00",
                "LastModified": "0001-01-01T00:00:00"
              }
            }
            """,
            json);
    }

    [Fact]
    public void DeserializePublicExtensionData()
    {
        var json = """
            {
              'Name':'Name!',
              'NoMatch':'NoMatch!',
              'ExtensionData':{'HAI':true}
            }
            """;

        var c = JsonConvert.DeserializeObject<PublicExtensionDataAttributeTestClass>(json);

        Assert.Equal("Name!", c.Name);
        Assert.Equal(2, c.ExtensionData.Count);

        Assert.Equal("NoMatch!", (string) c.ExtensionData["NoMatch"]);

        // the ExtensionData property is put into the extension data
        // inception
        var o = (JObject) c.ExtensionData["ExtensionData"];
        Assert.Equal(1, o.Count);
        Assert.True(JToken.DeepEquals(new JObject {{"HAI", true}}, o));
    }

    [Fact]
    public void FieldExtensionDataAttributeTest_Serialize()
    {
        var c = new FieldExtensionDataAttributeTestClass
        {
            ExtensionData = new Dictionary<object, object>()
        };

        var json = JsonConvert.SerializeObject(c);

        Assert.Equal("{}", json);
    }

    [Fact]
    public void FieldExtensionDataAttributeTest_Deserialize()
    {
        var c = JsonConvert.DeserializeObject<FieldExtensionDataAttributeTestClass>("{'first':1,'second':2}");

        Assert.Equal(2, c.ExtensionData.Count);
        Assert.Equal(1, (long) c.ExtensionData["first"]);
        Assert.Equal(2, (long) c.ExtensionData["second"]);
    }

    [Fact]
    public void MultipleExtensionDataAttributesTest()
    {
        var c = JsonConvert.DeserializeObject<MultipleExtensionDataAttributesTestClass>("{'first':[1],'second':[2]}");

        Assert.Equal(null, c.ExtensionData2);
        Assert.Equal(2, c.ExtensionData1.Count);
        Assert.Equal(1, (int) ((JArray) c.ExtensionData1["first"])[0]);
        Assert.Equal(2, (int) ((JArray) c.ExtensionData1["second"])[0]);
    }

    [Fact]
    public void ExtensionDataAttributesInheritanceTest()
    {
        var c = JsonConvert.DeserializeObject<ExtensionDataAttributesInheritanceTestClass>("{'first':1,'second':2}");

        Assert.Equal(null, c.ExtensionData1);
        Assert.Equal(null, c.ExtensionData2);
        Assert.Equal(2, c.ExtensionData0.Count);
        Assert.Equal(1, (int) c.ExtensionData0["first"]);
        Assert.Equal(2, (int) c.ExtensionData0["second"]);
    }

    public class TestClass
    {
        [JsonProperty("LastActivityDate")] public DateTime? LastActivityDate { get; set; }

        [JsonExtensionData] public Dictionary<string, object> CustomFields { get; set; }
    }

    [Fact]
    public void DeserializeNullableProperty()
    {
        var json = @"{ ""LastActivityDate"":null, ""CustomField1"":""Testing"" }";

        var c = JsonConvert.DeserializeObject<TestClass>(json);

        Assert.Equal(null, c.LastActivityDate);
        Assert.Equal(1, c.CustomFields.Count);
        Assert.Equal("Testing", (string) c.CustomFields["CustomField1"]);
    }

    public class DocNoSetter
    {
        public DocNoSetter()
        {
        }

        public DocNoSetter(JObject content) =>
            Content = content;

        [JsonProperty("_name")] public string Name { get; set; }

        [JsonExtensionData] public JObject Content { get; }
    }

    [Fact]
    public void SerializeExtensionData_NoSetter()
    {
        var json = JsonConvert.SerializeObject(new DocNoSetter(new(new JProperty("Property1", 123)))
        {
            Name = "documentName"
        });
        Assert.Equal(@"{""_name"":""documentName"",""Property1"":123}", json);
    }

    [Fact]
    public void SerializeExtensionData_NoSetterAndNoValue()
    {
        var json = JsonConvert.SerializeObject(new DocNoSetter(null)
        {
            Name = "documentName"
        });
        Assert.Equal(@"{""_name"":""documentName""}", json);
    }

    [Fact]
    public void DeserializeExtensionData_NoSetterAndNoExtensionData()
    {
        var doc = JsonConvert.DeserializeObject<DocNoSetter>(@"{""_name"":""documentName""}");

        Assert.Equal("documentName", doc.Name);
    }

    [Fact]
    public void DeserializeExtensionData_NoSetterAndWithExtensionData()
    {
        try
        {
            JsonConvert.DeserializeObject<DocNoSetter>(@"{""_name"":""documentName"",""Property1"":123}");
        }
        catch (JsonSerializationException exception)
        {
            Assert.Equal("Error setting value in extension data for type 'ExtensionDataTests+DocNoSetter'. Path 'Property1', line 1, position 39.", exception.Message);
            Assert.Equal("Cannot set value onto extension data member 'Content'. The extension data collection is null and it cannot be set.", exception.InnerException.Message);
        }
    }

    public class DocNoGetter
    {
        [JsonExtensionData]
        public JObject Content
        {
            set { }
        }
    }

    [Fact]
    public void SerializeExtensionData_NoGetter() =>
        XUnitAssert.Throws<JsonException>(
            () => JsonConvert.SerializeObject(new DocNoGetter()),
            "Invalid extension data attribute on 'ExtensionDataTests+DocNoGetter'. Member 'Content' must have a getter.");

    public class Item
    {
        [JsonExtensionData] public IDictionary<string, JToken> ExtensionData;

        public IEnumerable<string> Foo
        {
            get
            {
                yield return "foo";
                yield return "bar";
            }
        }
    }

    [Fact]
    public void Deserialize_WriteJsonDirectlyToJToken()
    {
        var serializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        var stringWriter = new StringWriter();
        serializer.Serialize(stringWriter, new Item());
        var str = stringWriter.GetStringBuilder().ToString();
        var deserialize = serializer.Deserialize<Item>(new JsonTextReader(new StringReader(str)));

        var value = deserialize.ExtensionData["Foo"]["$type"];
        Assert.Equal(JTokenType.String, value.Type);
        Assert.Equal("foo", (string) deserialize.ExtensionData["Foo"]["$values"][0]);
        Assert.Equal("bar", (string) deserialize.ExtensionData["Foo"]["$values"][1]);
    }

    public class ItemWithConstructor
    {
        [JsonExtensionData] public IDictionary<string, JToken> ExtensionData;

        public ItemWithConstructor(string temp)
        {
        }

        public IEnumerable<string> Foo
        {
            get
            {
                yield return "foo";
                yield return "bar";
            }
        }
    }

    [Fact]
    public void DeserializeWithConstructor_WriteJsonDirectlyToJToken()
    {
        var serializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        var stringWriter = new StringWriter();
        serializer.Serialize(stringWriter, new ItemWithConstructor(null));
        var str = stringWriter.GetStringBuilder().ToString();
        var deserialize = serializer.Deserialize<Item>(new JsonTextReader(new StringReader(str)));

        var value = deserialize.ExtensionData["Foo"]["$type"];
        Assert.Equal(JTokenType.String, value.Type);
        Assert.Equal("foo", (string) deserialize.ExtensionData["Foo"]["$values"][0]);
        Assert.Equal("bar", (string) deserialize.ExtensionData["Foo"]["$values"][1]);
    }
}