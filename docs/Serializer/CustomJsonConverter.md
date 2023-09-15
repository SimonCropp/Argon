# Custom JsonConverter

This sample creates a custom `Argon.JsonConverter` that overrides serialization to add a keys property.

<!-- snippet: CustomJsonConverterTypes -->
<a id='snippet-customjsonconvertertypes'></a>
```cs
public class KeysJsonConverter : JsonConverter
{
    readonly Type[] _types;

    public KeysJsonConverter(params Type[] types) =>
        _types = types;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var token = JToken.FromObject(value);

        if (token.Type != JTokenType.Object)
        {
            token.WriteTo(writer);
        }
        else
        {
            var o = (JObject) token;
            var propertyNames = o.Properties().Select(_ => _.Name).ToList();

            o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

            o.WriteTo(writer);
        }
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer) =>
        throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");

    public override bool CanRead => false;

    public override bool CanConvert(Type type) =>
        _types.Any(t => t == type);
}

public class Employee
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public IList<string> Roles { get; set; }
}
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/CustomJsonConverter.cs#L7-L51' title='Snippet source file'>snippet source</a> | <a href='#snippet-customjsonconvertertypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: CustomJsonConverterUsage -->
<a id='snippet-customjsonconverterusage'></a>
```cs
var employee = new Employee
{
    FirstName = "James",
    LastName = "Newton-King",
    Roles = new List<string>
    {
        "Admin"
    }
};

var json = JsonConvert.SerializeObject(employee, Formatting.Indented, new KeysJsonConverter(typeof(Employee)));

Console.WriteLine(json);
// {
//   "Keys": [
//     "FirstName",
//     "LastName",
//     "Roles"
//   ],
//   "FirstName": "James",
//   "LastName": "Newton-King",
//   "Roles": [
//     "Admin"
//   ]
// }

var newEmployee = JsonConvert.DeserializeObject<Employee>(json, new KeysJsonConverter(typeof(Employee)));

Console.WriteLine(newEmployee.FirstName);
// James
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/CustomJsonConverter.cs#L56-L89' title='Snippet source file'>snippet source</a> | <a href='#snippet-customjsonconverterusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
