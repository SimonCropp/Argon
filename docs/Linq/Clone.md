# Cloning JSON with JToken.DeepClone

This sample recursively clones a `Argon.JToken`  and all its children using `Argon.JToken.DeepClone`.

<!-- snippet: Clone -->
<a id='snippet-clone'></a>
```cs
var o1 = new JObject
{
    {"String", "A string!"},
    {"Items", new JArray(1, 2)}
};

Console.WriteLine(o1.ToString());
// {
//   "String": "A string!",
//   "Items": [
//     1,
//     2
//   ]
// }

var o2 = (JObject) o1.DeepClone();

Console.WriteLine(o2.ToString());
// {
//   "String": "A string!",
//   "Items": [
//     1,
//     2
//   ]
// }

Console.WriteLine(JToken.DeepEquals(o1, o2));
// true

Console.WriteLine(ReferenceEquals(o1, o2));
// false
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/Clone.cs#L10-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-clone' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
