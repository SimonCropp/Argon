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

using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using TestCase = Xunit.InlineDataAttribute;

namespace Argon.Tests.Linq;

public class JTokenTests : TestFixtureBase
{
    [Fact]
    public void DeepEqualsObjectOrder()
    {
        var ob1 = @"{""key1"":""1"",""key2"":""2""}";
        var ob2 = @"{""key2"":""2"",""key1"":""1""}";

        var j1 = JObject.Parse(ob1);
        var j2 = JObject.Parse(ob2);
        Xunit.Assert.True(j1.DeepEquals(j2));
    }

    [Fact]
    public void ReadFrom()
    {
        var o = (JObject)JToken.ReadFrom(new JsonTextReader(new StringReader("{'pie':true}")));
        Assert.True( (bool)o["pie"]);

        var a = (JArray)JToken.ReadFrom(new JsonTextReader(new StringReader("[1,2,3]")));
        Xunit.Assert.Equal(1, (int)a[0]);
        Xunit.Assert.Equal(2, (int)a[1]);
        Xunit.Assert.Equal(3, (int)a[2]);

        JsonReader reader = new JsonTextReader(new StringReader("{'pie':true}"));
        reader.Read();
        reader.Read();

        var p = (JProperty)JToken.ReadFrom(reader);
        Xunit.Assert.Equal("pie", p.Name);
        Assert.True( (bool)p.Value);

        var c = (JConstructor)JToken.ReadFrom(new JsonTextReader(new StringReader("new Date(1)")));
        Xunit.Assert.Equal("Date", c.Name);
        Xunit.Assert.True(JToken.DeepEquals(new JValue(1), c.Values().ElementAt(0)));

        var v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"""stringvalue""")));
        Xunit.Assert.Equal("stringvalue", (string)v);

        v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"1")));
        Xunit.Assert.Equal(1, (int)v);

        v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"1.1")));
        Xunit.Assert.Equal(1.1, (double)v);

        v = (JValue)JToken.ReadFrom(new JsonTextReader(new StringReader(@"""1970-01-01T00:00:00+12:31"""))
        {
            DateParseHandling = DateParseHandling.DateTimeOffset
        });
        Xunit.Assert.Equal(typeof(DateTimeOffset), v.Value.GetType());
        Xunit.Assert.Equal(new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, new TimeSpan(12, 31, 0)), v.Value);
    }

    [Fact]
    public void Load()
    {
        var o = (JObject)JToken.Load(new JsonTextReader(new StringReader("{'pie':true}")));
        Assert.True( (bool)o["pie"]);
    }

    [Fact]
    public void Parse()
    {
        var o = (JObject)JToken.Parse("{'pie':true}");
        Assert.True( (bool)o["pie"]);
    }

    [Fact]
    public void Parent()
    {
        var v = new JArray(new JConstructor("TestConstructor"), new JValue(new DateTime(2000, 12, 20)));

        Xunit.Assert.Equal(null, v.Parent);

        var o =
            new JObject(
                new JProperty("Test1", v),
                new JProperty("Test2", "Test2Value"),
                new JProperty("Test3", "Test3Value"),
                new JProperty("Test4", null)
            );

        Xunit.Assert.Equal(o.Property("Test1"), v.Parent);

        var p = new JProperty("NewProperty", v);

        // existing value should still have same parent
        Xunit.Assert.Equal(o.Property("Test1"), v.Parent);

        // new value should be cloned
        Assert.AreNotSame(p.Value, v);

        Xunit.Assert.Equal((DateTime)((JValue)p.Value[1]).Value, (DateTime)((JValue)v[1]).Value);

        Xunit.Assert.Equal(v, o["Test1"]);

        Xunit.Assert.Equal(null, o.Parent);
        var o1 = new JProperty("O1", o);
        Xunit.Assert.Equal(o, o1.Value);

        Xunit.Assert.NotEqual(null, o.Parent);
        var o2 = new JProperty("O2", o);

        Assert.AreNotSame(o1.Value, o2.Value);
        Xunit.Assert.Equal(o1.Value.Children().Count(), o2.Value.Children().Count());
        Assert.False( JToken.DeepEquals(o1, o2));
        Assert.True( JToken.DeepEquals(o1.Value, o2.Value));
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
        Xunit.Assert.Equal(6, (int)next);

        next = next.Next;
        Xunit.Assert.True(JToken.DeepEquals(new JArray(7, 8), next));

        next = next.Next;
        Xunit.Assert.True(JToken.DeepEquals(new JArray(9, 10), next));

        next = next.Next;
        Xunit.Assert.Null(next);
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
        Xunit.Assert.True(JToken.DeepEquals(new JArray(7, 8), previous));

        previous = previous.Previous;
        Xunit.Assert.Equal(6, (int)previous);

        previous = previous.Previous;
        Xunit.Assert.Equal(5, (int)previous);

        previous = previous.Previous;
        Xunit.Assert.Null(previous);
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

        Xunit.Assert.Equal(4, a.Count());
        Xunit.Assert.Equal(3, a.Children<JArray>().Count());
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

        Xunit.Assert.Equal(5, (int)a[1].Previous);
        Xunit.Assert.Equal(2, a[2].BeforeSelf().Count());
    }

    [Fact]
    public void BeforeSelf_NoParent_ReturnEmpty()
    {
        var o = new JObject();

        var result = o.BeforeSelf().ToList();
        Xunit.Assert.Equal(0, result.Count);
    }

    [Fact]
    public void BeforeSelf_OnlyChild_ReturnEmpty()
    {
        var a = new JArray();
        var o = new JObject();
        a.Add(o);

        var result = o.BeforeSelf().ToList();
        Xunit.Assert.Equal(0, result.Count);
    }

