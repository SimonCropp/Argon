# Deserialize with CustomCreationConverter

This sample creates a class that inherits from `Argon.CustomCreationConverter`
 that instantiates Employee instances for the Person type.

<!-- snippet: DeserializeCustomCreationConverterTypes -->
<a id='snippet-deserializecustomcreationconvertertypes'></a>
```cs
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
}

public class Employee : Person
{
    public string Department { get; set; }
    public string JobTitle { get; set; }
}

public class PersonConverter : CustomCreationConverter<Person>
{
    public override Person Create(Type type) =>
        new Employee();
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeCustomCreationConverter.cs#L7-L28' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializecustomcreationconvertertypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeCustomCreationConverterUsage -->
<a id='snippet-deserializecustomcreationconverterusage'></a>
```cs
var json = @"{
      'Department': 'Furniture',
      'JobTitle': 'Carpenter',
      'FirstName': 'John',
      'LastName': 'Joinery',
      'BirthDate': '1983-02-02T00:00:00'
    }";

var person = JsonConvert.DeserializeObject<Person>(json, new PersonConverter());

Console.WriteLine(person.GetType().Name);
// Employee

var employee = (Employee) person;

Console.WriteLine(employee.JobTitle);
// Carpenter
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeCustomCreationConverter.cs#L33-L53' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializecustomcreationconverterusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
