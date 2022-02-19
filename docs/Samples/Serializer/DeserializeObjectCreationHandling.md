# ObjectCreationHandling setting

This sample deserializes JSON with `Argon.ObjectCreationHandling` set to Replace so that collection values aren't duplicated.

<!-- snippet: DeserializeObjectCreationHandlingTypes -->
<a id='snippet-deserializeobjectcreationhandlingtypes'></a>
```cs
public class UserViewModel
{
    public string Name { get; set; }
    public IList<string> Offices { get; private set; }

    public UserViewModel()
    {
        Offices = new List<string>
        {
            "Auckland",
            "Wellington",
            "Christchurch"
        };
    }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeObjectCreationHandling.cs#L32-L48' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeobjectcreationhandlingtypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeObjectCreationHandlingUsage -->
<a id='snippet-deserializeobjectcreationhandlingusage'></a>
```cs
var json = @"{
      'Name': 'James',
      'Offices': [
        'Auckland',
        'Wellington',
        'Christchurch'
      ]
    }";

var model1 = JsonConvert.DeserializeObject<UserViewModel>(json);

foreach (var office in model1.Offices)
{
    Console.WriteLine(office);
}
// Auckland
// Wellington
// Christchurch
// Auckland
// Wellington
// Christchurch

var model2 = JsonConvert.DeserializeObject<UserViewModel>(json, new JsonSerializerSettings
{
    ObjectCreationHandling = ObjectCreationHandling.Replace
});

foreach (var office in model2.Offices)
{
    Console.WriteLine(office);
}
// Auckland
// Wellington
// Christchurch
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeObjectCreationHandling.cs#L53-L88' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeobjectcreationhandlingusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
