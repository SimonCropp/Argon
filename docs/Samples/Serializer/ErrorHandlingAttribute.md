# ErrorHandlingAttribute

This sample uses `Argon.Serialization.OnErrorAttribute` to ignore the exception thrown setting the Roles property.


<!-- snippet: ErrorHandlingAttributeTypes -->
<a id='snippet-errorhandlingattributetypes'></a>
```cs
public class Employee
{
    List<string> _roles;

    public string Name { get; set; }
    public int Age { get; set; }

    public List<string> Roles
    {
        get
        {
            if (_roles == null)
            {
                throw new("Roles not loaded!");
            }

            return _roles;
        }
        set => _roles = value;
    }

    public string Title { get; set; }

    [OnError]
    internal void OnError(StreamingContext context, ErrorContext errorContext)
    {
        errorContext.Handled = true;
    }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/ErrorHandlingAttribute.cs#L32-L62' title='Snippet source file'>snippet source</a> | <a href='#snippet-errorhandlingattributetypes' title='Start of snippet'>anchor</a></sup>
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
<sup><a href='/src/Tests/Documentation/Samples/Serializer/ErrorHandlingAttribute.cs#L67-L84' title='Snippet source file'>snippet source</a> | <a href='#snippet-errorhandlingattributeusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
