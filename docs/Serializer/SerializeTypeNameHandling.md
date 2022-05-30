# TypeNameHandling setting

This sample uses the `Argon.TypeNameHandling` setting to include type information when serializing JSON and read type information so that the correct types are created when deserializing JSON.

<!-- snippet: SerializeTypeNameHandlingTypes -->
<a id='snippet-serializetypenamehandlingtypes'></a>
```cs
public abstract class Business
{
    public string Name { get; set; }
}

public class Hotel : Business
{
    public int Stars { get; set; }
}

public class Stockholder
{
    public string FullName { get; set; }
    public IList<Business> Businesses { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeTypeNameHandling.cs#L7-L25' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializetypenamehandlingtypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializeTypeNameHandlingUsage -->
<a id='snippet-serializetypenamehandlingusage'></a>
```cs
var stockholder = new Stockholder
{
    FullName = "Steve Stockholder",
    Businesses = new List<Business>
    {
        new Hotel
        {
            Name = "Hudson Hotel",
            Stars = 4
        }
    }
};

var jsonTypeNameAll = JsonConvert.SerializeObject(stockholder, Formatting.Indented, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.All
});

Console.WriteLine(jsonTypeNameAll);
// {
//   "$type": "Argon.Samples.Stockholder, Tests",
//   "FullName": "Steve Stockholder",
//   "Businesses": {
//     "$type": "System.Collections.Generic.List`1[[Argon.Samples.Business, Tests]], mscorlib",
//     "$values": [
//       {
//         "$type": "Argon.Samples.Hotel, Argon.Tests",
//         "Stars": 4,
//         "Name": "Hudson Hotel"
//       }
//     ]
//   }
// }

var jsonTypeNameAuto = JsonConvert.SerializeObject(stockholder, Formatting.Indented, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Auto
});

Console.WriteLine(jsonTypeNameAuto);
// {
//   "FullName": "Steve Stockholder",
//   "Businesses": [
//     {
//       "$type": "Argon.Samples.Hotel, Tests",
//       "Stars": 4,
//       "Name": "Hudson Hotel"
//     }
//   ]
// }

// for security TypeNameHandling is required when deserializing
var newStockholder = JsonConvert.DeserializeObject<Stockholder>(jsonTypeNameAuto, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.Auto
});

Console.WriteLine(newStockholder.Businesses[0].GetType().Name);
// Hotel
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeTypeNameHandling.cs#L30-L92' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializetypenamehandlingusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
