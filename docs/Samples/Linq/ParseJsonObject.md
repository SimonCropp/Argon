# Parsing JSON Object using JObject.Parse

This sample parses a JSON object using `Argon.Linq.JObject.Parse(System.String)`.

<!-- snippet: ParseJsonObject -->
<a id='snippet-parsejsonobject'></a>
```cs
var json = @"{
      CPU: 'Intel',
      Drives: [
        'DVD read/writer',
        '500 gigabyte hard drive'
      ]
    }";

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
<sup><a href='/Src/Tests/Documentation/Samples/Linq/ParseJsonObject.cs#L35-L54' title='Snippet source file'>snippet source</a> | <a href='#snippet-parsejsonobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
