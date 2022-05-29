# Serialize Conditional Property

This sample uses a conditional property to exclude a property from serialization.

<!-- snippet: SerializeConditionalPropertyTypes -->
<a id='snippet-serializeconditionalpropertytypes'></a>
```cs
public class Employee
{
    public string Name { get; set; }
    public Employee Manager { get; set; }

    public bool ShouldSerializeManager() =>
        // don't serialize the Manager property if an employee is their own manager
        Manager != this;
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeConditionalProperty.cs#L7-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializeconditionalpropertytypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializeConditionalPropertyUsage -->
<a id='snippet-serializeconditionalpropertyusage'></a>
```cs
var joe = new Employee
{
    Name = "Joe Employee"
};
var mike = new Employee
{
    Name = "Mike Manager"
};

joe.Manager = mike;

// mike is his own manager
// ShouldSerialize will skip this property
mike.Manager = mike;

var json = JsonConvert.SerializeObject(new[] {joe, mike}, Formatting.Indented);

Console.WriteLine(json);
// [
//   {
//     "Name": "Joe Employee",
//     "Manager": {
//       "Name": "Mike Manager"
//     }
//   },
//   {
//     "Name": "Mike Manager"
//   }
// ]
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeConditionalProperty.cs#L24-L56' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializeconditionalpropertyusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
