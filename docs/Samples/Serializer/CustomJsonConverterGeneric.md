# Custom `JsonConverter<T>`

This sample creates a custom converter from `<c>JsonConverter<T>` that overrides serialization for the `Version` class with a custom display string.

<!-- snippet: CustomJsonConverterGenericTypes -->
<a id='snippet-customjsonconvertergenerictypes'></a>
```cs
public class VersionConverter : JsonConverter<Version>
{
    public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var s = (string)reader.Value;

        return new Version(s);
    }
}

public class NuGetPackage
{
    public string PackageId { get; set; }
    public Version Version { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/CustomJsonConverterGeneric.cs#L32-L53' title='Snippet source file'>snippet source</a> | <a href='#snippet-customjsonconvertergenerictypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: CustomJsonConverterGenericUsage -->
<a id='snippet-customjsonconvertergenericusage'></a>
```cs
var p1 = new NuGetPackage
{
    PackageId = "Argon",
    Version = new Version(10, 0, 4)
};

var json = JsonConvert.SerializeObject(p1, Formatting.Indented, new VersionConverter());

Console.WriteLine(json);
// {
//   "PackageId": "Argon",
//   "Version": "10.0.4"
// }

var p2 = JsonConvert.DeserializeObject<NuGetPackage>(json, new VersionConverter());

Console.WriteLine(p2.Version.ToString());
// 10.0.4
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/CustomJsonConverterGeneric.cs#L58-L77' title='Snippet source file'>snippet source</a> | <a href='#snippet-customjsonconvertergenericusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
