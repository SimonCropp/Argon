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
using System.Numerics;
using System.Text;
using Argon.Converters;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using TestCase = Xunit.InlineDataAttribute;

using Argon.Linq;
using System.IO;
using System.Linq;
using Argon.Utilities;

namespace Argon.Tests.Linq
{
    [TestFixture]
    public class JTokenTests : TestFixtureBase
    {
        [Fact]
        public void DeepEqualsObjectOrder()
        {
            var ob1 = @"{""key1"":""1"",""key2"":""2""}";
            var ob2 = @"{""key2"":""2"",""key1"":""1""}";

            var j1 = JObject.Parse(ob1);
            var j2 = JObject.Parse(ob2);
            Assert.IsTrue(j1.DeepEquals(j2));
        }

        [Fact]
        public void ReadFrom()
        {
            var o = (JObject)JToken.ReadFrom(new JsonTextReader(new StringReader("{'pie':true}")));
            Assert.AreEqual(true, (bool)o["pie"]);

            var a = (JArray)JToken.ReadFrom(new JsonTextReader(new StringReader("[1,2,3]")));
            Assert.AreEqual(1, (int)a[0]);
            Assert.AreEqual(2, (int)a[1]);
            Assert.AreEqual(3, (int)a[2]);

            JsonReader reader = new JsonTextReader(new StringReader("{'pie':true}"));
            reader.Read();
            reader.Read();

            var p = (JProperty)JToken.ReadFrom(reader);
            Assert.AreEqual("pie", p.Name);
            Assert.AreEqual(true, (bool)p.Value);

            var c = (JConstructor)JToken.ReadFrom(new JsonTextReader(new StringReader("new Date(1)")));
            Assert.AreEqual("Date", c.Name);
            Assert.IsTrue(JToken.DeepEquals(new JValue(1), c.Values().ElementAt(0)));

            JValue v;

            v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"""stringvalue""")));
            Assert.AreEqual("stringvalue", (string)v);

            v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"1")));
            Assert.AreEqual(1, (int)v);

            v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"1.1")));
            Assert.AreEqual(1.1, (double)v);

            v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"""1970-01-01T00:00:00+12:31"""))
            {
                DateParseHandling = DateParseHandling.DateTimeOffset
            });
            Assert.AreEqual(typeof(DateTimeOffset), v.Value.GetType());
            Assert.AreEqual(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, new TimeSpan(12, 31, 0)), v.Value);
        }

        [Fact]
        public void Load()
        {
            var o = (JObject)JToken.Load(new JsonTextReader(new StringReader("{'pie':true}")));
            Assert.AreEqual(true, (bool)o["pie"]);
        }

        [Fact]
        public void Parse()
        {
            var o = (JObject)JToken.Parse("{'pie':true}");
            Assert.AreEqual(true, (bool)o["pie"]);
        }

        [Fact]
        public void Parent()
        {
            var v = new JArray(new JConstructor("TestConstructor"), new JValue(new DateTime(2000, 12, 20)));

            Assert.AreEqual(null, v.Parent);

            var o =
                new JObject(
                    new JProperty("Test1", v),
                    new JProperty("Test2", "Test2Value"),
                    new JProperty("Test3", "Test3Value"),
                    new JProperty("Test4", null)
                    );

            Assert.AreEqual(o.Property("Test1"), v.Parent);

            var p = new JProperty("NewProperty", v);

            // existing value should still have same parent
            Assert.AreEqual(o.Property("Test1"), v.Parent);

            // new value should be cloned
            Assert.AreNotSame(p.Value, v);

            Assert.AreEqual((DateTime)((JValue)p.Value[1]).Value, (DateTime)((JValue)v[1]).Value);

            Assert.AreEqual(v, o["Test1"]);

            Assert.AreEqual(null, o.Parent);
            var o1 = new JProperty("O1", o);
            Assert.AreEqual(o, o1.Value);

            Assert.AreNotEqual(null, o.Parent);
            var o2 = new JProperty("O2", o);

            Assert.AreNotSame(o1.Value, o2.Value);
            Assert.AreEqual(o1.Value.Children().Count(), o2.Value.Children().Count());
            Assert.AreEqual(false, JToken.DeepEquals(o1, o2));
            Assert.AreEqual(true, JToken.DeepEquals(o1.Value, o2.Value));
        }

        [Fact]
        public void Next()
        {
            var a =
                new JArray(
                    5,
                    6,
                    new JArray(7, 8),
                    new JArray(9, 10)
                    );

            var next = a[0].Next;
            Assert.AreEqual(6, (int)next);

            next = next.Next;
            Assert.IsTrue(JToken.DeepEquals(new JArray(7, 8), next));

            next = next.Next;
            Assert.IsTrue(JToken.DeepEquals(new JArray(9, 10), next));

            next = next.Next;
            Assert.IsNull(next);
        }

        [Fact]
        public void Previous()
        {
            var a =
                new JArray(
                    5,
                    6,
                    new JArray(7, 8),
                    new JArray(9, 10)
                    );

            var previous = a[3].Previous;
            Assert.IsTrue(JToken.DeepEquals(new JArray(7, 8), previous));

            previous = previous.Previous;
            Assert.AreEqual(6, (int)previous);

            previous = previous.Previous;
            Assert.AreEqual(5, (int)previous);

            previous = previous.Previous;
            Assert.IsNull(previous);
        }

        [Fact]
        public void Children()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            Assert.AreEqual(4, a.Count());
            Assert.AreEqual(3, a.Children<JArray>().Count());
        }

        [Fact]
        public void BeforeAfter()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1, 2, 3),
                    new JArray(1, 2, 3),
                    new JArray(1, 2, 3)
                    );

            Assert.AreEqual(5, (int)a[1].Previous);
            Assert.AreEqual(2, a[2].BeforeSelf().Count());
        }

        [Fact]
        public void BeforeSelf_NoParent_ReturnEmpty()
        {
            var o = new JObject();

            var result = o.BeforeSelf().ToList();
            Assert.AreEqual(0, result.Count);
        }

        [Fact]
        public void BeforeSelf_OnlyChild_ReturnEmpty()
        {
            var a = new JArray();
            var o = new JObject();
            a.Add(o);

            var result = o.BeforeSelf().ToList();
            Assert.AreEqual(0, result.Count);
        }

