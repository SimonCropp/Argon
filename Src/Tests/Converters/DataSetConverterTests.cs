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
using Argon.Tests.TestObjects;
using System.Data;

namespace Argon.Tests.Converters;

public class DataSetConverterTests : TestFixtureBase
{
    [Fact]
    public void DeserializeInvalidDataTable()
    {
        var ex = ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<DataSet>("{\"pending_count\":23,\"completed_count\":45}"), "Unexpected JSON token when reading DataTable. Expected StartArray, got Integer. Path 'pending_count', line 1, position 19.");

        Assert.AreEqual(1, ex.LineNumber);
        Assert.AreEqual(19, ex.LinePosition);
        Assert.AreEqual("pending_count", ex.Path);
    }

    [Fact]
    public void SerializeAndDeserialize()
    {
        var dataSet = new DataSet("dataSet");
        dataSet.Namespace = "NetFrameWork";
        var table = new DataTable();
        var idColumn = new DataColumn("id", typeof(int));
        idColumn.AutoIncrement = true;

        var itemColumn = new DataColumn("item");
        table.Columns.Add(idColumn);
        table.Columns.Add(itemColumn);
        dataSet.Tables.Add(table);

        for (var i = 0; i < 2; i++)
        {
            var newRow = table.NewRow();
            newRow["item"] = "item " + i;
            table.Rows.Add(newRow);
        }

        dataSet.AcceptChanges();

        var json = JsonConvert.SerializeObject(dataSet, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""Table1"": [
    {
      ""id"": 0,
      ""item"": ""item 0""
    },
    {
      ""id"": 1,
      ""item"": ""item 1""
    }
  ]
}", json);

        var deserializedDataSet = JsonConvert.DeserializeObject<DataSet>(json);
        Assert.IsNotNull(deserializedDataSet);

        Assert.AreEqual(1, deserializedDataSet.Tables.Count);

        var dt = deserializedDataSet.Tables[0];

        Assert.AreEqual("Table1", dt.TableName);
        Assert.AreEqual(2, dt.Columns.Count);
        Assert.AreEqual("id", dt.Columns[0].ColumnName);
        Assert.AreEqual(typeof(long), dt.Columns[0].DataType);
        Assert.AreEqual("item", dt.Columns[1].ColumnName);
        Assert.AreEqual(typeof(string), dt.Columns[1].DataType);

        Assert.AreEqual(2, dt.Rows.Count);
    }

    public class DataSetTestClass
    {
        public DataSet Set { get; set; }
    }

    [Fact]
    public void WriteJsonNull()
    {
        var sw = new StringWriter();
        var jsonWriter = new JsonTextWriter(sw);

        var converter = new DataSetConverter();
        converter.WriteJson(jsonWriter, null, null);

        StringAssert.AreEqual(@"null", sw.ToString());
    }

    [Fact]
    public void SerializeNull()
    {
        var c1 = new DataSetTestClass
        {
            Set = null
        };

        var json = JsonConvert.SerializeObject(c1, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""Set"": null
}", json);

        var c2 = JsonConvert.DeserializeObject<DataSetTestClass>(json);

        Assert.AreEqual(null, c2.Set);
    }

    [Fact]
    public void SerializeNullRoot()
    {
        var json = JsonConvert.SerializeObject(null, typeof(DataSet), new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        });

        StringAssert.AreEqual(@"null", json);
    }

    [Fact]
    public void DeserializeNullTable()
    {
        var json = @"{
  ""TableName"": null
}";

        var ds = JsonConvert.DeserializeObject<DataSet>(json);

        Assert.True( ds.Tables.Contains("TableName"));
    }

    [Fact]
    public void SerializeMultiTableDataSet()
    {
        var ds = new DataSet();
        ds.Tables.Add(CreateDataTable("FirstTable", 2));
        ds.Tables.Add(CreateDataTable("SecondTable", 1));

        var json = JsonConvert.SerializeObject(ds, Formatting.Indented, new IsoDateTimeConverter());
        // {
        //   "FirstTable": [
        //     {
        //       "StringCol": "Item Name",
        //       "Int32Col": 1,
        //       "BooleanCol": true,
        //       "TimeSpanCol": "10.22:10:15.1000000",
        //       "DateTimeCol": "2000-12-29T00:00:00Z",
        //       "DecimalCol": 64.0021
        //     },
        //     {
        //       "StringCol": "Item Name",
        //       "Int32Col": 2,
        //       "BooleanCol": true,
        //       "TimeSpanCol": "10.22:10:15.1000000",
        //       "DateTimeCol": "2000-12-29T00:00:00Z",
        //       "DecimalCol": 64.0021
        //     }
        //   ],
        //   "SecondTable": [
        //     {
        //       "StringCol": "Item Name",
        //       "Int32Col": 1,
        //       "BooleanCol": true,
        //       "TimeSpanCol": "10.22:10:15.1000000",
        //       "DateTimeCol": "2000-12-29T00:00:00Z",
        //       "DecimalCol": 64.0021
        //     }
        //   ]
        // }

        var deserializedDs = JsonConvert.DeserializeObject<DataSet>(json, new IsoDateTimeConverter());

        StringAssert.AreEqual(@"{
  ""FirstTable"": [
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 1,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    },
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 2,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    }
  ],
  ""SecondTable"": [
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 1,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    }
  ]
}", json);

        Assert.IsNotNull(deserializedDs);
    }

    [Fact]
    public void DeserializeMultiTableDataSet()
    {
        var json = @"{
  ""FirstTable"": [
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 2147483647,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    }
  ],
  ""SecondTable"": [
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 2147483647,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    }
  ]
}";

        var ds = JsonConvert.DeserializeObject<DataSet>(json);
        Assert.IsNotNull(ds);

        Assert.AreEqual(2, ds.Tables.Count);
        Assert.AreEqual("FirstTable", ds.Tables[0].TableName);
        Assert.AreEqual("SecondTable", ds.Tables[1].TableName);

        var dt = ds.Tables[0];
        Assert.AreEqual("StringCol", dt.Columns[0].ColumnName);
        Assert.AreEqual(typeof(string), dt.Columns[0].DataType);
        Assert.AreEqual("Int32Col", dt.Columns[1].ColumnName);
        Assert.AreEqual(typeof(long), dt.Columns[1].DataType);
        Assert.AreEqual("BooleanCol", dt.Columns[2].ColumnName);
        Assert.AreEqual(typeof(bool), dt.Columns[2].DataType);
        Assert.AreEqual("TimeSpanCol", dt.Columns[3].ColumnName);
        Assert.AreEqual(typeof(string), dt.Columns[3].DataType);
        Assert.AreEqual("DateTimeCol", dt.Columns[4].ColumnName);
        Assert.AreEqual(typeof(DateTime), dt.Columns[4].DataType);
        Assert.AreEqual("DecimalCol", dt.Columns[5].ColumnName);
        Assert.AreEqual(typeof(double), dt.Columns[5].DataType);

        Assert.AreEqual(1, ds.Tables[0].Rows.Count);
        Assert.AreEqual(1, ds.Tables[1].Rows.Count);
    }

    DataTable CreateDataTable(string dataTableName, int rows)
    {
        // create a new DataTable.
        var myTable = new DataTable(dataTableName);

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

        for (var i = 1; i <= rows; i++)
        {
            var myNewRow = myTable.NewRow();

            myNewRow["StringCol"] = "Item Name";
            myNewRow["Int32Col"] = i;
            myNewRow["BooleanCol"] = true;
            myNewRow["TimeSpanCol"] = new TimeSpan(10, 22, 10, 15, 100);
            myNewRow["DateTimeCol"] = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc);
            myNewRow["DecimalCol"] = 64.0021;
            myTable.Rows.Add(myNewRow);
        }

        return myTable;
    }

    public class DataSetAndTableTestClass
    {
        public string Before { get; set; }
        public DataSet Set { get; set; }
        public string Middle { get; set; }
        public DataTable Table { get; set; }
        public string After { get; set; }
    }

    [Fact]
    public void SerializeWithCamelCaseResolver()
    {
        var ds = new DataSet();
        ds.Tables.Add(CreateDataTable("FirstTable", 2));
        ds.Tables.Add(CreateDataTable("SecondTable", 1));

        var json = JsonConvert.SerializeObject(ds, Formatting.Indented, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        StringAssert.AreEqual(@"{
  ""firstTable"": [
    {
      ""stringCol"": ""Item Name"",
      ""int32Col"": 1,
      ""booleanCol"": true,
      ""timeSpanCol"": ""10.22:10:15.1000000"",
      ""dateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""decimalCol"": 64.0021
    },
    {
      ""stringCol"": ""Item Name"",
      ""int32Col"": 2,
      ""booleanCol"": true,
      ""timeSpanCol"": ""10.22:10:15.1000000"",
      ""dateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""decimalCol"": 64.0021
    }
  ],
  ""secondTable"": [
    {
      ""stringCol"": ""Item Name"",
      ""int32Col"": 1,
      ""booleanCol"": true,
      ""timeSpanCol"": ""10.22:10:15.1000000"",
      ""dateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""decimalCol"": 64.0021
    }
  ]
}", json);
    }

    [Fact]
    public void SerializeDataSetProperty()
    {
        var ds = new DataSet();
        ds.Tables.Add(CreateDataTable("FirstTable", 2));
        ds.Tables.Add(CreateDataTable("SecondTable", 1));

        var c = new DataSetAndTableTestClass
        {
            Before = "Before",
            Set = ds,
            Middle = "Middle",
            Table = CreateDataTable("LoneTable", 2),
            After = "After"
        };

        var json = JsonConvert.SerializeObject(c, Formatting.Indented, new IsoDateTimeConverter());

        StringAssert.AreEqual(@"{
  ""Before"": ""Before"",
  ""Set"": {
    ""FirstTable"": [
      {
        ""StringCol"": ""Item Name"",
        ""Int32Col"": 1,
        ""BooleanCol"": true,
        ""TimeSpanCol"": ""10.22:10:15.1000000"",
        ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
        ""DecimalCol"": 64.0021
      },
      {
        ""StringCol"": ""Item Name"",
        ""Int32Col"": 2,
        ""BooleanCol"": true,
        ""TimeSpanCol"": ""10.22:10:15.1000000"",
        ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
        ""DecimalCol"": 64.0021
      }
    ],
    ""SecondTable"": [
      {
        ""StringCol"": ""Item Name"",
        ""Int32Col"": 1,
        ""BooleanCol"": true,
        ""TimeSpanCol"": ""10.22:10:15.1000000"",
        ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
        ""DecimalCol"": 64.0021
      }
    ]
  },
  ""Middle"": ""Middle"",
  ""Table"": [
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 1,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    },
    {
      ""StringCol"": ""Item Name"",
      ""Int32Col"": 2,
      ""BooleanCol"": true,
      ""TimeSpanCol"": ""10.22:10:15.1000000"",
      ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
      ""DecimalCol"": 64.0021
    }
  ],
  ""After"": ""After""
}", json);

        var c2 = JsonConvert.DeserializeObject<DataSetAndTableTestClass>(json, new IsoDateTimeConverter());

        Assert.AreEqual(c.Before, c2.Before);
        Assert.AreEqual(c.Set.Tables.Count, c2.Set.Tables.Count);
        Assert.AreEqual(c.Middle, c2.Middle);
        Assert.AreEqual(c.Table.Rows.Count, c2.Table.Rows.Count);
        Assert.AreEqual(c.After, c2.After);
    }

    [Fact]
    public void SerializedTypedDataSet()
    {
        var ds = new CustomerDataSet();
        ds.Customers.AddCustomersRow("234");

        var json = JsonConvert.SerializeObject(ds, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""Customers"": [
    {
      ""CustomerID"": ""234""
    }
  ]
}", json);

        var ds1 = new CustomerDataSet();
        var table = ds1.Tables["Customers"];
        var row = ds1.Tables["Customers"].NewRow();
        row["CustomerID"] = "234";

        table.Rows.Add(row);

        var json1 = JsonConvert.SerializeObject(ds1, Formatting.Indented);

        StringAssert.AreEqual(@"{
  ""Customers"": [
    {
      ""CustomerID"": ""234""
    }
  ]
}", json1);
    }

    [Fact]
    public void DeserializedTypedDataSet()
    {
        var json = @"{
  ""Customers"": [
    {
      ""CustomerID"": ""234""
    }
  ]
}";

        var ds = JsonConvert.DeserializeObject<CustomerDataSet>(json);

        Assert.AreEqual("234", ds.Customers[0].CustomerID);
    }

    [Fact]
    public void ContractResolverInsideConverter()
    {
        var test = new MultipleDataTablesJsonTest
        {
            TableWrapper1 = new DataTableWrapper { DataTableProperty = CreateDataTable(3, "Table1Col") },
            TableWrapper2 = new DataTableWrapper { DataTableProperty = CreateDataTable(3, "Table2Col") }
        };

        var json = JsonConvert.SerializeObject(test, Formatting.Indented, new LowercaseDataTableConverter());

        StringAssert.AreEqual(@"{
  ""TableWrapper1"": {
    ""DataTableProperty"": [
      {
        ""table1col1"": ""1"",
        ""table1col2"": ""2"",
        ""table1col3"": ""3""
      }
    ],
    ""StringProperty"": null,
    ""IntProperty"": 0
  },
  ""TableWrapper2"": {
    ""DataTableProperty"": [
      {
        ""table2col1"": ""1"",
        ""table2col2"": ""2"",
        ""table2col3"": ""3""
      }
    ],
    ""StringProperty"": null,
    ""IntProperty"": 0
  }
}", json);
    }

    static DataTable CreateDataTable(int cols, string colNamePrefix)
    {
        var table = new DataTable();
        for (var i = 1; i <= cols; i++)
        {
            table.Columns.Add(new DataColumn { ColumnName = colNamePrefix + i, DefaultValue = i });
        }
        table.Rows.Add(table.NewRow());
        return table;
    }

    public class DataTableWrapper
    {
        public DataTable DataTableProperty { get; set; }
        public String StringProperty { get; set; }
        public Int32 IntProperty { get; set; }
    }

    public class MultipleDataTablesJsonTest
    {
        public DataTableWrapper TableWrapper1 { get; set; }
        public DataTableWrapper TableWrapper2 { get; set; }
    }

    public class LowercaseDataTableConverter : DataTableConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dataTableSerializer = new JsonSerializer { ContractResolver = new LowercaseContractResolver() };

            base.WriteJson(writer, value, dataTableSerializer);
        }
    }

    public class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }
}