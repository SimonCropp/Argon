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

using System.Collections.Specialized;
using System.ComponentModel;
using Argon.Tests.TestObjects;
using Argon.Tests.TestObjects.Organization;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
#if !NET5_0_OR_GREATER
using System.Web.UI;
#endif

namespace Argon.Tests.Linq
{
    [TestFixture]
    public class JObjectTests : TestFixtureBase
    {
        [Fact]
        public void EmbedJValueStringInNewJObject()
        {
            string s = null;
            var v = new JValue(s);
            dynamic o = JObject.FromObject(new { title = v });

            string output = o.ToString();

            StringAssert.AreEqual(@"{
  ""title"": null
}", output);

            Assert.AreEqual(null, v.Value);
            Assert.IsNull((string)o.title);
        }

        [Fact]
        public void ReadWithSupportMultipleContent()
        {
            var json = @"{ 'name': 'Admin' }{ 'name': 'Publisher' }";

            IList<JObject> roles = new List<JObject>();

            var reader = new JsonTextReader(new StringReader(json));
            reader.SupportMultipleContent = true;

            while (true)
            {
                var role = (JObject)JToken.ReadFrom(reader);

                roles.Add(role);

                if (!reader.Read())
                {
                    break;
                }
            }

            Assert.AreEqual(2, roles.Count);
            Assert.AreEqual("Admin", (string)roles[0]["name"]);
            Assert.AreEqual("Publisher", (string)roles[1]["name"]);
        }

        [Fact]
        public void JObjectWithComments()
        {
            var json = @"{ /*comment2*/
        ""Name"": /*comment3*/ ""Apple"" /*comment4*/, /*comment5*/
        ""ExpiryDate"": ""\/Date(1230422400000)\/"",
        ""Price"": 3.99,
        ""Sizes"": /*comment6*/ [ /*comment7*/
          ""Small"", /*comment8*/
          ""Medium"" /*comment9*/,
          /*comment10*/ ""Large""
        /*comment11*/ ] /*comment12*/
      } /*comment13*/";

            var o = JToken.Parse(json);

            Assert.AreEqual("Apple", (string)o["Name"]);
        }

        [Fact]
        public void WritePropertyWithNoValue()
        {
            var o = new JObject {new JProperty("novalue")};

            StringAssert.AreEqual(@"{
  ""novalue"": null
}", o.ToString());
        }

        [Fact]
        public void Keys()
        {
            var o = new JObject();
            var d = (IDictionary<string, JToken>)o;

            Assert.AreEqual(0, d.Keys.Count);

            o["value"] = true;

            Assert.AreEqual(1, d.Keys.Count);
        }

        [Fact]
        public void TryGetValue()
        {
            var o = new JObject {{"PropertyNameValue", new JValue(1)}};
            Assert.AreEqual(1, o.Children().Count());

            Assert.AreEqual(false, o.TryGetValue("sdf", out var t));
            Assert.AreEqual(null, t);

            Assert.AreEqual(false, o.TryGetValue(null, out t));
            Assert.AreEqual(null, t);

            Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
            Assert.AreEqual(true, JToken.DeepEquals(new JValue(1), t));
        }

        [Fact]
        public void DictionaryItemShouldSet()
        {
            var o = new JObject
            {
                ["PropertyNameValue"] = new JValue(1)
            };
            Assert.AreEqual(1, o.Children().Count());

            Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out var t));
            Assert.AreEqual(true, JToken.DeepEquals(new JValue(1), t));

            o["PropertyNameValue"] = new JValue(2);
            Assert.AreEqual(1, o.Children().Count());

            Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
            Assert.AreEqual(true, JToken.DeepEquals(new JValue(2), t));

            o["PropertyNameValue"] = null;
            Assert.AreEqual(1, o.Children().Count());

            Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
            Assert.AreEqual(true, JToken.DeepEquals(JValue.CreateNull(), t));
        }

        [Fact]
        public void Remove()
        {
            var o = new JObject {{"PropertyNameValue", new JValue(1)}};
            Assert.AreEqual(1, o.Children().Count());

            Assert.AreEqual(false, o.Remove("sdf"));
            Assert.AreEqual(false, o.Remove(null));
            Assert.AreEqual(true, o.Remove("PropertyNameValue"));

            Assert.AreEqual(0, o.Children().Count());
        }

        [Fact]
        public void GenericCollectionRemove()
        {
            var v = new JValue(1);
            var o = new JObject {{"PropertyNameValue", v}};
            Assert.AreEqual(1, o.Children().Count());

            Assert.AreEqual(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue1", new JValue(1))));
            Assert.AreEqual(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(2))));
            Assert.AreEqual(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(1))));
            Assert.AreEqual(true, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", v)));

            Assert.AreEqual(0, o.Children().Count());
        }

        [Fact]
        public void DuplicatePropertyNameShouldThrow()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var o = new JObject
                {
                    {"PropertyNameValue", null},
                    {"PropertyNameValue", null}
                };
            }, "Can not add property PropertyNameValue to Argon.Linq.JObject. Property with the same name already exists on object.");
        }

        [Fact]
        public void GenericDictionaryAdd()
        {
            var o = new JObject {{"PropertyNameValue", new JValue(1)}};

            Assert.AreEqual(1, (int)o["PropertyNameValue"]);

            o.Add("PropertyNameValue1", null);
            Assert.AreEqual(null, ((JValue)o["PropertyNameValue1"]).Value);

            Assert.AreEqual(2, o.Children().Count());
        }

        [Fact]
        public void GenericCollectionAdd()
        {
            var o = new JObject();
            ((ICollection<KeyValuePair<string, JToken>>)o).Add(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(1)));

            Assert.AreEqual(1, (int)o["PropertyNameValue"]);
            Assert.AreEqual(1, o.Children().Count());
        }

        [Fact]
        public void GenericCollectionClear()
        {
            var o = new JObject {{"PropertyNameValue", new JValue(1)}};
            Assert.AreEqual(1, o.Children().Count());

            var p = (JProperty)o.Children().ElementAt(0);

            ((ICollection<KeyValuePair<string, JToken>>)o).Clear();
            Assert.AreEqual(0, o.Children().Count());

            Assert.AreEqual(null, p.Parent);
        }

        [Fact]
        public void GenericCollectionContains()
        {
            var v = new JValue(1);
            var o = new JObject {{"PropertyNameValue", v}};
            Assert.AreEqual(1, o.Children().Count());

            var contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(1)));
            Assert.AreEqual(false, contains);

            contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", v));
            Assert.AreEqual(true, contains);

            contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(2)));
            Assert.AreEqual(false, contains);

            contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue1", new JValue(1)));
            Assert.AreEqual(false, contains);

            contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(default(KeyValuePair<string, JToken>));
            Assert.AreEqual(false, contains);
        }

        [Fact]
        public void Contains()
        {
            var o = new JObject {{"PropertyNameValue", new JValue(1)}};
            Assert.AreEqual(1, o.Children().Count());

            var contains = o.ContainsKey("PropertyNameValue");
            Assert.AreEqual(true, contains);

            contains = o.ContainsKey("does not exist");
            Assert.AreEqual(false, contains);

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                contains = o.ContainsKey(null);
                Assert.AreEqual(false, contains);
            },
            @"Value cannot be null.
