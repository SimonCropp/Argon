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

using System;
using System.Collections.Generic;
using Argon.Serialization;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
using Argon.Linq;

namespace Argon.Tests.Serialization
{
    [TestFixture]
    public class CamelCaseNamingStrategyTests : TestFixtureBase
    {
        [Fact]
        public void JsonConvertSerializerSettings()
        {
            var person = new Person();
            person.BirthDate = new DateTime(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc);
            person.LastModified = new DateTime(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc);
            person.Name = "Name!";

            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var json = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

            StringAssert.AreEqual(@"{
  ""name"": ""Name!"",
  ""birthDate"": ""2000-11-20T23:55:44Z"",
  ""lastModified"": ""2000-11-20T23:55:44Z""
}", json);

            var deserializedPerson = JsonConvert.DeserializeObject<Person>(json, new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            });

            Assert.AreEqual(person.BirthDate, deserializedPerson.BirthDate);
            Assert.AreEqual(person.LastModified, deserializedPerson.LastModified);
            Assert.AreEqual(person.Name, deserializedPerson.Name);

            json = JsonConvert.SerializeObject(person, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Name"": ""Name!"",
  ""BirthDate"": ""2000-11-20T23:55:44Z"",
  ""LastModified"": ""2000-11-20T23:55:44Z""
}", json);
        }

        [Fact]
        public void JTokenWriter_OverrideSpecifiedName()
        {
            var ignoreAttributeOnClassTestClass = new JsonIgnoreAttributeOnClassTestClass();
            ignoreAttributeOnClassTestClass.Field = int.MinValue;

            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = true
                }
            };

            var serializer = new JsonSerializer();
            serializer.ContractResolver = contractResolver;

            var writer = new JTokenWriter();

            serializer.Serialize(writer, ignoreAttributeOnClassTestClass);

            var o = (JObject)writer.Token;
            var p = o.Property("theField");

            Assert.IsNotNull(p);
            Assert.AreEqual(int.MinValue, (int)p.Value);
        }

        [Fact]
        public void BlogPostExample()
        {
            var product = new Product
            {
                ExpiryDate = new DateTime(2010, 12, 20, 18, 1, 0, DateTimeKind.Utc),
                Name = "Widget",
                Price = 9.99m,
                Sizes = new[] { "Small", "Medium", "Large" }
            };

            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var json =
                JsonConvert.SerializeObject(
                    product,
                    Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = contractResolver }
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

            StringAssert.AreEqual(@"{
  ""name"": ""Widget"",
  ""expiryDate"": ""2010-12-20T18:01:00Z"",
  ""price"": 9.99,
  ""sizes"": [
    ""Small"",
    ""Medium"",
    ""Large""
  ]
}", json);
        }

        [Fact]
        public void DynamicCamelCasePropertyNames()
        {
            dynamic o = new TestDynamicObject();
            o.Text = "Text!";
            o.Integer = int.MaxValue;

            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true
                }
            };

            string json = JsonConvert.SerializeObject(o, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                });

            StringAssert.AreEqual(@"{
  ""explicit"": false,
  ""text"": ""Text!"",
  ""integer"": 2147483647,
  ""int"": 0,
  ""childObject"": null
}", json);
        }

        [Fact]
        public void DictionaryCamelCasePropertyNames_Disabled()
        {
            var values = new Dictionary<string, string>
            {
                { "First", "Value1!" },
                { "Second", "Value2!" }
            };

            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var json = JsonConvert.SerializeObject(values, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                });

            StringAssert.AreEqual(@"{
  ""First"": ""Value1!"",
  ""Second"": ""Value2!""
}", json);
        }

        [Fact]
        public void DictionaryCamelCasePropertyNames_Enabled()
        {
            var values = new Dictionary<string, string>
            {
                { "First", "Value1!" },
                { "Second", "Value2!" }
            };

            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = true
                }
            };

            var json = JsonConvert.SerializeObject(values, Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = contractResolver
                });

            StringAssert.AreEqual(@"{
  ""first"": ""Value1!"",
  ""second"": ""Value2!""
}", json);
        }

        public class PropertyAttributeNamingStrategyTestClass
        {
            [JsonProperty]
            public string HasNoAttributeNamingStrategy { get; set; }

            [JsonProperty(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
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

            StringAssert.AreEqual(@"{
  ""HasNoAttributeNamingStrategy"": ""Value1!"",
  ""hasAttributeNamingStrategy"": ""Value2!""
}", json);
        }

        [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
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

            StringAssert.AreEqual(@"{
  ""prop1"": ""Value1!"",
  ""prop2"": ""Value2!"",
  ""HasAttributeNamingStrategy"": null
}", json);
        }

        [JsonDictionary(NamingStrategyType = typeof(CamelCaseNamingStrategy), NamingStrategyParameters = new object[] { true, true })]
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

            StringAssert.AreEqual(@"{
  ""key1"": ""Value1!"",
  ""key2"": ""Value2!""
}", json);
        }
    }
}