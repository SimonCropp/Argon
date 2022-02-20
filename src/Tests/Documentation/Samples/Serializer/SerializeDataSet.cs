﻿#region License
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


using System.Data;
using Argon.DataSetConverters;
using Xunit;

namespace Argon.Tests.Documentation.Samples.Serializer;

public class SerializeDataSet : TestFixtureBase
{
    [Fact]
    public void Example()
    {
        #region SerializeDataSet
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
            newRow["item"] = $"item {i}";
            table.Rows.Add(newRow);
        }

        dataSet.AcceptChanges();
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        settings.AddDataSetConverters();
        var json = JsonConvert.SerializeObject(dataSet, settings);

        Console.WriteLine(json);
        // {
        //   "Table1": [
        //     {
        //       "id": 0,
        //       "item": "item 0"
        //     },
        //     {
        //       "id": 1,
        //       "item": "item 1"
        //     }
        //   ]
        // }
        #endregion

        XUnitAssert.AreEqualNormalized(@"{
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
    }
}