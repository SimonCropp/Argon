# Deserialize an Object

This sample deserializes JSON to an object.

<!-- snippet: DeserializeObjectTypes -->
<a id='snippet-deserializeobjecttypes'></a>
```cs
public class Account
{
    public string Email { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedDate { get; set; }
    public IList<string> Roles { get; set; }
}
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/DeserializeObject.cs#L7-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeobjecttypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeObjectUsage -->
<a id='snippet-deserializeobjectusage'></a>
```cs
var json = """
    {
      'Email': 'james@example.com',
      'Active': true,
      'CreatedDate': '2013-01-20T00:00:00Z',
      'Roles': [
        'User',
        'Admin'
      ]
    }
    """;

var account = JsonConvert.DeserializeObject<Account>(json);

Console.WriteLine(account.Email);
// james@example.com
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/DeserializeObject.cs#L22-L41' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeobjectusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
