# Create JSON declaratively with LINQ

This sample creates `Argon.Linq.JObject` and `Argon.Linq.JArray` instances declaratively  using LINQ.

<!-- snippet: CreateJsonDeclarativelyTypes -->
<a id='snippet-createjsondeclarativelytypes'></a>
```cs
public class Post
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Link { get; set; }
    public IList<string> Categories { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/CreateJsonDeclaratively.cs#L30-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-createjsondeclarativelytypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: CreateJsonDeclarativelyUsage -->
<a id='snippet-createjsondeclarativelyusage'></a>
```cs
var posts = GetPosts();

var rss =
    new JObject(
        new JProperty("channel",
            new JObject(
                new JProperty("title", "James Newton-King"),
                new JProperty("link", "http://james.newtonking.com"),
                new JProperty("description", "James Newton-King's blog."),
                new JProperty("item",
                    new JArray(
                        from p in posts
                        orderby p.Title
                        select new JObject(
                            new JProperty("title", p.Title),
                            new JProperty("description", p.Description),
                            new JProperty("link", p.Link),
                            new JProperty("category",
                                new JArray(
                                    from c in p.Categories
                                    select new JValue(c)))))))));

Console.WriteLine(rss.ToString());

// {
//   "channel": {
//     "title": "James Newton-King",
//     "link": "http://james.newtonking.com",
//     "description": "James Newton-King's blog.",
//     "item": [
//       {
//         "title": "Json.NET 1.3 + New license + Now on CodePlex",
//         "description": "Announcing the release of Json.NET 1.3, the MIT license and being available on CodePlex",
//         "link": "http://james.newtonking.com/projects/json-net.aspx",
//         "category": [
//           "Json.NET",
//           "CodePlex"
//         ]
//       },
//       {
//         "title": "LINQ to JSON beta",
//         "description": "Announcing LINQ to JSON",
//         "link": "http://james.newtonking.com/projects/json-net.aspx",
//         "category": [
//           "Json.NET",
//           "LINQ"
//         ]
//       }
//     ]
//   }
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/CreateJsonDeclaratively.cs#L60-L112' title='Snippet source file'>snippet source</a> | <a href='#snippet-createjsondeclarativelyusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
