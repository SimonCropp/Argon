# Create JSON from an Anonymous Type

This sample creates a `Argon.Linq.JObject` from an anonymous type.

<!-- snippet: CreateJsonAnonymousObjectTypes -->
<a id='snippet-createjsonanonymousobjecttypes'></a>
```cs
public class Post
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Link { get; set; }
    public IList<string> Categories { get; set; }
}
```
<sup><a href='/Src/Tests/Documentation/Samples/Linq/CreateJsonAnonymousObject.cs#L32-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-createjsonanonymousobjecttypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: CreateJsonAnonymousObjectUsage -->
<a id='snippet-createjsonanonymousobjectusage'></a>
```cs
var posts = new List<Post>
{
    new()
    {
        Title = "Episode VII",
        Description = "Episode VII production",
        Categories = new List<string>
        {
            "episode-vii",
            "movie"
        },
        Link = "episode-vii-production.aspx"
    }
};

var o = JObject.FromObject(new
{
    channel = new
    {
        title = "Star Wars",
        link = "http://www.starwars.com",
        description = "Star Wars blog.",
        item =
            from p in posts
            orderby p.Title
            select new
            {
                title = p.Title,
                description = p.Description,
                link = p.Link,
                category = p.Categories
            }
    }
});

Console.WriteLine(o.ToString());
// {
//   "channel": {
//     "title": "Star Wars",
//     "link": "http://www.starwars.com",
//     "description": "Star Wars blog.",
//     "item": [
//       {
//         "title": "Episode VII",
//         "description": "Episode VII production",
//         "link": "episode-vii-production.aspx",
//         "category": [
//           "episode-vii",
//           "movie"
//         ]
//       }
//     ]
//   }
// }
```
<sup><a href='/Src/Tests/Documentation/Samples/Linq/CreateJsonAnonymousObject.cs#L45-L100' title='Snippet source file'>snippet source</a> | <a href='#snippet-createjsonanonymousobjectusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
