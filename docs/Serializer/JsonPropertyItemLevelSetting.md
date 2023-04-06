# JsonPropertyAttribute items setting

This sample uses `Argon.JsonPropertyAttribute` to change how the property value's items are serialized, e.g. setting ItemIsReference to true on a property with a collection will serialize all the collection's items with reference tracking enabled.

<!-- snippet: JsonPropertyItemLevelSettingTypes -->
<a id='snippet-jsonpropertyitemlevelsettingtypes'></a>
```cs
public class Business
{
    public string Name { get; set; }

    [JsonProperty(ItemIsReference = true)] public IList<Employee> Employees { get; set; }
}

public class Employee
{
    public string Name { get; set; }

    [JsonProperty(IsReference = true)] public Employee Manager { get; set; }
}
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/JsonPropertyItemLevelSetting.cs#L7-L23' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonpropertyitemlevelsettingtypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: JsonPropertyItemLevelSettingUsage -->
<a id='snippet-jsonpropertyitemlevelsettingusage'></a>
```cs
var manager = new Employee
{
    Name = "George-Michael"
};
var worker = new Employee
{
    Name = "Maeby",
    Manager = manager
};

var business = new Business
{
    Name = "Acme Ltd.",
    Employees = new List<Employee>
    {
        manager,
        worker
    }
};

var json = JsonConvert.SerializeObject(business, Formatting.Indented);

Console.WriteLine(json);
// {
//   "Name": "Acme Ltd.",
//   "Employees": [
//     {
//       "$id": "1",
//       "Name": "George-Michael",
//       "Manager": null
//     },
//     {
//       "$id": "2",
//       "Name": "Maeby",
//       "Manager": {
//         "$ref": "1"
//       }
//     }
//   ]
// }
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/JsonPropertyItemLevelSetting.cs#L28-L71' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonpropertyitemlevelsettingusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
