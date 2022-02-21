# JsonPropertyAttribute property setting

This sample uses `Argon.JsonPropertyAttribute` to change how the property value is serialized.

<!-- snippet: JsonPropertyPropertyLevelSettingTypes -->
<a id='snippet-jsonpropertypropertylevelsettingtypes'></a>
```cs
public class Vessel
{
    public string Name { get; set; }
    public string Class { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? LaunchDate { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonPropertyPropertyLevelSetting.cs#L28-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonpropertypropertylevelsettingtypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: JsonPropertyPropertyLevelSettingUsage -->
<a id='snippet-jsonpropertypropertylevelsettingusage'></a>
```cs
var vessel = new Vessel
{
    Name = "Red October",
    Class = "Typhoon"
};

var json = JsonConvert.SerializeObject(vessel, Formatting.Indented);

Console.WriteLine(json);
// {
//   "Name": "Red October",
//   "Class": "Typhoon"
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonPropertyPropertyLevelSetting.cs#L42-L56' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonpropertypropertylevelsettingusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
