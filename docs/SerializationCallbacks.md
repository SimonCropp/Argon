# Serialization callbacks

A callback can be used to manipulate an object before and after its serialization and deserialization by the JsonSerializer.

 * OnSerializing
 * OnSerialized
 * OnDeserializing
 * OnDeserialized

To tell the serializer which methods should be called during the object's serialization lifecycle, decorate a method with the appropriate attribute (`OnSerializingAttribute`, `OnSerializedAttribute`, `OnDeserializingAttribute`, `OnDeserializedAttribute`).


## Example

Example object with serialization callback methods:</para>

<!-- snippet: SerializationCallbacksObject -->
<a id='snippet-serializationcallbacksobject'></a>
```cs
public class SerializationEventTestObject
{
    // 2222
    // This member is serialized and deserialized with no change.
    public int Member1 { get; set; }

    // The value of this field is set and reset during and
    // after serialization.
    public string Member2 { get; set; }

    // This field is not serialized. The OnDeserializedAttribute
    // is used to set the member value after serialization.
    [JsonIgnore]
    public string Member3 { get; set; }

    // This field is set to null, but populated after deserialization.
    public string Member4 { get; set; }

    public SerializationEventTestObject()
    {
        Member1 = 11;
        Member2 = "Hello World!";
        Member3 = "This is a nonserialized value";
        Member4 = null;
    }

    [OnSerializing]
    internal void OnSerializingMethod(StreamingContext context)
    {
        Member2 = "This value went into the data file during serialization.";
    }

    [OnSerialized]
    internal void OnSerializedMethod(StreamingContext context)
    {
        Member2 = "This value was reset after serialization.";
    }

    [OnDeserializing]
    internal void OnDeserializingMethod(StreamingContext context)
    {
        Member3 = "This value was set during deserialization";
    }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        Member4 = "This value was set after deserialization.";
    }
}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L113-L164' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializationcallbacksobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The example object being serialized and deserialized by Json.NET:</para>

<!-- snippet: SerializationCallbacksExample -->
<a id='snippet-serializationcallbacksexample'></a>
```cs
var obj = new SerializationEventTestObject();

Console.WriteLine(obj.Member1);
// 11
Console.WriteLine(obj.Member2);
// Hello World!
Console.WriteLine(obj.Member3);
// This is a nonserialized value
Console.WriteLine(obj.Member4);
// null

var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
// {
//   "Member1": 11,
//   "Member2": "This value went into the data file during serialization.",
//   "Member4": null
// }

Console.WriteLine(obj.Member1);
// 11
Console.WriteLine(obj.Member2);
// This value was reset after serialization.
Console.WriteLine(obj.Member3);
// This is a nonserialized value
Console.WriteLine(obj.Member4);
// null

obj = JsonConvert.DeserializeObject<SerializationEventTestObject>(json);

Console.WriteLine(obj.Member1);
// 11
Console.WriteLine(obj.Member2);
// This value went into the data file during serialization.
Console.WriteLine(obj.Member3);
// This value was set during deserialization
Console.WriteLine(obj.Member4);
// This value was set after deserialization.
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L169-L207' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializationcallbacksexample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
