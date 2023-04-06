# Serialize Unindented JSON

This sample serializes an object to JSON without any formatting or indentation whitespace.

<!-- snippet: SerializeUnindentedJsonTypes -->
<a id='snippet-serializeunindentedjsontypes'></a>
```cs
public class Account
{
    public string Email { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedDate { get; set; }
    public IList<string> Roles { get; set; }
}
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/SerializeUnindentedJson.cs#L7-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializeunindentedjsontypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializeUnindentedJsonUsage -->
<a id='snippet-serializeunindentedjsonusage'></a>
```cs
var account = new Account
{
    Email = "james@example.com",
    Active = true,
    CreatedDate = new(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
    Roles = new List<string>
    {
        "User",
        "Admin"
    }
};

var json = JsonConvert.SerializeObject(account);
// {"Email":"james@example.com","Active":true,"CreatedDate":"2013-01-20T00:00:00Z","Roles":["User","Admin"]}

Console.WriteLine(json);
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/SerializeUnindentedJson.cs#L22-L41' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializeunindentedjsonusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
