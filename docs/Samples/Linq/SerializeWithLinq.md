# Serializing to JSON with LINQ

This sample uses LINQ to JSON to manually convert a .NET type to JSON.

<!-- snippet: SerializeWithLinqTypes -->
<a id='snippet-serializewithlinqtypes'></a>
```cs
public class BlogPost
{
    public string Title { get; set; }
    public string AuthorName { get; set; }
    public string AuthorTwitter { get; set; }
    public string Body { get; set; }
    public DateTime PostedDate { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/SerializeWithLinq.cs#L11-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializewithlinqtypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializeWithLinqUsage -->
<a id='snippet-serializewithlinqusage'></a>
```cs
var blogPosts = new List<BlogPost>
{
    new()
    {
        Title = "Json.NET is awesome!",
        AuthorName = "James Newton-King",
        AuthorTwitter = "JamesNK",
        PostedDate = new(2013, 1, 23, 19, 30, 0),
        Body = @"<h3>Title!</h3><p>Content!</p>"
    }
};

var blogPostsArray = new JArray(
    blogPosts.Select(p => new JObject
    {
        {"Title", p.Title},
        {
            "Author", new JObject
            {
                {"Name", p.AuthorName},
                {"Twitter", p.AuthorTwitter}
            }
        },
        {"Date", p.PostedDate},
        {"BodyHtml", HttpUtility.HtmlEncode(p.Body)}
    })
);

Console.WriteLine(blogPostsArray.ToString());
// [
//   {
//     "Title": "Json.NET is awesome!",
//     "Author": {
//       "Name": "James Newton-King",
//       "Twitter": "JamesNK"
//     },
//     "Date": "2013-01-23T19:30:00",
//     "BodyHtml": "&lt;h3&gt;Title!&lt;/h3&gt;&lt;p&gt;Content!&lt;/p&gt;"
//   }
// ]
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/SerializeWithLinq.cs#L27-L70' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializewithlinqusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
