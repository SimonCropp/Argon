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
using System.ComponentModel;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Argon.Linq;
using System.Linq;

namespace Argon.Tests.Linq
{
    [TestFixture]
    public class JArrayTests : TestFixtureBase
    {
        [Fact]
        public void RemoveSpecificAndRemoveSelf()
        {
            var o = new JObject
            {
                { "results", new JArray(1, 2, 3, 4) }
            };

            var a = (JArray)o["results"];

            var last = a.Last();

            Assert.IsTrue(a.Remove(last));

            last = a.Last();
            last.Remove();

            Assert.AreEqual(2, a.Count);
        }

        [Fact]
        public void Clear()
        {
            var a = new JArray { 1 };
            Assert.AreEqual(1, a.Count);

            a.Clear();
            Assert.AreEqual(0, a.Count);
        }

        [Fact]
        public void AddToSelf()
        {
            var a = new JArray();
            a.Add(a);

            Assert.IsFalse(ReferenceEquals(a[0], a));
        }

        [Fact]
        public void Contains()
        {
            var v = new JValue(1);

            var a = new JArray { v };

            Assert.AreEqual(false, a.Contains(new JValue(2)));
            Assert.AreEqual(false, a.Contains(new JValue(1)));
            Assert.AreEqual(false, a.Contains(null));
            Assert.AreEqual(true, a.Contains(v));
        }

        [Fact]
        public void GenericCollectionCopyTo()
        {
            var j = new JArray();
            j.Add(new JValue(1));
            j.Add(new JValue(2));
            j.Add(new JValue(3));
            Assert.AreEqual(3, j.Count);

            var a = new JToken[5];

            ((ICollection<JToken>)j).CopyTo(a, 1);

            Assert.AreEqual(null, a[0]);

            Assert.AreEqual(1, (int)a[1]);

            Assert.AreEqual(2, (int)a[2]);

            Assert.AreEqual(3, (int)a[3]);

            Assert.AreEqual(null, a[4]);
        }

        [Fact]
        public void GenericCollectionCopyToNullArrayShouldThrow()
        {
            var j = new JArray();

            ExceptionAssert.Throws<ArgumentNullException>(() => { ((ICollection<JToken>)j).CopyTo(null, 0); },
                @"Value cannot be null.
Parameter name: array",
                "Value cannot be null. (Parameter 'array')");
        }

        [Fact]
        public void GenericCollectionCopyToNegativeArrayIndexShouldThrow()
        {
            var j = new JArray();

            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() => { ((ICollection<JToken>)j).CopyTo(new JToken[1], -1); },
                @"arrayIndex is less than 0.
Parameter name: arrayIndex",
                "arrayIndex is less than 0. (Parameter 'arrayIndex')");
        }

        [Fact]
        public void GenericCollectionCopyToArrayIndexEqualGreaterToArrayLengthShouldThrow()
        {
            var j = new JArray();

            ExceptionAssert.Throws<ArgumentException>(() => { ((ICollection<JToken>)j).CopyTo(new JToken[1], 1); }, @"arrayIndex is equal to or greater than the length of array.");
        }

