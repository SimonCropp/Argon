// Copyright (c) 2007 James Newton-King. All rights reserved.
// Use of this source code is governed by The MIT License,
// as found in the license.md file.

#if !NET5_0_OR_GREATER
using System.Data.Linq;
#endif
using System.Data.SqlTypes;

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

        Assert.Equal(new Binary(TestData), binaryClass.Binary);
        Assert.Equal(null, binaryClass.NullBinary);
    }

    [Fact]
    public void DeserializeBinaryClassFromJsonArray()
    {
        var json = @"{
  ""Binary"": [0, 1, 2, 3],
  ""NullBinary"": null
}";

        var binaryClass = JsonConvert.DeserializeObject<BinaryClass>(json, new BinaryConverter());

        Assert.Equal(new byte[] { 0, 1, 2, 3 }, binaryClass.Binary.ToArray());
        Assert.Equal(null, binaryClass.NullBinary);
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

        XUnitAssert.AreEqualNormalized(@"{
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

        XUnitAssert.AreEqualNormalized(@"{
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

        XUnitAssert.AreEqualNormalized(@"{
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

        Assert.Equal(new SqlBinary(TestData), sqlBinaryClass.SqlBinary);
        Assert.Equal(new SqlBinary(TestData), sqlBinaryClass.NullableSqlBinary1);
        Assert.Equal(null, sqlBinaryClass.NullableSqlBinary2);
    }

    [Fact]
    public void DeserializeByteArrayClass()
    {
        var json = @"{
  ""ByteArray"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullByteArray"": null
}";

        var byteArrayClass = JsonConvert.DeserializeObject<ByteArrayClass>(json);

        Assert.Equal(TestData, byteArrayClass.ByteArray);
        Assert.Equal(null, byteArrayClass.NullByteArray);
    }

    [Fact]
    public void DeserializeByteArrayFromJsonArray()
    {
        var json = @"{
  ""ByteArray"": [0, 1, 2, 3],
  ""NullByteArray"": null
}";

        var c = JsonConvert.DeserializeObject<ByteArrayClass>(json);
        Assert.NotNull(c.ByteArray);
        Assert.Equal(4, c.ByteArray.Length);
        Assert.Equal(new byte[] { 0, 1, 2, 3 }, c.ByteArray);
    }
}