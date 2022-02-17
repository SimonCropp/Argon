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
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Linq
{
    [TestFixture]
    public class JPropertyTests : TestFixtureBase
    {
        [Fact]
        public void NullValue()
        {
            var p = new JProperty("TestProperty", null);
            Assert.IsNotNull(p.Value);
            Assert.AreEqual(JTokenType.Null, p.Value.Type);
            Assert.AreEqual(p, p.Value.Parent);

            p.Value = null;
            Assert.IsNotNull(p.Value);
            Assert.AreEqual(JTokenType.Null, p.Value.Type);
            Assert.AreEqual(p, p.Value.Parent);
        }

        [Fact]
        public void ListChanged()
        {
            var p = new JProperty("TestProperty", null);
            IBindingList l = p;

            ListChangedType? listChangedType = null;
            int? index = null;

            l.ListChanged += (_, args) =>
            {
                listChangedType = args.ListChangedType;
                index = args.NewIndex;
            };

            p.Value = 1;

            Assert.AreEqual(ListChangedType.ItemChanged, listChangedType.Value);
            Assert.AreEqual(0, index.Value);
        }

        [Fact]
        public void IListCount()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            Assert.AreEqual(1, l.Count);
        }

        [Fact]
        public void IListClear()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            ExceptionAssert.Throws<JsonException>(() => { l.Clear(); }, "Cannot add or remove items from Argon.Linq.JProperty.");
        }

        [Fact]
        public void IListAdd()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            ExceptionAssert.Throws<JsonException>(() => { l.Add(null); }, "Argon.Linq.JProperty cannot have multiple values.");
        }

        [Fact]
        public void IListRemove()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            ExceptionAssert.Throws<JsonException>(() => { l.Remove(p.Value); }, "Cannot add or remove items from Argon.Linq.JProperty.");
        }

        [Fact]
        public void IListRemoveAt()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            ExceptionAssert.Throws<JsonException>(() => { l.RemoveAt(0); }, "Cannot add or remove items from Argon.Linq.JProperty.");
        }

        [Fact]
        public void JPropertyLinq()
        {
            var p = new JProperty("TestProperty", null);
            IList l = p;

            var result = l.Cast<JToken>().ToList();
            Assert.AreEqual(1, result.Count);
        }

        [Fact]
        public void JPropertyDeepEquals()
        {
            var p1 = new JProperty("TestProperty", null);
            var p2 = new JProperty("TestProperty", null);

            Assert.AreEqual(true, JToken.DeepEquals(p1, p2));
        }

        [Fact]
        public void JPropertyIndexOf()
        {
            var v = new JValue(1);
            var p1 = new JProperty("TestProperty", v);

            IList l1 = p1;
            Assert.AreEqual(0, l1.IndexOf(v));

            IList<JToken> l2 = p1;
            Assert.AreEqual(0, l2.IndexOf(v));
        }

        [Fact]
        public void JPropertyContains()
        {
            var v = new JValue(1);
            var p = new JProperty("TestProperty", v);

            Assert.AreEqual(true, p.Contains(v));
            Assert.AreEqual(false, p.Contains(new JValue(1)));
        }

        [Fact]
        public void Load()
        {
            JsonReader reader = new JsonTextReader(new StringReader("{'propertyname':['value1']}"));
            reader.Read();

            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
            reader.Read();

            var property = JProperty.Load(reader);
            Assert.AreEqual("propertyname", property.Name);
            Assert.IsTrue(JToken.DeepEquals(JArray.Parse("['value1']"), property.Value));

            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            reader = new JsonTextReader(new StringReader("{'propertyname':null}"));
            reader.Read();

            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
            reader.Read();

            property = JProperty.Load(reader);
            Assert.AreEqual("propertyname", property.Name);
            Assert.IsTrue(JToken.DeepEquals(JValue.CreateNull(), property.Value));

            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Fact]
        public void MultiContentConstructor()
        {
            var p = new JProperty("error", new List<string> { "one", "two" });
            var a = (JArray)p.Value;

            Assert.AreEqual(a.Count, 2);
            Assert.AreEqual("one", (string)a[0]);
            Assert.AreEqual("two", (string)a[1]);
        }

        [Fact]
        public void IListGenericAdd()
        {
            IList<JToken> t = new JProperty("error", new List<string> { "one", "two" });

            ExceptionAssert.Throws<JsonException>(() => { t.Add(1); }, "Argon.Linq.JProperty cannot have multiple values.");
        }

        [Fact]
        public void NullParent()
        {
            var json = @"{
                ""prop1"": {
                    ""foo"": ""bar""
                },
            }";

            var obj = JsonConvert.DeserializeObject<JObject>(json);

            var property = obj.Property("prop1");
            var value = property.Value;

            // remove value so it has no parent
            property.Value = null;

            property.Remove();
            obj.Add(new JProperty("prop2", value));

            Assert.AreEqual(((JProperty)value.Parent).Name, "prop2");
        }
    }
}