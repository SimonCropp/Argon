# Create JObject and JArray programatically

This sample creates `Argon.Linq.JObject` and `Argon.Linq.JArray` instances one at a time programatically.

<!-- snippet: CreateJsonManually -->
<a id='snippet-createjsonmanually'></a>
```cs
var array = new JArray
{
    "Manual text",
    new DateTime(2000, 5, 23)
};

var o = new JObject
{
    ["MyArray"] = array
};

var json = o.ToString();
// {
//   "MyArray": [
//     "Manual text",
//     "2000-05-23T00:00:00"
//   ]
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/CreateJsonManually.cs#L35-L54' title='Snippet source file'>snippet source</a> | <a href='#snippet-createjsonmanually' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
