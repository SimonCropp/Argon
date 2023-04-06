# Parsing JSON Object using JObject.Parse

This sample parses a JSON object using `Argon.JObject.Parse(System.String)`.

<!-- snippet: ParseJsonObject -->
<a id='snippet-parsejsonobject'></a>
```cs
var json = """
    {
      CPU: 'Intel',
      Drives: [
        'DVD read/writer',
        '500 gigabyte hard drive'
      ]
    }
    """;

var o = JObject.Parse(json);

Console.WriteLine(o.ToString());
// {
//   "CPU": "Intel",
//   "Drives": [
//     "DVD read/writer",
//     "500 gigabyte hard drive"
//   ]
// }
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/ParseJsonObject.cs#L12-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-parsejsonobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
