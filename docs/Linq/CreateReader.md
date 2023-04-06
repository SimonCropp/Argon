# Create JTokenReader from JToken

This sample creates a `Argon.JTokenReader` from a `Argon.JToken`.

<!-- snippet: CreateReader -->
<a id='snippet-createreader'></a>
```cs
var o = new JObject
{
    {"Cpu", "Intel"},
    {"Memory", 32},
    {
        "Drives", new JArray
        {
            "DVD",
            "SSD"
        }
    }
};

var reader = o.CreateReader();
while (reader.Read())
{
    Console.Write(reader.TokenType);
    if (reader.Value != null)
    {
        Console.Write($" - {reader.Value}");
    }

    Console.WriteLine();
}

// StartObject
// PropertyName - Cpu
// String - Intel
// PropertyName - Memory
// Integer - 32
// PropertyName - Drives
// StartArray
// String - DVD
// String - SSD
// EndArray
// EndObject
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/CreateReader.cs#L12-L51' title='Snippet source file'>snippet source</a> | <a href='#snippet-createreader' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
