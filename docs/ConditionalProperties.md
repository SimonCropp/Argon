# Conditional Property Serialization

Json.NET has the ability to conditionally serialize properties by placing a ShouldSerialize method on a class. This functionality is similar to the [XmlSerializer ShouldSerialize feature](http://msdn.microsoft.com/en-us/library/53b8022e.aspx).


## ShouldSerialize

To conditionally serialize a property, add a method that returns boolean with the same name as the property and then prefix the method name with ShouldSerialize. The result of the method determines whether the property is serialized. If the method returns true then the property will be serialized, if it returns false then the property will be skipped.

<!-- snippet: EmployeeShouldSerializeExample -->
<a id='snippet-employeeshouldserializeexample'></a>
```cs
public class Employee
{
    public string Name { get; set; }
    public Employee Manager { get; set; }

    public bool ShouldSerializeManager()
    {
        // don't serialize the Manager property if an employee is their own manager
        return Manager != this;
    }
}
```
<sup><a href='/src/Tests/Documentation/ConditionalPropertiesTests.cs#L60-L72' title='Snippet source file'>snippet source</a> | <a href='#snippet-employeeshouldserializeexample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: ShouldSerializeClassTest -->
<a id='snippet-shouldserializeclasstest'></a>
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

var json = JsonConvert.SerializeObject(new[] { joe, mike }, Formatting.Indented);
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
<sup><a href='/src/Tests/Documentation/ConditionalPropertiesTests.cs#L77-L105' title='Snippet source file'>snippet source</a> | <a href='#snippet-shouldserializeclasstest' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## IContractResolver

ShouldSerialize can also be set using an `Argon.Serialization.IContractResolver`. Conditionally serializing a property using an IContractResolver is useful avoid placing a ShouldSerialize method on a class or are unable to.

<!-- snippet: ShouldSerializeContractResolver -->
<a id='snippet-shouldserializecontractresolver'></a>
```cs
public class ShouldSerializeContractResolver : DefaultContractResolver
{
    public new static readonly ShouldSerializeContractResolver Instance = new();

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.DeclaringType == typeof(Employee) && property.PropertyName == "Manager")
        {
            property.ShouldSerialize =
                instance =>
                {
                    var e = (Employee)instance;
                    return e.Manager != e;
                };
        }

        return property;
    }
}
```
<sup><a href='/src/Tests/Documentation/ConditionalPropertiesTests.cs#L34-L56' title='Snippet source file'>snippet source</a> | <a href='#snippet-shouldserializecontractresolver' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * `Argon.JsonSerializer`
 * `Argon.Serialization.IContractResolver`
 * `Argon.Serialization.JsonProperty.ShouldSerialize`
