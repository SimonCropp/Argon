# Populate an Object

This sample populates an existing object instance with values from JSON.

<!-- snippet: PopulateObjectTypes -->
<a id='snippet-populateobjecttypes'></a>
```cs
public class Account
{
    public string Email { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<string> Roles { get; set; }
}
```
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/PopulateObject.cs#L32-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-populateobjecttypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: PopulateObjectUsage -->
<a id='snippet-populateobjectusage'></a>
```cs
var account = new Account
{
    Email = "james@example.com",
    Active = true,
    CreatedDate = new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
    Roles = new List<string>
    {
        "User",
        "Admin"
    }
};

var json = @"{
      'Active': false,
      'Roles': [
        'Expired'
      ]
    }";

JsonConvert.PopulateObject(json, account);

Console.WriteLine(account.Email);
// james@example.com

Console.WriteLine(account.Active);
// false

Console.WriteLine(string.Join(", ", account.Roles.ToArray()));
// User, Admin, Expired
```
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/PopulateObject.cs#L45-L75' title='Snippet source file'>snippet source</a> | <a href='#snippet-populateobjectusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
