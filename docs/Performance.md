# Performance

## Reuse Contract Resolver

The `Argon.Serialization.IContractResolver` resolves .NET types to contracts that are used during serialization inside JsonSerializer. Creating a contract involves inspecting a type with slow reflection, so contracts are typically cached by implementations of IContractResolver like `Argon.Serialization.DefaultContractResolver`.

To avoid the overhead of recreating contracts every time a JsonSerializer is used create the contract resolver once and reuse it. Note that if not using a contract resolver then a shared internal instance is automatically used when serializing and deserializing.

<!-- snippet: ReuseContractResolver -->
<a id='snippet-reusecontractresolver'></a>
```cs
// BAD - a new contract resolver is created each time, forcing slow reflection to be used
var json1 = JsonConvert.SerializeObject(person, new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new SnakeCaseNamingStrategy()
    }
});

// GOOD - reuse the contract resolver from a shared location
var json2 = JsonConvert.SerializeObject(person, new JsonSerializerSettings
{
    Formatting = Formatting.Indented,
    ContractResolver = AppSettings.SnakeCaseContractResolver
});

// GOOD - an internal contract resolver is used
var json3 = JsonConvert.SerializeObject(person, new JsonSerializerSettings
{
    Formatting = Formatting.Indented
});
```
<sup><a href='/Src/Tests/Documentation/PerformanceTests.cs#L100-L123' title='Snippet source file'>snippet source</a> | <a href='#snippet-reusecontractresolver' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Optimize Memory Usage

To keep an application consistently fast, it is important to minimize the amount of time the .NET framework spends performing [garbage collection](http://msdn.microsoft.com/en-us/library/ms973837.aspx)

Allocating too many objects or allocating very large objects can slow down or even halt an application while garbage collection is in progress.

To minimize memory usage and the number of objects allocated, Json.NET supports serializing and deserializing directly to a stream. Reading or writing JSON a piece at a time, instead of having the entire JSON string loaded into memory, is especially important when working with JSON documents greater than 85kb in size to avoid the JSON string ending up in the [large object heap](http://msdn.microsoft.com/en-us/magazine/cc534993.aspx)

<!-- snippet: DeserializeString -->
<a id='snippet-deserializestring'></a>
```cs
var client = new HttpClient();

// read the json into a string
// string could potentially be very large and cause memory problems
var json = client.GetStringAsync("http://www.test.com/large.json").Result;

var p = JsonConvert.DeserializeObject<Person>(json);
```
<sup><a href='/Src/Tests/Documentation/PerformanceTests.cs#L153-L161' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializestring' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeStream -->
<a id='snippet-deserializestream'></a>
```cs
var client = new HttpClient();

using var s = client.GetStreamAsync("http://www.test.com/large.json").Result;
using var sr = new StreamReader(s);
using JsonReader reader = new JsonTextReader(sr);
var serializer = new JsonSerializer();

// read the json from a stream
// json size doesn't matter because only a small piece is read at a time from the HTTP request
var p = serializer.Deserialize<Person>(reader);
```
<sup><a href='/Src/Tests/Documentation/PerformanceTests.cs#L167-L179' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializestream' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## JsonConverters

Passing a `Argon.JsonConverter` to SerializeObject or DeserializeObject provides a way to completely change how an object is serialized. There is, however, a small amount of overhead; the CanConvert method is called for every value to check whether serialization should be handled by that JsonConverter.

There are a couple of ways to continue to use JsonConverters without any overhead. The simplest way is to specify the JsonConverter using the `Argon.JsonConverterAttribute`. This attribute tells the serializer to always use that converter when serializing and deserializing the type, without the check.

<!-- snippet: JsonConverterAttribute -->
<a id='snippet-jsonconverterattribute'></a>
```cs
[JsonConverter(typeof(PersonConverter))]
public class Person
{
    public Person()
    {
        Likes = new List<string>();
    }

    public string Name { get; set; }
    public IList<string> Likes { get; private set; }
}
```
<sup><a href='/Src/Tests/Documentation/PerformanceTests.cs#L30-L42' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonconverterattribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If the class to convert isn't owned and is it nor possible to use an attribute, a JsonConverter can still be used by creating a `Argon.Serialization.IContractResolver`.
<!-- snippet: JsonConverterContractResolver -->
<a id='snippet-jsonconvertercontractresolver'></a>
```cs
public class ConverterContractResolver : DefaultContractResolver
{
    public new static readonly ConverterContractResolver Instance = new();

    protected override JsonContract CreateContract(Type objectType)
    {
        var contract = base.CreateContract(objectType);

        // this will only be called once and then cached
        if (objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset))
        {
            contract.Converter = new JavaScriptDateTimeConverter();
        }

        return contract;
    }
}
```
<sup><a href='/Src/Tests/Documentation/PerformanceTests.cs#L44-L62' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonconvertercontractresolver' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The IContractResolver in the example above will set all DateTimes to use the JavaScriptDateConverter.


## Manually Serialize

The absolute fastest way to read and write JSON is to use JsonTextReader/JsonTextWriter directly to manually serialize types. Using a reader or writer directly skips any of the overhead from a serializer, such as reflection.

<!-- snippet: ReaderWriter -->
<a id='snippet-readerwriter'></a>
```cs
public static string ToJson(this Person p)
{
    var stringWriter = new StringWriter();
    var jsonWriter = new JsonTextWriter(stringWriter);

    // {
    jsonWriter.WriteStartObject();

    // "name" : "Jerry"
    jsonWriter.WritePropertyName("name");
    jsonWriter.WriteValue(p.Name);

    // "likes": ["Comedy", "Superman"]
    jsonWriter.WritePropertyName("likes");
    jsonWriter.WriteStartArray();
    foreach (var like in p.Likes)
    {
        jsonWriter.WriteValue(like);
    }
    jsonWriter.WriteEndArray();

    // }
    jsonWriter.WriteEndObject();

    return stringWriter.ToString();
}
```
<sup><a href='/Src/Tests/Documentation/PerformanceTests.cs#L185-L212' title='Snippet source file'>snippet source</a> | <a href='#snippet-readerwriter' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If performance is important, then this is the best choice. More about using JsonReader/JsonWriter here: [ReadingWritingJSON]


## Related Topics

 * `Argon.JsonSerializer`
 * `Argon.JsonConverter`
 * `Argon.JsonConverterAttribute`
 * `Argon.JsonTextWriter`
 * `Argon.JsonTextReader`
