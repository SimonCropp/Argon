# Custom IContractResolver

This sample creates a custom `Argon.IContractResolver` that only serializes a type's properties that begin with a specified character.

<!-- snippet: CustomContractResolverUsage -->
<a id='snippet-customcontractresolverusage'></a>
```cs
var person = new Person
{
    FirstName = "Dennis",
    LastName = "Deepwater-Diver"
};

var startingWithF = JsonConvert.SerializeObject(person, Formatting.Indented,
    new JsonSerializerSettings {ContractResolver = new DynamicContractResolver('F')});

Console.WriteLine(startingWithF);
// {
//   "FirstName": "Dennis",
//   "FullName": "Dennis Deepwater-Diver"
// }

var startingWithL = JsonConvert.SerializeObject(person, Formatting.Indented,
    new JsonSerializerSettings {ContractResolver = new DynamicContractResolver('L')});

Console.WriteLine(startingWithL);
// {
//   "LastName": "Deepwater-Diver"
// }
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/CustomContractResolver.cs#L41-L66' title='Snippet source file'>snippet source</a> | <a href='#snippet-customcontractresolverusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