#nullable enable
    [Fact]
    public void Casting()
    {
        Xunit.Assert.Equal(1L, (long)new JValue(1));
        Xunit.Assert.Equal(2L, (long)new JArray(1, 2, 3)[1]);

        Xunit.Assert.Equal(new DateTime(2000, 12, 20), (DateTime)new JValue(new DateTime(2000, 12, 20)));
        Xunit.Assert.Equal(new DateTimeOffset(2000, 12, 20, 0, 0, 0, TimeSpan.Zero), (DateTimeOffset)new JValue(new DateTime(2000, 12, 20, 0, 0, 0, DateTimeKind.Utc)));
        Xunit.Assert.Equal(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero), (DateTimeOffset)new JValue(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)));
        Xunit.Assert.Equal(null, (DateTimeOffset?)new JValue((DateTimeOffset?)null));
        Xunit.Assert.Equal(null, (DateTimeOffset?)(JValue?)null);
        Assert.True( (bool)new JValue(true));
        Assert.True( (bool?)new JValue(true));
        Xunit.Assert.Equal(null, (bool?)(JValue?)null);
        Xunit.Assert.Equal(null, (bool?)JValue.CreateNull());
        Xunit.Assert.Equal(10, (long)new JValue(10));
        Xunit.Assert.Equal(null, (long?)new JValue((long?)null));
        Xunit.Assert.Equal(null, (long?)(JValue?)null);
        Xunit.Assert.Equal(null, (int?)new JValue((int?)null));
        Xunit.Assert.Equal(null, (int?)(JValue?)null);
        Xunit.Assert.Equal(null, (DateTime?)new JValue((DateTime?)null));
        Xunit.Assert.Equal(null, (DateTime?)(JValue?)null);
        Xunit.Assert.Equal(null, (short?)new JValue((short?)null));
        Xunit.Assert.Equal(null, (short?)(JValue?)null);
        Xunit.Assert.Equal(null, (float?)new JValue((float?)null));
        Xunit.Assert.Equal(null, (float?)(JValue?)null);
        Xunit.Assert.Equal(null, (double?)new JValue((double?)null));
        Xunit.Assert.Equal(null, (double?)(JValue?)null);
        Xunit.Assert.Equal(null, (decimal?)new JValue((decimal?)null));
        Xunit.Assert.Equal(null, (decimal?)(JValue?)null);
        Xunit.Assert.Equal(null, (uint?)new JValue((uint?)null));
        Xunit.Assert.Equal(null, (uint?)(JValue?)null);
        Xunit.Assert.Equal(null, (sbyte?)new JValue((sbyte?)null));
        Xunit.Assert.Equal(null, (sbyte?)(JValue?)null);
        Xunit.Assert.Equal(null, (byte?)new JValue((byte?)null));
        Xunit.Assert.Equal(null, (byte?)(JValue?)null);
        Xunit.Assert.Equal(null, (ulong?)new JValue((ulong?)null));
        Xunit.Assert.Equal(null, (ulong?)(JValue?)null);
        Xunit.Assert.Equal(null, (uint?)new JValue((uint?)null));
        Xunit.Assert.Equal(null, (uint?)(JValue?)null);
        Xunit.Assert.Equal(11.1f, (float)new JValue(11.1));
        Xunit.Assert.Equal(float.MinValue, (float)new JValue(float.MinValue));
        Xunit.Assert.Equal(1.1, (double)new JValue(1.1));
        Xunit.Assert.Equal(uint.MaxValue, (uint)new JValue(uint.MaxValue));
        Xunit.Assert.Equal(ulong.MaxValue, (ulong)new JValue(ulong.MaxValue));
        Xunit.Assert.Equal(ulong.MaxValue, (ulong)new JProperty("Test", new JValue(ulong.MaxValue)));
        Xunit.Assert.Equal(null, (string?)new JValue((string?)null));
        Xunit.Assert.Equal(5m, (decimal)new JValue(5L));
        Xunit.Assert.Equal(5m, (decimal?)new JValue(5L));
        Xunit.Assert.Equal(5f, (float)new JValue(5L));
        Xunit.Assert.Equal(5f, (float)new JValue(5m));
        Xunit.Assert.Equal(5f, (float?)new JValue(5m));
        Xunit.Assert.Equal(5, (byte)new JValue(5));
        Xunit.Assert.Equal(SByte.MinValue, (sbyte?)new JValue(SByte.MinValue));
        Xunit.Assert.Equal(SByte.MinValue, (sbyte)new JValue(SByte.MinValue));

        Xunit.Assert.Equal(null, (sbyte?)JValue.CreateNull());

        Xunit.Assert.Equal("1", (string?)new JValue(1));
        Xunit.Assert.Equal("1", (string?)new JValue(1.0));
        Xunit.Assert.Equal("1.0", (string?)new JValue(1.0m));
        Xunit.Assert.Equal("True", (string?)new JValue(true));
        Xunit.Assert.Equal(null, (string?)JValue.CreateNull());
        Xunit.Assert.Equal(null, (string?)(JValue?)null);
        Xunit.Assert.Equal("12/12/2000 12:12:12", (string?)new JValue(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc)));
        Xunit.Assert.Equal("12/12/2000 12:12:12 +00:00", (string?)new JValue(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero)));
        Assert.True( (bool)new JValue(1));
        Assert.True( (bool)new JValue(1.0));
        Assert.True( (bool)new JValue("true"));
        Assert.True( (bool)new JValue(true));
        Assert.True( (bool)new JValue(2));
        Assert.False( (bool)new JValue(0));
        Xunit.Assert.Equal(1, (int)new JValue(1));
        Xunit.Assert.Equal(1, (int)new JValue(1.0));
        Xunit.Assert.Equal(1, (int)new JValue("1"));
        Xunit.Assert.Equal(1, (int)new JValue(true));
        Xunit.Assert.Equal(1m, (decimal)new JValue(1));
        Xunit.Assert.Equal(1m, (decimal)new JValue(1.0));
        Xunit.Assert.Equal(1m, (decimal)new JValue("1"));
        Xunit.Assert.Equal(1m, (decimal)new JValue(true));
        Xunit.Assert.Equal(TimeSpan.FromMinutes(1), (TimeSpan)new JValue(TimeSpan.FromMinutes(1)));
        Xunit.Assert.Equal("00:01:00", (string?)new JValue(TimeSpan.FromMinutes(1)));
        Xunit.Assert.Equal(TimeSpan.FromMinutes(1), (TimeSpan)new JValue("00:01:00"));
        Xunit.Assert.Equal("46efe013-b56a-4e83-99e4-4dce7678a5bc", (string?)new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC")));
        Xunit.Assert.Equal("http://www.google.com/", (string?)new JValue(new Uri("http://www.google.com")));
        Xunit.Assert.Equal(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid)new JValue("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"));
        Xunit.Assert.Equal(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid)new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC")));
        Xunit.Assert.Equal(new Uri("http://www.google.com"), (Uri?)new JValue("http://www.google.com"));
        Xunit.Assert.Equal(new Uri("http://www.google.com"), (Uri?)new JValue(new Uri("http://www.google.com")));
        Xunit.Assert.Equal(null, (Uri?)JValue.CreateNull());
        Xunit.Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes("hi")), (string?)new JValue(Encoding.UTF8.GetBytes("hi")));
        Xunit.Assert.Equal((byte[])Encoding.UTF8.GetBytes("hi"), (byte[]?)new JValue(Convert.ToBase64String(Encoding.UTF8.GetBytes("hi"))));
        Xunit.Assert.Equal(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid)new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC").ToByteArray()));
        Xunit.Assert.Equal(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC"), (Guid?)new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC").ToByteArray()));
        Xunit.Assert.Equal((sbyte?)1, (sbyte?)new JValue((short?)1));

        Xunit.Assert.Equal(null, (Uri?)(JValue?)null);
        Xunit.Assert.Equal(null, (int?)(JValue?)null);
        Xunit.Assert.Equal(null, (uint?)(JValue?)null);
        Xunit.Assert.Equal(null, (Guid?)(JValue?)null);
        Xunit.Assert.Equal(null, (TimeSpan?)(JValue?)null);
        Xunit.Assert.Equal(null, (byte[]?)(JValue?)null);
        Xunit.Assert.Equal(null, (bool?)(JValue?)null);
        Xunit.Assert.Equal(null, (char?)(JValue?)null);
        Xunit.Assert.Equal(null, (DateTime?)(JValue?)null);
        Xunit.Assert.Equal(null, (DateTimeOffset?)(JValue?)null);
        Xunit.Assert.Equal(null, (short?)(JValue?)null);
        Xunit.Assert.Equal(null, (ushort?)(JValue?)null);
        Xunit.Assert.Equal(null, (byte?)(JValue?)null);
        Xunit.Assert.Equal(null, (byte?)(JValue?)null);
        Xunit.Assert.Equal(null, (sbyte?)(JValue?)null);
        Xunit.Assert.Equal(null, (sbyte?)(JValue?)null);
        Xunit.Assert.Equal(null, (long?)(JValue?)null);
        Xunit.Assert.Equal(null, (ulong?)(JValue?)null);
        Xunit.Assert.Equal(null, (double?)(JValue?)null);
        Xunit.Assert.Equal(null, (float?)(JValue?)null);

        var data = new byte[0];
        Xunit.Assert.Equal(data, (byte[]?)new JValue(data));

        Xunit.Assert.Equal(5, (int)new JValue(StringComparison.OrdinalIgnoreCase));

        var bigIntegerText = "1234567899999999999999999999999999999999999999999999999999999999999990";

        Xunit.Assert.Equal(BigInteger.Parse(bigIntegerText), new JValue(BigInteger.Parse(bigIntegerText)).Value);

        Xunit.Assert.Equal(BigInteger.Parse(bigIntegerText), new JValue(bigIntegerText).ToObject<BigInteger>());
        Xunit.Assert.Equal(new BigInteger(long.MaxValue), new JValue(long.MaxValue).ToObject<BigInteger>());
        Xunit.Assert.Equal(new BigInteger(4.5d), new JValue(4.5d).ToObject<BigInteger>());
        Xunit.Assert.Equal(new BigInteger(4.5f), new JValue(4.5f).ToObject<BigInteger>());
        Xunit.Assert.Equal(new BigInteger(byte.MaxValue), new JValue(byte.MaxValue).ToObject<BigInteger>());
        Xunit.Assert.Equal(new BigInteger(123), new JValue(123).ToObject<BigInteger>());
        Xunit.Assert.Equal(new BigInteger(123), new JValue(123).ToObject<BigInteger?>());
        Xunit.Assert.Equal(null, JValue.CreateNull().ToObject<BigInteger?>());

        var intData = BigInteger.Parse(bigIntegerText).ToByteArray();
        Xunit.Assert.Equal(BigInteger.Parse(bigIntegerText), new JValue(intData).ToObject<BigInteger>());

        Xunit.Assert.Equal(4.0d, (double)new JValue(new BigInteger(4.5d)));
        Assert.True( (bool)new JValue(new BigInteger(1)));
        Xunit.Assert.Equal(long.MaxValue, (long)new JValue(new BigInteger(long.MaxValue)));
        Xunit.Assert.Equal(long.MaxValue, (long)new JValue(new BigInteger(new byte[] { 255, 255, 255, 255, 255, 255, 255, 127 })));
        Xunit.Assert.Equal("9223372036854775807", (string?)new JValue(new BigInteger(long.MaxValue)));

        intData = (byte[]?)new JValue(new BigInteger(long.MaxValue));
        Xunit.Assert.Equal(new byte[] { 255, 255, 255, 255, 255, 255, 255, 127 }, intData);
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

        ExceptionAssert.Throws<ArgumentException>(() => { var i = new JValue(new Uri("http://www.google.com")).ToObject<BigInteger>(); }, "Can not convert Uri to BigInteger.");
        ExceptionAssert.Throws<ArgumentException>(() => { var i = JValue.CreateNull().ToObject<BigInteger>(); }, "Can not convert Null to BigInteger.");
        ExceptionAssert.Throws<ArgumentException>(() => { var i = new JValue(Guid.NewGuid()).ToObject<BigInteger>(); }, "Can not convert Guid to BigInteger.");
        ExceptionAssert.Throws<ArgumentException>(() => { var i = new JValue(Guid.NewGuid()).ToObject<BigInteger?>(); }, "Can not convert Guid to BigInteger.");

        ExceptionAssert.Throws<ArgumentException>(() => { var i = (sbyte?)new JValue(DateTime.Now); }, "Can not convert Date to SByte.");
        ExceptionAssert.Throws<ArgumentException>(() => { var i = (sbyte)new JValue(DateTime.Now); }, "Can not convert Date to SByte.");

        ExceptionAssert.Throws<ArgumentException>(() => { var i = new JValue("Ordinal1").ToObject<StringComparison>(); }, "Could not convert 'Ordinal1' to StringComparison.");
        ExceptionAssert.Throws<ArgumentException>(() => { var i = new JValue("Ordinal1").ToObject<StringComparison?>(); }, "Could not convert 'Ordinal1' to StringComparison.");
    }

    [Fact]
    public void ToObject()
    {
        Xunit.Assert.Equal((BigInteger)1, new JValue(1).ToObject(typeof(BigInteger)));
        Xunit.Assert.Equal((BigInteger)1, new JValue(1).ToObject(typeof(BigInteger?)));
        Xunit.Assert.Equal((BigInteger?)null, JValue.CreateNull().ToObject(typeof(BigInteger?)));
        Xunit.Assert.Equal((ushort)1, new JValue(1).ToObject(typeof(ushort)));
        Xunit.Assert.Equal((ushort)1, new JValue(1).ToObject(typeof(ushort?)));
        Xunit.Assert.Equal((uint)1L, new JValue(1).ToObject(typeof(uint)));
        Xunit.Assert.Equal((uint)1L, new JValue(1).ToObject(typeof(uint?)));
        Xunit.Assert.Equal((ulong)1L, new JValue(1).ToObject(typeof(ulong)));
        Xunit.Assert.Equal((ulong)1L, new JValue(1).ToObject(typeof(ulong?)));
        Xunit.Assert.Equal((sbyte)1L, new JValue(1).ToObject(typeof(sbyte)));
        Xunit.Assert.Equal((sbyte)1L, new JValue(1).ToObject(typeof(sbyte?)));
        Xunit.Assert.Equal(null, JValue.CreateNull().ToObject(typeof(sbyte?)));
        Xunit.Assert.Equal((byte)1L, new JValue(1).ToObject(typeof(byte)));
        Xunit.Assert.Equal((byte)1L, new JValue(1).ToObject(typeof(byte?)));
        Xunit.Assert.Equal((short)1L, new JValue(1).ToObject(typeof(short)));
        Xunit.Assert.Equal((short)1L, new JValue(1).ToObject(typeof(short?)));
        Xunit.Assert.Equal(1, new JValue(1).ToObject(typeof(int)));
        Xunit.Assert.Equal(1, new JValue(1).ToObject(typeof(int?)));
        Xunit.Assert.Equal(1L, new JValue(1).ToObject(typeof(long)));
        Xunit.Assert.Equal(1L, new JValue(1).ToObject(typeof(long?)));
        Xunit.Assert.Equal((float)1, new JValue(1.0).ToObject(typeof(float)));
        Xunit.Assert.Equal((float)1, new JValue(1.0).ToObject(typeof(float?)));
        Xunit.Assert.Equal((double)1, new JValue(1.0).ToObject(typeof(double)));
        Xunit.Assert.Equal((double)1, new JValue(1.0).ToObject(typeof(double?)));
        Xunit.Assert.Equal(1m, new JValue(1).ToObject(typeof(decimal)));
        Xunit.Assert.Equal(1m, new JValue(1).ToObject(typeof(decimal?)));
        Assert.True( new JValue(true).ToObject(typeof(bool)));
        Assert.True( new JValue(true).ToObject(typeof(bool?)));
        Xunit.Assert.Equal('b', new JValue('b').ToObject(typeof(char)));
        Xunit.Assert.Equal('b', new JValue('b').ToObject(typeof(char?)));
        Xunit.Assert.Equal(TimeSpan.MaxValue, new JValue(TimeSpan.MaxValue).ToObject(typeof(TimeSpan)));
        Xunit.Assert.Equal(TimeSpan.MaxValue, new JValue(TimeSpan.MaxValue).ToObject(typeof(TimeSpan?)));
        Xunit.Assert.Equal(DateTime.MaxValue, new JValue(DateTime.MaxValue).ToObject(typeof(DateTime)));
        Xunit.Assert.Equal(DateTime.MaxValue, new JValue(DateTime.MaxValue).ToObject(typeof(DateTime?)));
        Xunit.Assert.Equal(DateTimeOffset.MaxValue, new JValue(DateTimeOffset.MaxValue).ToObject(typeof(DateTimeOffset)));
        Xunit.Assert.Equal(DateTimeOffset.MaxValue, new JValue(DateTimeOffset.MaxValue).ToObject(typeof(DateTimeOffset?)));
        Xunit.Assert.Equal("b", new JValue("b").ToObject(typeof(string)));
        Xunit.Assert.Equal(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C"), new JValue(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C")).ToObject(typeof(Guid)));
        Xunit.Assert.Equal(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C"), new JValue(new Guid("A34B2080-B5F0-488E-834D-45D44ECB9E5C")).ToObject(typeof(Guid?)));
        Xunit.Assert.Equal(new Uri("http://www.google.com/"), new JValue(new Uri("http://www.google.com/")).ToObject(typeof(Uri)));
        Xunit.Assert.Equal(StringComparison.Ordinal, new JValue("Ordinal").ToObject(typeof(StringComparison)));
        Xunit.Assert.Equal(StringComparison.Ordinal, new JValue("Ordinal").ToObject(typeof(StringComparison?)));
        Xunit.Assert.Equal(null, JValue.CreateNull().ToObject(typeof(StringComparison?)));
    }

