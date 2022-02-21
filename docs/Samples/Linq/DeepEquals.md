# Comparing JSON with JToken.DeepEquals

This sample compares `Argon.Linq.JToken` instances using `Argon.Linq.JToken.DeepEquals(Argon.Linq.JToken,Argon.Linq.JToken)`, comparing the token and all child tokens.

<!-- snippet: DeepEquals -->
<a id='snippet-deepequals'></a>
```cs
var s1 = new JValue("A string");
var s2 = new JValue("A string");
var s3 = new JValue("A STRING");

Console.WriteLine(JToken.DeepEquals(s1, s2));
// true

Console.WriteLine(JToken.DeepEquals(s2, s3));
// false

var o1 = new JObject
{
    { "Integer", 12345 },
    { "String", "A string" },
    { "Items", new JArray(1, 2) }
};

var o2 = new JObject
{
    { "Integer", 12345 },
    { "String", "A string" },
    { "Items", new JArray(1, 2) }
};

Console.WriteLine(JToken.DeepEquals(o1, o2));
// true

Console.WriteLine(JToken.DeepEquals(s1, o1["String"]));
// true
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/DeepEquals.cs#L33-L63' title='Snippet source file'>snippet source</a> | <a href='#snippet-deepequals' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
