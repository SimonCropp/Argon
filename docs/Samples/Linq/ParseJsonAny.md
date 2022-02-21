# Parse JSON using JToken.Parse

This sample parses JSON using `Argon.Linq.JToken.Parse(System.String)`.

<!-- snippet: ParseJsonAny -->
<a id='snippet-parsejsonany'></a>
```cs
var t1 = JToken.Parse("{}");

Console.WriteLine(t1.Type);
// Object

var t2 = JToken.Parse("[]");

Console.WriteLine(t2.Type);
// Array

var t3 = JToken.Parse("null");

Console.WriteLine(t3.Type);
// Null

var t4 = JToken.Parse(@"'A string!'");

Console.WriteLine(t4.Type);
// String
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/ParseJsonAny.cs#L33-L53' title='Snippet source file'>snippet source</a> | <a href='#snippet-parsejsonany' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
