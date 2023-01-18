## Deserialize ExtensionData

This sample deserializes JSON to an object with extension data.

<!-- snippet: DeserializeExtensionDataTypes -->
<a id='snippet-deserializeextensiondatatypes'></a>
```cs
public class DirectoryAccount : IJsonOnDeserialized
{
    // normal deserialization
    public string DisplayName { get; set; }

    // these properties are set in OnDeserialized
    public string UserName { get; set; }
    public string Domain { get; set; }

    [JsonExtensionData] IDictionary<string, JToken> _additionalData;

    public void OnDeserialized()
    {
        // SAMAccountName is not deserialized to any property
        // and so it is added to the extension data dictionary
        var samAccountName = (string) _additionalData["SAMAccountName"];

        Domain = samAccountName.Split('\\')[0];
        UserName = samAccountName.Split('\\')[1];
    }

    public DirectoryAccount() =>
        _additionalData = new Dictionary<string, JToken>();
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeExtensionData.cs#L7-L34' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeextensiondatatypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeExtensionDataUsage -->
<a id='snippet-deserializeextensiondatausage'></a>
```cs
var json = """
    {
      'DisplayName': 'John Smith',
      'SAMAccountName': 'contoso\\johns'
    }
    """;

var account = JsonConvert.DeserializeObject<DirectoryAccount>(json);

Console.WriteLine(account.DisplayName);
// John Smith

Console.WriteLine(account.Domain);
// contoso

Console.WriteLine(account.UserName);
// johns
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeExtensionData.cs#L39-L59' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeextensiondatausage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
