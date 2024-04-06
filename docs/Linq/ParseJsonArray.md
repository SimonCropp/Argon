# Parse JSON using JArray.Parse

This sample parses a JSON array using `Argon.JArray.Parse(System.String)`.

<!-- snippet: ParseJsonArray -->
<a id='snippet-ParseJsonArray'></a>
```cs
var json = """
           [
             'Small',
             'Medium',
             'Large'
           ]
           """;

var a = JArray.Parse(json);

Console.WriteLine(a.ToString());
// [
//   "Small",
//   "Medium",
//   "Large"
// ]
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/ParseJsonArray.cs#L10-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-ParseJsonArray' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