#nullable enable
    [Fact]
    public void ImplicitCastingTo()
    {
        Xunit.Assert.True(JToken.DeepEquals(new JValue(new DateTime(2000, 12, 20)), (JValue)new DateTime(2000, 12, 20)));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)), (JValue)new DateTimeOffset(2000, 12, 20, 23, 50, 10, TimeSpan.Zero)));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((DateTimeOffset?)null), (JValue)(DateTimeOffset?)null));

        // had to remove implicit casting to avoid user reference to System.Numerics.dll
        Xunit.Assert.True(JToken.DeepEquals(new JValue(new BigInteger(1)), new JValue(new BigInteger(1))));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((BigInteger?)null), new JValue((BigInteger?)null)));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(true), (JValue)true));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(true), (JValue)true));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(true), (JValue)(bool?)true));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((bool?)null), (JValue)(bool?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(10), (JValue)10));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((long?)null), (JValue)(long?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((DateTime?)null), (JValue)(DateTime?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(long.MaxValue), (JValue)long.MaxValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((int?)null), (JValue)(int?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((short?)null), (JValue)(short?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((double?)null), (JValue)(double?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((uint?)null), (JValue)(uint?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((decimal?)null), (JValue)(decimal?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((ulong?)null), (JValue)(ulong?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((sbyte?)null), (JValue)(sbyte?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((sbyte)1), (JValue)(sbyte)1));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((byte?)null), (JValue)(byte?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((byte)1), (JValue)(byte)1));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((ushort?)null), (JValue)(ushort?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(short.MaxValue), (JValue)short.MaxValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(ushort.MaxValue), (JValue)ushort.MaxValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(11.1f), (JValue)11.1f));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(float.MinValue), (JValue)float.MinValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(double.MinValue), (JValue)double.MinValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(uint.MaxValue), (JValue)uint.MaxValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(ulong.MaxValue), (JValue)ulong.MaxValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(ulong.MinValue), (JValue)ulong.MinValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((string?)null), (JValue)(string?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue((DateTime?)null), (JValue)(DateTime?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(decimal.MaxValue), (JValue)decimal.MaxValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(decimal.MaxValue), (JValue)(decimal?)decimal.MaxValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(decimal.MinValue), (JValue)decimal.MinValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(float.MaxValue), (JValue)(float?)float.MaxValue));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(double.MaxValue), (JValue)(double?)double.MaxValue));
        Xunit.Assert.True(JToken.DeepEquals(JValue.CreateNull(), (JValue)(double?)null));

        Xunit.Assert.False(JToken.DeepEquals(new JValue(true), (JValue)(bool?)null));
        Xunit.Assert.False(JToken.DeepEquals(JValue.CreateNull(), (JValue?)(object?)null));

        var emptyData = new byte[0];
        Xunit.Assert.True(JToken.DeepEquals(new JValue(emptyData), (JValue)emptyData));
        Xunit.Assert.False(JToken.DeepEquals(new JValue(emptyData), (JValue)new byte[1]));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(Encoding.UTF8.GetBytes("Hi")), (JValue)Encoding.UTF8.GetBytes("Hi")));

        Xunit.Assert.True(JToken.DeepEquals(new JValue(TimeSpan.FromMinutes(1)), (JValue)TimeSpan.FromMinutes(1)));
        Xunit.Assert.True(JToken.DeepEquals(JValue.CreateNull(), (JValue)(TimeSpan?)null));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(TimeSpan.FromMinutes(1)), (JValue)(TimeSpan?)TimeSpan.FromMinutes(1)));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC")), (JValue)new Guid("46EFE013-B56A-4E83-99E4-4DCE7678A5BC")));
        Xunit.Assert.True(JToken.DeepEquals(new JValue(new Uri("http://www.google.com")), (JValue)new Uri("http://www.google.com")));
        Xunit.Assert.True(JToken.DeepEquals(JValue.CreateNull(), (JValue)(Uri?)null));
        Xunit.Assert.True(JToken.DeepEquals(JValue.CreateNull(), (JValue)(Guid?)null));
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

        Xunit.Assert.Equal(a, a.Root);
        Xunit.Assert.Equal(a, a[0].Root);
        Xunit.Assert.Equal(a, ((JArray)a[2])[0].Root);
    }

    [Fact]
    public void Remove()
    {
        var a =
            new JArray(
                5,
                6,
                new JArray(7, 8),
                new JArray(9, 10)
            );

        a[0].Remove();

        Xunit.Assert.Equal(6, (int)a[0]);

        a[1].Remove();

        Xunit.Assert.Equal(6, (int)a[0]);
        Xunit.Assert.True(JToken.DeepEquals(new JArray(9, 10), a[1]));
        Xunit.Assert.Equal(2, a.Count());

        var t = a[1];
        t.Remove();
        Xunit.Assert.Equal(6, (int)a[0]);
        Xunit.Assert.Null(t.Next);
        Xunit.Assert.Null(t.Previous);
        Xunit.Assert.Null(t.Parent);

        t = a[0];
        t.Remove();
        Xunit.Assert.Equal(0, a.Count());

        Xunit.Assert.Null(t.Next);
        Xunit.Assert.Null(t.Previous);
        Xunit.Assert.Null(t.Parent);
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

        Xunit.Assert.Equal(2, afterTokens.Count);
        Xunit.Assert.True(JToken.DeepEquals(new JArray(1, 2), afterTokens[0]));
        Xunit.Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), afterTokens[1]));
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

        Xunit.Assert.Equal(2, beforeTokens.Count);
        Xunit.Assert.True(JToken.DeepEquals(new JValue(5), beforeTokens[0]));
        Xunit.Assert.True(JToken.DeepEquals(new JArray(1), beforeTokens[1]));
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

        Xunit.Assert.True(a.HasValues);
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
        Xunit.Assert.Equal(2, ancestors.Count());
        Xunit.Assert.Equal(a[1], ancestors[0]);
        Xunit.Assert.Equal(a, ancestors[1]);
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
        Xunit.Assert.Equal(3, ancestors.Count());
        Xunit.Assert.Equal(t, ancestors[0]);
        Xunit.Assert.Equal(a[1], ancestors[1]);
        Xunit.Assert.Equal(a, ancestors[2]);
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
        Xunit.Assert.Equal(6, ancestors.Count());
        Xunit.Assert.Equal(t1, ancestors[0]);
        Xunit.Assert.Equal(a[1], ancestors[1]);
        Xunit.Assert.Equal(a, ancestors[2]);
        Xunit.Assert.Equal(t2, ancestors[3]);
        Xunit.Assert.Equal(o.Property("prop1"), ancestors[4]);
        Xunit.Assert.Equal(o, ancestors[5]);
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
        Xunit.Assert.Equal(4, ancestors.Count());
        Xunit.Assert.Equal(a[1], ancestors[0]);
        Xunit.Assert.Equal(a, ancestors[1]);
        Xunit.Assert.Equal(o.Property("prop1"), ancestors[2]);
        Xunit.Assert.Equal(o, ancestors[3]);
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
        Xunit.Assert.Equal(10, descendants.Count());
        Xunit.Assert.Equal(5, (int)descendants[0]);
        Xunit.Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), descendants[descendants.Count - 4]));
        Xunit.Assert.Equal(1, (int)descendants[descendants.Count - 3]);
        Xunit.Assert.Equal(2, (int)descendants[descendants.Count - 2]);
        Xunit.Assert.Equal(3, (int)descendants[descendants.Count - 1]);
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
        Xunit.Assert.Equal(12, descendants.Count());
        Xunit.Assert.Equal(5, (int)descendants[0]);
        Xunit.Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), descendants[descendants.Count - 6]));
        Xunit.Assert.Equal(1, (int)descendants[descendants.Count - 5]);
        Xunit.Assert.Equal(2, (int)descendants[descendants.Count - 4]);
        Xunit.Assert.Equal(3, (int)descendants[descendants.Count - 3]);
        Xunit.Assert.Equal(o.Property("prop1"), descendants[descendants.Count - 2]);
        Xunit.Assert.Equal(o["prop1"], descendants[descendants.Count - 1]);
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
        Xunit.Assert.Equal(11, descendantsAndSelf.Count());
        Xunit.Assert.Equal(a, descendantsAndSelf[0]);
        Xunit.Assert.Equal(5, (int)descendantsAndSelf[1]);
        Xunit.Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), descendantsAndSelf[descendantsAndSelf.Count - 4]));
        Xunit.Assert.Equal(1, (int)descendantsAndSelf[descendantsAndSelf.Count - 3]);
        Xunit.Assert.Equal(2, (int)descendantsAndSelf[descendantsAndSelf.Count - 2]);
        Xunit.Assert.Equal(3, (int)descendantsAndSelf[descendantsAndSelf.Count - 1]);
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
        Xunit.Assert.Equal(14, descendantsAndSelf.Count());
        Xunit.Assert.Equal(a, descendantsAndSelf[0]);
        Xunit.Assert.Equal(5, (int)descendantsAndSelf[1]);
        Xunit.Assert.True(JToken.DeepEquals(new JArray(1, 2, 3), descendantsAndSelf[descendantsAndSelf.Count - 7]));
        Xunit.Assert.Equal(1, (int)descendantsAndSelf[descendantsAndSelf.Count - 6]);
        Xunit.Assert.Equal(2, (int)descendantsAndSelf[descendantsAndSelf.Count - 5]);
        Xunit.Assert.Equal(3, (int)descendantsAndSelf[descendantsAndSelf.Count - 4]);
        Xunit.Assert.Equal(o, descendantsAndSelf[descendantsAndSelf.Count - 3]);
        Xunit.Assert.Equal(o.Property("prop1"), descendantsAndSelf[descendantsAndSelf.Count - 2]);
        Xunit.Assert.Equal(o["prop1"], descendantsAndSelf[descendantsAndSelf.Count - 1]);
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
        Xunit.Assert.NotNull(writer);
        Xunit.Assert.Equal(4, a.Count());

        writer.WriteValue("String");
        Xunit.Assert.Equal(5, a.Count());
        Xunit.Assert.Equal("String", (string)a[4]);

        writer.WriteStartObject();
        writer.WritePropertyName("Property");
        writer.WriteValue("PropertyValue");
        writer.WriteEnd();

        Xunit.Assert.Equal(6, a.Count());
        Xunit.Assert.True(JToken.DeepEquals(new JObject(new JProperty("Property", "PropertyValue")), a[5]));
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

        Xunit.Assert.Equal("First", (string)a[0]);
        Xunit.Assert.Equal(a, a[0].Parent);
        Xunit.Assert.Equal(a[1], a[0].Next);
        Xunit.Assert.Equal(5, a.Count());

        a.AddFirst("NewFirst");
        Xunit.Assert.Equal("NewFirst", (string)a[0]);
        Xunit.Assert.Equal(a, a[0].Parent);
        Xunit.Assert.Equal(a[1], a[0].Next);
        Xunit.Assert.Equal(6, a.Count());

        Xunit.Assert.Equal(a[0], a[0].Next.Previous);
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
        Xunit.Assert.Equal(5, (int)first);

        a.RemoveAll();
        Xunit.Assert.Equal(0, a.Count());

        Xunit.Assert.Null(first.Parent);
        Xunit.Assert.Null(first.Next);
    }

    [Fact]
    public void AddPropertyToArray()
    {
        ExceptionAssert.Throws<ArgumentException>(() =>
        {
            var a = new JArray {new JProperty("PropertyName")};
        }, "Can not add Argon.Linq.JProperty to Argon.Linq.JArray.");
    }

    [Fact]
    public void AddValueToObject()
    {
        ExceptionAssert.Throws<ArgumentException>(() =>
        {
            var o = new JObject {5};
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
        Xunit.Assert.Equal(int.MaxValue, (int)a[0]);
        Xunit.Assert.Equal(4, a.Count());

        a[1][0].Replace(new JValue("Test"));
        Xunit.Assert.Equal("Test", (string)a[1][0]);

        a[2].Replace(new JValue(int.MaxValue));
        Xunit.Assert.Equal(int.MaxValue, (int)a[2]);
        Xunit.Assert.Equal(4, a.Count());

        Xunit.Assert.True(JToken.DeepEquals(new JArray(int.MaxValue, new JArray("Test"), int.MaxValue, new JArray(1, 2, 3)), a));
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

        Xunit.Assert.Equal(@"[""2009-02-15T00:00:00Z""]", json);
    }

    [Fact]
    public void ToStringWithNoIndenting()
    {
        var a =
            new JArray(
                new JValue(new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc))
            );

        var json = a.ToString(Formatting.None, new IsoDateTimeConverter());

        Xunit.Assert.Equal(@"[""2009-02-15T00:00:00Z""]", json);
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

        Xunit.Assert.Equal(5, (int)a[0]);
        Xunit.Assert.Equal(1, a[1].Count());
        Xunit.Assert.Equal("pie", (string)a[2]);
        Xunit.Assert.Equal(5, a.Count());

        a[4].AddAfterSelf("lastpie");

        Xunit.Assert.Equal("lastpie", (string)a[5]);
        Xunit.Assert.Equal("lastpie", (string)a.Last);
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

        Xunit.Assert.Equal(5, (int)a[0]);
        Xunit.Assert.Equal("pie", (string)a[1]);
        Xunit.Assert.Equal(a, a[1].Parent);
        Xunit.Assert.Equal(a[2], a[1].Next);
        Xunit.Assert.Equal(5, a.Count());

        a[0].AddBeforeSelf("firstpie");

        Xunit.Assert.Equal("firstpie", (string)a[0]);
        Xunit.Assert.Equal(5, (int)a[1]);
        Xunit.Assert.Equal("pie", (string)a[2]);
        Xunit.Assert.Equal(a, a[0].Parent);
        Xunit.Assert.Equal(a[1], a[0].Next);
        Xunit.Assert.Equal(6, a.Count());

        a.Last.AddBeforeSelf("secondlastpie");

        Xunit.Assert.Equal("secondlastpie", (string)a[5]);
        Xunit.Assert.Equal(7, a.Count());
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

        Xunit.Assert.True(a.DeepEquals(a2));
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

        Xunit.Assert.True(a.DeepEquals(a2));
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

        Xunit.Assert.True(a.DeepEquals(a2));

        var d = 1 + 0.1 + 0.1 + 0.1;

        var v1 = new JValue(d);
        var v2 = new JValue(1.3);

        Xunit.Assert.True(v1.DeepEquals(v2));
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
        Xunit.Assert.Equal("Test1[0]", t.Path);

        t = o.SelectToken("Test2");
        Xunit.Assert.Equal("Test2", t.Path);

        t = o.SelectToken("");
        Xunit.Assert.Equal("", t.Path);

        t = o.SelectToken("Test4[0][0]");
        Xunit.Assert.Equal("Test4[0][0]", t.Path);

        t = o.SelectToken("Test4[0]");
        Xunit.Assert.Equal("Test4[0]", t.Path);

        t = t.DeepClone();
        Xunit.Assert.Equal("", t.Path);

        t = o.SelectToken("Test3.Test1[1].Test1");
        Xunit.Assert.Equal("Test3.Test1[1].Test1", t.Path);

        var a = new JArray(1);
        Xunit.Assert.Equal("", a.Path);

        Xunit.Assert.Equal("[0]", a[0].Path);
    }

    [Fact]
    public void Parse_NoComments()
    {
        var json = "{'prop':[1,2/*comment*/,3]}";

        var o = JToken.Parse(json, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore
        });

        Xunit.Assert.Equal(3, o["prop"].Count());
        Xunit.Assert.Equal(1, (int)o["prop"][0]);
        Xunit.Assert.Equal(2, (int)o["prop"][1]);
        Xunit.Assert.Equal(3, (int)o["prop"][2]);
    }

    [Fact]
    public void Parse_ExcessiveContentJustComments()
    {
        var json = @"{'prop':[1,2,3]}/*comment*/
//Another comment.";

        var o = JToken.Parse(json);

        Xunit.Assert.Equal(3, o["prop"].Count());
        Xunit.Assert.Equal(1, (int)o["prop"][0]);
        Xunit.Assert.Equal(2, (int)o["prop"][1]);
        Xunit.Assert.Equal(3, (int)o["prop"][2]);
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

        Xunit.Assert.Equal(expectedPath, path);

        var token = o.SelectToken(path);
        Xunit.Assert.Equal(v, token);
    }
}