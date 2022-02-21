# Write JSON with JsonTextWriter

This sample writes JSON using the `Argon.JsonTextWriter`.

<!-- snippet: WriteJsonWithJsonTextWriter -->
<a id='snippet-writejsonwithjsontextwriter'></a>
```cs
var stringBuilder = new StringBuilder();
var stringWriter = new StringWriter(stringBuilder);

using (JsonWriter writer = new JsonTextWriter(stringWriter))
{
    writer.Formatting = Formatting.Indented;

    writer.WriteStartObject();
    writer.WritePropertyName("CPU");
    writer.WriteValue("Intel");
    writer.WritePropertyName("PSU");
    writer.WriteValue("500W");
    writer.WritePropertyName("Drives");
    writer.WriteStartArray();
    writer.WriteValue("DVD read/writer");
    writer.WriteComment("(broken)");
    writer.WriteValue("500 gigabyte hard drive");
    writer.WriteValue("200 gigabyte hard drive");
    writer.WriteEnd();
    writer.WriteEndObject();
}

Console.WriteLine(stringBuilder.ToString());
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
<sup><a href='/src/Tests/Documentation/Samples/Json/WriteJsonWithJsonTextWriter.cs#L32-L66' title='Snippet source file'>snippet source</a> | <a href='#snippet-writejsonwithjsontextwriter' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
