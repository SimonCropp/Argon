# Creating JSON

As well as parsing JSON from existing JSON strings, LINQ to JSON objects can be created from scratch to create new JSON structures.


## Manually Creating JSON

Setting values and creating objects and arrays one at a time gives total control, but it is more verbose than other options.

<!-- snippet: LinqToJsonCreateNormal -->
<a id='snippet-linqtojsoncreatenormal'></a>
```cs
var array = new JArray();
var text = new JValue("Manual text");
var date = new JValue(new DateTime(2000, 5, 23));

array.Add(text);
array.Add(date);

var json = array.ToString();
// [
//   "Manual text",
//   "2000-05-23T00:00:00"
// ]
```
<sup><a href='/src/Tests/Documentation/LinqToJsonTests.cs#L81-L94' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsoncreatenormal' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Creating JSON with LINQ

Declaratively creating JSON objects using LINQ is a fast way to create JSON from collections of values.

<!-- snippet: LinqToJsonCreateDeclaratively -->
<a id='snippet-linqtojsoncreatedeclaratively'></a>
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

//{
//  "channel": {
//    "title": "James Newton-King",
//    "link": "http://james.newtonking.com",
//    "description": "James Newton-King\'s blog.",
//    "item": [
//      {
//        "title": "Json.NET 1.3 + New license + Now on CodePlex",
//        "description": "Announcing the release of Json.NET 1.3, the MIT license and being available on CodePlex",
//        "link": "http://james.newtonking.com/projects/json-net.aspx",
//        "category": [
//          "Json.NET",
//          "CodePlex"
//        ]
//      },
//      {
//        "title": "LINQ to JSON beta",
//        "description": "Announcing LINQ to JSON",
//        "link": "http://james.newtonking.com/projects/json-net.aspx",
//        "category": [
//          "Json.NET",
//          "LINQ"
//        ]
//      }
//    ]
//  }
//}
```
<sup><a href='/src/Tests/Documentation/LinqToJsonTests.cs#L113-L165' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsoncreatedeclaratively' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Creating JSON from an object

The last option is to create a JSON object from a non-JSON type using the `Argon.Linq.JObject.FromObject` method. Internally, FromObject will use the JsonSerializer to serialize the object to LINQ to JSON objects instead of text.

The example below shows creating a JSON object from an anonymous object, but any .NET type can be used with FromObject to create JSON.

<!-- snippet: LinqToJsonCreateFromObject -->
<a id='snippet-linqtojsoncreatefromobject'></a>
```cs
var o = JObject.FromObject(new
{
    channel = new
    {
        title = "James Newton-King",
        link = "http://james.newtonking.com",
        description = "James Newton-King's blog.",
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
```
<sup><a href='/src/Tests/Documentation/LinqToJsonTests.cs#L173-L193' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsoncreatefromobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * LINQtoJSON
 * `Argon.Linq.JObject.FromObject`
