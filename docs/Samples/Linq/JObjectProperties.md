# Using JObject.Properties

This sample gets an object's `Argon.Linq.JProperty` collection using `Argon.Linq.JObject.Properties`.

<!-- snippet: JObjectProperties -->
<a id='snippet-jobjectproperties'></a>
```cs
var o = new JObject
{
    { "name1", "value1" },
    { "name2", "value2" }
};

foreach (var property in o.Properties())
{
    Console.WriteLine($"{property.Name} - {property.Value}");
}
// name1 - value1
// name2 - value2

foreach (var property in o)
{
    Console.WriteLine($"{property.Key} - {property.Value}");
}
// name1 - value1
// name2 - value2
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/JObjectProperties.cs#L12-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-jobjectproperties' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
