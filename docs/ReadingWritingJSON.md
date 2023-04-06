# Basic Reading and Writing JSON

To manually read and write JSON, Json.NET provides the `Argon.JsonReader` and `Argon.JsonWriter` classes.


## JsonTextReader and JsonTextWriter

To quickly work with JSON, either the serializer - [SerializingJSON] - or using [LINQtoJSON] is recommended.

`Argon.JsonTextReader` and `Argon.JsonTextWriter` are used to read and write JSON text. The JsonTextWriter has settings on it to control how JSON is formatted when it is written. These options include formatting, indentation character, indent count, and quote character.

<!-- snippet: ReadingAndWritingJsonText -->
<a id='snippet-readingandwritingjsontext'></a>
```cs
var stringBuilder = new StringBuilder();
var stringWriter = new StringWriter(stringBuilder);

using var jsonWriter = new JsonTextWriter(stringWriter);
jsonWriter.Formatting = Formatting.Indented;

jsonWriter.WriteStartObject();
jsonWriter.WritePropertyName("CPU");
jsonWriter.WriteValue("Intel");
jsonWriter.WritePropertyName("PSU");
jsonWriter.WriteValue("500W");
jsonWriter.WritePropertyName("Drives");
jsonWriter.WriteStartArray();
jsonWriter.WriteValue("DVD read/writer");
jsonWriter.WriteComment("(broken)");
jsonWriter.WriteValue("500 gigabyte hard drive");
jsonWriter.WriteValue("200 gigabyte hard drive");
jsonWriter.WriteEnd();
jsonWriter.WriteEndObject();

// {
//   "CPU": "Intel",
//   "PSU": "500W",
//   "Drives": [
//     "DVD read/writer"
//     /*(broken)*/,
//     "500 gigabyte hard drive",
//     "200 gigabyte hard drive"
//   ]
// }
```
<sup><a href='/src/ArgonTests/Documentation/ReadingAndWritingJsonTests.cs#L12-L45' title='Snippet source file'>snippet source</a> | <a href='#snippet-readingandwritingjsontext' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

JsonTextReader has settings on it for reading different date formats, time zones, and the cultures when reading text values.

<!-- snippet: ReadingJsonText -->
<a id='snippet-readingjsontext'></a>
```cs
var json = """
    {
       'CPU': 'Intel',
       'PSU': '500W',
       'Drives': [
         'DVD read/writer'
         /*(broken)*/,
         '500 gigabyte hard drive',
         '200 gigabyte hard drive'
       ]
    }
    """;

var reader = new JsonTextReader(new StringReader(json));
while (reader.Read())
{
    if (reader.Value != null)
    {
        Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
    }
    else
    {
        Console.WriteLine("Token: {0}", reader.TokenType);
    }
}

// Token: StartObject
// Token: PropertyName, Value: CPU
// Token: String, Value: Intel
// Token: PropertyName, Value: PSU
// Token: String, Value: 500W
// Token: PropertyName, Value: Drives
// Token: StartArray
// Token: String, Value: DVD read/writer
// Token: Comment, Value: (broken)
// Token: String, Value: 500 gigabyte hard drive
// Token: String, Value: 200 gigabyte hard drive
// Token: EndArray
// Token: EndObject
```
<sup><a href='/src/ArgonTests/Documentation/ReadingAndWritingJsonTests.cs#L51-L93' title='Snippet source file'>snippet source</a> | <a href='#snippet-readingjsontext' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## JTokenReader and JTokenWriter</title>

`Argon.JTokenReader` and `Argon.JTokenWriter` read and write LINQ to JSON objects. These objects support the use LINQ to JSON objects with objects that read and write JSON, such as the JsonSerializer. For example to deserialize from a LINQ to JSON object into a regular .NET object and vice versa.

<!-- snippet: ReadingAndWritingJsonLinq -->
<a id='snippet-readingandwritingjsonlinq'></a>
```cs
var o = new JObject(
    new JProperty("Name", "John Smith"),
    new JProperty("BirthDate", new DateTime(1983, 3, 20))
);

var serializer = new JsonSerializer();
var p = (Person) serializer.Deserialize(new JTokenReader(o), typeof(Person));

Console.WriteLine(p.Name);
// John Smith
```
<sup><a href='/src/ArgonTests/Documentation/ReadingAndWritingJsonTests.cs#L99-L112' title='Snippet source file'>snippet source</a> | <a href='#snippet-readingandwritingjsonlinq' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * `Argon.JsonReader`
 * `Argon.JsonWriter`
 * `Argon.JTokenReader`
 * `Argon.JTokenWriter`
