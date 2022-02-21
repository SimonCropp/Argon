# Attributes

Attributes can be used to control how objects are serialized and deserialized.

 * `Argon.JsonObjectAttribute` - Placed on classes to control how they should be serialized as a JSON object.
 * `Argon.JsonArrayAttribute` - Placed on collections to control how they should be serialized as a JSON array.
 * `Argon.JsonDictionaryAttribute` - Placed on dictionaries to control how they should be serialized as a JSON object.
 * `Argon.JsonPropertyAttribute` - Placed on fields and properties to control how they should be serialized as a property in a JSON object.
 * `Argon.JsonConverterAttribute` - Placed on either classes or fields and properties to specify which JsonConverter should be used during serialization.
 * `Argon.JsonExtensionDataAttribute` - Placed on a collection field or property to deserialize properties with no matching class member into the specified collection and write values during serialization.
 * `Argon.JsonConstructorAttribute` - Placed on a constructor to specify that it should be used to create the class during deserialization.


## Standard .NET Serialization Attributes

As well as using the built-in Json.NET attributes, Json.NET also looks for the `System.SerializableAttribute` (if IgnoreSerializableAttribute on DefaultContractResolver is set to false) `DataContractAttribute`, `DataMemberAttribute`, and `NonSerializedAttribute` and attributes when determining how JSON is to be serialized and deserialized.

Json.NET attributes take precedence over standard .NET serialization attributes (e.g. if both JsonPropertyAttribute and DataMemberAttribute are present on a property and both customize the name, the name from JsonPropertyAttribute will be used).

<!-- snippet: SerializationAttributes -->
<a id='snippet-serializationattributes'></a>
```cs
[JsonObject(MemberSerialization.OptIn)]
public class Person
{
    // "John Smith"
    [JsonProperty]
    public string Name { get; set; }

    // "2000-12-15T22:11:03"
    [JsonProperty]
    public DateTime BirthDate { get; set; }

    // new Date(976918263055)
    [JsonProperty]
    public DateTime LastModified { get; set; }

    // not serialized because mode is opt-in
    public string Department { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L92-L111' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializationattributes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Json.NET Serialization Attributes


### JsonObjectAttribute

The MemberSerialization flag on this attribute specifies whether member serialization is opt-in (a member must have the JsonProperty or DataMember attribute to be serialized), opt-out (everything is serialized by default but can be ignored with the JsonIgnoreAttribute, Json.NET's default behavior) or fields (all public and private fields are serialized and properties are ignored).

Placing the the `System.Runtime.Serialization.DataContractAttribute` on a type is another way to default member serialization to opt-in.

The NamingStrategy setting on this attributes can be set to a `Argon.Serialization.NamingStrategy` type that specifies how property names are serialized.

Json.NET serializes .NET classes that implement IEnumerable as a JSON array populated with the IEnumerable values. Placing the `Argon.JsonObjectAttribute` overrides this behavior and forces the serializer to serialize the class's fields and properties.


### JsonArrayAttribute/JsonDictionaryAttribute

The `Argon.JsonArrayAttribute` and `Argon.JsonDictionaryAttribute` are used to specify whether a class is serialized as that collection type.

The collection attributes have options to customize the JsonConverter, type name handling, and reference handling that are applied to collection items.


### JsonPropertyAttribute

JsonPropertyAttribute has a number of uses:

 * By default, the JSON property will have the same name as the .NET property. This attribute allows the name to be customized.
 * JsonPropertyAttribute indicates that a property should be serialized when member serialization is set to opt-in.
 * It includes non-public properties in serialization and deserialization.
 * It can be used to customize type name, reference, null, and default value handling for the property value.
 * It can be used to customize the `Argon.Serialization.NamingStrategy` of the serialized property name.
 * It can be used to customize the property's collection items JsonConverter, type name handling, and reference handling.

The DataMemberAttribute can be used as a substitute for JsonPropertyAttribute.


### JsonIgnoreAttribute

Excludes a field or property from serialization.

The `System.NonSerializedAttribute` can be used as a substitute for JsonIgnoreAttribute.


## JsonConverterAttribute

The `Argon.JsonConverterAttribute` specifies which `Argon.JsonConverter` is used to convert an object.

The attribute can be placed on a class or a member. When placed on a class, the JsonConverter specified by the attribute will be the default way of serializing that class. When the attribute is on a field or property, then the specified JsonConverter will always be used to serialize that value.

The priority of which JsonConverter is used is member attribute, then class attribute, and finally any converters passed to the JsonSerializer.

<!-- snippet: JsonConverterAttributePropertyTypes -->
<a id='snippet-jsonconverterattributepropertytypes'></a>
```cs
public enum UserStatus
{
    NotConfirmed,
    Active,
    Deleted
}

