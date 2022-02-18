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

#if !NET5_0_OR_GREATER
using System.Data.Linq;
#endif
using System.Data.SqlTypes;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;

namespace Argon.Tests.Converters;

public class BinaryConverterTests : TestFixtureBase
{
    static readonly byte[] TestData = Encoding.UTF8.GetBytes("This is some test data!!!");

    public class ByteArrayClass
    {
        public byte[] ByteArray { get; set; }
        public byte[] NullByteArray { get; set; }
    }

#if !NET5_0_OR_GREATER
    [Fact]
    public void DeserializeBinaryClass()
    {
        var json = @"{
  ""Binary"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullBinary"": null
}";

        var binaryClass = JsonConvert.DeserializeObject<BinaryClass>(json, new BinaryConverter());

        Assert.AreEqual(new Binary(TestData), binaryClass.Binary);
        Assert.AreEqual(null, binaryClass.NullBinary);
    }

    [Fact]
    public void DeserializeBinaryClassFromJsonArray()
    {
        var json = @"{
  ""Binary"": [0, 1, 2, 3],
  ""NullBinary"": null
}";

        var binaryClass = JsonConvert.DeserializeObject<BinaryClass>(json, new BinaryConverter());

        Assert.AreEqual(new byte[] { 0, 1, 2, 3 }, binaryClass.Binary.ToArray());
        Assert.AreEqual(null, binaryClass.NullBinary);
    }

    public class BinaryClass
    {
        public Binary Binary { get; set; }
        public Binary NullBinary { get; set; }
    }

    [Fact]
    public void SerializeBinaryClass()
    {
        var binaryClass = new BinaryClass
        {
            Binary = new Binary(TestData),
            NullBinary = null
        };

        var json = JsonConvert.SerializeObject(binaryClass, Formatting.Indented, new BinaryConverter());

        StringAssert.AreEqual(@"{
  ""Binary"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullBinary"": null
}", json);
    }
#endif

    [Fact]
    public void SerializeByteArrayClass()
    {
        var byteArrayClass = new ByteArrayClass
        {
            ByteArray = TestData,
            NullByteArray = null
        };

        var json = JsonConvert.SerializeObject(byteArrayClass, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""ByteArray"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullByteArray"": null
}", json);
    }

    public class SqlBinaryClass
    {
        public SqlBinary SqlBinary { get; set; }
        public SqlBinary? NullableSqlBinary1 { get; set; }
        public SqlBinary? NullableSqlBinary2 { get; set; }
    }

    [Fact]
    public void SerializeSqlBinaryClass()
    {
        var sqlBinaryClass = new SqlBinaryClass
        {
            SqlBinary = new SqlBinary(TestData),
            NullableSqlBinary1 = new SqlBinary(TestData),
            NullableSqlBinary2 = null
        };

        var json = JsonConvert.SerializeObject(sqlBinaryClass, Formatting.Indented, new BinaryConverter());

        StringAssert.AreEqual(@"{
  ""SqlBinary"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullableSqlBinary1"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullableSqlBinary2"": null
}", json);
    }

    [Fact]
    public void DeserializeSqlBinaryClass()
    {
        var json = @"{
  ""SqlBinary"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullableSqlBinary1"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullableSqlBinary2"": null
}";

        var sqlBinaryClass = JsonConvert.DeserializeObject<SqlBinaryClass>(json, new BinaryConverter());

        Assert.AreEqual(new SqlBinary(TestData), sqlBinaryClass.SqlBinary);
        Assert.AreEqual(new SqlBinary(TestData), sqlBinaryClass.NullableSqlBinary1);
        Assert.AreEqual(null, sqlBinaryClass.NullableSqlBinary2);
    }

    [Fact]
    public void DeserializeByteArrayClass()
    {
        var json = @"{
  ""ByteArray"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullByteArray"": null
}";

        var byteArrayClass = JsonConvert.DeserializeObject<ByteArrayClass>(json);

        Xunit.Assert.Equal(TestData, byteArrayClass.ByteArray);
        Assert.AreEqual(null, byteArrayClass.NullByteArray);
    }

    [Fact]
    public void DeserializeByteArrayFromJsonArray()
    {
        var json = @"{
  ""ByteArray"": [0, 1, 2, 3],
  ""NullByteArray"": null
}";

        var c = JsonConvert.DeserializeObject<ByteArrayClass>(json);
        Assert.IsNotNull(c.ByteArray);
        Assert.AreEqual(4, c.ByteArray.Length);
        Xunit.Assert.Equal(new byte[] { 0, 1, 2, 3 }, c.ByteArray);
    }
}