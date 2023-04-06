# Deserialize an Anonymous Type

This sample deserializes JSON into an anonymous type.

<!-- snippet: DeserializeAnonymousType -->
<a id='snippet-deserializeanonymoustype'></a>
```cs
var definition = new {Name = ""};

var json1 = @"{'Name':'James'}";
var customer1 = JsonConvert.DeserializeAnonymousType(json1, definition);

Console.WriteLine(customer1.Name);
// James

var json2 = @"{'Name':'Mike'}";
var customer2 = JsonConvert.DeserializeAnonymousType(json2, definition);

Console.WriteLine(customer2.Name);
// Mike
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/DeserializeAnonymousType.cs#L10-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeanonymoustype' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
