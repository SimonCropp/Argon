# Create JSON using Collection Initializers

This sample creates `Argon.Linq.JObject` and `Argon.Linq.JArray` instances using the C# collection initializer syntax.

<!-- snippet: CreateJsonCollectionInitializer -->
<a id='snippet-createjsoncollectioninitializer'></a>
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

Console.WriteLine(o.ToString());
// {
//   "Cpu": "Intel",
//   "Memory": 32,
//   "Drives": [
//     "DVD",
//     "SSD"
//   ]
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/CreateJsonCollectionInitializer.cs#L12-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-createjsoncollectioninitializer' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
