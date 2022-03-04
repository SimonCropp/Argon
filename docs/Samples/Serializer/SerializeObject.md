# Serialize an Object

This sample serializes an object to JSON.

<!-- snippet: SerializeObjectTypes -->
<a id='snippet-serializeobjecttypes'></a>
```cs
public class Account
{
    public string Email { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedDate { get; set; }
    public IList<string> Roles { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeObject.cs#L7-L15' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializeobjecttypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializeObjectUsage -->
<a id='snippet-serializeobjectusage'></a>
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

var json = JsonConvert.SerializeObject(account, Formatting.Indented);
// {
//   "Email": "james@example.com",
//   "Active": true,
//   "CreatedDate": "2013-01-20T00:00:00Z",
//   "Roles": [
//     "User",
//     "Admin"
//   ]
// }

Console.WriteLine(json);
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeObject.cs#L20-L45' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializeobjectusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
