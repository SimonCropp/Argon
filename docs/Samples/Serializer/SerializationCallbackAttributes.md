# Serialization Callback Attributes

This sample uses serialization callback attributes (`OnSerializingAttribute`, `OnSerializedAttribute`, `OnDeserializingAttribute`, `OnDeserializedAttribute`) to manipulate an object before and after its serialization and deserialization.

<!-- snippet: SerializationCallbackAttributesTypes -->
<a id='snippet-serializationcallbackattributestypes'></a>
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
    [JsonIgnore] public string Member3 { get; set; }

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
    internal void OnSerializingMethod(StreamingContext context) =>
        Member2 = "This value went into the data file during serialization.";

    [OnSerialized]
    internal void OnSerializedMethod(StreamingContext context) =>
        Member2 = "This value was reset after serialization.";

    [OnDeserializing]
    internal void OnDeserializingMethod(StreamingContext context) =>
        Member3 = "This value was set during deserialization";

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context) =>
        Member4 = "This value was set after deserialization.";
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializationCallbackAttributes.cs#L7-L51' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializationcallbackattributestypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializationCallbackAttributesUsage -->
<a id='snippet-serializationcallbackattributesusage'></a>
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
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializationCallbackAttributes.cs#L56-L96' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializationcallbackattributesusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
