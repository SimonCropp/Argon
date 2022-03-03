# Serializing and Deserializing JSON

The quickest method of converting between JSON text and a .NET object is using the `Argon.JsonSerializer`. The JsonSerializer converts .NET objects into their JSON equivalent and back again by mapping the .NET object property names to the JSON property names and copies the values for you.


## JsonConvert

For simple scenarios where you want to convert to and from a JSON string, the `Argon.JsonConvert.SerializeObject` and `Argon.JsonConvert.DeserializeObject` methods on JsonConvert provide an easy-to-use wrapper over JsonSerializer.

<!-- snippet: SerializeObject -->
<a id='snippet-serializeobject'></a>
```cs
var product = new Product
{
    Name = "Apple",
    ExpiryDate = new(2008, 12, 28),
    Price = 3.99M,
    Sizes = new[] { "Small", "Medium", "Large" }
};

var output = JsonConvert.SerializeObject(product);
//{
//  "Name": "Apple",
//  "ExpiryDate": "2008-12-28T00:00:00",
//  "Price": 3.99,
//  "Sizes": [
//    "Small",
//    "Medium",
//    "Large"
//  ]
//}

var deserializedProduct = JsonConvert.DeserializeObject<Product>(output);
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L23-L45' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializeobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

SerializeObject and DeserializeObject both have overloads that take a `Argon.JsonSerializerSettings` object. JsonSerializerSettings lets you use many of the JsonSerializer settings listed below while still using the simple serialization methods.


## JsonSerializer

For more control over how an object is serialized, the `Argon.JsonSerializer` can be used directly.

The JsonSerializer is able to read and write JSON text directly to a stream via `Argon.JsonTextReader`
and `Argon.JsonTextWriter`. Other kinds of JsonWriters can also be used, such as `Argon.Linq.JTokenReader`/`Argon.Linq.JTokenWriter`, to convert your object to and from LINQ to JSON objects, or `Argon.Bson.BsonReader`/`Argon.Bson.BsonWriter`, to convert to and from BSON.

<!-- snippet: JsonSerializerToStream -->
<a id='snippet-jsonserializertostream'></a>
```cs
var product = new Product
{
    ExpiryDate = new(2008, 12, 28)
};

var serializer = new JsonSerializer
{
    NullValueHandling = NullValueHandling.Ignore
};

using var streamWriter = new StreamWriter(@"c:\json.txt");
using JsonWriter writer = new JsonTextWriter(streamWriter);
serializer.Serialize(writer, product);
// {"ExpiryDate":new Date(1230375600000),"Price":0}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L53-L69' title='Snippet source file'>snippet source</a> | <a href='#snippet-jsonserializertostream' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

JsonSerializer has a number of properties on it to customize how it serializes JSON. These can also be used with the methods on JsonConvert via the JsonSerializerSettings overloads.

See also: [SerializationSettings]


## Related Topics

 * SerializationGuide
 * SerializationSettings
 * SerializationAttributes
 * SerializingJSONFragments
 * `Argon.JsonConvert`
 * `Argon.JsonSerializer`
 * `Argon.JsonSerializerSettings`
