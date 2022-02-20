# Querying JSON with dynamic

This sample loads JSON and then queries values from it using C# dynamic functionality.

<!-- snippet: QueryJsonDynamic -->
<a id='snippet-queryjsondynamic'></a>
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

dynamic blogPosts = JArray.Parse(json);

var blogPost = blogPosts[0];

string title = blogPost.Title;

Console.WriteLine(title);
// Json.NET is awesome!

string author = blogPost.Author.Name;

Console.WriteLine(author);
// James Newton-King

DateTime postDate = blogPost.Date;

Console.WriteLine(postDate);
// 23/01/2013 7:30:00 p.m.
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/QueryJsonDynamic.cs#L36-L68' title='Snippet source file'>snippet source</a> | <a href='#snippet-queryjsondynamic' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
