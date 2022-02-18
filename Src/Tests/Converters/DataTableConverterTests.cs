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
using System.Data;
using Argon.Tests.TestObjects;

namespace Argon.Tests.Converters;

public class DataTableConverterTests : TestFixtureBase
{
    [Fact]
    public void DeserializeEmptyNestedArray()
    {
        var jsonString2 = @"[{""col1"": []}]";

        var dt = JsonConvert.DeserializeObject<DataTable>(jsonString2);

        Assert.AreEqual(1, dt.Columns.Count);
        Assert.AreEqual(typeof(string[]), dt.Columns["col1"].DataType);

        Assert.AreEqual(1, dt.Rows.Count);
        Assert.IsNotNull(dt.Rows[0]["col1"]);

        var value = (object[])dt.Rows[0]["col1"];
        Assert.AreEqual(0, value.Length);
    }

    [Fact]
    public void SerializeNullValues()
    {
        var dt = new DataTable();
        var types = new List<Type>
        {
            typeof(TimeSpan),
            typeof(char[]),
            typeof(Type),
            typeof(Object),
            typeof(byte[]),
            typeof(Uri),
            typeof(Guid),
            typeof(BigInteger)
        };

        foreach (var ss in types)
        {
            dt.Columns.Add(ss.Name, ss);
        }

        dt.Rows.Add(types.Select(_ => (object)null).ToArray());

        var sw = new StringWriter();
        var jsonWriter = new JsonTextWriter(sw);
        jsonWriter.Formatting = Formatting.Indented;

        var converter = new DataTableConverter();
        converter.WriteJson(jsonWriter, dt, new JsonSerializer());

        StringAssert.AreEqual(@"[
  {
    ""TimeSpan"": null,
    ""Char[]"": null,
    ""Type"": null,
    ""Object"": null,
    ""Byte[]"": null,
    ""Uri"": null,
    ""Guid"": null,
    ""BigInteger"": null
  }
]", sw.ToString());
    }

    [Fact]
    public void SerializeValues()
    {
        var dt = new DataTable();
        var types = new Dictionary<Type, object>
        {
            [typeof(TimeSpan)] = TimeSpan.Zero,
            [typeof(char[])] = new char[] {'a', 'b', 'c' },
            [typeof(Type)] = typeof(string),
            [typeof(Object)] = new(),
            [typeof(byte[])] = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            [typeof(Uri)] = new Uri("http://localhost"),
            [typeof(Guid)] = new Guid(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11),
            [typeof(BigInteger)] = BigInteger.Parse("10000000000000000000000000000000000")
        };

        foreach (var ss in types)
        {
            dt.Columns.Add(ss.Key.Name, ss.Key);
        }

        dt.Rows.Add(types.Select(t => t.Value).ToArray());

        var sw = new StringWriter();
        var jsonWriter = new JsonTextWriter(sw);
        jsonWriter.Formatting = Formatting.Indented;

        var converter = new DataTableConverter();
        converter.WriteJson(jsonWriter, dt, new JsonSerializer());

        var stringName = typeof(string).AssemblyQualifiedName;

        StringAssert.AreEqual(@"[
  {
    ""TimeSpan"": ""00:00:00"",
    ""Char[]"": [
      ""a"",
      ""b"",
      ""c""
    ],
    ""Type"": """ + stringName + @""",
    ""Object"": {},
    ""Byte[]"": ""AQIDBAUGBwg="",
    ""Uri"": ""http://localhost"",
    ""Guid"": ""00000001-0002-0003-0405-060708090a0b"",
    ""BigInteger"": 10000000000000000000000000000000000
  }
]", sw.ToString());
    }

    [Fact]
    public void WriteJsonNull()
    {
        var sw = new StringWriter();
        var jsonWriter = new JsonTextWriter(sw);

        var converter = new DataTableConverter();
        converter.WriteJson(jsonWriter, null, null);

        StringAssert.AreEqual(@"null", sw.ToString());
    }

    [Fact]
    public void Deserialize()
    {
        var json = @"[
  {
    ""id"": 0,
    ""item"": ""item 0"",
    ""DataTableCol"": [
      {
        ""NestedStringCol"": ""0!""
      }
    ],
    ""ArrayCol"": [
      0
    ],
    ""DateCol"": ""2000-12-29T00:00:00Z""
  },
  {
    ""id"": 1,
    ""item"": ""item 1"",
    ""DataTableCol"": [
      {
        ""NestedStringCol"": ""1!""
      }
    ],
    ""ArrayCol"": [
      1
    ],
    ""DateCol"": ""2000-12-29T00:00:00Z""
  }
]";