public class User
{
    public string UserName { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public UserStatus Status { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonConverterAttributeProperty.cs#L30-L45' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonconverterattributepropertytypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This example shows the JsonConverterAttribute being applied to a property.

To apply a JsonConverter to the items in a collection, use either `Argon.JsonArrayAttribute`, `Argon.JsonDictionaryAttribute` or `Argon.JsonPropertyAttribute` and set the ItemConverterType property to the converter type you want to use.

### JsonExtensionDataAttribute

The `Argon.JsonExtensionDataAttribute` instructs the `Argon.JsonSerializer` to deserialize properties with no matching field or property on the type into the specified collection. During serialization the values in this collection are written back to the instance's JSON object.

All extension data values will be written during serialization, even if a property the same name has already been written.

This example shows the JsonExtensionDataAttribute being applied to a field, unmatched JSON properties being added to the field's collection during deserialization.

<!-- snippet: DeserializeExtensionDataTypes -->
<a id='snippet-deserializeextensiondatatypes'></a>
```cs
public class DirectoryAccount
{
    // normal deserialization
    public string DisplayName { get; set; }

    // these properties are set in OnDeserialized
    public string UserName { get; set; }
    public string Domain { get; set; }

    [JsonExtensionData]
    IDictionary<string, JToken> _additionalData;

    [OnDeserialized]
    void OnDeserialized(StreamingContext context)
    {
        // SAMAccountName is not deserialized to any property
        // and so it is added to the extension data dictionary
        var samAccountName = (string)_additionalData["SAMAccountName"];

        Domain = samAccountName.Split('\\')[0];
        UserName = samAccountName.Split('\\')[1];
    }

    public DirectoryAccount()
    {
        _additionalData = new Dictionary<string, JToken>();
    }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeExtensionData.cs#L28-L57' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeextensiondatatypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeExtensionDataUsage -->
<a id='snippet-deserializeextensiondatausage'></a>
```cs
var json = @"{
      'DisplayName': 'John Smith',
      'SAMAccountName': 'contoso\\johns'
    }";

var account = JsonConvert.DeserializeObject<DirectoryAccount>(json);

Console.WriteLine(account.DisplayName);
// John Smith

Console.WriteLine(account.Domain);
// contoso

Console.WriteLine(account.UserName);
// johns
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeExtensionData.cs#L62-L78' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeextensiondatausage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### JsonConstructorAttribute

The `Argon.JsonConstructorAttribute` instructs the `Argon.JsonSerializer` to use a specific constructor when deserializing a class. It can be used to create a class using a parameterized constructor instead of the default constructor, or to pick which specific parameterized constructor to use if there are multiple.

<!-- snippet: JsonConstructorAttributeTypes -->
<a id='snippet-jsonconstructorattributetypes'></a>
```cs
public class User
{
    public string UserName { get; private set; }
    public bool Enabled { get; private set; }

    public User()
    {
    }

    [JsonConstructor]
    public User(string userName, bool enabled)
    {
        UserName = userName;
        Enabled = enabled;
    }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonConstructorAttribute.cs#L30-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonconstructorattributetypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: JsonConstructorAttributeUsage -->
<a id='snippet-jsonconstructorattributeusage'></a>
```cs
var json = @"{
      ""UserName"": ""domain\\username"",
      ""Enabled"": true
    }";

var user = JsonConvert.DeserializeObject<User>(json);

Console.WriteLine(user.UserName);
// domain\username
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/JsonConstructorAttribute.cs#L52-L62' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonconstructorattributeusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * `Argon.JsonObjectAttribute`
 * `Argon.JsonArrayAttribute`
 * `Argon.JsonDictionaryAttribute`
 * `Argon.JsonPropertyAttribute`
 * `Argon.JsonConverterAttribute`
 * `Argon.JsonExtensionDataAttribute`
 * `Argon.JsonConstructorAttribute`
