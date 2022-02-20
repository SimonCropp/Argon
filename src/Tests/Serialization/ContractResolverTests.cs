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

using System.ComponentModel;
using Xunit;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
using System.Text.RegularExpressions;

namespace Argon.Tests.Serialization;

public class DynamicContractResolver : DefaultContractResolver
{
    readonly char _startingWithChar;

    public DynamicContractResolver(char startingWithChar)
    {
        _startingWithChar = startingWithChar;
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);

        // only serializer properties that start with the specified character
        properties =
            properties.Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();

        return properties;
    }
}

public class EscapedPropertiesContractResolver : DefaultContractResolver
{
    public string PropertyPrefix { get; set; }
    public string PropertySuffix { get; set; }

    protected override string ResolvePropertyName(string propertyName)
    {
        return base.ResolvePropertyName(PropertyPrefix + propertyName + PropertySuffix);
    }
}

public class Book
{
    public string BookName { get; set; }
    public decimal BookPrice { get; set; }
    public string AuthorName { get; set; }
    public int AuthorAge { get; set; }
    public string AuthorCountry { get; set; }
}

public class IPersonContractResolver : DefaultContractResolver
{
    protected override JsonContract CreateContract(Type objectType)
    {
        if (objectType == typeof(Employee))
        {
            objectType = typeof(IPerson);
        }

        return base.CreateContract(objectType);
    }
}

public class AddressWithDataMember
{
    [DataMember(Name = "CustomerAddress1")]
    public string AddressLine1 { get; set; }
}

public class ContractResolverTests : TestFixtureBase
{
    [Fact]
    public void ResolveSerializableContract()
    {
        var contractResolver = new DefaultContractResolver();
        var contract = contractResolver.ResolveContract(typeof(ISerializableTestObject));

        Assert.Equal(JsonContractType.Serializable, contract.ContractType);
    }

    [Fact]
    public void ResolveSerializableWithoutAttributeContract()
    {
        var contractResolver = new DefaultContractResolver();
        var contract = contractResolver.ResolveContract(typeof(ISerializableWithoutAttributeTestObject));

        Assert.Equal(JsonContractType.Object, contract.ContractType);
    }

    [Fact]
    public void ResolveObjectContractWithFieldsSerialization()
    {
        var contractResolver = new DefaultContractResolver
        {
            IgnoreSerializableAttribute = false
        };
        var contract = (JsonObjectContract)contractResolver.ResolveContract(typeof(AnswerFilterModel));

        Assert.Equal(MemberSerialization.Fields, contract.MemberSerialization);
    }

    [Fact]
    public void JsonPropertyDefaultValue()
    {
        var p = new JsonProperty();

        Assert.Equal(null, p.GetResolvedDefaultValue());
        Assert.Equal(null, p.DefaultValue);

        p.PropertyType = typeof(int);

        Assert.Equal(0, p.GetResolvedDefaultValue());
        Assert.Equal(null, p.DefaultValue);

        p.PropertyType = typeof(DateTime);

        Assert.Equal(new DateTime(), p.GetResolvedDefaultValue());
        Assert.Equal(null, p.DefaultValue);

        p.PropertyType = null;

        Assert.Equal(null, p.GetResolvedDefaultValue());
        Assert.Equal(null, p.DefaultValue);

        p.PropertyType = typeof(CompareOptions);

        Assert.Equal(CompareOptions.None, (CompareOptions)p.GetResolvedDefaultValue());
        Assert.Equal(null, p.DefaultValue);
    }

