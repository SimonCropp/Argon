# Using LINQ to JSON Annotations

This sample uses annotations with LINQ to JSON objects.

<!-- snippet: JTokenAnnotation -->
<a id='snippet-jtokenannotation'></a>
```cs
var o = JObject.Parse(@"{
      'name': 'Bill G',
      'age': 58,
      'country': 'United States',
      'employer': 'Microsoft'
    }");

o.AddAnnotation(new HashSet<string>());
o.PropertyChanged += (_, args) => o.Annotation<HashSet<string>>().Add(args.PropertyName);

o["age"] = 59;
o["employer"] = "Bill & Melinda Gates Foundation";

var changedProperties = o.Annotation<HashSet<string>>();
// age
// employer
```
<sup><a href='/Src/Tests/Documentation/Samples/Linq/JTokenAnnotation.cs#L35-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-jtokenannotation' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
