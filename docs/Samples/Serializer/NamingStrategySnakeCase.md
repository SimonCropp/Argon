# Snake case property names

This sample uses a `Argon.Serialization.SnakeCaseNamingStrategy` specified using a contract resolver to snake case serialized property names.

<!-- snippet: NamingStrategySnakeCaseTypes -->
<a id='snippet-namingstrategysnakecasetypes'></a>
```cs
public class User
{
    public string UserName { get; set; }
    public bool Enabled { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/NamingStrategySnakeCase.cs#L32-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-namingstrategysnakecasetypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: NamingStrategySnakeCaseUsage -->
<a id='snippet-namingstrategysnakecaseusage'></a>
```cs
var user1 = new User
{
    UserName = "jamesn",
    Enabled = true
};

var contractResolver = new DefaultContractResolver
{
    NamingStrategy = new SnakeCaseNamingStrategy()
};

var json = JsonConvert.SerializeObject(user1, new JsonSerializerSettings
{
    ContractResolver = contractResolver,
    Formatting = Formatting.Indented
});

Console.WriteLine(json);
// {
//   "user_name": "jamesn",
//   "enabled": true
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/NamingStrategySnakeCase.cs#L43-L66' title='Snippet source file'>snippet source</a> | <a href='#snippet-namingstrategysnakecaseusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
