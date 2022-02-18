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

using System.Dynamic;
using Xunit;

namespace Argon.Tests.Linq;

public class DynamicTests : TestFixtureBase
{
    [Fact]
    public void AccessPropertyValue()
    {
        var rawJson = @"{
  ""task"": {
    ""dueDate"": ""2012-12-03T00:00:00""
  }
}";

        var dyn = JsonConvert.DeserializeObject<dynamic>(rawJson);
        DateTime dueDate = dyn.task.dueDate.Value;

        Assert.Equal(new DateTime(2012, 12, 3, 0, 0, 0, DateTimeKind.Unspecified), dueDate);
    }

    [Fact]
    public void PropertyDoesNotEqualNull()
    {
        var session = JsonConvert.DeserializeObject<dynamic>("{}");
        if (session.sessionInfo != null)
        {
            XUnitAssert.Fail();
        }
    }

    void UpdateValueCount(IDictionary<string, int> counts, dynamic d)
    {
        string s = d.ToString();

        if (!counts.TryGetValue(s, out var c))
        {
            c = 0;
        }

        c++;
        counts[s] = c;
    }

    [Fact]
    public void DeserializeLargeDynamic()
    {
        dynamic d;

        using (var jsonFile = System.IO.File.OpenText("large.json"))
        using (var jsonTextReader = new JsonTextReader(jsonFile))
        {
            var serializer = new JsonSerializer();
            d = serializer.Deserialize(jsonTextReader);
        }

        IDictionary<string, int> counts = new Dictionary<string, int>();

        var sw = new Stopwatch();
        sw.Start();

        var count = 0;
        foreach (var o in d)
        {
            if (count > 10)
            {
                break;
            }

            foreach (var friend in o.friends)
            {
                UpdateValueCount(counts, friend.id);
                UpdateValueCount(counts, ((string)friend.name).Split(' ')[0]);
            }

            count++;
        }

        Console.WriteLine($"Time (secs): {sw.Elapsed.TotalSeconds}");
    }

    [Fact]
    public void JObjectPropertyNames()
    {
        var o = new JObject(
            new JProperty("ChildValue", "blah blah"));

        dynamic d = o;

        d.First = "A value!";

        Assert.Equal(new JValue("A value!"), d.First);
        Assert.Equal("A value!", (string)d.First);

        d.First = null;
        Assert.Equal(JTokenType.Null, d.First.Type);

        Assert.True(d.Remove("First"));
        Assert.Null(d.First);

        JValue v1 = d.ChildValue;
        JValue v2 = d["ChildValue"];
        Assert.Equal(v1, v2);

        var newValue1 = new JValue("Blah blah");
        d.NewValue = newValue1;
        JValue newValue2 = d.NewValue;

        Assert.True(ReferenceEquals(newValue1, newValue2));
    }

    [Fact]
    public void JObjectCount()
    {
        var o = new JObject();

        dynamic d = o;

        long? c1 = d.Count;

        o["Count"] = 99;

        long? c2 = d.Count;

        Assert.Equal(null, c1);
        Assert.Equal(99, c2);
    }

    [Fact]
    public void JObjectEnumerator()
    {
        var o = new JObject(
            new JProperty("ChildValue", "blah blah"));

        dynamic d = o;

        foreach (JProperty value in d)
        {
            Assert.Equal("ChildValue", value.Name);
            Assert.Equal("blah blah", (string)value.Value);
        }

        foreach (var value in d)
        {
            Assert.Equal("ChildValue", value.Name);
            Assert.Equal("blah blah", (string)value.Value);
        }
    }

    [Fact]
    public void JObjectPropertyNameWithJArray()
    {
        var o = new JObject(
            new JProperty("ChildValue", "blah blah"));

        dynamic d = o;

        d.First = new JArray();
        d.First.Add("Hi");

        Assert.Equal(1, d.First.Count);
    }

    [Fact]
    public void JObjectPropertyNameWithNonToken()
    {
        XUnitAssert.Throws<ArgumentException>(() =>
        {
            dynamic d = new JObject();

            d.First = new[] { "One", "II", "3" };
        }, "Could not determine JSON object type for type System.String[].");
    }

    [Fact]
    public void JObjectMethods()
    {
        var o = new JObject(
            new JProperty("ChildValue", "blah blah"));

        dynamic d = o;

        d.Add("NewValue", 1);

        object count = d.Count;

        Assert.Null(count);
        Assert.Null(d["Count"]);

        Assert.True(d.TryGetValue("ChildValue", out JToken v));
        Assert.Equal("blah blah", (string)v);
    }

    [Fact]
    public void JValueEquals()
    {
        var o = new JObject(
            new JProperty("Null", JValue.CreateNull()),
            new JProperty("Integer", new JValue(1)),
            new JProperty("Float", new JValue(1.1d)),
            new JProperty("Decimal", new JValue(1.1m)),
            new JProperty("DateTime", new JValue(new DateTime(2000, 12, 29, 23, 51, 10, DateTimeKind.Utc))),
            new JProperty("Boolean", new JValue(true)),
            new JProperty("String", new JValue("A string lol!")),
            new JProperty("Bytes", new JValue(Encoding.UTF8.GetBytes("A string lol!"))),
            new JProperty("Uri", new Uri("http://json.codeplex.com/")),
            new JProperty("Guid", new Guid("EA27FE1D-0D80-44F2-BF34-4654156FA7AF")),
            new JProperty("TimeSpan", TimeSpan.FromDays(1))
            , new JProperty("BigInteger", BigInteger.Parse("1"))
        );

        dynamic d = o;

        Assert.True(d.Null == d.Null);
        Assert.True(d.Null == null);
        Assert.True(d.Null == JValue.CreateNull());
        Assert.False(d.Null == 1);

        Assert.True(d.Integer == d.Integer);
        Assert.True(d.Integer > 0);
        Assert.True(d.Integer > 0.0m);
        Assert.True(d.Integer > 0.0f);
        Assert.True(d.Integer > null);
        Assert.True(d.Integer >= null);
        Assert.True(d.Integer == 1);
        Assert.True(d.Integer == 1m);
        Assert.True(d.Integer != 1.1f);
        Assert.True(d.Integer != 1.1d);

        Assert.True(d.Decimal == d.Decimal);
        Assert.True(d.Decimal > 0);
        Assert.True(d.Decimal > 0.0m);
        Assert.True(d.Decimal > 0.0f);
        Assert.True(d.Decimal > null);
        Assert.True(d.Decimal >= null);
        Assert.True(d.Decimal == 1.1);
        Assert.True(d.Decimal == 1.1m);
        Assert.True(d.Decimal != 1.0f);
        Assert.True(d.Decimal != 1.0d);
        Assert.True(d.Decimal > new BigInteger(0));

        Assert.True(d.Float == d.Float);
        Assert.True(d.Float > 0);
        Assert.True(d.Float > 0.0m);
        Assert.True(d.Float > 0.0f);
        Assert.True(d.Float > null);
        Assert.True(d.Float >= null);
        Assert.True(d.Float < 2);
        Assert.True(d.Float <= 1.1);
        Assert.True(d.Float == 1.1);
        Assert.True(d.Float == 1.1m);
        Assert.True(d.Float != 1.0f);
        Assert.True(d.Float != 1.0d);
        Assert.True(d.Float > new BigInteger(0));

        Assert.True(d.BigInteger == d.BigInteger);
        Assert.True(d.BigInteger > 0);
        Assert.True(d.BigInteger > 0.0m);
        Assert.True(d.BigInteger > 0.0f);
        Assert.True(d.BigInteger > null);
        Assert.True(d.BigInteger >= null);
        Assert.True(d.BigInteger < 2);
        Assert.True(d.BigInteger <= 1.1);
        Assert.True(d.BigInteger == 1);
        Assert.True(d.BigInteger == 1m);
        Assert.True(d.BigInteger != 1.1f);
        Assert.True(d.BigInteger != 1.1d);

        Assert.True(d.Bytes == d.Bytes);
        Assert.True(d.Bytes == Encoding.UTF8.GetBytes("A string lol!"));
        Assert.True(d.Bytes == new JValue(Encoding.UTF8.GetBytes("A string lol!")));

        Assert.True(d.Uri == d.Uri);
        Assert.True(d.Uri == new Uri("http://json.codeplex.com/"));
        Assert.True(d.Uri > new Uri("http://abc.org/"));
        Assert.True(d.Uri >= new Uri("http://abc.com/"));
        Assert.True(d.Uri > null);
        Assert.True(d.Uri >= null);

        Assert.True(d.Guid == d.Guid);
        Assert.True(d.Guid == new Guid("EA27FE1D-0D80-44F2-BF34-4654156FA7AF"));
        Assert.True(d.Guid > new Guid("AAAAAAAA-0D80-44F2-BF34-4654156FA7AF"));
        Assert.True(d.Guid >= new Guid("AAAAAAAA-0D80-44F2-BF34-4654156FA7AF"));
        Assert.True(d.Guid > null);
        Assert.True(d.Guid >= null);

        Assert.True(d.TimeSpan == d.TimeSpan);
        Assert.True(d.TimeSpan == TimeSpan.FromDays(1));
        Assert.True(d.TimeSpan > TimeSpan.FromHours(1));
        Assert.True(d.TimeSpan >= TimeSpan.FromHours(1));
        Assert.True(d.TimeSpan > null);
        Assert.True(d.TimeSpan >= null);
    }

    [Fact]
    public void JValueAddition()
    {
        var o = new JObject(
            new JProperty("Null", JValue.CreateNull()),
            new JProperty("Integer", new JValue(1)),
            new JProperty("Float", new JValue(1.1d)),
            new JProperty("Decimal", new JValue(1.1m)),
            new JProperty("DateTime", new JValue(new DateTime(2000, 12, 29, 23, 51, 10, DateTimeKind.Utc))),
            new JProperty("Boolean", new JValue(true)),
            new JProperty("String", new JValue("A string lol!")),
            new JProperty("Bytes", new JValue(Encoding.UTF8.GetBytes("A string lol!"))),
            new JProperty("Uri", new Uri("http://json.codeplex.com/")),
            new JProperty("Guid", new Guid("EA27FE1D-0D80-44F2-BF34-4654156FA7AF")),
            new JProperty("TimeSpan", TimeSpan.FromDays(1))
            , new JProperty("BigInteger", new BigInteger(100))
        );

        dynamic d = o;

        #region Add
        var r = d.String + " LAMO!";
        Assert.Equal("A string lol! LAMO!", (string)r);
        r += " gg";
        Assert.Equal("A string lol! LAMO! gg", (string)r);

        r = d.String + null;
        Assert.Equal("A string lol!", (string)r);
        r += null;
        Assert.Equal("A string lol!", (string)r);

        r = d.Integer + 1;
        Assert.Equal(2, (int)r);
        r += 2;
        Assert.Equal(4, (int)r);

        r = d.Integer + 1.1;
        Assert.Equal(2.1, (double)r);
        r += 2;
        Assert.Equal(4.1, (double)r);

        r = d.Integer + 1.1d;
        Assert.Equal(2.1m, (decimal)r);
        r += 2;
        Assert.Equal(4.1m, (decimal)r);

        r = d.Integer + null;
        Assert.Equal(null, r.Value);
        r += 2;
        Assert.Equal(null, r.Value);

        r = d.Float + 1;
        Assert.Equal(2.1d, (double)r);
        r += 2;
        Assert.Equal(4.1d, (double)r);

        r = d.Float + 1.1;
        Assert.Equal(2.2d, (double)r);
        r += 2;
        Assert.Equal(4.2d, (double)r);

        r = d.Float + 1.1d;
        Assert.Equal(2.2m, (decimal)r);
        r += 2;
        Assert.Equal(4.2m, (decimal)r);

        r = d.Float + null;
        Assert.Equal(null, r.Value);
        r += 2;
        Assert.Equal(null, r.Value);

        r = d.Decimal + 1;
        Assert.Equal(2.1m, (decimal)r);
        r += 2;
        Assert.Equal(4.1m, (decimal)r);

        r = d.Decimal + 1.1;
        Assert.Equal(2.2m, (decimal)r);
        r += 2;
        Assert.Equal(4.2m, (decimal)r);

        r = d.Decimal + 1.1d;
        Assert.Equal(2.2m, (decimal)r);
        r += 2;
        Assert.Equal(4.2m, (decimal)r);

        r = d.Decimal + null;
        Assert.Equal(null, r.Value);
        r += 2;
        Assert.Equal(null, r.Value);

        r = d.BigInteger + null;
        Assert.Equal(null, r.Value);
        r += 2;
        Assert.Equal(null, r.Value);

        r = d.BigInteger + 1;
        Assert.Equal(101, (int)r);
        r += 2;
        Assert.Equal(103, (int)r);

        r = d.BigInteger + 1.1d;
        Assert.Equal(101m, (decimal)r);
        r += 2;
        Assert.Equal(103m, (decimal)r);
        #endregion

        #region Subtract
        r = d.Integer - 1;
        Assert.Equal(0, (int)r);
        r -= 2;
        Assert.Equal(-2, (int)r);

        r = d.Integer - 1.1;
        XUnitAssert.AreEqual(-0.1d, (double)r, 0.00001);
        r -= 2;
        Assert.Equal(-2.1d, (double)r);

        r = d.Integer - 1.1d;
        Assert.Equal(-0.1m, (decimal)r);
        r -= 2;
        Assert.Equal(-2.1m, (decimal)r);

        r = d.Integer - null;
        Assert.Equal(null, r.Value);
        r -= 2;
        Assert.Equal(null, r.Value);

        r = d.Float - 1;
        XUnitAssert.AreEqual(0.1d, (double)r, 0.00001);
        r -= 2;
        Assert.Equal(-1.9d, (double)r);

        r = d.Float - 1.1;
        Assert.Equal(0d, (double)r);
        r -= 2;
        Assert.Equal(-2d, (double)r);

        r = d.Float - 1.1d;
        Assert.Equal(0m, (decimal)r);
        r -= 2;
        Assert.Equal(-2m, (decimal)r);

        r = d.Float - null;
        Assert.Equal(null, r.Value);
        r -= 2;
        Assert.Equal(null, r.Value);

        r = d.Decimal - 1;
        Assert.Equal(0.1m, (decimal)r);
        r -= 2;
        Assert.Equal(-1.9m, (decimal)r);

        r = d.Decimal - 1.1;
        Assert.Equal(0m, (decimal)r);
        r -= 2;
        Assert.Equal(-2m, (decimal)r);

        r = d.Decimal - 1.1d;
        Assert.Equal(0m, (decimal)r);
        r -= 2;
        Assert.Equal(-2m, (decimal)r);

        r = d.Decimal - null;
        Assert.Equal(null, r.Value);
        r -= 2;
        Assert.Equal(null, r.Value);

        r = d.BigInteger - null;
        Assert.Equal(null, r.Value);
        r -= 2;
        Assert.Equal(null, r.Value);

        r = d.BigInteger - 1.1d;
        Assert.Equal(99m, (decimal)r);
        r -= 2;
        Assert.Equal(97m, (decimal)r);
        #endregion

        #region Multiply
        r = d.Integer * 1;
        Assert.Equal(1, (int)r);
        r *= 2;
        Assert.Equal(2, (int)r);

        r = d.Integer * 1.1;
        Assert.Equal(1.1d, (double)r);
        r *= 2;
        Assert.Equal(2.2d, (double)r);

        r = d.Integer * 1.1d;
        Assert.Equal(1.1m, (decimal)r);
        r *= 2;
        Assert.Equal(2.2m, (decimal)r);

        r = d.Integer * null;
        Assert.Equal(null, r.Value);
        r *= 2;
        Assert.Equal(null, r.Value);

        r = d.Float * 1;
        Assert.Equal(1.1d, (double)r);
        r *= 2;
        Assert.Equal(2.2d, (double)r);

        r = d.Float * 1.1;
        XUnitAssert.AreEqual(1.21d, (double)r, 0.00001);
        r *= 2;
        XUnitAssert.AreEqual(2.42d, (double)r, 0.00001);

        r = d.Float * 1.1d;
        Assert.Equal(1.21m, (decimal)r);
        r *= 2;
        Assert.Equal(2.42m, (decimal)r);

        r = d.Float * null;
        Assert.Equal(null, r.Value);
        r *= 2;
        Assert.Equal(null, r.Value);

        r = d.Decimal * 1;
        Assert.Equal(1.1m, (decimal)r);
        r *= 2;
        Assert.Equal(2.2m, (decimal)r);

        r = d.Decimal * 1.1;
        Assert.Equal(1.21m, (decimal)r);
        r *= 2;
        Assert.Equal(2.42m, (decimal)r);

        r = d.Decimal * 1.1d;
        Assert.Equal(1.21m, (decimal)r);
        r *= 2;
        Assert.Equal(2.42m, (decimal)r);

        r = d.Decimal * null;
        Assert.Equal(null, r.Value);
        r *= 2;
        Assert.Equal(null, r.Value);

        r = d.BigInteger * 1.1d;
        Assert.Equal(100m, (decimal)r);
        r *= 2;
        Assert.Equal(200m, (decimal)r);

        r = d.BigInteger * null;
        Assert.Equal(null, r.Value);
        r *= 2;
        Assert.Equal(null, r.Value);
        #endregion

        #region Divide
        r = d.Integer / 1;
        Assert.Equal(1, (int)r);
        r /= 2;
        Assert.Equal(0, (int)r);

        r = d.Integer / 1.1;
        Assert.Equal(0.9090909090909091d, (double)r);
        r /= 2;
        XUnitAssert.AreEqual(0.454545454545455d, (double)r, 0.00001);

        r = d.Integer / 1.1d;
        Assert.Equal(0.909090909090909m, (decimal)r);
        r /= 2;
        Assert.Equal(0.454545454545454m, (decimal)r);

        r = d.Integer / null;
        Assert.Equal(null, r.Value);
        r /= 2;
        Assert.Equal(null, r.Value);

        r = d.Float / 1;
        Assert.Equal(1.1d, (double)r);
        r /= 2;
        Assert.Equal(0.55d, (double)r);

        r = d.Float / 1.1;
        XUnitAssert.AreEqual(1d, (double)r, 0.00001);
        r /= 2;
        XUnitAssert.AreEqual(0.5d, (double)r, 0.00001);

        r = d.Float / 1.1d;
        Assert.Equal(1m, (decimal)r);
        r /= 2;
        Assert.Equal(0.5m, (decimal)r);

        r = d.Float / null;
        Assert.Equal(null, r.Value);
        r /= 2;
        Assert.Equal(null, r.Value);

        r = d.Decimal / 1;
        Assert.Equal(1.1m, (decimal)r);
        r /= 2;
        Assert.Equal(0.55m, (decimal)r);

        r = d.Decimal / 1.1;
        Assert.Equal(1m, (decimal)r);
        r /= 2;
        Assert.Equal(0.5m, (decimal)r);

        r = d.Decimal / 1.1d;
        Assert.Equal(1m, (decimal)r);
        r /= 2;
        Assert.Equal(0.5m, (decimal)r);

        r = d.Decimal / null;
        Assert.Equal(null, r.Value);
        r /= 2;
        Assert.Equal(null, r.Value);

        r = d.BigInteger / 1.1d;
        Assert.Equal(100m, (decimal)r);
        r /= 2;
        Assert.Equal(50m, (decimal)r);

        r = d.BigInteger / null;
        Assert.Equal(null, r.Value);
        r /= 2;
        Assert.Equal(null, r.Value);
        #endregion
    }

    [Fact]
    public void JValueToString()
    {
        var o = new JObject(
            new JProperty("Null", JValue.CreateNull()),
            new JProperty("Integer", new JValue(1)),
            new JProperty("Float", new JValue(1.1)),
            new JProperty("DateTime", new JValue(new DateTime(2000, 12, 29, 23, 51, 10, DateTimeKind.Utc))),
            new JProperty("Boolean", new JValue(true)),
            new JProperty("String", new JValue("A string lol!")),
            new JProperty("Bytes", new JValue(Encoding.UTF8.GetBytes("A string lol!"))),
            new JProperty("Uri", new Uri("http://json.codeplex.com/")),
            new JProperty("Guid", new Guid("EA27FE1D-0D80-44F2-BF34-4654156FA7AF")),
            new JProperty("TimeSpan", TimeSpan.FromDays(1))
            , new JProperty("BigInteger", new BigInteger(100))
        );

        dynamic d = o;

        Assert.Equal("", d.Null.ToString());
        Assert.Equal("1", d.Integer.ToString());
        Assert.Equal("1.1", d.Float.ToString(CultureInfo.InvariantCulture));
        Assert.Equal("12/29/2000 23:51:10", d.DateTime.ToString(null, CultureInfo.InvariantCulture));
        Assert.Equal("True", d.Boolean.ToString());
        Assert.Equal("A string lol!", d.String.ToString());
        Assert.Equal("System.Byte[]", d.Bytes.ToString());
        Assert.Equal("http://json.codeplex.com/", d.Uri.ToString());
        Assert.Equal("ea27fe1d-0d80-44f2-bf34-4654156fa7af", d.Guid.ToString());
        Assert.Equal("1.00:00:00", d.TimeSpan.ToString());
        Assert.Equal("100", d.BigInteger.ToString());
    }

    [Fact]
    public void JObjectGetDynamicPropertyNames()
    {
        var o = new JObject(
            new JProperty("ChildValue", "blah blah"),
            new JProperty("Hello Joe", null));

        dynamic d = o;

        var memberNames = o.GetDynamicMemberNames().ToList();

        Assert.Equal(2, memberNames.Count);
        Assert.Equal("ChildValue", memberNames[0]);
        Assert.Equal("Hello Joe", memberNames[1]);

        o = new JObject(
            new JProperty("ChildValue1", "blah blah"),
            new JProperty("Hello Joe1", null));

        d = o;

        memberNames = o.GetDynamicMemberNames().ToList();

        Assert.Equal(2, memberNames.Count);
        Assert.Equal("ChildValue1", memberNames[0]);
        Assert.Equal("Hello Joe1", memberNames[1]);
    }

    [Fact]
    public void JValueConvert()
    {
        AssertValueConverted<bool>(true);
        AssertValueConverted<bool?>(true);
        AssertValueConverted<bool?>(false);
        AssertValueConverted<bool?>(null);
        AssertValueConverted<bool?>("true", true);
        AssertValueConverted<byte[]>(null);
        AssertValueConverted<byte[]>(Encoding.UTF8.GetBytes("blah"));
        AssertValueConverted<DateTime>(new DateTime(2000, 12, 20, 23, 59, 2, DateTimeKind.Utc));
        AssertValueConverted<DateTime?>(new DateTime(2000, 12, 20, 23, 59, 2, DateTimeKind.Utc));
        AssertValueConverted<DateTime?>(null);
        AssertValueConverted<DateTimeOffset>(new DateTimeOffset(2000, 12, 20, 23, 59, 2, TimeSpan.FromHours(1)));
        AssertValueConverted<DateTimeOffset?>(new DateTimeOffset(2000, 12, 20, 23, 59, 2, TimeSpan.FromHours(1)));
        AssertValueConverted<DateTimeOffset?>(null);
        AssertValueConverted<decimal>(99.9m);
        AssertValueConverted<decimal?>(99.9m);
        AssertValueConverted<decimal>(1m);
        AssertValueConverted<decimal>(1.1f, 1.1m);
        AssertValueConverted<decimal>("1.1", 1.1m);
        AssertValueConverted<double>(99.9);
        AssertValueConverted<double>(99.9d);
        AssertValueConverted<double?>(99.9d);
        AssertValueConverted<float>(99.9f);
        AssertValueConverted<float?>(99.9f);
        AssertValueConverted<int>(int.MinValue);
        AssertValueConverted<int?>(int.MinValue);
        AssertValueConverted<long>(long.MaxValue);
        AssertValueConverted<long?>(long.MaxValue);
        AssertValueConverted<short>(short.MaxValue);
        AssertValueConverted<short?>(short.MaxValue);
        AssertValueConverted<string>("blah");
        AssertValueConverted<string>(null);
        AssertValueConverted<string>(1, "1");
        AssertValueConverted<uint>(uint.MinValue);
        AssertValueConverted<uint?>(uint.MinValue);
        AssertValueConverted<uint?>("1", (uint)1);
        AssertValueConverted<ulong>(ulong.MaxValue);
        AssertValueConverted<ulong?>(ulong.MaxValue);
        AssertValueConverted<ushort>(ushort.MinValue);
        AssertValueConverted<ushort?>(ushort.MinValue);
        AssertValueConverted<ushort?>(null);
        AssertValueConverted<TimeSpan>(TimeSpan.FromDays(1));
        AssertValueConverted<TimeSpan?>(TimeSpan.FromDays(1));
        AssertValueConverted<TimeSpan?>(null);
        AssertValueConverted<Guid>(new Guid("60304274-CD13-4060-B38C-057C8557AB54"));
        AssertValueConverted<Guid?>(new Guid("60304274-CD13-4060-B38C-057C8557AB54"));
        AssertValueConverted<Guid?>(null);
        AssertValueConverted<Uri>(new Uri("http://json.codeplex.com/"));
        AssertValueConverted<Uri>(null);
        AssertValueConverted<BigInteger>(new BigInteger(100));
        AssertValueConverted<BigInteger?>(null);
    }

    static void AssertValueConverted<T>(object value)
    {
        AssertValueConverted<T>(value, value);
    }

    static void AssertValueConverted<T>(object value, object expected)
    {
        var v = new JValue(value);
        dynamic d = v;

        T t = d;
        Assert.Equal(expected, t);
    }

    [Fact]
    public void DynamicSerializerExample()
    {
        dynamic value = new DynamicDictionary();

        value.Name = "Arine Admin";
        value.Enabled = true;
        value.Roles = new[] { "Admin", "User" };

        string json = JsonConvert.SerializeObject(value, Formatting.Indented);
        // {
        //   "Name": "Arine Admin",
        //   "Enabled": true,
        //   "Roles": [
        //     "Admin",
        //     "User"
        //   ]
        // }

        dynamic newValue = JsonConvert.DeserializeObject<DynamicDictionary>(json);

        string role = newValue.Roles[0];
        // Admin
    }

    [Fact]
    public void DynamicLinqExample()
    {
        var oldAndBusted = new JObject
        {
            ["Name"] = "Arnie Admin",
            ["Enabled"] = true,
            ["Roles"] = new JArray(new[] { "Admin", "User" })
        };

        var oldRole = (string)oldAndBusted["Roles"][0];
        // Admin

        dynamic newHotness = new JObject();
        newHotness.Name = "Arnie Admin";
        newHotness.Enabled = true;
        newHotness.Roles = new JArray(new[] { "Admin", "User" });

        string newRole = newHotness.Roles[0];
        // Admin

        Assert.Equal("Admin", oldRole);
        Assert.Equal("Admin", newRole);
    }

    [Fact]
    public void ImprovedDynamicLinqExample()
    {
        dynamic product = new JObject();
        product.ProductName = "Elbow Grease";
        product.Enabled = true;
        product.Price = 4.90m;
        product.StockCount = 9000;
        product.StockValue = 44100;

        // All Elbow Grease must go sale!
        // 50% off price

        product.Price = product.Price / 2;
        product.StockValue = product.StockCount * product.Price;
        product.ProductName = product.ProductName + " (SALE)";

        string json = product.ToString();
        // {
        //   "ProductName": "Elbow Grease (SALE)",
        //   "Enabled": true,
        //   "Price": 2.45,
        //   "StockCount": 9000,
        //   "StockValue": 22050.00
        // }

        XUnitAssert.AreEqualNormalized(@"{
  ""ProductName"": ""Elbow Grease (SALE)"",
  ""Enabled"": true,
  ""Price"": 2.45,
  ""StockCount"": 9000,
  ""StockValue"": 22050.00
}", json);
    }

    [Fact]
    public void DynamicAccess_ToJToken_ShouldNotFail()
    {
        var g = Guid.NewGuid();
        dynamic json = JObject.FromObject(new { uid = g });
        JToken token = json.uid;

        Assert.Equal(g, ((JValue)token).Value);
    }

    [Fact]
    public void DynamicAccess_ToJTokenExplicit_ShouldNotFail()
    {
        var g = Guid.NewGuid();
        dynamic json = JObject.FromObject(new { uid = g });
        var token = (JToken)json.uid;

        Assert.Equal(g, ((JValue)token).Value);
    }

    [Fact]
    public void DynamicAccess_ToJTokenSafeCast_ShouldNotFail()
    {
        var g = Guid.NewGuid();
        dynamic json = JObject.FromObject(new { uid = g });
        var token = json.uid as JToken;

        Assert.Equal(g, ((JValue)token).Value);
    }

    [Fact]
    public void IndexAccess_ToJToken_ShouldNotFail()
    {
        var g = Guid.NewGuid();
        var json = JObject.FromObject(new { uid = g });
        var token = json["uid"];

        Assert.Equal(g, ((JValue)token).Value);
    }

    [Fact]
    public void DynamicAccess_ToJToken_ShouldFail()
    {
        var g = Guid.NewGuid();
        dynamic json = JObject.FromObject(new { uid = g });

        XUnitAssert.Throws<InvalidOperationException>(
            () => { JObject token = json.uid; },
            "Can not convert from System.Guid to Argon.Linq.JObject.");
    }
}

public class DynamicDictionary : DynamicObject
{
    readonly IDictionary<string, object> _values = new Dictionary<string, object>();

    public override IEnumerable<string> GetDynamicMemberNames()
    {
        return _values.Keys;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        result = _values[binder.Name];
        return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        _values[binder.Name] = value;
        return true;
    }
}