# Configure NamingStrategy property name serialization

This sample configures a `Argon.Serialization.CamelCaseNamingStrategy` to not camel case properties that already have a name specified with an attribute.

<!-- snippet: NamingStrategySkipSpecifiedNamesTypes -->
<a id='snippet-namingstrategyskipspecifiednamestypes'></a>
```cs
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [JsonProperty(PropertyName = "UPN")]
    public string Upn { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/NamingStrategySkipSpecifiedNames.cs#L28-L36' title='Snippet source file'>snippet source</a> | <a href='#snippet-namingstrategyskipspecifiednamestypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: NamingStrategySkipSpecifiedNamesUsage -->
<a id='snippet-namingstrategyskipspecifiednamesusage'></a>
```cs
var user = new User
{
    FirstName = "John",
    LastName = "Smith",
    Upn = "john.smith@acme.com"
};

var contractResolver = new DefaultContractResolver
{
    NamingStrategy = new CamelCaseNamingStrategy
    {
        OverrideSpecifiedNames = false
    }
};

var json = JsonConvert.SerializeObject(user, new JsonSerializerSettings
{
    ContractResolver = contractResolver,
    Formatting = Formatting.Indented
});

Console.WriteLine(json);
// {
//   "firstName": "John",
//   "lastName": "Smith",
//   "UPN": "john.smith@acme.com"
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/NamingStrategySkipSpecifiedNames.cs#L41-L69' title='Snippet source file'>snippet source</a> | <a href='#snippet-namingstrategyskipspecifiednamesusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
