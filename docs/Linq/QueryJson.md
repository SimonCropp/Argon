# Querying JSON with complex JSON Path

This sample loads JSON and then queries values from it using `Argon.JToken.Item(System.Object)` indexer and then casts the returned tokens to .NET values.

<!-- snippet: QueryJson -->
<a id='snippet-queryjson'></a>
```cs
var json = """
    {
      'channel': {
        'title': 'James Newton-King',
        'link': 'http://james.newtonking.com',
        'description': 'James Newton-King\'s blog.',
        'item': [
          {
            'title': 'Json.NET 1.3 + New license + Now on CodePlex',
            'description': 'Announcing the release of Json.NET 1.3, the MIT license and the source on CodePlex',
            'link': 'http://james.newtonking.com/projects/json-net.aspx',
            'category': [
              'Json.NET',
              'CodePlex'
            ]
          },
          {
            'title': 'LINQ to JSON beta',
            'description': 'Announcing LINQ to JSON',
            'link': 'http://james.newtonking.com/projects/json-net.aspx',
            'category': [
              'Json.NET',
              'LINQ'
            ]
          }
        ]
      }
    }
    """;

var rss = JObject.Parse(json);

var rssTitle = (string) rss["channel"]["title"];

Console.WriteLine(rssTitle);
// James Newton-King

var itemTitle = (string) rss["channel"]["item"][0]["title"];

Console.WriteLine(itemTitle);
// Json.NET 1.3 + New license + Now on CodePlex

var categories = (JArray) rss["channel"]["item"][0]["category"];

Console.WriteLine(categories);
// [
//   "Json.NET",
//   "CodePlex"
// ]

var categoriesText = categories.Select(c => (string) c).ToArray();

Console.WriteLine(string.Join(", ", categoriesText));
// Json.NET, CodePlex
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/QueryJson.cs#L12-L69' title='Snippet source file'>snippet source</a> | <a href='#snippet-queryjson' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
