# Convert JSON to a Type

This sample converts LINQ to JSON objects to .NET types using `Argon.Linq.JToken.ToObject<T>`.

<!-- snippet: ToObjectComplex -->
<a id='snippet-toobjectcomplex'></a>
```cs
var json = @"{
      'd': [
        {
          'Name': 'John Smith'
        },
        {
          'Name': 'Mike Smith'
        }
      ]
    }";

var o = JObject.Parse(json);

var a = (JArray)o["d"];

var person = a.ToObject<IList<Person>>();

Console.WriteLine(person[0].Name);
// John Smith

Console.WriteLine(person[1].Name);
// Mike Smith
```
<sup><a href='/Src/Tests/Documentation/Samples/Linq/ToObjectComplex.cs#L42-L65' title='Snippet source file'>snippet source</a> | <a href='#snippet-toobjectcomplex' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
