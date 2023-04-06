# Creates JTokenWriter JToken

This sample creates a `Argon.JTokenWriter` from a `Argon.JToken`.

<!-- snippet: CreateWriter -->
<a id='snippet-createwriter'></a>
```cs
var o = new JObject
{
    {"name1", "value1"},
    {"name2", "value2"}
};

var writer = o.CreateWriter();
writer.WritePropertyName("name3");
writer.WriteStartArray();
writer.WriteValue(1);
writer.WriteValue(2);
writer.WriteEndArray();

Console.WriteLine(o.ToString());
// {
//   "name1": "value1",
//   "name2": "value2",
//   "name3": [
//     1,
//     2
//   ]
// }
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/CreateWriter.cs#L12-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-createwriter' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
