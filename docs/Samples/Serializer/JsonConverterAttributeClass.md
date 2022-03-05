# JsonConverterAttribute on a class

This sample uses the `Argon.JsonConverterAttribute` to specify that a `Argon.JsonConverter` should be used when serializing and deserializing a class.

<!-- snippet: JsonConverterAttributeClassTypes -->
<a id='snippet-jsonconverterattributeclasstypes'></a>
```cs
public class UserConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var user = (User) value;

        writer.WriteValue(user.UserName);
    }

    public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
    {
        var user = new User
        {
            UserName = (string) reader.Value
        };

        return user;
    }

    public override bool CanConvert(Type type)
    {
        return type == typeof(User);
    }
}

[JsonConverter(typeof(UserConverter))]
public class User
{
    public string UserName { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonConverterAttributeClass.cs#L7-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonconverterattributeclasstypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: JsonConverterAttributeClassUsage -->
<a id='snippet-jsonconverterattributeclassusage'></a>
```cs
var user = new User
{
    UserName = @"domain\username"
};

var json = JsonConvert.SerializeObject(user, Formatting.Indented);

Console.WriteLine(json);
// "domain\\username"
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonConverterAttributeClass.cs#L45-L57' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonconverterattributeclassusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
