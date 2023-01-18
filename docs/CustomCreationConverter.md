# CustomCreationConverter

The `Argon.CustomCreationConverter<T>` is a JsonConverter that provides a way to customize how an object is created during JSON deserialization. Once the object has been created it will then have values populated onto it by the serializer.

<!-- snippet: CustomCreationConverterObject -->
<a id='snippet-customcreationconverterobject'></a>
```cs
public interface IPerson
{
    string FirstName { get; set; }
    string LastName { get; set; }
    DateTime BirthDate { get; set; }
}

public class Employee : IPerson
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }

    public string Department { get; set; }
    public string JobTitle { get; set; }
}

public class PersonConverter : CustomCreationConverter<IPerson>
{
    public override IPerson Create(Type type) =>
        new Employee();
}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L427-L452' title='Snippet source file'>snippet source</a> | <a href='#snippet-customcreationconverterobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: CustomCreationConverterExample -->
<a id='snippet-customcreationconverterexample'></a>
```cs
//[
//  {
//    "FirstName": "Maurice",
//    "LastName": "Moss",
//    "BirthDate": "1981-03-08T00:00Z",
//    "Department": "IT",
//    "JobTitle": "Support"
//  },
//  {
//    "FirstName": "Jen",
//    "LastName": "Barber",
//    "BirthDate": "1985-12-10T00:00Z",
//    "Department": "IT",
//    "JobTitle": "Manager"
//  }
//]

var people = JsonConvert.DeserializeObject<List<IPerson>>(json, new PersonConverter());

var person = people[0];

Console.WriteLine(person.GetType());
// Argon.Tests.Employee

Console.WriteLine(person.FirstName);
// Maurice

var employee = (Employee) person;

Console.WriteLine(employee.JobTitle);
// Support
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L476-L510' title='Snippet source file'>snippet source</a> | <a href='#snippet-customcreationconverterexample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * `Argon.CustomCreationConverter<T>`
