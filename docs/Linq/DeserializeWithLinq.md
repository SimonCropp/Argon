# Deserializing from JSON with LINQ

This sample uses LINQ to JSON to manually convert JSON to a .NET type.

<!-- snippet: DeserializeWithLinqTypes -->
<a id='snippet-deserializewithlinqtypes'></a>
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
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/DeserializeWithLinq.cs#L9-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializewithlinqtypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeWithLinqUsage -->
<a id='snippet-deserializewithlinqusage'></a>
```cs
var json = @"[
      {
        'Title': 'Json.NET is awesome!',
        'Author': {
          'Name': 'James Newton-King',
          'Twitter': '@JamesNK',
          'Picture': '/jamesnk.png'
        },
        'Date': '2013-01-23T19:30:00',
        'BodyHtml': '&lt;h3&gt;Title!&lt;/h3&gt;\r\n&lt;p&gt;Content!&lt;/p&gt;'
      }
    ]";

var blogPostArray = JArray.Parse(json);

var blogPosts = blogPostArray.Select(p => new BlogPost
{
    Title = (string) p["Title"],
    AuthorName = (string) p["Author"]["Name"],
    AuthorTwitter = (string) p["Author"]["Twitter"],
    PostedDate = (DateTime) p["Date"],
    Body = HttpUtility.HtmlDecode((string) p["BodyHtml"])
}).ToList();

Console.WriteLine(blogPosts[0].Body);
// <h3>Title!</h3>
// <p>Content!</p>
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Linq/DeserializeWithLinq.cs#L25-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializewithlinqusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