        var deserializedDataTable = JsonConvert.DeserializeObject<DataTable>(json);
        Assert.IsNotNull(deserializedDataTable);

        Assert.AreEqual(string.Empty, deserializedDataTable.TableName);
        Assert.AreEqual(5, deserializedDataTable.Columns.Count);
        Assert.AreEqual("id", deserializedDataTable.Columns[0].ColumnName);
        Assert.AreEqual(typeof(long), deserializedDataTable.Columns[0].DataType);
        Assert.AreEqual("item", deserializedDataTable.Columns[1].ColumnName);
        Assert.AreEqual(typeof(string), deserializedDataTable.Columns[1].DataType);
        Assert.AreEqual("DataTableCol", deserializedDataTable.Columns[2].ColumnName);
        Assert.AreEqual(typeof(DataTable), deserializedDataTable.Columns[2].DataType);
        Assert.AreEqual("ArrayCol", deserializedDataTable.Columns[3].ColumnName);
        Assert.AreEqual(typeof(long[]), deserializedDataTable.Columns[3].DataType);
        Assert.AreEqual("DateCol", deserializedDataTable.Columns[4].ColumnName);
        Assert.AreEqual(typeof(DateTime), deserializedDataTable.Columns[4].DataType);

        Assert.AreEqual(2, deserializedDataTable.Rows.Count);