Parameter name: propertyName",
            "Value cannot be null. (Parameter 'propertyName')");
        }

        [Fact]
        public void GenericDictionaryContains()
        {
            var o = new JObject {{"PropertyNameValue", new JValue(1)}};
            Assert.AreEqual(1, o.Children().Count());

            var contains = ((IDictionary<string, JToken>)o).ContainsKey("PropertyNameValue");
            Assert.AreEqual(true, contains);
        }

        [Fact]
        public void GenericCollectionCopyTo()
        {
            var o = new JObject
            {
                {"PropertyNameValue", new JValue(1)},
                {"PropertyNameValue2", new JValue(2)},
                {"PropertyNameValue3", new JValue(3)}
            };
            Assert.AreEqual(3, o.Children().Count());

            var a = new KeyValuePair<string, JToken>[5];

            ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(a, 1);

            Assert.AreEqual(default(KeyValuePair<string, JToken>), a[0]);

            Assert.AreEqual("PropertyNameValue", a[1].Key);
            Assert.AreEqual(1, (int)a[1].Value);

            Assert.AreEqual("PropertyNameValue2", a[2].Key);
            Assert.AreEqual(2, (int)a[2].Value);

            Assert.AreEqual("PropertyNameValue3", a[3].Key);
            Assert.AreEqual(3, (int)a[3].Value);

            Assert.AreEqual(default(KeyValuePair<string, JToken>), a[4]);
        }

        [Fact]
        public void GenericCollectionCopyToNullArrayShouldThrow()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var o = new JObject();
                ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(null, 0);
            },
            @"Value cannot be null.
Parameter name: array",
            "Value cannot be null. (Parameter 'array')");
        }

        [Fact]
        public void GenericCollectionCopyToNegativeArrayIndexShouldThrow()
        {
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var o = new JObject();
                ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[1], -1);
            },
            @"arrayIndex is less than 0.