#nullable enable
        [Fact]
        public void Casting()
        {
            Assert.AreEqual(1L, (long)(new JValue(1)));
            Assert.AreEqual(2L, (long)new JArray(1, 2, 3)[1]);

            Assert.AreEqual(new DateTime(2000, 12, 20), (DateTime)new JValue(new DateTime(2000, 12, 20)));
            Assert.AreEqual(new DateTimeOffset(2000, 12, 20, 0, 0, 0, TimeSpan.Zero), (DateTimeOffset)new JValue(new DateTime(2000, 12, 20, 0, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero), (DateTimeOffset)new JValue(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)));
            Assert.AreEqual(null, (DateTimeOffset?)new JValue((DateTimeOffset?)null));
            Assert.AreEqual(null, (DateTimeOffset?)(JValue?)null);
            Assert.AreEqual(true, (bool)new JValue(true));
            Assert.AreEqual(true, (bool?)new JValue(true));
            Assert.AreEqual(null, (bool?)((JValue?)null));
            Assert.AreEqual(null, (bool?)JValue.CreateNull());
            Assert.AreEqual(10, (long)new JValue(10));
            Assert.AreEqual(null, (long?)new JValue((long?)null));
            Assert.AreEqual(null, (long?)(JValue?)null);
            Assert.AreEqual(null, (int?)new JValue((int?)null));
            Assert.AreEqual(null, (int?)(JValue?)null);
            Assert.AreEqual(null, (DateTime?)new JValue((DateTime?)null));
            Assert.AreEqual(null, (DateTime?)(JValue?)null);
            Assert.AreEqual(null, (short?)new JValue((short?)null));
            Assert.AreEqual(null, (short?)(JValue?)null);
            Assert.AreEqual(null, (float?)new JValue((float?)null));
            Assert.AreEqual(null, (float?)(JValue?)null);
            Assert.AreEqual(null, (double?)new JValue((double?)null));
            Assert.AreEqual(null, (double?)(JValue?)null);
            Assert.AreEqual(null, (decimal?)new JValue((decimal?)null));
            Assert.AreEqual(null, (decimal?)(JValue?)null);
            Assert.AreEqual(null, (uint?)new JValue((uint?)null));
            Assert.AreEqual(null, (uint?)(JValue?)null);
            Assert.AreEqual(null, (sbyte?)new JValue((sbyte?)null));
            Assert.AreEqual(null, (sbyte?)(JValue?)null);
            Assert.AreEqual(null, (byte?)new JValue((byte?)null));
            Assert.AreEqual(null, (byte?)(JValue?)null);
            Assert.AreEqual(null, (ulong?)new JValue((ulong?)null));
            Assert.AreEqual(null, (ulong?)(JValue?)null);
            Assert.AreEqual(null, (uint?)new JValue((uint?)null));
            Assert.AreEqual(null, (uint?)(JValue?)null);
            Assert.AreEqual(11.1f, (float)new JValue(11.1));
            Assert.AreEqual(float.MinValue, (float)new JValue(float.MinValue));
            Assert.AreEqual(1.1, (double)new JValue(1.1));
            Assert.AreEqual(uint.MaxValue, (uint)new JValue(uint.MaxValue));
            Assert.AreEqual(ulong.MaxValue, (ulong)new JValue(ulong.MaxValue));
            Assert.AreEqual(ulong.MaxValue, (ulong)new JProperty("Test", new JValue(ulong.MaxValue)));
            Assert.AreEqual(null, (string?)new JValue((string?)null));
            Assert.AreEqual(5m, (decimal)(new JValue(5L)));
            Assert.AreEqual(5m, (decimal?)(new JValue(5L)));
            Assert.AreEqual(5f, (float)(new JValue(5L)));
            Assert.AreEqual(5f, (float)(new JValue(5m)));
            Assert.AreEqual(5f, (float?)(new JValue(5m)));
            Assert.AreEqual(5, (byte)(new JValue(5)));
            Assert.AreEqual(SByte.MinValue, (sbyte?)(new JValue(SByte.MinValue)));
            Assert.AreEqual(SByte.MinValue, (sbyte)(new JValue(SByte.MinValue)));

            Assert.AreEqual(null, (sbyte?)JValue.CreateNull());

            Assert.AreEqual("1", (string?)(new JValue(1)));
            Assert.AreEqual("1", (string?)(new JValue(1.0)));
            Assert.AreEqual("1.0", (string?)(new JValue(1.0m)));
            Assert.AreEqual("True", (string?)(new JValue(true)));
            Assert.AreEqual(null, (string?)(JValue.CreateNull()));
            Assert.AreEqual(null, (string?)(JValue?)null);
            Assert.AreEqual("12/12/2000 12:12:12", (string?)(new JValue(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc))));
            Assert.AreEqual("12/12/2000 12:12:12 +00:00", (string?)(new JValue(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero))));
            Assert.AreEqual(true, (bool)(new JValue(1)));
            Assert.AreEqual(true, (bool)(new JValue(1.0)));
            Assert.AreEqual(true, (bool)(new JValue("true")));
            Assert.AreEqual(true, (bool)(new JValue(true)));
            Assert.AreEqual(true, (bool)(new JValue(2)));
            Assert.AreEqual(false, (bool)(new JValue(0)));
            Assert.AreEqual(1, (int)(new JValue(1)));
            Assert.AreEqual(1, (int)(new JValue(1.0)));
            Assert.AreEqual(1, (int)(new JValue("1")));
            Assert.AreEqual(1, (int)(new JValue(true)));
            Assert.AreEqual(1m, (decimal)(new JValue(1)));
            Assert.AreEqual(1m, (decimal)(new JValue(1.0)));
            Assert.AreEqual(1m, (decimal)(new JValue("1")));
            Assert.AreEqual(1m, (decimal)(new JValue(true)));
            Assert.AreEqual(TimeSpan.FromMinutes(1), (TimeSpan)(new JValue(TimeSpan.FromMinutes(1))));
            Assert.AreEqual("00:01:00", (string?)(new JValue(TimeSpan.FromMinutes(1))));
            Assert.AreEqual(TimeSpan.FromMinutes(1), (TimeSpan)(new JValue("00:01:00")));
            Assert.AreEqual("46efe013-b56a-4e83-99e4-4dce7678a5bc", (string?)(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"))));
            Assert.AreEqual("http://www.google.com/", (string?)(new JValue(new Uri("http://www.google.com"))));
            Assert.AreEqual(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid)(new JValue("46EFE013-B56A-4E83-99E4-4DCE7678A5BC")));
            Assert.AreEqual(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid)(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"))));
            Assert.AreEqual(new Uri("http://www.google.com"), (Uri?)(new JValue("http://www.google.com")));
            Assert.AreEqual(new Uri("http://www.google.com"), (Uri?)(new JValue(new Uri("http://www.google.com"))));
            Assert.AreEqual(null, (Uri?)(JValue.CreateNull()));
            Assert.AreEqual(Convert.ToBase64String(Encoding.UTF8.GetBytes("hi")), (string?)(new JValue(Encoding.UTF8.GetBytes("hi"))));
            CollectionAssert.AreEquivalent((byte[])Encoding.UTF8.GetBytes("hi"), (byte[]?)(new JValue(Convert.ToBase64String(Encoding.UTF8.GetBytes("hi")))));
            Assert.AreEqual(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid)(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC").ToByteArray())));
            Assert.AreEqual(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid?)(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC").ToByteArray())));
            Assert.AreEqual((sbyte?)1, (sbyte?)(new JValue((short?)1)));

            Assert.AreEqual(null, (Uri?)(JValue?)null);
            Assert.AreEqual(null, (int?)(JValue?)null);
            Assert.AreEqual(null, (uint?)(JValue?)null);
            Assert.AreEqual(null, (Guid?)(JValue?)null);
            Assert.AreEqual(null, (TimeSpan?)(JValue?)null);
            Assert.AreEqual(null, (byte[]?)(JValue?)null);
            Assert.AreEqual(null, (bool?)(JValue?)null);
            Assert.AreEqual(null, (char?)(JValue?)null);
            Assert.AreEqual(null, (DateTime?)(JValue?)null);
            Assert.AreEqual(null, (DateTimeOffset?)(JValue?)null);
            Assert.AreEqual(null, (short?)(JValue?)null);
            Assert.AreEqual(null, (ushort?)(JValue?)null);
            Assert.AreEqual(null, (byte?)(JValue?)null);
            Assert.AreEqual(null, (byte?)(JValue?)null);
            Assert.AreEqual(null, (sbyte?)(JValue?)null);
            Assert.AreEqual(null, (sbyte?)(JValue?)null);
            Assert.AreEqual(null, (long?)(JValue?)null);
            Assert.AreEqual(null, (ulong?)(JValue?)null);
            Assert.AreEqual(null, (double?)(JValue?)null);
            Assert.AreEqual(null, (float?)(JValue?)null);

            var data = new byte[0];
            Assert.AreEqual(data, (byte[]?)(new JValue(data)));

            Assert.AreEqual(5, (int)(new JValue(StringComparison.OrdinalIgnoreCase)));

            var bigIntegerText = "1234567899999999999999999999999999999999999999999999999999999999999990";

            Assert.AreEqual(BigInteger.Parse(bigIntegerText), (new JValue(BigInteger.Parse(bigIntegerText))).Value);

            Assert.AreEqual(BigInteger.Parse(bigIntegerText), (new JValue(bigIntegerText)).ToObject<BigInteger>());
            Assert.AreEqual(new BigInteger(long.MaxValue), (new JValue(long.MaxValue)).ToObject<BigInteger>());
            Assert.AreEqual(new BigInteger(4.5d), (new JValue((4.5d))).ToObject<BigInteger>());
            Assert.AreEqual(new BigInteger(4.5f), (new JValue((4.5f))).ToObject<BigInteger>());
            Assert.AreEqual(new BigInteger(byte.MaxValue), (new JValue(byte.MaxValue)).ToObject<BigInteger>());
            Assert.AreEqual(new BigInteger(123), (new JValue(123)).ToObject<BigInteger>());
            Assert.AreEqual(new BigInteger(123), (new JValue(123)).ToObject<BigInteger?>());
            Assert.AreEqual(null, (JValue.CreateNull()).ToObject<BigInteger?>());

            var intData = BigInteger.Parse(bigIntegerText).ToByteArray();
            Assert.AreEqual(BigInteger.Parse(bigIntegerText), (new JValue(intData)).ToObject<BigInteger>());

            Assert.AreEqual(4.0d, (double)(new JValue(new BigInteger(4.5d))));
            Assert.AreEqual(true, (bool)(new JValue(new BigInteger(1))));
            Assert.AreEqual(long.MaxValue, (long)(new JValue(new BigInteger(long.MaxValue))));
            Assert.AreEqual(long.MaxValue, (long)(new JValue(new BigInteger(new byte[] { 255, 255, 255, 255, 255, 255, 255, 127 }))));
            Assert.AreEqual("9223372036854775807", (string?)(new JValue(new BigInteger(long.MaxValue))));

            intData = (byte[]?)new JValue(new BigInteger(long.MaxValue));
            CollectionAssert.AreEqual(new byte[] { 255, 255, 255, 255, 255, 255, 255, 127 }, intData);
        }
#nullable disable

        [Fact]
        public void FailedCasting()
        {
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(true); }, "Can not convert Boolean to DateTime.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(1); }, "Can not convert Integer to DateTime.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(1.1); }, "Can not convert Float to DateTime.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(1.1m); }, "Can not convert Float to DateTime.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(TimeSpan.Zero); }, "Can not convert TimeSpan to DateTime.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(new Uri("http://www.google.com")); }, "Can not convert Uri to DateTime.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (DateTime)JValue.CreateNull(); }, "Can not convert Null to DateTime.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (DateTime)new JValue(Guid.NewGuid()); }, "Can not convert Guid to DateTime.");

            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Uri)new JValue(true); }, "Can not convert Boolean to Uri.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Uri)new JValue(1); }, "Can not convert Integer to Uri.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Uri)new JValue(1.1); }, "Can not convert Float to Uri.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Uri)new JValue(1.1m); }, "Can not convert Float to Uri.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Uri)new JValue(TimeSpan.Zero); }, "Can not convert TimeSpan to Uri.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Uri)new JValue(Guid.NewGuid()); }, "Can not convert Guid to Uri.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Uri)new JValue(DateTime.Now); }, "Can not convert Date to Uri.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Uri)new JValue(DateTimeOffset.Now); }, "Can not convert Date to Uri.");

            ExceptionAssert.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(true); }, "Can not convert Boolean to TimeSpan.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(1); }, "Can not convert Integer to TimeSpan.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(1.1); }, "Can not convert Float to TimeSpan.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(1.1m); }, "Can not convert Float to TimeSpan.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (TimeSpan)JValue.CreateNull(); }, "Can not convert Null to TimeSpan.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(Guid.NewGuid()); }, "Can not convert Guid to TimeSpan.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(DateTime.Now); }, "Can not convert Date to TimeSpan.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(DateTimeOffset.Now); }, "Can not convert Date to TimeSpan.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (TimeSpan)new JValue(new Uri("http://www.google.com")); }, "Can not convert Uri to TimeSpan.");

            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Guid)new JValue(true); }, "Can not convert Boolean to Guid.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Guid)new JValue(1); }, "Can not convert Integer to Guid.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Guid)new JValue(1.1); }, "Can not convert Float to Guid.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Guid)new JValue(1.1m); }, "Can not convert Float to Guid.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Guid)JValue.CreateNull(); }, "Can not convert Null to Guid.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Guid)new JValue(DateTime.Now); }, "Can not convert Date to Guid.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Guid)new JValue(DateTimeOffset.Now); }, "Can not convert Date to Guid.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Guid)new JValue(TimeSpan.FromMinutes(1)); }, "Can not convert TimeSpan to Guid.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Guid)new JValue(new Uri("http://www.google.com")); }, "Can not convert Uri to Guid.");

            ExceptionAssert.Throws<ArgumentException>(() => { var i = (DateTimeOffset)new JValue(true); }, "Can not convert Boolean to DateTimeOffset.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (Uri)new JValue(true); }, "Can not convert Boolean to Uri.");

            ExceptionAssert.Throws<ArgumentException>(() => { var i = (new JValue(new Uri("http://www.google.com"))).ToObject<BigInteger>(); }, "Can not convert Uri to BigInteger.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (JValue.CreateNull()).ToObject<BigInteger>(); }, "Can not convert Null to BigInteger.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (new JValue(Guid.NewGuid())).ToObject<BigInteger>(); }, "Can not convert Guid to BigInteger.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (new JValue(Guid.NewGuid())).ToObject<BigInteger?>(); }, "Can not convert Guid to BigInteger.");

            ExceptionAssert.Throws<ArgumentException>(() => { var i = (sbyte?)new JValue(DateTime.Now); }, "Can not convert Date to SByte.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (sbyte)new JValue(DateTime.Now); }, "Can not convert Date to SByte.");

            ExceptionAssert.Throws<ArgumentException>(() => { var i = (new JValue("Ordinal1")).ToObject<StringComparison>(); }, "Could not convert 'Ordinal1' to StringComparison.");
            ExceptionAssert.Throws<ArgumentException>(() => { var i = (new JValue("Ordinal1")).ToObject<StringComparison?>(); }, "Could not convert 'Ordinal1' to StringComparison.");
        }

        [Fact]
        public void ToObject()
        {
            Assert.AreEqual((BigInteger)1, (new JValue(1).ToObject(typeof(BigInteger))));
            Assert.AreEqual((BigInteger)1, (new JValue(1).ToObject(typeof(BigInteger?))));
            Assert.AreEqual((BigInteger?)null, (JValue.CreateNull().ToObject(typeof(BigInteger?))));
            Assert.AreEqual((ushort)1, (new JValue(1).ToObject(typeof(ushort))));
            Assert.AreEqual((ushort)1, (new JValue(1).ToObject(typeof(ushort?))));
            Assert.AreEqual((uint)1L, (new JValue(1).ToObject(typeof(uint))));
            Assert.AreEqual((uint)1L, (new JValue(1).ToObject(typeof(uint?))));
            Assert.AreEqual((ulong)1L, (new JValue(1).ToObject(typeof(ulong))));
            Assert.AreEqual((ulong)1L, (new JValue(1).ToObject(typeof(ulong?))));
            Assert.AreEqual((sbyte)1L, (new JValue(1).ToObject(typeof(sbyte))));
            Assert.AreEqual((sbyte)1L, (new JValue(1).ToObject(typeof(sbyte?))));
            Assert.AreEqual(null, (JValue.CreateNull().ToObject(typeof(sbyte?))));
            Assert.AreEqual((byte)1L, (new JValue(1).ToObject(typeof(byte))));
            Assert.AreEqual((byte)1L, (new JValue(1).ToObject(typeof(byte?))));
            Assert.AreEqual((short)1L, (new JValue(1).ToObject(typeof(short))));
            Assert.AreEqual((short)1L, (new JValue(1).ToObject(typeof(short?))));
            Assert.AreEqual(1, (new JValue(1).ToObject(typeof(int))));
            Assert.AreEqual(1, (new JValue(1).ToObject(typeof(int?))));
            Assert.AreEqual(1L, (new JValue(1).ToObject(typeof(long))));
            Assert.AreEqual(1L, (new JValue(1).ToObject(typeof(long?))));
            Assert.AreEqual((float)1, (new JValue(1.0).ToObject(typeof(float))));
            Assert.AreEqual((float)1, (new JValue(1.0).ToObject(typeof(float?))));
            Assert.AreEqual((double)1, (new JValue(1.0).ToObject(typeof(double))));
            Assert.AreEqual((double)1, (new JValue(1.0).ToObject(typeof(double?))));
            Assert.AreEqual(1m, (new JValue(1).ToObject(typeof(decimal))));
            Assert.AreEqual(1m, (new JValue(1).ToObject(typeof(decimal?))));
            Assert.AreEqual(true, (new JValue(true).ToObject(typeof(bool))));
            Assert.AreEqual(true, (new JValue(true).ToObject(typeof(bool?))));
            Assert.AreEqual('b', (new JValue('b').ToObject(typeof(char))));
            Assert.AreEqual('b', (new JValue('b').ToObject(typeof(char?))));
            Assert.AreEqual(TimeSpan.MaxValue, (new JValue(TimeSpan.MaxValue).ToObject(typeof(TimeSpan))));
            Assert.AreEqual(TimeSpan.MaxValue, (new JValue(TimeSpan.MaxValue).ToObject(typeof(TimeSpan?))));
            Assert.AreEqual(DateTime.MaxValue, (new JValue(DateTime.MaxValue).ToObject(typeof(DateTime))));
            Assert.AreEqual(DateTime.MaxValue, (new JValue(DateTime.MaxValue).ToObject(typeof(DateTime?))));
            Assert.AreEqual(DateTimeOffset.MaxValue, (new JValue(DateTimeOffset.MaxValue).ToObject(typeof(DateTimeOffset))));
            Assert.AreEqual(DateTimeOffset.MaxValue, (new JValue(DateTimeOffset.MaxValue).ToObject(typeof(DateTimeOffset?))));
            Assert.AreEqual("b", (new JValue("b").ToObject(typeof(string))));
            Assert.AreEqual(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C"), (new JValue(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C")).ToObject(typeof(Guid))));
            Assert.AreEqual(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C"), (new JValue(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C")).ToObject(typeof(Guid?))));
            Assert.AreEqual(new Uri("http://www.google.com/"), (new JValue(new Uri("http://www.google.com/")).ToObject(typeof(Uri))));
            Assert.AreEqual(StringComparison.Ordinal, (new JValue("Ordinal").ToObject(typeof(StringComparison))));
            Assert.AreEqual(StringComparison.Ordinal, (new JValue("Ordinal").ToObject(typeof(StringComparison?))));
            Assert.AreEqual(null, (JValue.CreateNull().ToObject(typeof(StringComparison?))));
        }

