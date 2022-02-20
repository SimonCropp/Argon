# Preserving Object References

By default Json.NET will serialize all objects it encounters by value. If a list contains two Person references and both references point to the same object, then the JsonSerializer will write out all the names and values for each reference.

<!-- snippet: PreservingObjectReferencesOff -->
<a id='snippet-preservingobjectreferencesoff'></a>
```cs
var p = new Person
{
    BirthDate = new DateTime(1980, 12, 23, 0, 0, 0, DateTimeKind.Utc),
    LastModified = new DateTime(2009, 2, 20, 12, 59, 21, DateTimeKind.Utc),
    Name = "James"
};

var people = new List<Person>
{
    p,
    p
};

var json = JsonConvert.SerializeObject(people, Formatting.Indented);
//[
//  {
//    "Name": "James",
//    "BirthDate": "1980-12-23T00:00:00Z",
//    "LastModified": "2009-02-20T12:59:21Z"
//  },
//  {
//    "Name": "James",
//    "BirthDate": "1980-12-23T00:00:00Z",
//    "LastModified": "2009-02-20T12:59:21Z"
//  }
//]
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L333-L360' title='Snippet source file'>snippet source</a> | <a href='#snippet-preservingobjectreferencesoff' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

In most cases this is the desired result, but in certain scenarios writing the second item in the list as a reference to the first is a better solution. If the above JSON was deserialized now, then the returned list would contain two completely separate Person objects with the same values. Writing references by value will also cause problems on objects where a circular reference occurs.


## PreserveReferencesHandling

Setting `Argon.PreserveReferencesHandling` will track object references when serializing and deserializing JSON.

<!-- snippet: PreservingObjectReferencesOn -->
<a id='snippet-preservingobjectreferenceson'></a>
```cs
var json = JsonConvert.SerializeObject(people, Formatting.Indented,
    new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

//[
//  {
//    "$id": "1",
//    "Name": "James",
//    "BirthDate": "1983-03-08T00:00Z",
//    "LastModified": "2012-03-21T05:40Z"
//  },
//  {
//    "$ref": "1"
//  }
//]

var deserializedPeople = JsonConvert.DeserializeObject<List<Person>>(json,
    new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

Console.WriteLine(deserializedPeople.Count);
// 2

var p1 = deserializedPeople[0];
var p2 = deserializedPeople[1];

Console.WriteLine(p1.Name);
// James
Console.WriteLine(p2.Name);
// James

var equal = ReferenceEquals(p1, p2);
// true
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L389-L421' title='Snippet source file'>snippet source</a> | <a href='#snippet-preservingobjectreferenceson' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The first Person in the list is serialized with the addition of an object ID. The second Person in JSON is now only a reference to the first.

With PreserveReferencesHandling on, now only one Person object is created on deserialization and the list contains two references to it, mirroring what was started with.

Metadata properties like `$id` must be located at the beginning of a JSON object to be successfully detected during deserialization. If it is not possible control the order of properties in the JSON object then `Argon.MetadataPropertyHandling` can be used to remove this restriction.

References cannot be preserved when a value is set via a non-default constructor. With a non-default constructor, child values must be created before the parent value so they can be passed into the constructor, making tracking reference impossible. `System.Runtime.Serialization.ISerializable` types are an example of a class whose values are populated with a non-default constructor and won't work with PreserveReferencesHandling.


## IsReference

The PreserveReferencesHandling setting on the JsonSerializer will change how all objects are serialized and deserialized. For fine grain control over which objects and members should be serialized as a reference there is the IsReference property on the JsonObjectAttribute, JsonArrayAttribute and JsonPropertyAttribute.

Setting IsReference on JsonObjectAttribute or JsonArrayAttribute to true will mean the JsonSerializer will always serialize the type the attribute is against as a reference. Setting IsReference on the JsonPropertyAttribute to true will serialize only that property as a reference.

<!-- snippet: PreservingObjectReferencesAttribute -->
<a id='snippet-preservingobjectreferencesattribute'></a>
```cs
[JsonObject(IsReference = true)]
public class EmployeeReference
{
    public string Name { get; set; }
    public EmployeeReference Manager { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L426-L433' title='Snippet source file'>snippet source</a> | <a href='#snippet-preservingobjectreferencesattribute' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## IReferenceResolver

To customize how references are generated and resolved the `Argon.Serialization.IReferenceResolver` interface is available to inherit from and use with the JsonSerializer.


## Related Topics

 * `Argon.PreserveReferencesHandling`
