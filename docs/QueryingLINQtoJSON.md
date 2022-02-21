# Querying JSON with LINQ

LINQ to JSON provides methods for getting data from its objects. The index methods on JObject/JArray supports quickly get data by its property name on an object or index in a collection, while `Argon.Linq.JToken.Children` allows the retrieval of ranges of data as `IEnumerable<JToken>` to then query using LINQ.


## Getting values by Property Name or Collection Index

The simplest way to get a value from LINQ to JSON is to use the `Argon.Linq.JToken.Item(System.Object)` index on JObject/JArray and then cast the returned `Argon.Linq.JValue` to the type required.

<!-- snippet: LinqToJsonSimpleQuerying -->
<a id='snippet-linqtojsonsimplequerying'></a>
```cs
var json = @"{
      'channel': {
        'title': 'James Newton-King',
        'link': 'http://james.newtonking.com',
        'description': 'James Newton-King\'s blog.',
        'item': [
          {
            'title': 'Json.NET 1.3 + New license + Now on CodePlex',
            'description': 'Announcing the release of Json.NET 1.3, the MIT license and the source on CodePlex',
            'link': 'http://james.newtonking.com/projects/json-net.aspx',
            'categories': [
              'Json.NET',
              'CodePlex'
            ]
          },
          {
            'title': 'LINQ to JSON beta',
            'description': 'Announcing LINQ to JSON',
            'link': 'http://james.newtonking.com/projects/json-net.aspx',
            'categories': [
              'Json.NET',
              'LINQ'
            ]
          }
        ]
      }
    }";

var rss = JObject.Parse(json);

var rssTitle = (string)rss["channel"]["title"];
// James Newton-King

var itemTitle = (string)rss["channel"]["item"][0]["title"];
// Json.NET 1.3 + New license + Now on CodePlex

var categories = (JArray)rss["channel"]["item"][0]["categories"];
// ["Json.NET", "CodePlex"]

IList<string> categoriesText = categories.Select(c => (string)c).ToList();
// Json.NET
// CodePlex
```
<sup><a href='/src/Tests/Documentation/LinqToJsonTests.cs#L239-L282' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsonsimplequerying' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Querying with LINQ

JObject/JArray can also be queried using LINQ. `Argon.Linq.JToken.Children` returns the children values of a JObject/JArray as an `IEnumerable<JToken>` that can then be queried with the standard Where/OrderBy/Select LINQ operators.
        
`Argon.Linq.JToken.Children` returns all the children of a token. If it is a JObject it will return a collection of properties to work with, and if it is a JArray a collection of the array's values will be returned.

<!-- snippet: LinqToJsonQuerying -->
<a id='snippet-linqtojsonquerying'></a>
```cs
var postTitles =
    from p in rss["channel"]["item"]
    select (string)p["title"];

foreach (var item in postTitles)
{
    Console.WriteLine(item);
}

//LINQ to JSON beta
//Json.NET 1.3 + New license + Now on CodePlex

var categories =
    from c in rss["channel"]["item"].SelectMany(i => i["categories"]).Values<string>()
    group c by c
    into g
    orderby g.Count() descending
    select new { Category = g.Key, Count = g.Count() };

foreach (var c in categories)
{
    Console.WriteLine($"{c.Category} - Count: {c.Count}");
}

//Json.NET - Count: 2
//LINQ - Count: 1
//CodePlex - Count: 1
```
<sup><a href='/src/Tests/Documentation/LinqToJsonTests.cs#L316-L344' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsonquerying' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

LINQ to JSON can also be used to manually convert JSON to a .NET object.

<!-- snippet: LinqToJsonDeserializeObject -->
<a id='snippet-linqtojsondeserializeobject'></a>
```cs
public class Shortie
{
    public string Original { get; set; }
    public string Shortened { get; set; }
    public string Short { get; set; }
    public ShortieException Error { get; set; }
}

public class ShortieException
{
    public int Code { get; set; }
    public string ErrorMessage { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/LinqToJsonTests.cs#L350-L364' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsondeserializeobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Manually serializing and deserializing between .NET objects is useful when working with JSON that doesn't closely match the .NET objects.

<!-- snippet: LinqToJsonDeserializeExample -->
<a id='snippet-linqtojsondeserializeexample'></a>
```cs
var jsonText = @"{
      'short': {
        'original': 'http://www.foo.com/',
        'short': 'krehqk',
        'error': {
          'code': 0,
          'msg': 'No action taken'
        }
      }
    }";

var json = JObject.Parse(jsonText);

var shortie = new Shortie
{
    Original = (string)json["short"]["original"],
    Short = (string)json["short"]["short"],
    Error = new ShortieException
    {
        Code = (int)json["short"]["error"]["code"],
        ErrorMessage = (string)json["short"]["error"]["msg"]
    }
};

Console.WriteLine(shortie.Original);
// http://www.foo.com/

Console.WriteLine(shortie.Error.ErrorMessage);
// No action taken
```
<sup><a href='/src/Tests/Documentation/LinqToJsonTests.cs#L369-L399' title='Snippet source file'>snippet source</a> | <a href='#snippet-linqtojsondeserializeexample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * LINQtoJSON
 * `Argon.Linq.JToken.Item(System.Object)`
 * `Argon.Linq.JToken.Children`
