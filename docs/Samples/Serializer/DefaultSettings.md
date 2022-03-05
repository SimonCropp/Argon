# Serialize with DefaultSettings

This sample serializes and deserializes JSON using `Argon.JsonConvert.DefaultSettings`.

<!-- snippet: DefaultSettingsUsage -->
<a id='snippet-defaultsettingsusage'></a>
```cs
// settings will automatically be used by JsonConvert.SerializeObject/DeserializeObject
JsonConvert.DefaultSettings = () => new()
{
    Formatting = Formatting.Indented,
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};

var s = new Staff
{
    FirstName = "Eric",
    LastName = "Example",
    BirthDate = new(1980, 4, 20, 0, 0, 0, DateTimeKind.Utc),
    Department = "IT",
    JobTitle = "Web Dude"
};

json = JsonConvert.SerializeObject(s);
// {
//   "firstName": "Eric",
//   "lastName": "Example",
//   "birthDate": "1980-04-20T00:00:00Z",
//   "department": "IT",
//   "jobTitle": "Web Dude"
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DefaultSettings.cs#L16-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-defaultsettingsusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
