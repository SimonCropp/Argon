# NullValueHandling setting

This sample serializes an object to JSON with `Argon.NullValueHandling` set to Ignore so that properties with a default value of null aren't included in the JSON result.

<!-- snippet: NullValueHandlingIgnoreTypes -->
<a id='snippet-nullvaluehandlingignoretypes'></a>
```cs
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Person Partner { get; set; }
    public decimal? Salary { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/NullValueHandlingIgnore.cs#L28-L36' title='Snippet source file'>snippet source</a> | <a href='#snippet-nullvaluehandlingignoretypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: NullValueHandlingIgnoreUsage -->
<a id='snippet-nullvaluehandlingignoreusage'></a>
```cs
var person = new Person
{
    Name = "Nigal Newborn",
    Age = 1
};

var jsonIncludeNullValues = JsonConvert.SerializeObject(person, Formatting.Indented);

Console.WriteLine(jsonIncludeNullValues);
// {
//   "Name": "Nigal Newborn",
//   "Age": 1,
//   "Partner": null,
//   "Salary": null
// }

var jsonIgnoreNullValues = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore
});

Console.WriteLine(jsonIgnoreNullValues);
// {
//   "Name": "Nigal Newborn",
//   "Age": 1
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/NullValueHandlingIgnore.cs#L41-L68' title='Snippet source file'>snippet source</a> | <a href='#snippet-nullvaluehandlingignoreusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
