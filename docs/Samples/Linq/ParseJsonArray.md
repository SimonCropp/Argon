# Parse JSON using JArray.Parse

This sample parses a JSON array using `Argon.Linq.JArray.Parse(System.String)`.

<!-- snippet: ParseJsonArray -->
<a id='snippet-parsejsonarray'></a>
```cs
var json = @"[
      'Small',
      'Medium',
      'Large'
    ]";

var a = JArray.Parse(json);

Console.WriteLine(a.ToString());
// [
//   "Small",
//   "Medium",
//   "Large"
// ]
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/ParseJsonArray.cs#L12-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-parsejsonarray' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