    [Fact]
    public void ListInterface()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonArrayContract)resolver.ResolveContract(typeof(IList<int>));

        Assert.True(contract.IsInstantiable);
        Assert.Equal(typeof(List<int>), contract.CreatedType);
        Assert.NotNull(contract.DefaultCreator);
    }

    [Fact]
    public void PropertyAttributeProvider()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(Invoice));

        var property = contract.Properties["FollowUpDays"];
        Assert.Equal(1, property.AttributeProvider.GetAttributes(false).Count);
        Assert.Equal(typeof(DefaultValueAttribute), property.AttributeProvider.GetAttributes(false)[0].GetType());
    }

    [Fact]
    public void AbstractTestClass()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonObjectContract) resolver.ResolveContract(typeof(AbstractTestClass));

        Assert.False(contract.IsInstantiable);
        Assert.Null(contract.DefaultCreator);
        Assert.Null(contract.OverrideCreator);

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<AbstractTestClass>(@"{Value:'Value!'}",
                new JsonSerializerSettings
                {
                    ContractResolver = resolver
                }),
            "Could not create an instance of type Argon.Tests.TestObjects.AbstractTestClass. Type is an interface or abstract class and cannot be instantiated. Path 'Value', line 1, position 7.");

        contract.DefaultCreator = () => new AbstractImplementationTestClass();

        var o = JsonConvert.DeserializeObject<AbstractTestClass>(
            @"{Value:'Value!'}",
            new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

        Assert.Equal("Value!", o.Value);
    }

    [Fact]
    public void AbstractListTestClass()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonArrayContract)resolver.ResolveContract(typeof(AbstractListTestClass<int>));

        Assert.False(contract.IsInstantiable);
        Assert.Null(contract.DefaultCreator);
        Assert.False(contract.HasParameterizedCreatorInternal);

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<AbstractListTestClass<int>>(@"[1,2]",
                new JsonSerializerSettings
        {
            ContractResolver = resolver
        }), 
            "Could not create an instance of type Argon.Tests.TestObjects.AbstractListTestClass`1[System.Int32]. Type is an interface or abstract class and cannot be instantiated. Path '', line 1, position 1.");

        contract.DefaultCreator = () => new AbstractImplementationListTestClass<int>();

        var l = JsonConvert.DeserializeObject<AbstractListTestClass<int>>(@"[1,2]",
            new JsonSerializerSettings
        {
            ContractResolver = resolver
        });

        Assert.Equal(2, l.Count);
        Assert.Equal(1, l[0]);
        Assert.Equal(2, l[1]);
    }

    public class CustomList<T> : List<T>
    {
    }

    [Fact]
    public void ListInterfaceDefaultCreator()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonArrayContract) resolver.ResolveContract(typeof(IList<int>));

        Assert.True(contract.IsInstantiable);
        Assert.NotNull(contract.DefaultCreator);

        contract.DefaultCreator = () => new CustomList<int>();

        var l = JsonConvert.DeserializeObject<IList<int>>(
            @"[1,2,3]",
            new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

        Assert.Equal(typeof(CustomList<int>), l.GetType());
        Assert.Equal(3, l.Count);
        Assert.Equal(1, l[0]);
        Assert.Equal(2, l[1]);
        Assert.Equal(3, l[2]);
    }

    public class CustomDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
    }

    [Fact]
    public void DictionaryInterfaceDefaultCreator()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonDictionaryContract)resolver.ResolveContract(typeof(IDictionary<string, int>));

        Assert.True(contract.IsInstantiable);
        Assert.NotNull(contract.DefaultCreator);

        contract.DefaultCreator = () => new CustomDictionary<string, int>();

        var d = JsonConvert.DeserializeObject<IDictionary<string, int>>(@"{key1:1,key2:2}", new JsonSerializerSettings
        {
            ContractResolver = resolver
        });

        Assert.Equal(typeof(CustomDictionary<string, int>), d.GetType());
        Assert.Equal(2, d.Count);
        Assert.Equal(1, d["key1"]);
        Assert.Equal(2, d["key2"]);
    }

    [Fact]
    public void AbstractDictionaryTestClass()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonDictionaryContract) resolver.ResolveContract(typeof(AbstractDictionaryTestClass<string, int>));

        Assert.False(contract.IsInstantiable);
        Assert.Null(contract.DefaultCreator);
        Assert.False(contract.HasParameterizedCreatorInternal);

        XUnitAssert.Throws<JsonSerializationException>(
            () => JsonConvert.DeserializeObject<AbstractDictionaryTestClass<string, int>>(@"{key1:1,key2:2}",
                new JsonSerializerSettings
                {
                    ContractResolver = resolver
                }),
            "Could not create an instance of type Argon.Tests.TestObjects.AbstractDictionaryTestClass`2[System.String,System.Int32]. Type is an interface or abstract class and cannot be instantiated. Path 'key1', line 1, position 6.");

        contract.DefaultCreator = () => new AbstractImplementationDictionaryTestClass<string, int>();

        var d = JsonConvert.DeserializeObject<AbstractDictionaryTestClass<string, int>>(
            @"{key1:1,key2:2}",
            new JsonSerializerSettings
            {
                ContractResolver = resolver
            });

        Assert.Equal(2, d.Count);
        Assert.Equal(1, d["key1"]);
        Assert.Equal(2, d["key2"]);
    }

    [Fact]
    public void SerializeWithEscapedPropertyName()
    {
        var json = JsonConvert.SerializeObject(
            new AddressWithDataMember
            {
                AddressLine1 = "value!"
            },
            new JsonSerializerSettings
            {
                ContractResolver = new EscapedPropertiesContractResolver
                {
                    PropertySuffix = @"-'-""-"
                }
            });

        Assert.Equal(@"{""AddressLine1-'-\""-"":""value!""}", json);

        var reader = new JsonTextReader(new StringReader(json));
        reader.Read();
        reader.Read();

        Assert.Equal(@"AddressLine1-'-""-", reader.Value);
    }

    [Fact]
    public void SerializeWithHtmlEscapedPropertyName()
    {
        var json = JsonConvert.SerializeObject(
            new AddressWithDataMember
            {
                AddressLine1 = "value!"
            },
            new JsonSerializerSettings
            {
                ContractResolver = new EscapedPropertiesContractResolver
                {
                    PropertyPrefix = "<b>",
                    PropertySuffix = "</b>"
                },
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            });

        Assert.Equal(@"{""\u003cb\u003eAddressLine1\u003c/b\u003e"":""value!""}", json);

        var reader = new JsonTextReader(new StringReader(json));
        reader.Read();
        reader.Read();

        Assert.Equal(@"<b>AddressLine1</b>", reader.Value);
    }

    [Fact]
    public void CalculatingPropertyNameEscapedSkipping()
    {
        var p = new JsonProperty { PropertyName = "abc" };
        Assert.True(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "123" };
        Assert.True(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "._-" };
        Assert.True(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "!@#" };
        Assert.True(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "$%^" };
        Assert.True(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "?*(" };
        Assert.True(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = ")_+" };
        Assert.True(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "=:," };
        Assert.True(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = null };
        Assert.True(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "&" };
        Assert.False(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "<" };
        Assert.False(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = ">" };
        Assert.False(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "'" };
        Assert.False(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = @"""" };
        Assert.False(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = Environment.NewLine };
        Assert.False(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "\0" };
        Assert.False(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "\n" };
        Assert.False(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "\v" };
        Assert.False(p._skipPropertyNameEscape);

        p = new JsonProperty { PropertyName = "\u00B9" };
        Assert.False(p._skipPropertyNameEscape);
    }

    [Fact]
    public void DeserializeDataMemberClassWithNoDataContract()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(AddressWithDataMember));

        Assert.Equal("AddressLine1", contract.Properties[0].PropertyName);
    }

    [Fact]
    public void ResolveProperties_IgnoreStatic()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(NumberFormatInfo));

        Assert.False(contract.Properties.Any(c => c.PropertyName == "InvariantInfo"));
    }

    [Fact]
    public void ParameterizedCreator()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(PublicParameterizedConstructorWithPropertyNameConflictWithAttribute));

        Assert.Null(contract.DefaultCreator);
        Assert.NotNull(contract.ParameterizedCreator);
        Assert.Equal(1, contract.CreatorParameters.Count);
        Assert.Equal("name", contract.CreatorParameters[0].PropertyName);

        contract.ParameterizedCreator = null;
        Assert.Null(contract.ParameterizedCreator);
    }

    [Fact]
    public void OverrideCreator()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(MultipleParametrizedConstructorsJsonConstructor));

        Assert.Null(contract.DefaultCreator);
        Assert.NotNull(contract.OverrideCreator);
        Assert.Equal(2, contract.CreatorParameters.Count);
        Assert.Equal("Value", contract.CreatorParameters[0].PropertyName);
        Assert.Equal("Age", contract.CreatorParameters[1].PropertyName);

        contract.OverrideCreator = null;
        Assert.Null(contract.OverrideCreator);
    }

    [Fact]
    public void CustomOverrideCreator()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(MultipleParametrizedConstructorsJsonConstructor));

        var ensureCustomCreatorCalled = false;

        contract.OverrideCreator = args =>
        {
            ensureCustomCreatorCalled = true;
            return new MultipleParametrizedConstructorsJsonConstructor((string)args[0], (int)args[1]);
        };
        Assert.NotNull(contract.OverrideCreator);

        var o = JsonConvert.DeserializeObject<MultipleParametrizedConstructorsJsonConstructor>("{Value:'value!', Age:1}", new JsonSerializerSettings
        {
            ContractResolver = resolver
        });

        Assert.Equal("value!", o.Value);
        Assert.Equal(1, o.Age);
        Assert.True(ensureCustomCreatorCalled);
    }

    [Fact]
    public void SerializeInterface()
    {
        var employee = new Employee
        {
            BirthDate = new DateTime(1977, 12, 30, 1, 1, 1, DateTimeKind.Utc),
            FirstName = "Maurice",
            LastName = "Moss",
            Department = "IT",
            JobTitle = "Support"
        };

        var iPersonJson = JsonConvert.SerializeObject(employee, Formatting.Indented,
            new JsonSerializerSettings { ContractResolver = new IPersonContractResolver() });

        var o = JObject.Parse(iPersonJson);

        Assert.Equal("Maurice", (string)o["FirstName"]);
        Assert.Equal("Moss", (string)o["LastName"]);
        Assert.Equal(new DateTime(1977, 12, 30, 1, 1, 1, DateTimeKind.Utc), (DateTime)o["BirthDate"]);
    }

    [Fact]
    public void SingleTypeWithMultipleContractResolvers()
    {
        var book = new Book
        {
            BookName = "The Gathering Storm",
            BookPrice = 16.19m,
            AuthorName = "Brandon Sanderson",
            AuthorAge = 34,
            AuthorCountry = "United States of America"
        };

        var startingWithA = JsonConvert.SerializeObject(book, Formatting.Indented,
            new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('A') });

        // {
        //   "AuthorName": "Brandon Sanderson",
        //   "AuthorAge": 34,
        //   "AuthorCountry": "United States of America"
        // }

        var startingWithB = JsonConvert.SerializeObject(book, Formatting.Indented,
            new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('B') });

        // {
        //   "BookName": "The Gathering Storm",
        //   "BookPrice": 16.19
        // }

        XUnitAssert.AreEqualNormalized(@"{
  ""AuthorName"": ""Brandon Sanderson"",
  ""AuthorAge"": 34,
  ""AuthorCountry"": ""United States of America""
}", startingWithA);

        XUnitAssert.AreEqualNormalized(@"{
  ""BookName"": ""The Gathering Storm"",
  ""BookPrice"": 16.19
}", startingWithB);
    }

#pragma warning disable 618
    [Fact]
    public void SerializeCompilerGeneratedMembers()
    {
        var structTest = new StructTest
        {
            IntField = 1,
            IntProperty = 2,
            StringField = "Field",
            StringProperty = "Property"
        };

        var skipCompilerGeneratedResolver = new DefaultContractResolver();

        var skipCompilerGeneratedJson = JsonConvert.SerializeObject(structTest, Formatting.Indented,
            new JsonSerializerSettings { ContractResolver = skipCompilerGeneratedResolver });

        XUnitAssert.AreEqualNormalized(@"{
  ""StringField"": ""Field"",
  ""IntField"": 1,
  ""StringProperty"": ""Property"",
  ""IntProperty"": 2
}", skipCompilerGeneratedJson);

        var includeCompilerGeneratedResolver = new IncludeCompilerGeneratedResolver
        {
            SerializeCompilerGeneratedMembers = true
        };

        var includeCompilerGeneratedJson = JsonConvert.SerializeObject(structTest, Formatting.Indented,
            new JsonSerializerSettings
            {
                ContractResolver = includeCompilerGeneratedResolver
            });

        var o = JObject.Parse(includeCompilerGeneratedJson);

        Console.WriteLine(includeCompilerGeneratedJson);

        Assert.Equal("Property", (string)o["<StringProperty>k__BackingField"]);
        Assert.Equal(2, (int)o["<IntProperty>k__BackingField"]);
    }

    public class IncludeCompilerGeneratedResolver : DefaultContractResolver
    {
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var members = base.GetSerializableMembers(objectType);
            var compilerGenerated = objectType.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(x=>x.Name.StartsWith('<'));
            members.AddRange(compilerGenerated);
            return members;
        }
    }
#pragma warning restore 618

    public class ClassWithExtensionData
    {
        [JsonExtensionData]
        public IDictionary<string, object> Data { get; set; }
    }

    [Fact]
    public void ExtensionDataGetterCanBeIteratedMultipleTimes()
    {
        var resolver = new DefaultContractResolver();
        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(ClassWithExtensionData));

        var myClass = new ClassWithExtensionData
        {
            Data = new Dictionary<string, object>
            {
                { "SomeField", "Field" },
            }
        };

        var getter = contract.ExtensionDataGetter;

        IEnumerable<KeyValuePair<object, object>> dictionaryData = getter(myClass).ToDictionary(kv => kv.Key, kv => kv.Value);
        Assert.True(dictionaryData.Any());
        Assert.True(dictionaryData.Any());

        var extensionData = getter(myClass);
        Assert.True(extensionData.Any());
        Assert.True(extensionData.Any()); // second test fails if the enumerator returned isn't reset
    }

    public class ClassWithShouldSerialize
    {
        public string Prop1 { get; set; }
        public string Prop2 { get; set; }

        public bool ShouldSerializeProp1()
        {
            return false;
        }
    }

    [Fact]
    public void DefaultContractResolverIgnoreShouldSerializeTrue()
    {
        var resolver = new DefaultContractResolver
        {
            IgnoreShouldSerializeMembers = true
        };

        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(ClassWithShouldSerialize));

        var property1 = contract.Properties["Prop1"];
        Assert.Equal(null, property1.ShouldSerialize);

        var property2 = contract.Properties["Prop2"];
        Assert.Equal(null, property2.ShouldSerialize);
    }

    [Fact]
    public void DefaultContractResolverIgnoreShouldSerializeUnset()
    {
        var resolver = new DefaultContractResolver();

        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(ClassWithShouldSerialize));

        var property1 = contract.Properties["Prop1"];
        Assert.NotEqual(null, property1.ShouldSerialize);

        var property2 = contract.Properties["Prop2"];
        Assert.Equal(null, property2.ShouldSerialize);
    }

    public class ClassWithIsSpecified
    {
        [JsonProperty]
        public string Prop1 { get; set; }
        [JsonProperty]
        public string Prop2 { get; set; }
        [JsonProperty]
        public string Prop3 { get; set; }
        [JsonProperty]
        public string Prop4 { get; set; }
        [JsonProperty]
        public string Prop5 { get; set; }

        public bool Prop1Specified;
        public bool Prop2Specified { get; set; }
        public static bool Prop3Specified { get; set; }
        public event Func<bool> Prop4Specified;
        public static bool Prop5Specified;

        protected virtual bool OnProp4Specified()
        {
            return Prop4Specified?.Invoke() ?? false;
        }
    }

    [Fact]
    public void NonGenericDictionary_KeyValueTypes()
    {
        var resolver = new DefaultContractResolver();

        var c = (JsonDictionaryContract)resolver.ResolveContract(typeof(IDictionary));

        Assert.Null(c.DictionaryKeyType);
        Assert.Null(c.DictionaryValueType);
    }

    [Fact]
    public void DefaultContractResolverIgnoreIsSpecifiedTrue()
    {
        var resolver = new DefaultContractResolver
        {
            IgnoreIsSpecifiedMembers = true
        };

        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(ClassWithIsSpecified));

        var property1 = contract.Properties["Prop1"];
        Assert.Equal(null, property1.GetIsSpecified);
        Assert.Equal(null, property1.SetIsSpecified);

        var property2 = contract.Properties["Prop2"];
        Assert.Equal(null, property2.GetIsSpecified);
        Assert.Equal(null, property2.SetIsSpecified);

        var property3 = contract.Properties["Prop3"];
        Assert.Equal(null, property3.GetIsSpecified);
        Assert.Equal(null, property3.SetIsSpecified);

        var property4 = contract.Properties["Prop4"];
        Assert.Equal(null, property4.GetIsSpecified);
        Assert.Equal(null, property4.SetIsSpecified);

        var property5 = contract.Properties["Prop5"];
        Assert.Equal(null, property5.GetIsSpecified);
        Assert.Equal(null, property5.SetIsSpecified);
    }

    [Fact]
    public void DefaultContractResolverIgnoreIsSpecifiedUnset()
    {
        var resolver = new DefaultContractResolver();

        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(ClassWithIsSpecified));

        var property1 = contract.Properties["Prop1"];
        Assert.NotEqual(null, property1.GetIsSpecified);
        Assert.NotEqual(null, property1.SetIsSpecified);

        var property2 = contract.Properties["Prop2"];
        Assert.NotEqual(null, property2.GetIsSpecified);
        Assert.NotEqual(null, property2.SetIsSpecified);

        var property3 = contract.Properties["Prop3"];
        Assert.Equal(null, property3.GetIsSpecified);
        Assert.Equal(null, property3.SetIsSpecified);

        var property4 = contract.Properties["Prop4"];
        Assert.Equal(null, property4.GetIsSpecified);
        Assert.Equal(null, property4.SetIsSpecified);

        var property5 = contract.Properties["Prop5"];
        Assert.Equal(null, property5.GetIsSpecified);
        Assert.Equal(null, property5.SetIsSpecified);
    }

    [Fact]
    public void JsonRequiredAttribute()
    {
        var resolver = new DefaultContractResolver();

        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(RequiredPropertyTestClass));

        var property1 = contract.Properties["Name"];

        Assert.Equal(Required.Always, property1.Required);
        XUnitAssert.True(property1.IsRequiredSpecified);
    }

    [Fact]
    public void JsonPropertyAttribute_Required()
    {
        var resolver = new DefaultContractResolver();

        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(RequiredObject));

        var unset = contract.Properties["UnsetProperty"];

        Assert.Equal(Required.Default, unset.Required);
        XUnitAssert.False(unset.IsRequiredSpecified);

        var allowNull = contract.Properties["AllowNullProperty"];

        Assert.Equal(Required.AllowNull, allowNull.Required);
        XUnitAssert.True(allowNull.IsRequiredSpecified);
    }

    [Fact]
    public void InternalConverter_Object_NotSet()
    {
        var resolver = new DefaultContractResolver();

        var contract = (JsonObjectContract)resolver.ResolveContract(typeof(object));

        Assert.Null(contract.InternalConverter);
    }

    [Fact]
    public void InternalConverter_Regex_Set()
    {
        var resolver = new DefaultContractResolver();

        var contract = resolver.ResolveContract(typeof(Regex));

        Assert.IsType(typeof(RegexConverter), contract.InternalConverter);
    }
}