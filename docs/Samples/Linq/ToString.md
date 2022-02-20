# Write JSON text with JToken.ToString

This sample converts LINQ to JSON objects to JSON.

<!-- snippet: ToString -->
<a id='snippet-tostring'></a>
```cs
var o = JObject.Parse(@"{'string1':'value','integer2':99,'datetime3':'2000-05-23T00:00:00'}");

Console.WriteLine(o.ToString());
// {
//   "string1": "value",
//   "integer2": 99,
//   "datetime3": "2000-05-23T00:00:00"
// }

Console.WriteLine(o.ToString(Formatting.None));
// {"string1":"value","integer2":99,"datetime3":"2000-05-23T00:00:00"}

Console.WriteLine(o.ToString(Formatting.None, new JavaScriptDateTimeConverter()));
// {"string1":"value","integer2":99,"datetime3":new Date(959032800000)}
```
<sup><a href='/Src/Tests/Documentation/Samples/Linq/ToString.cs#L35-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-tostring' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
