# Merging JSON

This sample merges LINQ to JSON objects.

<!-- snippet: MergeJson -->
<a id='snippet-mergejson'></a>
```cs
var o1 = JObject.Parse(@"{
      'FirstName': 'John',
      'LastName': 'Smith',
      'Enabled': false,
      'Roles': [ 'User' ]
    }");
var o2 = JObject.Parse(@"{
      'Enabled': true,
      'Roles': [ 'User', 'Admin' ]
    }");

o1.Merge(o2, new JsonMergeSettings
{
    // union array values together to avoid duplicates
    MergeArrayHandling = MergeArrayHandling.Union
});

var json = o1.ToString();
// {
//   "FirstName": "John",
//   "LastName": "Smith",
//   "Enabled": true,
//   "Roles": [
//     "User",
//     "Admin"
//   ]
// }
```
<sup><a href='/Src/Tests/Documentation/Samples/Linq/MergeJson.cs#L35-L63' title='Snippet source file'>snippet source</a> | <a href='#snippet-mergejson' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