        [Fact]
        public void GenericCollectionCopyToInsufficientArrayCapacity()
        {
            var j = new JArray();
            j.Add(new JValue(1));
            j.Add(new JValue(2));
            j.Add(new JValue(3));

            ExceptionAssert.Throws<ArgumentException>(() => { ((ICollection<JToken>)j).CopyTo(new JToken[3], 1); }, @"The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
        }

        [Fact]
        public void Remove()
        {
            var v = new JValue(1);
            var j = new JArray();
            j.Add(v);

            Assert.AreEqual(1, j.Count);

            Assert.AreEqual(false, j.Remove(new JValue(1)));
            Assert.AreEqual(false, j.Remove(null));
            Assert.AreEqual(true, j.Remove(v));
            Assert.AreEqual(false, j.Remove(v));

            Assert.AreEqual(0, j.Count);
        }

        [Fact]
        public void IndexOf()
        {
            var v1 = new JValue(1);
            var v2 = new JValue(1);
            var v3 = new JValue(1);

            var j = new JArray();

            j.Add(v1);
            Assert.AreEqual(0, j.IndexOf(v1));

            j.Add(v2);
            Assert.AreEqual(0, j.IndexOf(v1));
            Assert.AreEqual(1, j.IndexOf(v2));

            j.AddFirst(v3);
            Assert.AreEqual(1, j.IndexOf(v1));
            Assert.AreEqual(2, j.IndexOf(v2));
            Assert.AreEqual(0, j.IndexOf(v3));

            v3.Remove();
            Assert.AreEqual(0, j.IndexOf(v1));
            Assert.AreEqual(1, j.IndexOf(v2));
            Assert.AreEqual(-1, j.IndexOf(v3));
        }

        [Fact]
        public void RemoveAt()
        {
            var v1 = new JValue(1);
            var v2 = new JValue(1);
            var v3 = new JValue(1);

            var j = new JArray();

            j.Add(v1);
            j.Add(v2);
            j.Add(v3);

            Assert.AreEqual(true, j.Contains(v1));
            j.RemoveAt(0);
            Assert.AreEqual(false, j.Contains(v1));

            Assert.AreEqual(true, j.Contains(v3));
            j.RemoveAt(1);
            Assert.AreEqual(false, j.Contains(v3));

            Assert.AreEqual(1, j.Count);
        }

        [Fact]
        public void RemoveAtOutOfRangeIndexShouldError()
        {
            var j = new JArray();

            ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => { j.RemoveAt(0); },
                @"Index is equal to or greater than Count.
Parameter name: index",
                "Index is equal to or greater than Count. (Parameter 'index')");
        }

