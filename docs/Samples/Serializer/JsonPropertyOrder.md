# JsonPropertyAttribute order

This sample uses `Argon.JsonPropertyAttribute` to order of properties when they are serialized to JSON.

<!-- snippet: JsonPropertyOrderTypes -->
<a id='snippet-jsonpropertyordertypes'></a>
```cs
public class Account
{
    public string EmailAddress { get; set; }

    // appear last
    [JsonProperty(Order = 1)]
    public bool Deleted { get; set; }

    [JsonProperty(Order = 2)]
    public DateTime DeletedDate { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    // appear first
    [JsonProperty(Order = -2)]
    public string FullName { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonPropertyOrder.cs#L30-L49' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonpropertyordertypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: JsonPropertyOrderUsage -->
<a id='snippet-jsonpropertyorderusage'></a>
```cs
var account = new Account
{
    FullName = "Aaron Account",
    EmailAddress = "aaron@example.com",
    Deleted = true,
    DeletedDate = new DateTime(2013, 1, 25),
    UpdatedDate = new DateTime(2013, 1, 25),
    CreatedDate = new DateTime(2010, 10, 1)
};

var json = JsonConvert.SerializeObject(account, Formatting.Indented);

Console.WriteLine(json);
// {
//   "FullName": "Aaron Account",
//   "EmailAddress": "aaron@example.com",
//   "CreatedDate": "2010-10-01T00:00:00",
//   "UpdatedDate": "2013-01-25T00:00:00",
//   "Deleted": true,
//   "DeletedDate": "2013-01-25T00:00:00"
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonPropertyOrder.cs#L54-L76' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonpropertyorderusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
