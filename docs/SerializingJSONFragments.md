# Deserializing Partial JSON Fragments

Often when working with large JSON documents you're only interested in a small fragment of information. This scenario can be annoying when you want to deserialize that JSON fragment into .NET objects because you have to define .NET classes for the entire JSON result.

With Json.NET it is easy to get around this problem. Using LINQ to JSON you can extract the pieces of JSON you want to deserialize before passing them to the Json.NET serializer.

snippet: SerializingPartialJsonFragmentsObject

snippet: SerializingPartialJsonFragmentsExample


## Related Topics

 * `Argon.JsonReader`
 * `Argon.JsonWriter`
 * `Argon.Linq.JTokenReader`
 * `Argon.Linq.JTokenWriter`
 * `Argon.Bson.BsonReader`
 * `Argon.Bson.BsonWriter`