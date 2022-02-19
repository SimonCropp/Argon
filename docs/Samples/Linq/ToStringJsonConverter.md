# Using JToken.ToString with JsonConverter

This sample uses a `Argon.JsonConverter` to customize converting LINQ to JSON objects to JSON.

<!-- snippet: ToStringJsonConverter -->
<a id='snippet-tostringjsonconverter'></a>
```cs
var o = JObject.Parse(@"{'string1':'value','integer2':99,'datetime3':'2000-05-23T00:00:00'}");

Console.WriteLine(o.ToString(Formatting.None, new JavaScriptDateTimeConverter()));
// {"string1":"value","integer2":99,"datetime3":new Date(959032800000)}
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/ToStringJsonConverter.cs#L35-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-tostringjsonconverter' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