        var dr1 = deserializedDataTable.Rows[0];
        Assert.AreEqual(0L, dr1["id"]);
        Assert.AreEqual("item 0", dr1["item"]);
        Assert.AreEqual("0!", ((DataTable)dr1["DataTableCol"]).Rows[0]["NestedStringCol"]);
        Assert.AreEqual(0L, ((long[])dr1["ArrayCol"])[0]);
        Assert.AreEqual(new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc), dr1["DateCol"]);

        var dr2 = deserializedDataTable.Rows[1];
        Assert.AreEqual(1L, dr2["id"]);
        Assert.AreEqual("item 1", dr2["item"]);
        Assert.AreEqual("1!", ((DataTable)dr2["DataTableCol"]).Rows[0]["NestedStringCol"]);
        Assert.AreEqual(1L, ((long[])dr2["ArrayCol"])[0]);
        Assert.AreEqual(new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc), dr2["DateCol"]);
    }

    [Fact]
    public void DeserializeParseHandling()
    {
        var json = @"[
  {
    ""DateCol"": ""2000-12-29T00:00:00Z"",
    ""FloatCol"": 99.9999999999999999999
  },
  {
    ""DateCol"": ""2000-12-29T00:00:00Z"",
    ""FloatCol"": 99.9999999999999999999
  }
]";

        var deserializedDataTable = JsonConvert.DeserializeObject<DataTable>(json, new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.DateTimeOffset,
            FloatParseHandling = FloatParseHandling.Decimal
        });
        Assert.IsNotNull(deserializedDataTable);

        Assert.AreEqual(string.Empty, deserializedDataTable.TableName);
        Assert.AreEqual(2, deserializedDataTable.Columns.Count);
        Assert.AreEqual("DateCol", deserializedDataTable.Columns[0].ColumnName);
        Assert.AreEqual(typeof(DateTimeOffset), deserializedDataTable.Columns[0].DataType);
        Assert.AreEqual("FloatCol", deserializedDataTable.Columns[1].ColumnName);
        Assert.AreEqual(typeof(decimal), deserializedDataTable.Columns[1].DataType);

        Assert.AreEqual(2, deserializedDataTable.Rows.Count);

        var dr1 = deserializedDataTable.Rows[0];
        Assert.AreEqual(new DateTimeOffset(2000, 12, 29, 0, 0, 0, TimeSpan.Zero), dr1["DateCol"]);
        Assert.AreEqual(99.9999999999999999999m, dr1["FloatCol"]);

        var dr2 = deserializedDataTable.Rows[1];
        Assert.AreEqual(new DateTimeOffset(2000, 12, 29, 0, 0, 0, TimeSpan.Zero), dr2["DateCol"]);
        Assert.AreEqual(99.9999999999999999999m, dr2["FloatCol"]);
    }

    [Fact]
    public void Serialize()
    {
        // create a new DataTable.
        var myTable = new DataTable("blah");

        // create DataColumn objects of data types.
        var colString = new DataColumn("StringCol");
        colString.DataType = typeof(string);
        myTable.Columns.Add(colString);

        var colInt32 = new DataColumn("Int32Col");
        colInt32.DataType = typeof(int);
        myTable.Columns.Add(colInt32);

        var colBoolean = new DataColumn("BooleanCol");
        colBoolean.DataType = typeof(bool);
        myTable.Columns.Add(colBoolean);

        var colTimeSpan = new DataColumn("TimeSpanCol");
        colTimeSpan.DataType = typeof(TimeSpan);
        myTable.Columns.Add(colTimeSpan);

        var colDateTime = new DataColumn("DateTimeCol");
        colDateTime.DataType = typeof(DateTime);
        colDateTime.DateTimeMode = DataSetDateTime.Utc;
        myTable.Columns.Add(colDateTime);

        var colDecimal = new DataColumn("DecimalCol");
        colDecimal.DataType = typeof(decimal);
        myTable.Columns.Add(colDecimal);

        var colDataTable = new DataColumn("DataTableCol");
        colDataTable.DataType = typeof(DataTable);
        myTable.Columns.Add(colDataTable);

        var colArray = new DataColumn("ArrayCol");
        colArray.DataType = typeof(int[]);
        myTable.Columns.Add(colArray);

        var colBytes = new DataColumn("BytesCol");
        colBytes.DataType = typeof(byte[]);
        myTable.Columns.Add(colBytes);

        // populate one row with values.
        var myNewRow = myTable.NewRow();

        myNewRow["StringCol"] = "Item Name";
        myNewRow["Int32Col"] = 2147483647;
        myNewRow["BooleanCol"] = true;
        myNewRow["TimeSpanCol"] = new TimeSpan(10, 22, 10, 15, 100);
        myNewRow["DateTimeCol"] = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc);
        myNewRow["DecimalCol"] = 64.0021;
        myNewRow["ArrayCol"] = new[] { 1 };
        myNewRow["BytesCol"] = Encoding.UTF8.GetBytes("Hello world");

        var nestedTable = new DataTable("Nested");
        var nestedColString = new DataColumn("NestedStringCol");
        nestedColString.DataType = typeof(string);
        nestedTable.Columns.Add(nestedColString);
        var myNewNestedRow = nestedTable.NewRow();
        myNewNestedRow["NestedStringCol"] = "Nested!";
        nestedTable.Rows.Add(myNewNestedRow);

        myNewRow["DataTableCol"] = nestedTable;
        myTable.Rows.Add(myNewRow);

        var json = JsonConvert.SerializeObject(myTable, Formatting.Indented);
        StringAssert.AreEqual(@"[
  {
    ""StringCol"": ""Item Name"",
    ""Int32Col"": 2147483647,
    ""BooleanCol"": true,
    ""TimeSpanCol"": ""10.22:10:15.1000000"",
    ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
    ""DecimalCol"": 64.0021,
    ""DataTableCol"": [
      {
        ""NestedStringCol"": ""Nested!""
      }
    ],
    ""ArrayCol"": [
      1
    ],
    ""BytesCol"": ""SGVsbG8gd29ybGQ=""
  }
]", json);
    }

    public class TestDataTableConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var d = (DataTable)value;
            writer.WriteValue(d.TableName);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //reader.Read();
            var d = new DataTable((string)reader.Value);

            return d;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataTable);
        }
    }

    [Fact]
    public void PassedInJsonConverterOverridesInternalConverter()
    {
        var t1 = new DataTable("Custom");

        var json = JsonConvert.SerializeObject(t1, Formatting.Indented, new TestDataTableConverter());
        Assert.AreEqual(@"""Custom""", json);

        var t2 = JsonConvert.DeserializeObject<DataTable>(json, new TestDataTableConverter());
        Assert.AreEqual(t1.TableName, t2.TableName);
    }

