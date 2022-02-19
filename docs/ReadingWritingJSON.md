# Basic Reading and Writing JSON

To manually read and write JSON, Json.NET provides the `Argon.JsonReader` and `Argon.JsonWriter` classes.



## JsonTextReader and JsonTextWriter


JsonReader and JsonWriter are low-level classes and are primarily for internal use by Json.NET.

To quickly work with JSON, either the serializer - [SerializingJSON] - or using [LINQtoJSON] is recommended.

`Argon.JsonTextReader` and `Argon.JsonTextWriter` are used to read and write JSON text. The JsonTextWriter has settings on it to control how JSON is formatted when it is written. These options include formatting, indentation character, indent count, and quote character.

snippet: ReadingAndWritingJsonText

JsonTextReader has settings on it for reading different date formats, time zones, and the cultures when reading text values.

snippet: ReadingJsonText

## JTokenReader and JTokenWriter</title>

`Argon.Linq.JTokenReader` and `Argon.Linq.JTokenWriter` read and write LINQ to JSON objects. They are located in the `Argon.Linq` namespace. These objects support the use LINQ to JSON objects with objects that read and write JSON, such as the JsonSerializer. For example to deserialize from a LINQ to JSON object into a regular .NET object and vice versa.


snippet: ReadingAndWritingJsonLinq
      </content>
    </section>

## Related Topics

 * `Argon.JsonReader`
 * `Argon.JsonWriter`
 * `Argon.Linq.JTokenReader`
 * `Argon.Linq.JTokenWriter`
 * `Argon.Bson.BsonReader`
 * `Argon.Bson.BsonWriter`