        [Fact]
        public void RemoveAtNegativeIndexShouldError()
        {
            var j = new JArray();

            ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => { j.RemoveAt(-1); },
                @"Index is less than 0.
Parameter name: index",
                "Index is less than 0. (Parameter 'index')");
        }

        [Fact]
        public void Insert()
        {
            var v1 = new JValue(1);
            var v2 = new JValue(2);
            var v3 = new JValue(3);
            var v4 = new JValue(4);

            var j = new JArray();

            j.Add(v1);
            j.Add(v2);
            j.Add(v3);
            j.Insert(1, v4);

            Assert.AreEqual(0, j.IndexOf(v1));
            Assert.AreEqual(1, j.IndexOf(v4));
            Assert.AreEqual(2, j.IndexOf(v2));
            Assert.AreEqual(3, j.IndexOf(v3));
        }

        [Fact]
        public void AddFirstAddedTokenShouldBeFirst()
        {
            var v1 = new JValue(1);
            var v2 = new JValue(2);
            var v3 = new JValue(3);

            var j = new JArray();
            Assert.AreEqual(null, j.First);
            Assert.AreEqual(null, j.Last);

            j.AddFirst(v1);
            Assert.AreEqual(v1, j.First);
            Assert.AreEqual(v1, j.Last);

            j.AddFirst(v2);
            Assert.AreEqual(v2, j.First);
            Assert.AreEqual(v1, j.Last);

            j.AddFirst(v3);
            Assert.AreEqual(v3, j.First);
            Assert.AreEqual(v1, j.Last);
        }

        [Fact]
        public void InsertShouldInsertAtZeroIndex()
        {
            var v1 = new JValue(1);
            var v2 = new JValue(2);

            var j = new JArray();

            j.Insert(0, v1);
            Assert.AreEqual(0, j.IndexOf(v1));

            j.Insert(0, v2);
            Assert.AreEqual(1, j.IndexOf(v1));
            Assert.AreEqual(0, j.IndexOf(v2));
        }

        [Fact]
        public void InsertNull()
        {
            var j = new JArray();
            j.Insert(0, null);

            Assert.AreEqual(null, ((JValue)j[0]).Value);
        }

        [Fact]
        public void InsertNegativeIndexShouldThrow()
        {
            var j = new JArray();

            ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => { j.Insert(-1, new JValue(1)); },
                @"Index was out of range. Must be non-negative and less than the size of the collection.
Parameter name: index",
                "Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index')");
        }

        [Fact]
        public void InsertOutOfRangeIndexShouldThrow()
        {
            var j = new JArray();

            ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => { j.Insert(2, new JValue(1)); },
                @"Index must be within the bounds of the List.
Parameter name: index",
                "Index must be within the bounds of the List. (Parameter 'index')");
        }

        [Fact]
        public void Item()
        {
            var v1 = new JValue(1);
            var v2 = new JValue(2);
            var v3 = new JValue(3);
            var v4 = new JValue(4);

            var j = new JArray();

            j.Add(v1);
            j.Add(v2);
            j.Add(v3);

            j[1] = v4;

            Assert.AreEqual(null, v2.Parent);
            Assert.AreEqual(-1, j.IndexOf(v2));
            Assert.AreEqual(j, v4.Parent);
            Assert.AreEqual(1, j.IndexOf(v4));
        }

        [Fact]
        public void Parse_ShouldThrowOnUnexpectedToken()
        {
            var json = @"{""prop"":""value""}";

            ExceptionAssert.Throws<JsonReaderException>(() => { JArray.Parse(json); }, "Error reading JArray from JsonReader. Current JsonReader item is not an array: StartObject. Path '', line 1, position 1.");
        }

        public class ListItemFields
        {
            public string ListItemText { get; set; }
            public object ListItemValue { get; set; }
        }

        [Fact]
        public void ArrayOrder()
        {
            var itemZeroText = "Zero text";

            IEnumerable<ListItemFields> t = new List<ListItemFields>
            {
                new ListItemFields { ListItemText = "First", ListItemValue = 1 },
                new ListItemFields { ListItemText = "Second", ListItemValue = 2 },
                new ListItemFields { ListItemText = "Third", ListItemValue = 3 }
            };

            var optionValues =
                new JObject(
                    new JProperty("options",
                        new JArray(
                            new JObject(
                                new JProperty("text", itemZeroText),
                                new JProperty("value", "0")),
                            from r in t
                            orderby r.ListItemValue
                            select new JObject(
                                new JProperty("text", r.ListItemText),
                                new JProperty("value", r.ListItemValue.ToString())))));

            var result = "myOptions = " + optionValues.ToString();

            StringAssert.AreEqual(@"myOptions = {
  ""options"": [
    {
      ""text"": ""Zero text"",
      ""value"": ""0""
    },
    {
      ""text"": ""First"",
      ""value"": ""1""
    },
    {
      ""text"": ""Second"",
      ""value"": ""2""
    },
    {
      ""text"": ""Third"",
      ""value"": ""3""
    }
  ]
}", result);
        }

        [Fact]
        public void Iterate()
        {
            var a = new JArray(1, 2, 3, 4, 5);

            var i = 1;
            foreach (var token in a)
            {
                Assert.AreEqual(i, (int)token);
                i++;
            }
        }

        [Fact]
        public void ITypedListGetItemProperties()
        {
            var p1 = new JProperty("Test1", 1);
            var p2 = new JProperty("Test2", "Two");
            ITypedList a = new JArray(new JObject(p1, p2));

            var propertyDescriptors = a.GetItemProperties(null);
            Assert.IsNotNull(propertyDescriptors);
            Assert.AreEqual(2, propertyDescriptors.Count);
            Assert.AreEqual("Test1", propertyDescriptors[0].Name);
            Assert.AreEqual("Test2", propertyDescriptors[1].Name);
        }

        [Fact]
        public void AddArrayToSelf()
        {
            var a = new JArray(1, 2);
            a.Add(a);

            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(1, (int)a[0]);
            Assert.AreEqual(2, (int)a[1]);
            Assert.AreNotSame(a, a[2]);
        }

        [Fact]
        public void SetValueWithInvalidIndex()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var a = new JArray();
                a["badvalue"] = new JValue(3);
            }, @"Set JArray values with invalid key value: ""badvalue"". Int32 array index expected.");
        }

        [Fact]
        public void SetValue()
        {
            object key = 0;

            var a = new JArray((object)null);
            a[key] = new JValue(3);

            Assert.AreEqual(3, (int)a[key]);
        }

        [Fact]
        public void ReplaceAll()
        {
            var a = new JArray(new[] { 1, 2, 3 });
            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(1, (int)a[0]);
            Assert.AreEqual(2, (int)a[1]);
            Assert.AreEqual(3, (int)a[2]);

            a.ReplaceAll(1);
            Assert.AreEqual(1, a.Count);
            Assert.AreEqual(1, (int)a[0]);
        }

        [Fact]
        public void ParseIncomplete()
        {
            ExceptionAssert.Throws<JsonReaderException>(() => { JArray.Parse("[1"); }, "Unexpected end of content while loading JArray. Path '[0]', line 1, position 2.");
        }

        [Fact]
        public void InsertAddEnd()
        {
            var array = new JArray();
            array.Insert(0, 123);
            array.Insert(1, 456);

            Assert.AreEqual(2, array.Count);
            Assert.AreEqual(123, (int)array[0]);
            Assert.AreEqual(456, (int)array[1]);
        }

        [Fact]
        public void ParseAdditionalContent()
        {
            var json = @"[
""Small"",
""Medium"",
""Large""
], 987987";

            ExceptionAssert.Throws<JsonReaderException>(() => { JArray.Parse(json); }, "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 1.");
        }

        [Fact]
        public void ToListOnEmptyArray()
        {
            var json = @"{""decks"":[]}";

            var decks = (JArray)JObject.Parse(json)["decks"];
            IList<JToken> l = decks.ToList();
            Assert.AreEqual(0, l.Count);

            json = @"{""decks"":[1]}";

            decks = (JArray)JObject.Parse(json)["decks"];
            l = decks.ToList();
            Assert.AreEqual(1, l.Count);
        }

        [Fact]
        public void Parse_NoComments()
        {
            var json = "[1,2/*comment*/,3]";

            var a = JArray.Parse(json, new JsonLoadSettings());

            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(1, (int)a[0]);
            Assert.AreEqual(2, (int)a[1]);
            Assert.AreEqual(3, (int)a[2]);

            a = JArray.Parse(json, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore
            });

            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(1, (int)a[0]);
            Assert.AreEqual(2, (int)a[1]);
            Assert.AreEqual(3, (int)a[2]);

            a = JArray.Parse(json, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Load
            });

            Assert.AreEqual(4, a.Count);
            Assert.AreEqual(1, (int)a[0]);
            Assert.AreEqual(2, (int)a[1]);
            Assert.AreEqual(JTokenType.Comment, a[2].Type);
            Assert.AreEqual(3, (int)a[3]);
        }

        [Fact]
        public void Parse_ExcessiveContentJustComments()
        {
            var json = @"[1,2,3]/*comment*/
//Another comment.";

            var a = JArray.Parse(json);

            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(1, (int)a[0]);
            Assert.AreEqual(2, (int)a[1]);
            Assert.AreEqual(3, (int)a[2]);
        }

        [Fact]
        public void Parse_ExcessiveContent()
        {
            var json = @"[1,2,3]/*comment*/
//Another comment.
[]";

            ExceptionAssert.Throws<JsonReaderException>(() => JArray.Parse(json),
                "Additional text encountered after finished reading JSON content: [. Path '', line 3, position 0.");
        }

        [Fact]
        public void Parse_LineInfo()
        {
            var json = "[1,2,3]";

            var a = JArray.Parse(json, new JsonLoadSettings());

            Assert.AreEqual(true, ((IJsonLineInfo)a).HasLineInfo());
            Assert.AreEqual(true, ((IJsonLineInfo)a[0]).HasLineInfo());
            Assert.AreEqual(true, ((IJsonLineInfo)a[1]).HasLineInfo());
            Assert.AreEqual(true, ((IJsonLineInfo)a[2]).HasLineInfo());

            a = JArray.Parse(json, new JsonLoadSettings
            {
                LineInfoHandling = LineInfoHandling.Ignore
            });

            Assert.AreEqual(false, ((IJsonLineInfo)a).HasLineInfo());
            Assert.AreEqual(false, ((IJsonLineInfo)a[0]).HasLineInfo());
            Assert.AreEqual(false, ((IJsonLineInfo)a[1]).HasLineInfo());
            Assert.AreEqual(false, ((IJsonLineInfo)a[2]).HasLineInfo());

            a = JArray.Parse(json, new JsonLoadSettings
            {
                LineInfoHandling = LineInfoHandling.Load
            });

            Assert.AreEqual(true, ((IJsonLineInfo)a).HasLineInfo());
            Assert.AreEqual(true, ((IJsonLineInfo)a[0]).HasLineInfo());
            Assert.AreEqual(true, ((IJsonLineInfo)a[1]).HasLineInfo());
            Assert.AreEqual(true, ((IJsonLineInfo)a[2]).HasLineInfo());
        }
    }
}