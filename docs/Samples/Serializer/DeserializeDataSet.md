# Deserialize a DataSet

This sample deserializes JSON to a `System.Data.DataSet`.

<!-- snippet: DeserializeDataSet -->
<a id='snippet-deserializedataset'></a>
```cs
var json = @"{
      'Table1': [
        {
          'id': 0,
          'item': 'item 0'
        },
        {
          'id': 1,
          'item': 'item 1'
        }
      ]
    }";

var dataSet = JsonConvert.DeserializeObject<DataSet>(json);

var dataTable = dataSet.Tables["Table1"];

Console.WriteLine(dataTable.Rows.Count);
// 2

foreach (DataRow row in dataTable.Rows)
{
    Console.WriteLine($"{row["id"]} - {row["item"]}");
}
// 0 - item 0
// 1 - item 1
```
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/DeserializeDataSet.cs#L37-L64' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializedataset' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
