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
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using Argon.Converters;
using Argon.Tests.TestObjects;

namespace Argon.Tests.Converters
{
    [TestFixture]
    public class JavaScriptDateTimeConverterTests : TestFixtureBase
    {
        [Fact]
        public void SerializeDateTime()
        {
            var converter = new JavaScriptDateTimeConverter();

            var d = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc);
            string result;

            result = JsonConvert.SerializeObject(d, converter);
            Assert.AreEqual("new Date(976918263055)", result);
        }

        [Fact]
        public void SerializeDateTimeOffset()
        {
            var converter = new JavaScriptDateTimeConverter();

            var now = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero);
            string result;

            result = JsonConvert.SerializeObject(now, converter);
            Assert.AreEqual("new Date(976918263055)", result);
        }

        [Fact]
        public void SerializeNullableDateTimeClass()
        {
            var t = new NullableDateTimeTestClass
            {
                DateTimeField = null,
                DateTimeOffsetField = null
            };

            var converter = new JavaScriptDateTimeConverter();

            string result;

            result = JsonConvert.SerializeObject(t, converter);
            Assert.AreEqual(@"{""PreField"":null,""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":null}", result);

            t = new NullableDateTimeTestClass
            {
                DateTimeField = new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc),
                DateTimeOffsetField = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero)
            };

            result = JsonConvert.SerializeObject(t, converter);
            Assert.AreEqual(@"{""PreField"":null,""DateTimeField"":new Date(976918263055),""DateTimeOffsetField"":new Date(976918263055),""PostField"":null}", result);
        }

        [Fact]
        public void DeserializeNullToNonNullable()
        {
            ExceptionAssert.Throws<Exception>(() =>
            {
                var c2 =
                    JsonConvert.DeserializeObject<DateTimeTestClass>(@"{""PreField"":""Pre"",""DateTimeField"":null,""DateTimeOffsetField"":null,""PostField"":""Post""}", new JavaScriptDateTimeConverter());
            }, "Cannot convert null value to System.DateTime. Path 'DateTimeField', line 1, position 38.");
        }

        [Fact]
        public void DeserializeDateTimeOffset()
        {
            var converter = new JavaScriptDateTimeConverter();
            var start = new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero);

            var json = JsonConvert.SerializeObject(start, converter);

            var result = JsonConvert.DeserializeObject<DateTimeOffset>(json, converter);
            Assert.AreEqual(new DateTimeOffset(2000, 12, 15, 22, 11, 3, 55, TimeSpan.Zero), result);
        }

        [Fact]
        public void DeserializeDateTime()
        {
            var converter = new JavaScriptDateTimeConverter();

            var result = JsonConvert.DeserializeObject<DateTime>("new Date(976918263055)", converter);
            Assert.AreEqual(new DateTime(2000, 12, 15, 22, 11, 3, 55, DateTimeKind.Utc), result);
        }

        [Fact]
        public void DeserializeDateTime_MultipleArguments()
        {
            var converter = new JavaScriptDateTimeConverter();

            DateTime result;

            result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11)", converter);
            Assert.AreEqual(new DateTime(2000, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), result);

            result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 12)", converter);
            Assert.AreEqual(new DateTime(2000, 12, 12, 0, 0, 0, 0, DateTimeKind.Utc), result);

            result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 12, 20)", converter);
            Assert.AreEqual(new DateTime(2000, 12, 12, 20, 0, 0, 0, DateTimeKind.Utc), result);

            result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 12, 20, 1)", converter);
            Assert.AreEqual(new DateTime(2000, 12, 12, 20, 1, 0, 0, DateTimeKind.Utc), result);

            result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 12, 20, 1, 2)", converter);
            Assert.AreEqual(new DateTime(2000, 12, 12, 20, 1, 2, 0, DateTimeKind.Utc), result);

            result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 12, 20, 1, 2, 3)", converter);
            Assert.AreEqual(new DateTime(2000, 12, 12, 20, 1, 2, 3, DateTimeKind.Utc), result);

            result = JsonConvert.DeserializeObject<DateTime>("new Date(2000, 11, 1, 0, 0, 0, 0)", converter);
            Assert.AreEqual(new DateTime(2000, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), result);
        }

        [Fact]
        public void DeserializeDateTime_TooManyArguments()
        {
            var converter = new JavaScriptDateTimeConverter();

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<DateTime>("new Date(1, 2, 3, 4, 5, 6, 7, 8)", converter);
            }, "Unexpected number of arguments when reading date constructor. Path '', line 1, position 32.");
        }

        [Fact]
        public void DeserializeDateTime_NoArguments()
        {
            var converter = new JavaScriptDateTimeConverter();

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<DateTime>("new Date()", converter);
            }, "Date constructor has no arguments. Path '', line 1, position 10.");
        }

        [Fact]
        public void DeserializeDateTime_NotArgumentsNotClosed()
        {
            var converter = new JavaScriptDateTimeConverter();

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<DateTime>("new Date(", converter);
            }, "Unexpected end when reading date constructor. Path '', line 1, position 9.");
        }

        [Fact]
        public void DeserializeDateTime_NotClosed()
        {
            var converter = new JavaScriptDateTimeConverter();

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<DateTime>("new Date(2, 3", converter);
            }, "Unexpected end when reading date constructor. Path '[1]', line 1, position 13.");
        }

        [Fact]
        public void ConverterList()
        {
            var l1 = new ConverterList<object>();
            l1.Add(new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc));
            l1.Add(new DateTime(1983, 10, 9, 23, 10, 0, DateTimeKind.Utc));

            var json = JsonConvert.SerializeObject(l1, Formatting.Indented);
            StringAssert.AreEqual(@"[
  new Date(
    976651800000
  ),
  new Date(
    434589000000
  )
]", json);

            var l2 = JsonConvert.DeserializeObject<ConverterList<object>>(json);
            Assert.IsNotNull(l2);

            Assert.AreEqual(new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc), l2[0]);
            Assert.AreEqual(new DateTime(1983, 10, 9, 23, 10, 0, DateTimeKind.Utc), l2[1]);
        }

        [Fact]
        public void ConverterDictionary()
        {
            var l1 = new ConverterDictionary<object>();
            l1.Add("First", new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc));
            l1.Add("Second", new DateTime(1983, 10, 9, 23, 10, 0, DateTimeKind.Utc));

            var json = JsonConvert.SerializeObject(l1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""First"": new Date(
    976651800000
  ),
  ""Second"": new Date(
    434589000000
  )
}", json);

            var l2 = JsonConvert.DeserializeObject<ConverterDictionary<object>>(json);
            Assert.IsNotNull(l2);

            Assert.AreEqual(new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc), l2["First"]);
            Assert.AreEqual(new DateTime(1983, 10, 9, 23, 10, 0, DateTimeKind.Utc), l2["Second"]);
        }

        [Fact]
        public void ConverterObject()
        {
            var l1 = new ConverterObject();
            l1.Object1 = new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc);
            l1.Object2 = null;
            l1.ObjectNotHandled = new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc);

            var json = JsonConvert.SerializeObject(l1, Formatting.Indented);
            StringAssert.AreEqual(@"{
  ""Object1"": new Date(
    976651800000
  ),
  ""Object2"": null,
  ""ObjectNotHandled"": 631122486000000000
}", json);

            var l2 = JsonConvert.DeserializeObject<ConverterObject>(json);
            Assert.IsNotNull(l2);

            //Assert.AreEqual(new DateTime(2000, 12, 12, 20, 10, 0, DateTimeKind.Utc), l2["First"]);
            //Assert.AreEqual(new DateTime(1983, 10, 9, 23, 10, 0, DateTimeKind.Utc), l2["Second"]);
        }
    }

    [JsonArray(ItemConverterType = typeof(JavaScriptDateTimeConverter))]
    public class ConverterList<T> : List<T>
    {
    }

    [JsonDictionary(ItemConverterType = typeof(JavaScriptDateTimeConverter))]
    public class ConverterDictionary<T> : Dictionary<string, T>
    {
    }

    [JsonObject(ItemConverterType = typeof(JavaScriptDateTimeConverter))]
    public class ConverterObject
    {
        public object Object1 { get; set; }
        public object Object2 { get; set; }

        [JsonConverter(typeof(DateIntConverter))]
        public object ObjectNotHandled { get; set; }
    }

    public class DateIntConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var d = (DateTime?)value;
            if (d == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(d.Value.Ticks);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new DateTime(Convert.ToInt64(reader.Value), DateTimeKind.Utc);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }
    }
}