Parameter name: arrayIndex",
            "arrayIndex is less than 0. (Parameter 'arrayIndex')");
        }

        [Fact]
        public void GenericCollectionCopyToArrayIndexEqualGreaterToArrayLengthShouldThrow()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var o = new JObject();
                ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[1], 1);
            }, @"arrayIndex is equal to or greater than the length of array.");
        }

        [Fact]
        public void GenericCollectionCopyToInsufficientArrayCapacity()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var o = new JObject
                {
                    {"PropertyNameValue", new JValue(1)},
                    {"PropertyNameValue2", new JValue(2)},
                    {"PropertyNameValue3", new JValue(3)}
                };

                ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[3], 1);
            }, @"The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
        }

        [Fact]
        public void FromObjectRaw()
        {
            var raw = new PersonRaw
            {
                FirstName = "FirstNameValue",
                RawContent = new JRaw("[1,2,3,4,5]"),
                LastName = "LastNameValue"
            };

            var o = JObject.FromObject(raw);

            Assert.AreEqual("FirstNameValue", (string)o["first_name"]);
            Assert.AreEqual(JTokenType.Raw, ((JValue)o["RawContent"]).Type);
            Assert.AreEqual("[1,2,3,4,5]", (string)o["RawContent"]);
            Assert.AreEqual("LastNameValue", (string)o["last_name"]);
        }

        [Fact]
        public void JTokenReader()
        {
            var raw = new PersonRaw
            {
                FirstName = "FirstNameValue",
                RawContent = new JRaw("[1,2,3,4,5]"),
                LastName = "LastNameValue"
            };

            var o = JObject.FromObject(raw);

            JsonReader reader = new JTokenReader(o);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Raw, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
        }

        [Fact]
        public void DeserializeFromRaw()
        {
            var raw = new PersonRaw
            {
                FirstName = "FirstNameValue",
                RawContent = new JRaw("[1,2,3,4,5]"),
                LastName = "LastNameValue"
            };

            var o = JObject.FromObject(raw);

            JsonReader reader = new JTokenReader(o);
            var serializer = new JsonSerializer();
            raw = (PersonRaw)serializer.Deserialize(reader, typeof(PersonRaw));

            Assert.AreEqual("FirstNameValue", raw.FirstName);
            Assert.AreEqual("LastNameValue", raw.LastName);
            Assert.AreEqual("[1,2,3,4,5]", raw.RawContent.Value);
        }

        [Fact]
        public void Parse_ShouldThrowOnUnexpectedToken()
        {
            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                var json = @"[""prop""]";
                JObject.Parse(json);
            }, "Error reading JObject from JsonReader. Current JsonReader item is not an object: StartArray. Path '', line 1, position 1.");
        }

        [Fact]
        public void ParseJavaScriptDate()
        {
            var json = @"[new Date(1207285200000)]";

            var a = (JArray)JsonConvert.DeserializeObject(json);
            var v = (JValue)a[0];

            Assert.AreEqual(DateTimeUtils.ConvertJavaScriptTicksToDateTime(1207285200000), (DateTime)v);
        }

        [Fact]
        public void GenericValueCast()
        {
            var json = @"{""foo"":true}";
            var o = (JObject)JsonConvert.DeserializeObject(json);
            var value = o.Value<bool?>("foo");
            Assert.AreEqual(true, value);

            json = @"{""foo"":null}";
            o = (JObject)JsonConvert.DeserializeObject(json);
            value = o.Value<bool?>("foo");
            Assert.AreEqual(null, value);
        }

        [Fact]
        public void Blog()
        {
            ExceptionAssert.Throws<JsonReaderException>(() => { JObject.Parse(@"{
    ""name"": ""James"",
    ]!#$THIS IS: BAD JSON![{}}}}]
  }"); }, "Invalid property identifier character: ]. Path 'name', line 3, position 4.");
        }

        [Fact]
        public void RawChildValues()
        {
            var o = new JObject
            {
                ["val1"] = new JRaw("1"),
                ["val2"] = new JRaw("1")
            };

            var json = o.ToString();

            StringAssert.AreEqual(@"{
  ""val1"": 1,
  ""val2"": 1
}", json);
        }

        [Fact]
        public void Iterate()
        {
            var o = new JObject
            {
                {"PropertyNameValue1", new JValue(1)},
                {"PropertyNameValue2", new JValue(2)}
            };

            JToken t = o;

            var i = 1;
            foreach (JProperty property in t)
            {
                Assert.AreEqual("PropertyNameValue" + i, property.Name);
                Assert.AreEqual(i, (int)property.Value);

                i++;
            }
        }

        [Fact]
        public void KeyValuePairIterate()
        {
            var o = new JObject
            {
                {"PropertyNameValue1", new JValue(1)},
                {"PropertyNameValue2", new JValue(2)}
            };

            var i = 1;
            foreach (var pair in o)
            {
                Assert.AreEqual("PropertyNameValue" + i, pair.Key);
                Assert.AreEqual(i, (int)pair.Value);

                i++;
            }
        }

        [Fact]
        public void WriteObjectNullStringValue()
        {
            string s = null;
            var v = new JValue(s);
            Assert.AreEqual(null, v.Value);
            Assert.AreEqual(JTokenType.String, v.Type);

            var o = new JObject
            {
                ["title"] = v
            };

            var output = o.ToString();

            StringAssert.AreEqual(@"{
  ""title"": null
}", output);
        }

        [Fact]
        public void Example()
        {
            var json = @"{
        ""Name"": ""Apple"",
        ""Expiry"": new Date(1230422400000),
        ""Price"": 3.99,
        ""Sizes"": [
          ""Small"",
          ""Medium"",
          ""Large""
        ]
      }";

            var o = JObject.Parse(json);

            var name = (string)o["Name"];
            // Apple

            var sizes = (JArray)o["Sizes"];

            var smallest = (string)sizes[0];
            // Small

            Assert.AreEqual("Apple", name);
            Assert.AreEqual("Small", smallest);
        }

        [Fact]
        public void DeserializeClassManually()
        {
            var jsonText = @"{
  ""short"":
  {
    ""original"":""http://www.foo.com/"",
    ""short"":""krehqk"",
    ""error"":
    {
      ""code"":0,
      ""msg"":""No action taken""
    }
  }
}";

            var json = JObject.Parse(jsonText);

            var shortie = new Shortie
            {
                Original = (string)json["short"]["original"],
                Short = (string)json["short"]["short"],
                Error = new ShortieException
                {
                    Code = (int)json["short"]["error"]["code"],
                    ErrorMessage = (string)json["short"]["error"]["msg"]
                }
            };

            Assert.AreEqual("http://www.foo.com/", shortie.Original);
            Assert.AreEqual("krehqk", shortie.Short);
            Assert.AreEqual(null, shortie.Shortened);
            Assert.AreEqual(0, shortie.Error.Code);
            Assert.AreEqual("No action taken", shortie.Error.ErrorMessage);
        }

        [Fact]
        public void JObjectContainingHtml()
        {
            var o = new JObject
            {
                ["rc"] = new JValue(200),
                ["m"] = new JValue(""),
                ["o"] = new JValue(@"<div class='s1'>" + StringUtils.CarriageReturnLineFeed + @"</div>")
            };

            StringAssert.AreEqual(@"{
  ""rc"": 200,
  ""m"": """",
  ""o"": ""<div class='s1'>\r\n</div>""
}", o.ToString());
        }

        [Fact]
        public void ImplicitValueConversions()
        {
            var moss = new JObject
            {
                ["FirstName"] = new JValue("Maurice"),
                ["LastName"] = new JValue("Moss"),
                ["BirthDate"] = new JValue(new DateTime(1977, 12, 30)),
                ["Department"] = new JValue("IT"),
                ["JobTitle"] = new JValue("Support")
            };

            StringAssert.AreEqual(@"{
  ""FirstName"": ""Maurice"",
  ""LastName"": ""Moss"",
  ""BirthDate"": ""1977-12-30T00:00:00"",
  ""Department"": ""IT"",
  ""JobTitle"": ""Support""
}", moss.ToString());

            var jen = new JObject
            {
                ["FirstName"] = "Jen",
                ["LastName"] = "Barber",
                ["BirthDate"] = new DateTime(1978, 3, 15),
                ["Department"] = "IT",
                ["JobTitle"] = "Manager"
            };

            StringAssert.AreEqual(@"{
  ""FirstName"": ""Jen"",
  ""LastName"": ""Barber"",
  ""BirthDate"": ""1978-03-15T00:00:00"",
  ""Department"": ""IT"",
  ""JobTitle"": ""Manager""
}", jen.ToString());
        }

        [Fact]
        public void ReplaceJPropertyWithJPropertyWithSameName()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");

            var o = new JObject(p1, p2);
            IList l = o;
            Assert.AreEqual(p1, l[0]);
            Assert.AreEqual(p2, l[1]);

            var p3 = new JProperty("Test1", "III");

            p1.Replace(p3);
            Assert.AreEqual(null, p1.Parent);
            Assert.AreEqual(l, p3.Parent);

            Assert.AreEqual(p3, l[0]);
            Assert.AreEqual(p2, l[1]);

            Assert.AreEqual(2, l.Count);
            Assert.AreEqual(2, o.Properties().Count());

            var p4 = new JProperty("Test4", "IV");

            p2.Replace(p4);
            Assert.AreEqual(null, p2.Parent);
            Assert.AreEqual(l, p4.Parent);

            Assert.AreEqual(p3, l[0]);
            Assert.AreEqual(p4, l[1]);
        }

        [Fact]
        public void PropertyChanging()
        {
            object changing = null;
            object changed = null;
            var changingCount = 0;
            var changedCount = 0;

            var o = new JObject();
            o.PropertyChanging += (sender, args) =>
            {
                var s = (JObject)sender;
                changing = s[args.PropertyName] != null ? ((JValue)s[args.PropertyName]).Value : null;
                changingCount++;
            };
            o.PropertyChanged += (sender, args) =>
            {
                var s = (JObject)sender;
                changed = s[args.PropertyName] != null ? ((JValue)s[args.PropertyName]).Value : null;
                changedCount++;
            };

            o["StringValue"] = "value1";
            Assert.AreEqual(null, changing);
            Assert.AreEqual("value1", changed);
            Assert.AreEqual("value1", (string)o["StringValue"]);
            Assert.AreEqual(1, changingCount);
            Assert.AreEqual(1, changedCount);

            o["StringValue"] = "value1";
            Assert.AreEqual(1, changingCount);
            Assert.AreEqual(1, changedCount);

            o["StringValue"] = "value2";
            Assert.AreEqual("value1", changing);
            Assert.AreEqual("value2", changed);
            Assert.AreEqual("value2", (string)o["StringValue"]);
            Assert.AreEqual(2, changingCount);
            Assert.AreEqual(2, changedCount);

            o["StringValue"] = null;
            Assert.AreEqual("value2", changing);
            Assert.AreEqual(null, changed);
            Assert.AreEqual(null, (string)o["StringValue"]);
            Assert.AreEqual(3, changingCount);
            Assert.AreEqual(3, changedCount);

            o["NullValue"] = null;
            Assert.AreEqual(null, changing);
            Assert.AreEqual(null, changed);
            Assert.AreEqual(JValue.CreateNull(), o["NullValue"]);
            Assert.AreEqual(4, changingCount);
            Assert.AreEqual(4, changedCount);

            o["NullValue"] = null;
            Assert.AreEqual(4, changingCount);
            Assert.AreEqual(4, changedCount);
        }

        [Fact]
        public void PropertyChanged()
        {
            object changed = null;
            var changedCount = 0;

            var o = new JObject();
            o.PropertyChanged += (sender, args) =>
            {
                var s = (JObject)sender;
                changed = s[args.PropertyName] != null ? ((JValue)s[args.PropertyName]).Value : null;
                changedCount++;
            };

            o["StringValue"] = "value1";
            Assert.AreEqual("value1", changed);
            Assert.AreEqual("value1", (string)o["StringValue"]);
            Assert.AreEqual(1, changedCount);

            o["StringValue"] = "value1";
            Assert.AreEqual(1, changedCount);

            o["StringValue"] = "value2";
            Assert.AreEqual("value2", changed);
            Assert.AreEqual("value2", (string)o["StringValue"]);
            Assert.AreEqual(2, changedCount);

            o["StringValue"] = null;
            Assert.AreEqual(null, changed);
            Assert.AreEqual(null, (string)o["StringValue"]);
            Assert.AreEqual(3, changedCount);

            o["NullValue"] = null;
            Assert.AreEqual(null, changed);
            Assert.AreEqual(JValue.CreateNull(), o["NullValue"]);
            Assert.AreEqual(4, changedCount);

            o["NullValue"] = null;
            Assert.AreEqual(4, changedCount);
        }

        [Fact]
        public void IListContains()
        {
            var p = new JProperty("Test", 1);
            IList l = new JObject(p);

            Assert.IsTrue(l.Contains(p));
            Assert.IsFalse(l.Contains(new JProperty("Test", 1)));
        }

        [Fact]
        public void IListIndexOf()
        {
            var p = new JProperty("Test", 1);
            IList l = new JObject(p);

            Assert.AreEqual(0, l.IndexOf(p));
            Assert.AreEqual(-1, l.IndexOf(new JProperty("Test", 1)));
        }

        [Fact]
        public void IListClear()
        {
            var p = new JProperty("Test", 1);
            IList l = new JObject(p);

            Assert.AreEqual(1, l.Count);

            l.Clear();

            Assert.AreEqual(0, l.Count);
        }

        [Fact]
        public void IListCopyTo()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            var a = new object[l.Count];

            l.CopyTo(a, 0);

            Assert.AreEqual(p1, a[0]);
            Assert.AreEqual(p2, a[1]);
        }

        [Fact]
        public void IListAdd()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            var p3 = new JProperty("Test3", "III");

            l.Add(p3);

            Assert.AreEqual(3, l.Count);
            Assert.AreEqual(p3, l[2]);
        }

        [Fact]
        public void IListAddBadToken()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var p1 = new JProperty("Test1", 1);
                var p2 = new JProperty("Test2", "Two");
                IList l = new JObject(p1, p2);

                l.Add(new JValue("Bad!"));
            }, "Can not add Argon.Linq.JValue to Argon.Linq.JObject.");
        }

        [Fact]
        public void IListAddBadValue()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var p1 = new JProperty("Test1", 1);
                var p2 = new JProperty("Test2", "Two");
                IList l = new JObject(p1, p2);

                l.Add("Bad!");
            }, "Argument is not a JToken.");
        }

        [Fact]
        public void IListAddPropertyWithExistingName()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var p1 = new JProperty("Test1", 1);
                var p2 = new JProperty("Test2", "Two");
                IList l = new JObject(p1, p2);

                var p3 = new JProperty("Test2", "II");

                l.Add(p3);
            }, "Can not add property Test2 to Argon.Linq.JObject. Property with the same name already exists on object.");
        }

        [Fact]
        public void IListRemove()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            var p3 = new JProperty("Test3", "III");

            // won't do anything
            l.Remove(p3);
            Assert.AreEqual(2, l.Count);

            l.Remove(p1);
            Assert.AreEqual(1, l.Count);
            Assert.IsFalse(l.Contains(p1));
            Assert.IsTrue(l.Contains(p2));

            l.Remove(p2);
            Assert.AreEqual(0, l.Count);
            Assert.IsFalse(l.Contains(p2));
            Assert.AreEqual(null, p2.Parent);
        }

        [Fact]
        public void IListRemoveAt()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            // won't do anything
            l.RemoveAt(0);

            l.Remove(p1);
            Assert.AreEqual(1, l.Count);

            l.Remove(p2);
            Assert.AreEqual(0, l.Count);
        }

        [Fact]
        public void IListInsert()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            var p3 = new JProperty("Test3", "III");

            l.Insert(1, p3);
            Assert.AreEqual(l, p3.Parent);

            Assert.AreEqual(p1, l[0]);
            Assert.AreEqual(p3, l[1]);
            Assert.AreEqual(p2, l[2]);
        }

        [Fact]
        public void IListIsReadOnly()
        {
            IList l = new JObject();
            Assert.IsFalse(l.IsReadOnly);
        }

        [Fact]
        public void IListIsFixedSize()
        {
            IList l = new JObject();
            Assert.IsFalse(l.IsFixedSize);
        }

        [Fact]
        public void IListSetItem()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            var p3 = new JProperty("Test3", "III");

            l[0] = p3;

            Assert.AreEqual(p3, l[0]);
            Assert.AreEqual(p2, l[1]);
        }

        [Fact]
        public void IListSetItemAlreadyExists()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var p1 = new JProperty("Test1", 1);
                var p2 = new JProperty("Test2", "Two");
                IList l = new JObject(p1, p2);

                var p3 = new JProperty("Test3", "III");

                l[0] = p3;
                l[1] = p3;
            }, "Can not add property Test3 to Argon.Linq.JObject. Property with the same name already exists on object.");
        }

        [Fact]
        public void IListSetItemInvalid()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var p1 = new JProperty("Test1", 1);
                var p2 = new JProperty("Test2", "Two");
                IList l = new JObject(p1, p2);

                l[0] = new JValue(true);
            }, @"Can not add Argon.Linq.JValue to Argon.Linq.JObject.");
        }

        [Fact]
        public void IListSyncRoot()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            Assert.IsNotNull(l.SyncRoot);
        }

        [Fact]
        public void IListIsSynchronized()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList l = new JObject(p1, p2);

            Assert.IsFalse(l.IsSynchronized);
        }

        [Fact]
        public void GenericListJTokenContains()
        {
            var p = new JProperty("Test", 1);
            IList<JToken> l = new JObject(p);

            Assert.IsTrue(l.Contains(p));
            Assert.IsFalse(l.Contains(new JProperty("Test", 1)));
        }

        [Fact]
        public void GenericListJTokenIndexOf()
        {
            var p = new JProperty("Test", 1);
            IList<JToken> l = new JObject(p);

            Assert.AreEqual(0, l.IndexOf(p));
            Assert.AreEqual(-1, l.IndexOf(new JProperty("Test", 1)));
        }

        [Fact]
        public void GenericListJTokenClear()
        {
            var p = new JProperty("Test", 1);
            IList<JToken> l = new JObject(p);

            Assert.AreEqual(1, l.Count);

            l.Clear();

            Assert.AreEqual(0, l.Count);
        }

        [Fact]
        public void GenericListJTokenCopyTo()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            var a = new JToken[l.Count];

            l.CopyTo(a, 0);

            Assert.AreEqual(p1, a[0]);
            Assert.AreEqual(p2, a[1]);
        }

        [Fact]
        public void GenericListJTokenAdd()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            var p3 = new JProperty("Test3", "III");

            l.Add(p3);

            Assert.AreEqual(3, l.Count);
            Assert.AreEqual(p3, l[2]);
        }

        [Fact]
        public void GenericListJTokenAddBadToken()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var p1 = new JProperty("Test1", 1);
                var p2 = new JProperty("Test2", "Two");
                IList<JToken> l = new JObject(p1, p2);

                l.Add(new JValue("Bad!"));
            }, "Can not add Argon.Linq.JValue to Argon.Linq.JObject.");
        }

        [Fact]
        public void GenericListJTokenAddBadValue()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var p1 = new JProperty("Test1", 1);
                var p2 = new JProperty("Test2", "Two");
                IList<JToken> l = new JObject(p1, p2);

                // string is implicitly converted to JValue
                l.Add("Bad!");
            }, "Can not add Argon.Linq.JValue to Argon.Linq.JObject.");
        }

        [Fact]
        public void GenericListJTokenAddPropertyWithExistingName()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var p1 = new JProperty("Test1", 1);
                var p2 = new JProperty("Test2", "Two");
                IList<JToken> l = new JObject(p1, p2);

                var p3 = new JProperty("Test2", "II");

                l.Add(p3);
            }, "Can not add property Test2 to Argon.Linq.JObject. Property with the same name already exists on object.");
        }

        [Fact]
        public void GenericListJTokenRemove()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            var p3 = new JProperty("Test3", "III");

            // won't do anything
            Assert.IsFalse(l.Remove(p3));
            Assert.AreEqual(2, l.Count);

            Assert.IsTrue(l.Remove(p1));
            Assert.AreEqual(1, l.Count);
            Assert.IsFalse(l.Contains(p1));
            Assert.IsTrue(l.Contains(p2));

            Assert.IsTrue(l.Remove(p2));
            Assert.AreEqual(0, l.Count);
            Assert.IsFalse(l.Contains(p2));
            Assert.AreEqual(null, p2.Parent);
        }

        [Fact]
        public void GenericListJTokenRemoveAt()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            // won't do anything
            l.RemoveAt(0);

            l.Remove(p1);
            Assert.AreEqual(1, l.Count);

            l.Remove(p2);
            Assert.AreEqual(0, l.Count);
        }

        [Fact]
        public void GenericListJTokenInsert()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            var p3 = new JProperty("Test3", "III");

            l.Insert(1, p3);
            Assert.AreEqual(l, p3.Parent);

            Assert.AreEqual(p1, l[0]);
            Assert.AreEqual(p3, l[1]);
            Assert.AreEqual(p2, l[2]);
        }

        [Fact]
        public void GenericListJTokenIsReadOnly()
        {
            IList<JToken> l = new JObject();
            Assert.IsFalse(l.IsReadOnly);
        }

        [Fact]
        public void GenericListJTokenSetItem()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            IList<JToken> l = new JObject(p1, p2);

            var p3 = new JProperty("Test3", "III");

            l[0] = p3;

            Assert.AreEqual(p3, l[0]);
            Assert.AreEqual(p2, l[1]);
        }

        [Fact]
        public void GenericListJTokenSetItemAlreadyExists()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var p1 = new JProperty("Test1", 1);
                var p2 = new JProperty("Test2", "Two");
                IList<JToken> l = new JObject(p1, p2);

                var p3 = new JProperty("Test3", "III");

                l[0] = p3;
                l[1] = p3;
            }, "Can not add property Test3 to Argon.Linq.JObject. Property with the same name already exists on object.");
        }

        [Fact]
        public void IBindingListSortDirection()
        {
            IBindingList l = new JObject();
            Assert.AreEqual(ListSortDirection.Ascending, l.SortDirection);
        }

        [Fact]
        public void IBindingListSortProperty()
        {
            IBindingList l = new JObject();
            Assert.AreEqual(null, l.SortProperty);
        }

        [Fact]
        public void IBindingListSupportsChangeNotification()
        {
            IBindingList l = new JObject();
            Assert.AreEqual(true, l.SupportsChangeNotification);
        }

        [Fact]
        public void IBindingListSupportsSearching()
        {
            IBindingList l = new JObject();
            Assert.AreEqual(false, l.SupportsSearching);
        }

        [Fact]
        public void IBindingListSupportsSorting()
        {
            IBindingList l = new JObject();
            Assert.AreEqual(false, l.SupportsSorting);
        }

        [Fact]
        public void IBindingListAllowEdit()
        {
            IBindingList l = new JObject();
            Assert.AreEqual(true, l.AllowEdit);
        }

        [Fact]
        public void IBindingListAllowNew()
        {
            IBindingList l = new JObject();
            Assert.AreEqual(true, l.AllowNew);
        }

        [Fact]
        public void IBindingListAllowRemove()
        {
            IBindingList l = new JObject();
            Assert.AreEqual(true, l.AllowRemove);
        }

        [Fact]
        public void IBindingListAddIndex()
        {
            IBindingList l = new JObject();
            // do nothing
            l.AddIndex(null);
        }

        [Fact]
        public void IBindingListApplySort()
        {
            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                IBindingList l = new JObject();
                l.ApplySort(null, ListSortDirection.Ascending);
            }, "Specified method is not supported.");
        }

        [Fact]
        public void IBindingListRemoveSort()
        {
            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                IBindingList l = new JObject();
                l.RemoveSort();
            }, "Specified method is not supported.");
        }

        [Fact]
        public void IBindingListRemoveIndex()
        {
            IBindingList l = new JObject();
            // do nothing
            l.RemoveIndex(null);
        }

        [Fact]
        public void IBindingListFind()
        {
            ExceptionAssert.Throws<NotSupportedException>(() =>
            {
                IBindingList l = new JObject();
                l.Find(null, null);
            }, "Specified method is not supported.");
        }

        [Fact]
        public void IBindingListIsSorted()
        {
            IBindingList l = new JObject();
            Assert.AreEqual(false, l.IsSorted);
        }

        [Fact]
        public void IBindingListAddNew()
        {
            ExceptionAssert.Throws<JsonException>(() =>
            {
                IBindingList l = new JObject();
                l.AddNew();
            }, "Could not determine new value to add to 'Argon.Linq.JObject'.");
        }

        [Fact]
        public void IBindingListAddNewWithEvent()
        {
            var o = new JObject();
            o._addingNew += (_, e) => e.NewObject = new JProperty("Property!");

            IBindingList l = o;
            var newObject = l.AddNew();
            Assert.IsNotNull(newObject);

            var p = (JProperty)newObject;
            Assert.AreEqual("Property!", p.Name);
            Assert.AreEqual(o, p.Parent);
        }

        [Fact]
        public void ITypedListGetListName()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            ITypedList l = new JObject(p1, p2);

            Assert.AreEqual(string.Empty, l.GetListName(null));
        }

        [Fact]
        public void ITypedListGetItemProperties()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            ITypedList l = new JObject(p1, p2);

            var propertyDescriptors = l.GetItemProperties(null);
            Assert.IsNull(propertyDescriptors);
        }

        [Fact]
        public void ListChanged()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            var o = new JObject(p1, p2);

            ListChangedType? changedType = null;
            int? index = null;

            o.ListChanged += (_, a) =>
            {
                changedType = a.ListChangedType;
                index = a.NewIndex;
            };

            var p3 = new JProperty("Test3", "III");

            o.Add(p3);
            Assert.AreEqual(changedType, ListChangedType.ItemAdded);
            Assert.AreEqual(index, 2);
            Assert.AreEqual(p3, ((IList<JToken>)o)[index.Value]);

            var p4 = new JProperty("Test4", "IV");

            ((IList<JToken>)o)[index.Value] = p4;
            Assert.AreEqual(changedType, ListChangedType.ItemChanged);
            Assert.AreEqual(index, 2);
            Assert.AreEqual(p4, ((IList<JToken>)o)[index.Value]);
            Assert.IsFalse(((IList<JToken>)o).Contains(p3));
            Assert.IsTrue(((IList<JToken>)o).Contains(p4));

            o["Test1"] = 2;
            Assert.AreEqual(changedType, ListChangedType.ItemChanged);
            Assert.AreEqual(index, 0);
            Assert.AreEqual(2, (int)o["Test1"]);
        }

        [Fact]
        public void CollectionChanged()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            var o = new JObject(p1, p2);

            NotifyCollectionChangedAction? changedType = null;
            int? index = null;

            o._collectionChanged += (_, a) =>
            {
                changedType = a.Action;
                index = a.NewStartingIndex;
            };

            var p3 = new JProperty("Test3", "III");

            o.Add(p3);
            Assert.AreEqual(changedType, NotifyCollectionChangedAction.Add);
            Assert.AreEqual(index, 2);
            Assert.AreEqual(p3, ((IList<JToken>)o)[index.Value]);

            var p4 = new JProperty("Test4", "IV");

            ((IList<JToken>)o)[index.Value] = p4;
            Assert.AreEqual(changedType, NotifyCollectionChangedAction.Replace);
            Assert.AreEqual(index, 2);
            Assert.AreEqual(p4, ((IList<JToken>)o)[index.Value]);
            Assert.IsFalse(((IList<JToken>)o).Contains(p3));
            Assert.IsTrue(((IList<JToken>)o).Contains(p4));

            o["Test1"] = 2;
            Assert.AreEqual(changedType, NotifyCollectionChangedAction.Replace);
            Assert.AreEqual(index, 0);
            Assert.AreEqual(2, (int)o["Test1"]);
        }

        [Fact]
        public void GetGeocodeAddress()
        {
            var json = @"{
  ""name"": ""Address: 435 North Mulford Road Rockford, IL 61107"",
  ""Status"": {
    ""code"": 200,
    ""request"": ""geocode""
  },
  ""Placemark"": [ {
    ""id"": ""p1"",
    ""address"": ""435 N Mulford Rd, Rockford, IL 61107, USA"",
    ""AddressDetails"": {
   ""Accuracy"" : 8,
   ""Country"" : {
      ""AdministrativeArea"" : {
         ""AdministrativeAreaName"" : ""IL"",
         ""SubAdministrativeArea"" : {
            ""Locality"" : {
               ""LocalityName"" : ""Rockford"",
               ""PostalCode"" : {
                  ""PostalCodeNumber"" : ""61107""
               },
               ""Thoroughfare"" : {
                  ""ThoroughfareName"" : ""435 N Mulford Rd""
               }
            },
            ""SubAdministrativeAreaName"" : ""Winnebago""
         }
      },
      ""CountryName"" : ""USA"",
      ""CountryNameCode"" : ""US""
   }
},
    ""ExtendedData"": {
      ""LatLonBox"": {
        ""north"": 42.2753076,
        ""south"": 42.2690124,
        ""east"": -88.9964645,
        ""west"": -89.0027597
      }
    },
    ""Point"": {
      ""coordinates"": [ -88.9995886, 42.2721596, 0 ]
    }
  } ]
}";

            var o = JObject.Parse(json);

            var searchAddress = (string)o["Placemark"][0]["AddressDetails"]["Country"]["AdministrativeArea"]["SubAdministrativeArea"]["Locality"]["Thoroughfare"]["ThoroughfareName"];
            Assert.AreEqual("435 N Mulford Rd", searchAddress);
        }

        [Fact]
        public void SetValueWithInvalidPropertyName()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var o = new JObject
                {
                    [0] = new JValue(3)
                };
            }, "Set JObject values with invalid key value: 0. Object property name expected.");
        }

        [Fact]
        public void SetValue()
        {
            object key = "TestKey";

            var o = new JObject
            {
                [key] = new JValue(3)
            };

            Assert.AreEqual(3, (int)o[key]);
        }

        [Fact]
        public void ParseMultipleProperties()
        {
            var json = @"{
        ""Name"": ""Name1"",
        ""Name"": ""Name2""
      }";

            var o = JObject.Parse(json);
            var value = (string)o["Name"];

            Assert.AreEqual("Name2", value);
        }

        [Fact]
        public void ParseMultipleProperties_EmptySettings()
        {
            var json = @"{
        ""Name"": ""Name1"",
        ""Name"": ""Name2""
      }";

            var o = JObject.Parse(json, new JsonLoadSettings());
            var value = (string)o["Name"];

            Assert.AreEqual("Name2", value);
        }

        [Fact]
        public void ParseMultipleProperties_IgnoreDuplicateSetting()
        {
            var json = @"{
        ""Name"": ""Name1"",
        ""Name"": ""Name2""
      }";

            var o = JObject.Parse(json, new JsonLoadSettings
            {
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Ignore
            });
            var value = (string)o["Name"];

            Assert.AreEqual("Name1", value);
        }

        [Fact]
        public void ParseMultipleProperties_ReplaceDuplicateSetting()
        {
            var json = @"{
        ""Name"": ""Name1"",
        ""Name"": ""Name2""
      }";

            var o = JObject.Parse(json, new JsonLoadSettings
            {
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace
            });
            var value = (string)o["Name"];

            Assert.AreEqual("Name2", value);
        }

        [Fact]
        public void WriteObjectNullDBNullValue()
        {
            var dbNull = DBNull.Value;
            var v = new JValue(dbNull);
            Assert.AreEqual(DBNull.Value, v.Value);
            Assert.AreEqual(JTokenType.Null, v.Type);

            var o = new JObject
            {
                ["title"] = v
            };

            var output = o.ToString();

            StringAssert.AreEqual(@"{
  ""title"": null
}", output);
        }

        [Fact]
        public void InvalidValueCastExceptionMessage()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var json = @"{
  ""responseData"": {}, 
  ""responseDetails"": null, 
  ""responseStatus"": 200
}";

                var o = JObject.Parse(json);

                var name = (string)o["responseData"];
            }, "Can not convert Object to String.");
        }

        [Fact]
        public void InvalidPropertyValueCastExceptionMessage()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var json = @"{
  ""responseData"": {}, 
  ""responseDetails"": null, 
  ""responseStatus"": 200
}";

                var o = JObject.Parse(json);

                var name = (string)o.Property("responseData");
            }, "Can not convert Object to String.");
        }

        [Fact]
        public void ParseIncomplete()
        {
            ExceptionAssert.Throws<Exception>(() => { JObject.Parse("{ foo:"); }, "Unexpected end of content while loading JObject. Path 'foo', line 1, position 6.");
        }

        [Fact]
        public void LoadFromNestedObject()
        {
            var jsonText = @"{
  ""short"":
  {
    ""error"":
    {
      ""code"":0,
      ""msg"":""No action taken""
    }
  }
}";

            JsonReader reader = new JsonTextReader(new StringReader(jsonText));
            reader.Read();
            reader.Read();
            reader.Read();
            reader.Read();
            reader.Read();

            var o = (JObject)JToken.ReadFrom(reader);
            Assert.IsNotNull(o);
            StringAssert.AreEqual(@"{
  ""code"": 0,
  ""msg"": ""No action taken""
}", o.ToString(Formatting.Indented));
        }

        [Fact]
        public void LoadFromNestedObjectIncomplete()
        {
            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                var jsonText = @"{
  ""short"":
  {
    ""error"":
    {
      ""code"":0";

                JsonReader reader = new JsonTextReader(new StringReader(jsonText));
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();

                JToken.ReadFrom(reader);
            }, "Unexpected end of content while loading JObject. Path 'short.error.code', line 6, position 14.");
        }

        [Fact]
        public void GetProperties()
        {
            var o = JObject.Parse("{'prop1':12,'prop2':'hi!','prop3':null,'prop4':[1,2,3]}");

            ICustomTypeDescriptor descriptor = o;

            var properties = descriptor.GetProperties();
            Assert.AreEqual(4, properties.Count);

            var prop1 = properties[0];
            Assert.AreEqual("prop1", prop1.Name);
            Assert.AreEqual(typeof(object), prop1.PropertyType);
            Assert.AreEqual(typeof(JObject), prop1.ComponentType);
            Assert.AreEqual(false, prop1.CanResetValue(o));
            Assert.AreEqual(false, prop1.ShouldSerializeValue(o));

            var prop2 = properties[1];
            Assert.AreEqual("prop2", prop2.Name);
            Assert.AreEqual(typeof(object), prop2.PropertyType);
            Assert.AreEqual(typeof(JObject), prop2.ComponentType);
            Assert.AreEqual(false, prop2.CanResetValue(o));
            Assert.AreEqual(false, prop2.ShouldSerializeValue(o));

            var prop3 = properties[2];
            Assert.AreEqual("prop3", prop3.Name);
            Assert.AreEqual(typeof(object), prop3.PropertyType);
            Assert.AreEqual(typeof(JObject), prop3.ComponentType);
            Assert.AreEqual(false, prop3.CanResetValue(o));
            Assert.AreEqual(false, prop3.ShouldSerializeValue(o));

            var prop4 = properties[3];
            Assert.AreEqual("prop4", prop4.Name);
            Assert.AreEqual(typeof(object), prop4.PropertyType);
            Assert.AreEqual(typeof(JObject), prop4.ComponentType);
            Assert.AreEqual(false, prop4.CanResetValue(o));
            Assert.AreEqual(false, prop4.ShouldSerializeValue(o));
        }

        [Fact]
        public void ParseEmptyObjectWithComment()
        {
            var o = JObject.Parse("{ /* A Comment */ }");
            Assert.AreEqual(0, o.Count);
        }

        [Fact]
        public void FromObjectTimeSpan()
        {
            var v = (JValue)JToken.FromObject(TimeSpan.FromDays(1));
            Assert.AreEqual(v.Value, TimeSpan.FromDays(1));

            Assert.AreEqual("1.00:00:00", v.ToString());
        }

        [Fact]
        public void FromObjectUri()
        {
            var v = (JValue)JToken.FromObject(new Uri("http://www.stuff.co.nz"));
            Assert.AreEqual(v.Value, new Uri("http://www.stuff.co.nz"));

            Assert.AreEqual("http://www.stuff.co.nz/", v.ToString());
        }

        [Fact]
        public void FromObjectGuid()
        {
            var v = (JValue)JToken.FromObject(new Guid("9065ACF3-C820-467D-BE50-8D4664BEAF35"));
            Assert.AreEqual(v.Value, new Guid("9065ACF3-C820-467D-BE50-8D4664BEAF35"));

            Assert.AreEqual("9065acf3-c820-467d-be50-8d4664beaf35", v.ToString());
        }

        [Fact]
        public void ParseAdditionalContent()
        {
            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                var json = @"{
""Name"": ""Apple"",
""Expiry"": new Date(1230422400000),
""Price"": 3.99,
""Sizes"": [
""Small"",
""Medium"",
""Large""
]
}, 987987";

                var o = JObject.Parse(json);
            }, "Additional text encountered after finished reading JSON content: ,. Path '', line 10, position 1.");
        }

        [Fact]
        public void DeepEqualsIgnoreOrder()
        {
            var o1 = new JObject(
                new JProperty("null", null),
                new JProperty("integer", 1),
                new JProperty("string", "string!"),
                new JProperty("decimal", 0.5m),
                new JProperty("array", new JArray(1, 2)));

            Assert.IsTrue(o1.DeepEquals(o1));

            var o2 = new JObject(
                new JProperty("null", null),
                new JProperty("string", "string!"),
                new JProperty("decimal", 0.5m),
                new JProperty("integer", 1),
                new JProperty("array", new JArray(1, 2)));

            Assert.IsTrue(o1.DeepEquals(o2));

            var o3 = new JObject(
                new JProperty("null", null),
                new JProperty("string", "string!"),
                new JProperty("decimal", 0.5m),
                new JProperty("integer", 2),
                new JProperty("array", new JArray(1, 2)));

            Assert.IsFalse(o1.DeepEquals(o3));

            var o4 = new JObject(
                new JProperty("null", null),
                new JProperty("string", "string!"),
                new JProperty("decimal", 0.5m),
                new JProperty("integer", 1),
                new JProperty("array", new JArray(2, 1)));

            Assert.IsFalse(o1.DeepEquals(o4));

            var o5 = new JObject(
                new JProperty("null", null),
                new JProperty("string", "string!"),
                new JProperty("decimal", 0.5m),
                new JProperty("integer", 1));

            Assert.IsFalse(o1.DeepEquals(o5));

            Assert.IsFalse(o1.DeepEquals(null));
        }

        [Fact]
        public void ToListOnEmptyObject()
        {
            var o = JObject.Parse(@"{}");
            IList<JToken> l1 = o.ToList<JToken>();
            Assert.AreEqual(0, l1.Count);

            IList<KeyValuePair<string, JToken>> l2 = o.ToList<KeyValuePair<string, JToken>>();
            Assert.AreEqual(0, l2.Count);

            o = JObject.Parse(@"{'hi':null}");

            l1 = o.ToList<JToken>();
            Assert.AreEqual(1, l1.Count);

            l2 = o.ToList<KeyValuePair<string, JToken>>();
            Assert.AreEqual(1, l2.Count);
        }

        [Fact]
        public void EmptyObjectDeepEquals()
        {
            Assert.IsTrue(JToken.DeepEquals(new JObject(), new JObject()));

            var a = new JObject();
            var b = new JObject {{"hi", "bye"}};

            b.Remove("hi");

            Assert.IsTrue(JToken.DeepEquals(a, b));
            Assert.IsTrue(JToken.DeepEquals(b, a));
        }

        [Fact]
        public void GetValueBlogExample()
        {
            var o = JObject.Parse(@"{
        'name': 'Lower',
        'NAME': 'Upper'
      }");

            var exactMatch = (string)o.GetValue("NAME", StringComparison.OrdinalIgnoreCase);
            // Upper

            var ignoreCase = (string)o.GetValue("Name", StringComparison.OrdinalIgnoreCase);
            // Lower

            Assert.AreEqual("Upper", exactMatch);
            Assert.AreEqual("Lower", ignoreCase);
        }

        [Fact]
        public void GetValue()
        {
            var a = new JObject
            {
                ["Name"] = "Name!",
                ["name"] = "name!",
                ["title"] = "Title!"
            };

            Assert.AreEqual(null, a.GetValue("NAME", StringComparison.Ordinal));
            Assert.AreEqual(null, a.GetValue("NAME"));
            Assert.AreEqual(null, a.GetValue("TITLE"));
            Assert.AreEqual("Name!", (string)a.GetValue("NAME", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("name!", (string)a.GetValue("name", StringComparison.Ordinal));
            Assert.AreEqual(null, a.GetValue(null, StringComparison.Ordinal));
            Assert.AreEqual(null, a.GetValue(null));

            Assert.IsFalse(a.TryGetValue("NAME", StringComparison.Ordinal, out var v));
            Assert.AreEqual(null, v);

            Assert.IsFalse(a.TryGetValue("NAME", out v));
            Assert.IsFalse(a.TryGetValue("TITLE", out v));

            Assert.IsTrue(a.TryGetValue("NAME", StringComparison.OrdinalIgnoreCase, out v));
            Assert.AreEqual("Name!", (string)v);

            Assert.IsTrue(a.TryGetValue("name", StringComparison.Ordinal, out v));
            Assert.AreEqual("name!", (string)v);

            Assert.IsFalse(a.TryGetValue(null, StringComparison.Ordinal, out v));
        }

        public class FooJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var token = JToken.FromObject(value, new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                if (token.Type == JTokenType.Object)
                {
                    var o = (JObject)token;
                    o.AddFirst(new JProperty("foo", "bar"));
                    o.WriteTo(writer);
                }
                else
                {
                    token.WriteTo(writer);
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotSupportedException("This custom converter only supportes serialization and not deserialization.");
            }

            public override bool CanRead => false;

            public override bool CanConvert(Type objectType)
            {
                return true;
            }
        }

        [Fact]
        public void FromObjectInsideConverterWithCustomSerializer()
        {
            var p = new Person
            {
                Name = "Daniel Wertheim",
            };

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new FooJsonConverter() },
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var json = JsonConvert.SerializeObject(p, settings);

            Assert.AreEqual(@"{""foo"":""bar"",""name"":""Daniel Wertheim"",""birthDate"":""0001-01-01T00:00:00"",""lastModified"":""0001-01-01T00:00:00""}", json);
        }

        [Fact]
        public void Parse_NoComments()
        {
            var json = "{'prop':[1,2/*comment*/,3]}";

            var o = JObject.Parse(json, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore
            });

            Assert.AreEqual(3, o["prop"].Count());
            Assert.AreEqual(1, (int)o["prop"][0]);
            Assert.AreEqual(2, (int)o["prop"][1]);
            Assert.AreEqual(3, (int)o["prop"][2]);
        }

        [Fact]
        public void Parse_ExcessiveContentJustComments()
        {
            var json = @"{'prop':[1,2,3]}/*comment*/
//Another comment.";

            var o = JObject.Parse(json);

            Assert.AreEqual(3, o["prop"].Count());
            Assert.AreEqual(1, (int)o["prop"][0]);
            Assert.AreEqual(2, (int)o["prop"][1]);
            Assert.AreEqual(3, (int)o["prop"][2]);
        }

        [Fact]
        public void Parse_ExcessiveContent()
        {
            var json = @"{'prop':[1,2,3]}/*comment*/
//Another comment.
[]";

            ExceptionAssert.Throws<JsonReaderException>(() => JObject.Parse(json),
                "Additional text encountered after finished reading JSON content: [. Path '', line 3, position 0.");
        }

        [Fact]
        public void GetPropertyOwner_ReturnsJObject()
        {
            ICustomTypeDescriptor o = new JObject
            {
                ["prop1"] = 1
            };

            var properties = o.GetProperties();
            Assert.AreEqual(1, properties.Count);

            var pd = properties[0];      
            Assert.AreEqual("prop1", pd.Name);

            var owner = o.GetPropertyOwner(pd);
            Assert.AreEqual(o, owner);

            var value = pd.GetValue(owner);
            Assert.AreEqual(1, (int)(JToken)value);
        }

        [Fact]
        public void Property()
        {
            var a = new JObject
            {
                ["Name"] = "Name!",
                ["name"] = "name!",
                ["title"] = "Title!"
            };

            Assert.AreEqual(null, a.Property("NAME", StringComparison.Ordinal));
            Assert.AreEqual(null, a.Property("NAME"));
            Assert.AreEqual(null, a.Property("TITLE"));
            Assert.AreEqual(null, a.Property(null, StringComparison.Ordinal));
            Assert.AreEqual(null, a.Property(null, StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(null, a.Property(null));

            // Return first match when ignoring case
            Assert.AreEqual("Name", a.Property("NAME", StringComparison.OrdinalIgnoreCase).Name);
            // Return exact match before ignoring case
            Assert.AreEqual("name", a.Property("name", StringComparison.OrdinalIgnoreCase).Name);
            // Return exact match without ignoring case
            Assert.AreEqual("name", a.Property("name", StringComparison.Ordinal).Name);
        }
    }
}