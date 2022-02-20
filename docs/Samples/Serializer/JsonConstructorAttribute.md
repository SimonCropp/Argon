# JsonConstructorAttribute

This sample uses the `Argon.JsonConstructorAttribute` to specify that a constructor should be used to create a class during deserialization.

<!-- snippet: JsonConstructorAttributeTypes -->
<a id='snippet-jsonconstructorattributetypes'></a>
```cs
public class User
{
    public string UserName { get; private set; }
    public bool Enabled { get; private set; }

    public User()
    {
    }

    [JsonConstructor]
    public User(string userName, bool enabled)
    {
        UserName = userName;
        Enabled = enabled;
    }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonConstructorAttribute.cs#L33-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonconstructorattributetypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: JsonConstructorAttributeUsage -->
<a id='snippet-jsonconstructorattributeusage'></a>
```cs
var json = @"{
      ""UserName"": ""domain\\username"",
      ""Enabled"": true
    }";

var user = JsonConvert.DeserializeObject<User>(json);

Console.WriteLine(user.UserName);
// domain\username
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonConstructorAttribute.cs#L55-L65' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonconstructorattributeusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