#pragma warning disable 618
    [Fact]
    public void RoundtripBsonBytes()
    {
        var g = new Guid("EDE9A599-A7D9-44A9-9243-7C287049DD20");

        var table = new DataTable();
        table.Columns.Add("data", typeof(byte[]));
        table.Columns.Add("id", typeof(Guid));
        table.Rows.Add(Encoding.UTF8.GetBytes("Hello world!"), g);

        var serializer = new JsonSerializer();

        var ms = new MemoryStream();
        var bw = new BsonWriter(ms);

        serializer.Serialize(bw, table);

        var o = JToken.ReadFrom(new BsonReader(new MemoryStream(ms.ToArray())) { ReadRootValueAsArray = true });
        StringAssert.AreEqual(@"[
  {
    ""data"": ""SGVsbG8gd29ybGQh"",
    ""id"": ""ede9a599-a7d9-44a9-9243-7c287049dd20""
  }
]", o.ToString());

        var deserializedDataTable = serializer.Deserialize<DataTable>(new BsonReader(new MemoryStream(ms.ToArray())) { ReadRootValueAsArray = true });

        Assert.AreEqual(string.Empty, deserializedDataTable.TableName);
        Assert.AreEqual(2, deserializedDataTable.Columns.Count);
        Assert.AreEqual("data", deserializedDataTable.Columns[0].ColumnName);
        Assert.AreEqual(typeof(byte[]), deserializedDataTable.Columns[0].DataType);
        Assert.AreEqual("id", deserializedDataTable.Columns[1].ColumnName);
        Assert.AreEqual(typeof(Guid), deserializedDataTable.Columns[1].DataType);

        Assert.AreEqual(1, deserializedDataTable.Rows.Count);

        var dr1 = deserializedDataTable.Rows[0];
        Xunit.Assert.Equal(Encoding.UTF8.GetBytes("Hello world!"), (byte[])dr1["data"]);
        Assert.AreEqual(g, (Guid)dr1["id"]);
    }
