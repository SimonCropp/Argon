# LINQ to JSON with JToken.ToObject

This sample converts LINQ to JSON objects to .NET types using `Argon.JToken.ToObject<T>`.

<!-- snippet: ToObjectGeneric -->
<a id='snippet-ToObjectGeneric'></a>
```cs
var v1 = new JValue(true);

var b = v1.ToObject<bool>();

Console.WriteLine(b);
// true

var i = v1.ToObject<int>();

Console.WriteLine(i);
// 1

var s = v1.ToObject<string>();

Console.WriteLine(s);
// "True"
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/ToObjectGeneric.cs#L10-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-ToObjectGeneric' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
