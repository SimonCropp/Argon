# Serialize a DataSet

This sample serializes a `System.Data.DataSet` to JSON.

<!-- snippet: SerializeDataSet -->
<a id='snippet-serializedataset'></a>
```cs
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
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeDataSet.cs#L34-L75' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializedataset' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
