// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

using System.Dynamic;
using TestObjects;

public class SnakeCaseNamingStrategyTests : TestFixtureBase
{
    [Fact]
    public void JsonConvertSerializerSettings()
    {
        var person = new Person
        {
            BirthDate = new(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc),
            LastModified = new(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc),
            Name = "Name!"
        };

        var contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };

        var json = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
        {
            ContractResolver = contractResolver
        });

        XUnitAssert.AreEqualNormalized(@"{
  ""name"": ""Name!"",
  ""birth_date"": ""2000-11-20T23:55:44Z"",
  ""last_modified"": ""2000-11-20T23:55:44Z""
}", json);

        var deserializedPerson = JsonConvert.DeserializeObject<Person>(json, new JsonSerializerSettings
        {
            ContractResolver = contractResolver
        });

        Assert.Equal(person.BirthDate, deserializedPerson.BirthDate);
        Assert.Equal(person.LastModified, deserializedPerson.LastModified);
        Assert.Equal(person.Name, deserializedPerson.Name);

        json = JsonConvert.SerializeObject(person, Formatting.Indented);
        XUnitAssert.AreEqualNormalized(@"{
  ""Name"": ""Name!"",
  ""BirthDate"": ""2000-11-20T23:55:44Z"",
  ""LastModified"": ""2000-11-20T23:55:44Z""
}", json);
    }

    [Fact]
    public void JTokenWriter_OverrideSpecifiedName()
    {
        var ignoreAttributeOnClassTestClass = new JsonIgnoreAttributeOnClassTestClass
        {
            Field = int.MinValue
        };

        var contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy
            {
                OverrideSpecifiedNames = true
            }
        };

        var serializer = new JsonSerializer
        {
            ContractResolver = contractResolver
        };

        var writer = new JTokenWriter();

        serializer.Serialize(writer, ignoreAttributeOnClassTestClass);

        var o = (JObject) writer.Token;
        var p = o.Property("the_field");

        Assert.Equal(int.MinValue, (int) p.Value);
    }

    [Fact]
    public void BlogPostExample()
    {
        var product = new Product
        {
            ExpiryDate = new(2010, 12, 20, 18, 1, 0, DateTimeKind.Utc),
            Name = "Widget",
            Price = 9.99m,
            Sizes = new[] {"Small", "Medium", "Large"}
        };

        var contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };

        var json =
            JsonConvert.SerializeObject(
                product,
                Formatting.Indented,
                new JsonSerializerSettings {ContractResolver = contractResolver}
            );

        //{
        //  "name": "Widget",
        //  "expiryDate": "\/Date(1292868060000)\/",
        //  "price": 9.99,
        //  "sizes": [
        //    "Small",
        //    "Medium",
        //    "Large"
        //  ]
        //}

        XUnitAssert.AreEqualNormalized(@"{
  ""name"": ""Widget"",
  ""expiry_date"": ""2010-12-20T18:01:00Z"",
  ""price"": 9.99,
  ""sizes"": [
    ""Small"",
    ""Medium"",
    ""Large""
  ]
}", json);
    }

    [Fact]
    public void DynamicSnakeCasePropertyNames()
    {
        dynamic o = new TestDynamicObject();
        o.Text = "Text!";
        o.Integer = int.MaxValue;

        var contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy
            {
                ProcessDictionaryKeys = true
            }
        };

        string json = JsonConvert.SerializeObject(o, Formatting.Indented,
            new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

        XUnitAssert.AreEqualNormalized(@"{
  ""explicit"": false,
  ""text"": ""Text!"",
  ""integer"": 2147483647,
  ""int"": 0,
  ""child_object"": null
}", json);
    }

    public class DynamicChildObject
    {
        public string Text { get; set; }
        public int Integer { get; set; }
    }

    public class TestDynamicObject : DynamicObject
    {
        public int Int;

        [JsonProperty] public bool Explicit;

        public DynamicChildObject ChildObject { get; set; }

        internal Dictionary<string, object> Members { get; }

        public TestDynamicObject() =>
            Members = new();

        public override IEnumerable<string> GetDynamicMemberNames() =>
            Members.Keys.Union(new[] {"Int", "ChildObject"});

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            var targetType = binder.Type;

            if (targetType == typeof(IDictionary<string, object>) ||
                targetType == typeof(IDictionary))
            {
                result = new Dictionary<string, object>(Members);
                return true;
            }

            return base.TryConvert(binder, out result);
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder) =>
            Members.Remove(binder.Name);

        public override bool TryGetMember(GetMemberBinder binder, out object result) =>
            Members.TryGetValue(binder.Name, out result);

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Members[binder.Name] = value;
            return true;
        }
    }

    [Fact]
    public void DictionarySnakeCasePropertyNames_Disabled()
    {
        var values = new Dictionary<string, string>
        {
            {"First", "Value1!"},
            {"Second", "Value2!"}
        };

        var contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy()
        };

        var json = JsonConvert.SerializeObject(values, Formatting.Indented,
            new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

        XUnitAssert.AreEqualNormalized(@"{
  ""First"": ""Value1!"",
  ""Second"": ""Value2!""
}", json);
    }

    [Fact]
    public void DictionarySnakeCasePropertyNames_Enabled()
    {
        var values = new Dictionary<string, string>
        {
            {"First", "Value1!"},
            {"Second", "Value2!"}
        };

        var contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy
            {
                ProcessDictionaryKeys = true
            }
        };

        var json = JsonConvert.SerializeObject(values, Formatting.Indented,
            new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

        XUnitAssert.AreEqualNormalized(@"{
  ""first"": ""Value1!"",
  ""second"": ""Value2!""
}", json);
    }

    public class PropertyAttributeNamingStrategyTestClass
    {
        [JsonProperty] public string HasNoAttributeNamingStrategy { get; set; }

        [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
        public string HasAttributeNamingStrategy { get; set; }
    }

    [Fact]
    public void JsonPropertyAttribute_NamingStrategyType()
    {
        var c = new PropertyAttributeNamingStrategyTestClass
        {
            HasNoAttributeNamingStrategy = "Value1!",
            HasAttributeNamingStrategy = "Value2!"
        };

        var json = JsonConvert.SerializeObject(c, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"{
  ""HasNoAttributeNamingStrategy"": ""Value1!"",
  ""has_attribute_naming_strategy"": ""Value2!""
}", json);
    }

    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ContainerAttributeNamingStrategyTestClass
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }

        [JsonProperty(NamingStrategyType = typeof(DefaultNamingStrategy))]
        public string HasAttributeNamingStrategy { get; set; }
    }

    [Fact]
    public void JsonObjectAttribute_NamingStrategyType()
    {
        var c = new ContainerAttributeNamingStrategyTestClass
        {
            Prop1 = "Value1!",
            Prop2 = "Value2!"
        };

        var json = JsonConvert.SerializeObject(c, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"{
  ""prop1"": ""Value1!"",
  ""prop2"": ""Value2!"",
  ""HasAttributeNamingStrategy"": null
}", json);
    }

    [JsonDictionary(NamingStrategyType = typeof(SnakeCaseNamingStrategy), NamingStrategyParameters = new object[] {true, true})]
    public class DictionaryAttributeNamingStrategyTestClass : Dictionary<string, string>
    {
    }

    [Fact]
    public void JsonDictionaryAttribute_NamingStrategyType()
    {
        var c = new DictionaryAttributeNamingStrategyTestClass
        {
            ["Key1"] = "Value1!",
            ["Key2"] = "Value2!"
        };

        var json = JsonConvert.SerializeObject(c, Formatting.Indented);

        XUnitAssert.AreEqualNormalized(@"{
  ""key1"": ""Value1!"",
  ""key2"": ""Value2!""
}", json);
    }
}