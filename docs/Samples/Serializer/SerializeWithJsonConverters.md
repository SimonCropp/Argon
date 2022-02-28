# Serialize with JsonConverters

This sample uses a `Argon.JsonConverter` to customize how JSON is serialized.

<!-- snippet: SerializeWithJsonConvertersUsage -->
<a id='snippet-serializewithjsonconvertersusage'></a>
```cs
var stringComparisons = new List<StringComparison>
{
    StringComparison.CurrentCulture,
    StringComparison.Ordinal
};

var jsonWithoutConverter = JsonConvert.SerializeObject(stringComparisons);

Console.WriteLine(jsonWithoutConverter);
// [0,4]

var jsonWithConverter = JsonConvert.SerializeObject(stringComparisons, new StringEnumConverter());

Console.WriteLine(jsonWithConverter);
// ["CurrentCulture","Ordinal"]

var newStringComparsions = JsonConvert.DeserializeObject<List<StringComparison>>(
    jsonWithConverter,
    new StringEnumConverter());

Console.WriteLine(string.Join(", ", newStringComparsions.Select(c => c.ToString()).ToArray()));
// CurrentCulture, Ordinal
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeWithJsonConverters.cs#L10-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializewithjsonconvertersusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
