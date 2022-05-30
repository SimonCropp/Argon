# Custom IContractResolver

This sample uses a custom `Argon.IContractResolver` to modify how objects are serialized.

<!-- snippet: SerializeContractResolverTypes -->
<a id='snippet-serializecontractresolvertypes'></a>
```cs
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeContractResolver.cs#L7-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializecontractresolvertypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializeContractResolverUsage -->
<a id='snippet-serializecontractresolverusage'></a>
```cs
var person = new Person
{
    FirstName = "Sarah",
    LastName = "Security"
};

var json = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
});

Console.WriteLine(json);
// {
//   "firstName": "Sarah",
//   "lastName": "Security",
//   "fullName": "Sarah Security"
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeContractResolver.cs#L22-L42' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializecontractresolverusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
