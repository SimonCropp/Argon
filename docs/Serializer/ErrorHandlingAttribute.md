# ErrorHandlingAttribute

This sample uses `Argon.OnErrorAttribute` to ignore the exception thrown setting the Roles property.


<!-- snippet: ErrorHandlingAttributeTypes -->
<a id='snippet-errorhandlingattributetypes'></a>
```cs
public class Employee :
    IJsonOnError
{
    List<string> roles;

    public string Name { get; set; }
    public int Age { get; set; }

    public List<string> Roles
    {
        get
        {
            if (roles == null)
            {
                throw new("Roles not loaded!");
            }

            return roles;
        }
        set => roles = value;
    }

    public string Title { get; set; }

    public void OnError(object originalObject, ErrorLocation location, Exception exception, Action markAsHandled) =>
        markAsHandled();
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/ErrorHandlingAttribute.cs#L7-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-errorhandlingattributetypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: ErrorHandlingAttributeUsage -->
<a id='snippet-errorhandlingattributeusage'></a>
```cs
var person = new Employee
{
    Name = "George Michael Bluth",
    Age = 16,
    Roles = null,
    Title = "Mister Manager"
};

var json = JsonConvert.SerializeObject(person, Formatting.Indented);

Console.WriteLine(json);
// {
//   "Name": "George Michael Bluth",
//   "Age": 16,
//   "Title": "Mister Manager"
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/ErrorHandlingAttribute.cs#L42-L61' title='Snippet source file'>snippet source</a> | <a href='#snippet-errorhandlingattributeusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
