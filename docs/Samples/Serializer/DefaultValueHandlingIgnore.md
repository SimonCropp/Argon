# DefaultValueHandling setting

This sample uses the `Argon.DefaultValueHandling` setting to not serialize properties with a default value.

<!-- snippet: DefaultValueHandlingIgnoreTypes -->
<a id='snippet-defaultvaluehandlingignoretypes'></a>
```cs
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Person Partner { get; set; }
    public decimal? Salary { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DefaultValueHandlingIgnore.cs#L7-L15' title='Snippet source file'>snippet source</a> | <a href='#snippet-defaultvaluehandlingignoretypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DefaultValueHandlingIgnoreUsage -->
<a id='snippet-defaultvaluehandlingignoreusage'></a>
```cs
var person = new Person();

var jsonIncludeDefaultValues = JsonConvert.SerializeObject(person, Formatting.Indented);

Console.WriteLine(jsonIncludeDefaultValues);
// {
//   "Name": null,
//   "Age": 0,
//   "Partner": null,
//   "Salary": null
// }

var jsonIgnoreDefaultValues = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
{
    DefaultValueHandling = DefaultValueHandling.Ignore
});

Console.WriteLine(jsonIgnoreDefaultValues);
// {}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DefaultValueHandlingIgnore.cs#L20-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-defaultvaluehandlingignoreusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