#nullable enable
        [Fact]
        public void ImplicitCastingTo()
        {
            Assert.IsTrue(JToken.DeepEquals(new JValue(new DateTime(2000, 12, 20)), (JValue)new DateTime(2000, 12, 20)));
            Assert.IsTrue(JToken.DeepEquals(new JValue(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)), (JValue)new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)));
            Assert.IsTrue(JToken.DeepEquals(new JValue((DateTimeOffset?)null), (JValue)(DateTimeOffset?)null));

            // had to remove implicit casting to avoid user reference to System.Numerics.dll
            Assert.IsTrue(JToken.DeepEquals(new JValue(new BigInteger(1)), new JValue(new BigInteger(1))));
            Assert.IsTrue(JToken.DeepEquals(new JValue((BigInteger?)null), new JValue((BigInteger?)null)));
            Assert.IsTrue(JToken.DeepEquals(new JValue(true), (JValue)true));
            Assert.IsTrue(JToken.DeepEquals(new JValue(true), (JValue)true));
            Assert.IsTrue(JToken.DeepEquals(new JValue(true), (JValue)(bool?)true));
            Assert.IsTrue(JToken.DeepEquals(new JValue((bool?)null), (JValue)(bool?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue(10), (JValue)10));
            Assert.IsTrue(JToken.DeepEquals(new JValue((long?)null), (JValue)(long?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue((DateTime?)null), (JValue)(DateTime?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue(long.MaxValue), (JValue)long.MaxValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue((int?)null), (JValue)(int?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue((short?)null), (JValue)(short?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue((double?)null), (JValue)(double?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue((uint?)null), (JValue)(uint?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue((decimal?)null), (JValue)(decimal?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue((ulong?)null), (JValue)(ulong?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue((sbyte?)null), (JValue)(sbyte?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue((sbyte)1), (JValue)(sbyte)1));
            Assert.IsTrue(JToken.DeepEquals(new JValue((byte?)null), (JValue)(byte?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue((byte)1), (JValue)(byte)1));
            Assert.IsTrue(JToken.DeepEquals(new JValue((ushort?)null), (JValue)(ushort?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue(short.MaxValue), (JValue)short.MaxValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue(ushort.MaxValue), (JValue)ushort.MaxValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue(11.1f), (JValue)11.1f));
            Assert.IsTrue(JToken.DeepEquals(new JValue(float.MinValue), (JValue)float.MinValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue(double.MinValue), (JValue)double.MinValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue(uint.MaxValue), (JValue)uint.MaxValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue(ulong.MaxValue), (JValue)ulong.MaxValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue(ulong.MinValue), (JValue)ulong.MinValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue((string?)null), (JValue)(string?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue((DateTime?)null), (JValue)(DateTime?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue(decimal.MaxValue), (JValue)decimal.MaxValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue(decimal.MaxValue), (JValue)(decimal?)decimal.MaxValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue(decimal.MinValue), (JValue)decimal.MinValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue(float.MaxValue), (JValue)(float?)float.MaxValue));
            Assert.IsTrue(JToken.DeepEquals(new JValue(double.MaxValue), (JValue)(double?)double.MaxValue));
            Assert.IsTrue(JToken.DeepEquals(JValue.CreateNull(), (JValue)(double?)null));

            Assert.IsFalse(JToken.DeepEquals(new JValue(true), (JValue)(bool?)null));
            Assert.IsFalse(JToken.DeepEquals(JValue.CreateNull(), (JValue?)(object?)null));

            var emptyData = new byte[0];
            Assert.IsTrue(JToken.DeepEquals(new JValue(emptyData), (JValue)emptyData));
            Assert.IsFalse(JToken.DeepEquals(new JValue(emptyData), (JValue)new byte[1]));
            Assert.IsTrue(JToken.DeepEquals(new JValue(Encoding.UTF8.GetBytes("Hi")), (JValue)Encoding.UTF8.GetBytes("Hi")));

            Assert.IsTrue(JToken.DeepEquals(new JValue(TimeSpan.FromMinutes(1)), (JValue)TimeSpan.FromMinutes(1)));
            Assert.IsTrue(JToken.DeepEquals(JValue.CreateNull(), (JValue)(TimeSpan?)null));
            Assert.IsTrue(JToken.DeepEquals(new JValue(TimeSpan.FromMinutes(1)), (JValue)(TimeSpan?)TimeSpan.FromMinutes(1)));
            Assert.IsTrue(JToken.DeepEquals(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC")), (JValue)new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC")));
            Assert.IsTrue(JToken.DeepEquals(new JValue(new Uri("http://www.google.com")), (JValue)new Uri("http://www.google.com")));
            Assert.IsTrue(JToken.DeepEquals(JValue.CreateNull(), (JValue)(Uri?)null));
            Assert.IsTrue(JToken.DeepEquals(JValue.CreateNull(), (JValue)(Guid?)null));
        }
#nullable disable

        [Fact]
        public void Root()
        {
            var a =
                new JArray(
                    5,
                    6,
                    new JArray(7, 8),
                    new JArray(9, 10)
                    );

            Assert.AreEqual(a, a.Root);
            Assert.AreEqual(a, a[0].Root);
            Assert.AreEqual(a, ((JArray)a[2])[0].Root);
        }

        [Fact]
        public void Remove()
        {
            JToken t;
            var a =
                new JArray(
                    5,
                    6,
                    new JArray(7, 8),
                    new JArray(9, 10)
                    );

            a[0].Remove();

            Assert.AreEqual(6, (int)a[0]);

            a[1].Remove();

            Assert.AreEqual(6, (int)a[0]);
            Assert.IsTrue(JToken.DeepEquals(new JArray(9, 10), a[1]));
            Assert.AreEqual(2, a.Count());

            t = a[1];
            t.Remove();
            Assert.AreEqual(6, (int)a[0]);
            Assert.IsNull(t.Next);
            Assert.IsNull(t.Previous);
            Assert.IsNull(t.Parent);

            t = a[0];
            t.Remove();
            Assert.AreEqual(0, a.Count());

            Assert.IsNull(t.Next);
            Assert.IsNull(t.Previous);
            Assert.IsNull(t.Parent);
        }

        [Fact]
        public void AfterSelf()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var t = a[1];
            var afterTokens = t.AfterSelf().ToList();

            Assert.AreEqual(2, afterTokens.Count);
            Assert.IsTrue(JToken.DeepEquals(new JArray(1, 2), afterTokens[0]));
            Assert.IsTrue(JToken.DeepEquals(new JArray(1, 2, 3), afterTokens[1]));
        }

        [Fact]
        public void BeforeSelf()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var t = a[2];
            var beforeTokens = t.BeforeSelf().ToList();

            Assert.AreEqual(2, beforeTokens.Count);
            Assert.IsTrue(JToken.DeepEquals(new JValue(5), beforeTokens[0]));
            Assert.IsTrue(JToken.DeepEquals(new JArray(1), beforeTokens[1]));
        }

        [Fact]
        public void HasValues()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            Assert.IsTrue(a.HasValues);
        }

        [Fact]
        public void Ancestors()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var t = a[1][0];
            var ancestors = t.Ancestors().ToList();
            Assert.AreEqual(2, ancestors.Count());
            Assert.AreEqual(a[1], ancestors[0]);
            Assert.AreEqual(a, ancestors[1]);
        }

        [Fact]
        public void AncestorsAndSelf()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var t = a[1][0];
            var ancestors = t.AncestorsAndSelf().ToList();
            Assert.AreEqual(3, ancestors.Count());
            Assert.AreEqual(t, ancestors[0]);
            Assert.AreEqual(a[1], ancestors[1]);
            Assert.AreEqual(a, ancestors[2]);
        }

        [Fact]
        public void AncestorsAndSelf_Many()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var o = new JObject
            {
                { "prop1", "value1" }
            };

            var t1 = a[1][0];
            var t2 = o["prop1"];

            var source = new List<JToken> { t1, t2 };

            var ancestors = source.AncestorsAndSelf().ToList();
            Assert.AreEqual(6, ancestors.Count());
            Assert.AreEqual(t1, ancestors[0]);
            Assert.AreEqual(a[1], ancestors[1]);
            Assert.AreEqual(a, ancestors[2]);
            Assert.AreEqual(t2, ancestors[3]);
            Assert.AreEqual(o.Property("prop1"), ancestors[4]);
            Assert.AreEqual(o, ancestors[5]);
        }

        [Fact]
        public void Ancestors_Many()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var o = new JObject
            {
                { "prop1", "value1" }
            };

            var t1 = a[1][0];
            var t2 = o["prop1"];

            var source = new List<JToken> { t1, t2 };

            var ancestors = source.Ancestors().ToList();
            Assert.AreEqual(4, ancestors.Count());
            Assert.AreEqual(a[1], ancestors[0]);
            Assert.AreEqual(a, ancestors[1]);
            Assert.AreEqual(o.Property("prop1"), ancestors[2]);
            Assert.AreEqual(o, ancestors[3]);
        }

        [Fact]
        public void Descendants()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var descendants = a.Descendants().ToList();
            Assert.AreEqual(10, descendants.Count());
            Assert.AreEqual(5, (int)descendants[0]);
            Assert.IsTrue(JToken.DeepEquals(new JArray(1, 2, 3), descendants[descendants.Count - 4]));
            Assert.AreEqual(1, (int)descendants[descendants.Count - 3]);
            Assert.AreEqual(2, (int)descendants[descendants.Count - 2]);
            Assert.AreEqual(3, (int)descendants[descendants.Count - 1]);
        }

        [Fact]
        public void Descendants_Many()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var o = new JObject
            {
                { "prop1", "value1" }
            };

            var source = new List<JContainer> { a, o };

            var descendants = source.Descendants().ToList();
            Assert.AreEqual(12, descendants.Count());
            Assert.AreEqual(5, (int)descendants[0]);
            Assert.IsTrue(JToken.DeepEquals(new JArray(1, 2, 3), descendants[descendants.Count - 6]));
            Assert.AreEqual(1, (int)descendants[descendants.Count - 5]);
            Assert.AreEqual(2, (int)descendants[descendants.Count - 4]);
            Assert.AreEqual(3, (int)descendants[descendants.Count - 3]);
            Assert.AreEqual(o.Property("prop1"), descendants[descendants.Count - 2]);
            Assert.AreEqual(o["prop1"], descendants[descendants.Count - 1]);
        }

        [Fact]
        public void DescendantsAndSelf()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var descendantsAndSelf = a.DescendantsAndSelf().ToList();
            Assert.AreEqual(11, descendantsAndSelf.Count());
            Assert.AreEqual(a, descendantsAndSelf[0]);
            Assert.AreEqual(5, (int)descendantsAndSelf[1]);
            Assert.IsTrue(JToken.DeepEquals(new JArray(1, 2, 3), descendantsAndSelf[descendantsAndSelf.Count - 4]));
            Assert.AreEqual(1, (int)descendantsAndSelf[descendantsAndSelf.Count - 3]);
            Assert.AreEqual(2, (int)descendantsAndSelf[descendantsAndSelf.Count - 2]);
            Assert.AreEqual(3, (int)descendantsAndSelf[descendantsAndSelf.Count - 1]);
        }

        [Fact]
        public void DescendantsAndSelf_Many()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var o = new JObject
            {
                { "prop1", "value1" }
            };

            var source = new List<JContainer> { a, o };

            var descendantsAndSelf = source.DescendantsAndSelf().ToList();
            Assert.AreEqual(14, descendantsAndSelf.Count());
            Assert.AreEqual(a, descendantsAndSelf[0]);
            Assert.AreEqual(5, (int)descendantsAndSelf[1]);
            Assert.IsTrue(JToken.DeepEquals(new JArray(1, 2, 3), descendantsAndSelf[descendantsAndSelf.Count - 7]));
            Assert.AreEqual(1, (int)descendantsAndSelf[descendantsAndSelf.Count - 6]);
            Assert.AreEqual(2, (int)descendantsAndSelf[descendantsAndSelf.Count - 5]);
            Assert.AreEqual(3, (int)descendantsAndSelf[descendantsAndSelf.Count - 4]);
            Assert.AreEqual(o, descendantsAndSelf[descendantsAndSelf.Count - 3]);
            Assert.AreEqual(o.Property("prop1"), descendantsAndSelf[descendantsAndSelf.Count - 2]);
            Assert.AreEqual(o["prop1"], descendantsAndSelf[descendantsAndSelf.Count - 1]);
        }

        [Fact]
        public void CreateWriter()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var writer = a.CreateWriter();
            Assert.IsNotNull(writer);
            Assert.AreEqual(4, a.Count());

            writer.WriteValue("String");
            Assert.AreEqual(5, a.Count());
            Assert.AreEqual("String", (string)a[4]);

            writer.WriteStartObject();
            writer.WritePropertyName("Property");
            writer.WriteValue("PropertyValue");
            writer.WriteEnd();

            Assert.AreEqual(6, a.Count());
            Assert.IsTrue(JToken.DeepEquals(new JObject(new JProperty("Property", "PropertyValue")), a[5]));
        }

        [Fact]
        public void AddFirst()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            a.AddFirst("First");

            Assert.AreEqual("First", (string)a[0]);
            Assert.AreEqual(a, a[0].Parent);
            Assert.AreEqual(a[1], a[0].Next);
            Assert.AreEqual(5, a.Count());

            a.AddFirst("NewFirst");
            Assert.AreEqual("NewFirst", (string)a[0]);
            Assert.AreEqual(a, a[0].Parent);
            Assert.AreEqual(a[1], a[0].Next);
            Assert.AreEqual(6, a.Count());

            Assert.AreEqual(a[0], a[0].Next.Previous);
        }

        [Fact]
        public void RemoveAll()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            var first = a.First;
            Assert.AreEqual(5, (int)first);

            a.RemoveAll();
            Assert.AreEqual(0, a.Count());

            Assert.IsNull(first.Parent);
            Assert.IsNull(first.Next);
        }

        [Fact]
        public void AddPropertyToArray()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var a = new JArray();
                a.Add(new JProperty("PropertyName"));
            }, "Can not add Argon.Linq.JProperty to Argon.Linq.JArray.");
        }

        [Fact]
        public void AddValueToObject()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                var o = new JObject();
                o.Add(5);
            }, "Can not add Argon.Linq.JValue to Argon.Linq.JObject.");
        }

        [Fact]
        public void Replace()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            a[0].Replace(new JValue(int.MaxValue));
            Assert.AreEqual(int.MaxValue, (int)a[0]);
            Assert.AreEqual(4, a.Count());

            a[1][0].Replace(new JValue("Test"));
            Assert.AreEqual("Test", (string)a[1][0]);

            a[2].Replace(new JValue(int.MaxValue));
            Assert.AreEqual(int.MaxValue, (int)a[2]);
            Assert.AreEqual(4, a.Count());

            Assert.IsTrue(JToken.DeepEquals(new JArray(int.MaxValue, new JArray("Test"), int.MaxValue, new JArray(1, 2, 3)), a));
        }

        [Fact]
        public void ToStringWithConverters()
        {
            var a =
                new JArray(
                    new JValue(new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc))
                    );

            var json = a.ToString(Formatting.Indented, new IsoDateTimeConverter());

            StringAssert.AreEqual(@"[
  ""2009-02-15T00:00:00Z""
]", json);

            json = JsonConvert.SerializeObject(a, new IsoDateTimeConverter());

            Assert.AreEqual(@"[""2009-02-15T00:00:00Z""]", json);
        }

        [Fact]
        public void ToStringWithNoIndenting()
        {
            var a =
                new JArray(
                    new JValue(new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc))
                    );

            var json = a.ToString(Formatting.None, new IsoDateTimeConverter());

            Assert.AreEqual(@"[""2009-02-15T00:00:00Z""]", json);
        }

        [Fact]
        public void AddAfterSelf()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            a[1].AddAfterSelf("pie");

            Assert.AreEqual(5, (int)a[0]);
            Assert.AreEqual(1, a[1].Count());
            Assert.AreEqual("pie", (string)a[2]);
            Assert.AreEqual(5, a.Count());

            a[4].AddAfterSelf("lastpie");

            Assert.AreEqual("lastpie", (string)a[5]);
            Assert.AreEqual("lastpie", (string)a.Last);
        }

        [Fact]
        public void AddBeforeSelf()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3)
                    );

            a[1].AddBeforeSelf("pie");

            Assert.AreEqual(5, (int)a[0]);
            Assert.AreEqual("pie", (string)a[1]);
            Assert.AreEqual(a, a[1].Parent);
            Assert.AreEqual(a[2], a[1].Next);
            Assert.AreEqual(5, a.Count());

            a[0].AddBeforeSelf("firstpie");

            Assert.AreEqual("firstpie", (string)a[0]);
            Assert.AreEqual(5, (int)a[1]);
            Assert.AreEqual("pie", (string)a[2]);
            Assert.AreEqual(a, a[0].Parent);
            Assert.AreEqual(a[1], a[0].Next);
            Assert.AreEqual(6, a.Count());

            a.Last.AddBeforeSelf("secondlastpie");

            Assert.AreEqual("secondlastpie", (string)a[5]);
            Assert.AreEqual(7, a.Count());
        }

        [Fact]
        public void DeepClone()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3),
                    new JObject(
                        new JProperty("First", new JValue(Encoding.UTF8.GetBytes("Hi"))),
                        new JProperty("Second", 1),
                        new JProperty("Third", null),
                        new JProperty("Fourth", new JConstructor("Date", 12345)),
                        new JProperty("Fifth", double.PositiveInfinity),
                        new JProperty("Sixth", double.NaN)
                        )
                    );

            var a2 = (JArray)a.DeepClone();

            StringAssert.AreEqual(@"[
  5,
  [
    1
  ],
  [
    1,
    2
  ],
  [
    1,
    2,
    3
  ],
  {
    ""First"": ""SGk="",
    ""Second"": 1,
    ""Third"": null,
    ""Fourth"": new Date(
      12345
    ),
    ""Fifth"": ""Infinity"",
    ""Sixth"": ""NaN""
  }
]", a2.ToString(Formatting.Indented));

            Assert.IsTrue(a.DeepEquals(a2));
        }

        [Fact]
        public void Clone()
        {
            var a =
                new JArray(
                    5,
                    new JArray(1),
                    new JArray(1, 2),
                    new JArray(1, 2, 3),
                    new JObject(
                        new JProperty("First", new JValue(Encoding.UTF8.GetBytes("Hi"))),
                        new JProperty("Second", 1),
                        new JProperty("Third", null),
                        new JProperty("Fourth", new JConstructor("Date", 12345)),
                        new JProperty("Fifth", double.PositiveInfinity),
                        new JProperty("Sixth", double.NaN)
                        )
                    );

            ICloneable c = a;

            var a2 = (JArray)c.Clone();

            Assert.IsTrue(a.DeepEquals(a2));
        }

        [Fact]
        public void DoubleDeepEquals()
        {
            var a =
                new JArray(
                    double.NaN,
                    double.PositiveInfinity,
                    double.NegativeInfinity
                    );

            var a2 = (JArray)a.DeepClone();

            Assert.IsTrue(a.DeepEquals(a2));

            var d = 1 + 0.1 + 0.1 + 0.1;

            var v1 = new JValue(d);
            var v2 = new JValue(1.3);

            Assert.IsTrue(v1.DeepEquals(v2));
        }

        [Fact]
        public void ParseAdditionalContent()
        {
            ExceptionAssert.Throws<JsonReaderException>(() =>
            {
                var json = @"[
""Small"",
""Medium"",
""Large""
],";

                JToken.Parse(json);
            }, "Additional text encountered after finished reading JSON content: ,. Path '', line 5, position 1.");
        }

        [Fact]
        public void Path()
        {
            var o =
                new JObject(
                    new JProperty("Test1", new JArray(1, 2, 3)),
                    new JProperty("Test2", "Test2Value"),
                    new JProperty("Test3", new JObject(new JProperty("Test1", new JArray(1, new JObject(new JProperty("Test1", 1)), 3)))),
                    new JProperty("Test4", new JConstructor("Date", new JArray(1, 2, 3)))
                    );

            var t = o.SelectToken("Test1[0]");
            Assert.AreEqual("Test1[0]", t.Path);

            t = o.SelectToken("Test2");
            Assert.AreEqual("Test2", t.Path);

            t = o.SelectToken("");
            Assert.AreEqual("", t.Path);

            t = o.SelectToken("Test4[0][0]");
            Assert.AreEqual("Test4[0][0]", t.Path);

            t = o.SelectToken("Test4[0]");
            Assert.AreEqual("Test4[0]", t.Path);

            t = t.DeepClone();
            Assert.AreEqual("", t.Path);

            t = o.SelectToken("Test3.Test1[1].Test1");
            Assert.AreEqual("Test3.Test1[1].Test1", t.Path);

            var a = new JArray(1);
            Assert.AreEqual("", a.Path);

            Assert.AreEqual("[0]", a[0].Path);
        }

        [Fact]
        public void Parse_NoComments()
        {
            var json = "{'prop':[1,2/*comment*/,3]}";

            var o = JToken.Parse(json, new JsonLoadSettings
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

            var o = JToken.Parse(json);

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
{}";

            ExceptionAssert.Throws<JsonReaderException>(() => JToken.Parse(json),
                "Additional text encountered after finished reading JSON content: {. Path '', line 3, position 0.");
        }

        [Theory]
        [TestCase("test customer", "['test customer']")]
        [TestCase("test customer's", "['test customer\\'s']")]
        [TestCase("testcustomer's", "['testcustomer\\'s']")]
        [TestCase("testcustomer", "testcustomer")]
        [TestCase("test.customer", "['test.customer']")]
        [TestCase("test\rcustomer", "['test\\rcustomer']")]
        [TestCase("test\ncustomer", "['test\\ncustomer']")]
        [TestCase("test\tcustomer", "['test\\tcustomer']")]
        [TestCase("test\bcustomer", "['test\\bcustomer']")]
        [TestCase("test\fcustomer", "['test\\fcustomer']")]
        [TestCase("test/customer", "['test/customer']")]
        [TestCase("test\\customer", "['test\\\\customer']")]
        [TestCase("\"test\"customer", "['\"test\"customer']")]
        public void PathEscapingTest(string name, string expectedPath)
        {
            var v = new JValue("12345");
            var o = new JObject
            {
                [name] = v
            };

            var path = v.Path;

            Assert.AreEqual(expectedPath, path);

            var token = o.SelectToken(path);
            Assert.AreEqual(v, token);
        }
    }
}