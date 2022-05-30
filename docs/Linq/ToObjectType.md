# Convert JSON to a Type

This sample converts LINQ to JSON objects to .NET types using `Argon.JToken.ToObject(System.Type)`.

<!-- snippet: ToObjectType -->
<a id='snippet-toobjecttype'></a>
```cs
var v1 = new JValue(true);

var b = (bool) v1.ToObject(typeof(bool));

Console.WriteLine(b);
// true

var i = (int) v1.ToObject(typeof(int));

Console.WriteLine(i);
// 1

var s = (string) v1.ToObject(typeof(string));

Console.WriteLine(s);
// "True"
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/ToObjectType.cs#L12-L31' title='Snippet source file'>snippet source</a> | <a href='#snippet-toobjecttype' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
