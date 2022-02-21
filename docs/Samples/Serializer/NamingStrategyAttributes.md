# JsonObjectAttribute NamingStrategy setting

This sample uses `Argon.Serialization.NamingStrategy` types specified on attributes to control serialized property names.

<!-- snippet: NamingStrategyAttributesTypes -->
<a id='snippet-namingstrategyattributestypes'></a>
```cs
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [JsonProperty(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public int SnakeRating { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/NamingStrategyAttributes.cs#L30-L39' title='Snippet source file'>snippet source</a> | <a href='#snippet-namingstrategyattributestypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: NamingStrategyAttributesUsage -->
<a id='snippet-namingstrategyattributesusage'></a>
```cs
var user = new User
{
    FirstName = "Tom",
    LastName = "Riddle",
    SnakeRating = 10
};

var json = JsonConvert.SerializeObject(user, Formatting.Indented);

Console.WriteLine(json);
// {
//   "firstName": "Tom",
//   "lastName": "Riddle",
//   "snake_rating": 10
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/NamingStrategyAttributes.cs#L44-L60' title='Snippet source file'>snippet source</a> | <a href='#snippet-namingstrategyattributesusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