#pragma warning restore 618

    [Fact]
    public void SerializeDataTableWithNull()
    {
        var table = new DataTable();
        table.Columns.Add("item");
        table.Columns.Add("price", typeof(double));
        table.Rows.Add("shirt", 49.99);
        table.Rows.Add("pants", 54.99);
        table.Rows.Add("shoes"); // no price

        var json = JsonConvert.SerializeObject(table);
        Assert.AreEqual(@"["
                        + @"{""item"":""shirt"",""price"":49.99},"
                        + @"{""item"":""pants"",""price"":54.99},"
                        + @"{""item"":""shoes"",""price"":null}]", json);
    }

    [Fact]
    public void SerializeDataTableWithNullAndIgnoreNullHandling()
    {
        var table = new DataTable();
        table.Columns.Add("item");
        table.Columns.Add("price", typeof(double));
        table.Rows.Add("shirt", 49.99);
        table.Rows.Add("pants", 54.99);
        table.Rows.Add("shoes"); // no price

        var json = JsonConvert.SerializeObject(table, Formatting.None, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        Assert.AreEqual(@"["
                        + @"{""item"":""shirt"",""price"":49.99},"
                        + @"{""item"":""pants"",""price"":54.99},"
                        + @"{""item"":""shoes""}]", json);
    }

    [Fact]
    public void DerializeDataTableWithImplicitNull()
    {
        const string json = @"["
                            + @"{""item"":""shirt"",""price"":49.99},"
                            + @"{""item"":""pants"",""price"":54.99},"
                            + @"{""item"":""shoes""}]";
        var table = JsonConvert.DeserializeObject<DataTable>(json);
        Assert.AreEqual("shirt", table.Rows[0]["item"]);
        Assert.AreEqual("pants", table.Rows[1]["item"]);
        Assert.AreEqual("shoes", table.Rows[2]["item"]);
        Assert.AreEqual(49.99, (double)table.Rows[0]["price"], 0.01);
        Assert.AreEqual(54.99, (double)table.Rows[1]["price"], 0.01);
        CustomAssert.IsInstanceOfType(typeof(DBNull), table.Rows[2]["price"]);
    }

    [Fact]
    public void DerializeDataTableWithExplicitNull()
    {
        const string json = @"["
                            + @"{""item"":""shirt"",""price"":49.99},"
                            + @"{""item"":""pants"",""price"":54.99},"
                            + @"{""item"":""shoes"",""price"":null}]";
        var table = JsonConvert.DeserializeObject<DataTable>(json);
        Assert.AreEqual("shirt", table.Rows[0]["item"]);
        Assert.AreEqual("pants", table.Rows[1]["item"]);
        Assert.AreEqual("shoes", table.Rows[2]["item"]);
        Assert.AreEqual(49.99, (double)table.Rows[0]["price"], 0.01);
        Assert.AreEqual(54.99, (double)table.Rows[1]["price"], 0.01);
        CustomAssert.IsInstanceOfType(typeof(DBNull), table.Rows[2]["price"]);
    }

    [Fact]
    public void SerializeKeyValuePairWithDataTableKey()
    {
        var table = new DataTable();
        var idColumn = new DataColumn("id", typeof(int));
        idColumn.AutoIncrement = true;

        var itemColumn = new DataColumn("item");
        table.Columns.Add(idColumn);
        table.Columns.Add(itemColumn);

        var r = table.NewRow();
        r["item"] = "item!";
        r.EndEdit();
        table.Rows.Add(r);

        var pair = new KeyValuePair<DataTable, int>(table, 1);
        var serializedpair = JsonConvert.SerializeObject(pair, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""Key"": [
    {
      ""id"": 0,
      ""item"": ""item!""
    }
  ],
  ""Value"": 1
}", serializedpair);

        var pair2 = (KeyValuePair<DataTable, int>)JsonConvert.DeserializeObject(serializedpair, typeof(KeyValuePair<DataTable, int>));

        Assert.AreEqual(1, pair2.Value);
        Assert.AreEqual(1, pair2.Key.Rows.Count);
        Assert.AreEqual("item!", pair2.Key.Rows[0]["item"]);
    }

    [Fact]
    public void SerializedTypedDataTable()
    {
        var dt = new CustomerDataSet.CustomersDataTable();
        dt.AddCustomersRow("432");

        var json = JsonConvert.SerializeObject(dt, Formatting.Indented);

        StringAssert.AreEqual(@"[
  {
    ""CustomerID"": ""432""
  }
]", json);
    }

    [Fact]
    public void DeserializedTypedDataTable()
    {
        var json = @"[
  {
    ""CustomerID"": ""432""
  }
]";

        var dt = JsonConvert.DeserializeObject<CustomerDataSet.CustomersDataTable>(json);

        Assert.AreEqual("432", dt[0].CustomerID);
    }

    public class DataTableTestClass
    {
        public DataTable Table { get; set; }
    }

    [Fact]
    public void SerializeNull()
    {
        var c1 = new DataTableTestClass
        {
            Table = null
        };

        var json = JsonConvert.SerializeObject(c1, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""Table"": null
}", json);

        var c2 = JsonConvert.DeserializeObject<DataTableTestClass>(json);

        Assert.AreEqual(null, c2.Table);
    }

    [Fact]
    public void SerializeNullRoot()
    {
        var json = JsonConvert.SerializeObject(null, typeof(DataTable), new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        });

        StringAssert.AreEqual(@"null", json);
    }

    [Fact]
    public void DeserializedTypedDataTableWithConverter()
    {
        var json = @"{
  ""TestTable"": [
    {
      ""DateTimeValue"": ""2015-11-28T00:00:00""
    },
    {
      ""DateTimeValue"": null
    }
  ]
}";

        var ds = JsonConvert.DeserializeObject<SqlTypesDataSet>(json, new SqlDateTimeConverter());

        Assert.AreEqual(new System.Data.SqlTypes.SqlDateTime(2015, 11, 28), ds.TestTable[0].DateTimeValue);
        Assert.AreEqual(System.Data.SqlTypes.SqlDateTime.Null, ds.TestTable[1].DateTimeValue);

        var json2 = JsonConvert.SerializeObject(ds, Formatting.Indented, new SqlDateTimeConverter());

        StringAssert.AreEqual(json, json2);
    }

    internal class SqlDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(System.Data.SqlTypes.SqlDateTime).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null || reader.Value == DBNull.Value)
            {
                return System.Data.SqlTypes.SqlDateTime.Null;
            }
            else
            {
                return new System.Data.SqlTypes.SqlDateTime((DateTime)serializer.Deserialize(reader));
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (((System.Data.SqlTypes.SqlDateTime)value).IsNull)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(((System.Data.SqlTypes.SqlDateTime)value).Value);
            }
        }
    }

    [Fact]
    public void HandleColumnOnError()
    {
        var json = "[{\"timeCol\":\"\"}]";
        DataTable table = JsonConvert.DeserializeObject<CustomDataTable>(json);

        Assert.AreEqual(DBNull.Value, table.Rows[0]["timeCol"]);
    }

    [JsonConverter(typeof(DataTableConverterTest))]
    public class CustomDataTable : DataTable
    {
    }

    public class DataTableConverterTest : DataTableConverter
    {
        protected DataTable CreateTable()
        {
            var table = new CustomDataTable();
            table.Columns.Add("timeCol", typeof(DateTime));
            return table;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (existingValue == null)
            {
                existingValue = CreateTable();
            }

            serializer.Error += OnError;
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            finally
            {
                serializer.Error -= OnError;
            }
        }

        void OnError(object sender, Argon.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }
    }
}