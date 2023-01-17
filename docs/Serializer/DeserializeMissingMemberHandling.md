# MissingMemberHandling setting

This sample attempts to deserialize JSON with `Argon.MissingMemberHandling` set to error and a JSON property that doesn't match to a member, causing an exception.

<!-- snippet: DeserializeMissingMemberHandlingTypes -->
<a id='snippet-deserializemissingmemberhandlingtypes'></a>
```cs
public class Account
{
    public string FullName { get; set; }
    public bool Deleted { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeMissingMemberHandling.cs#L7-L15' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializemissingmemberhandlingtypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeMissingMemberHandlingUsage -->
<a id='snippet-deserializemissingmemberhandlingusage'></a>
```cs
var json = """
    {
      'FullName': 'Dan Deleted',
      'Deleted': true,
      'DeletedDate': '2013-01-20T00:00:00'
    }
    """;

try
{
    JsonConvert.DeserializeObject<Account>(json, new JsonSerializerSettings
    {
        MissingMemberHandling = MissingMemberHandling.Error
    });
}
catch (JsonSerializationException exception)
{
    Console.WriteLine(exception.Message);
    // Could not find member 'DeletedDate' on object of type 'Account'. Path 'DeletedDate', line 4, position 23.
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeMissingMemberHandling.cs#L20-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializemissingmemberhandlingusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
