# Deserializing Partial JSON Fragments

Often when working with large JSON documents you're only interested in a small fragment of information. This scenario can be annoying when you want to deserialize that JSON fragment into .NET objects because you have to define .NET classes for the entire JSON result.

With Json.NET it is easy to get around this problem. Using LINQ to JSON you can extract the pieces of JSON you want to deserialize before passing them to the Json.NET serializer.

<!-- snippet: SerializingPartialJsonFragmentsObject -->
<a id='snippet-serializingpartialjsonfragmentsobject'></a>
```cs
public class SearchResult
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Url { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L960-L969' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializingpartialjsonfragmentsobject' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializingPartialJsonFragmentsExample -->
<a id='snippet-serializingpartialjsonfragmentsexample'></a>
```cs
var googleSearchText = """
    {
      'responseData': {
        'results': [
          {
            'GsearchResultClass': 'GwebSearch',
            'unescapedUrl': 'http://en.wikipedia.org/wiki/Paris_Hilton',
            'url': 'http://en.wikipedia.org/wiki/Paris_Hilton',
            'visibleUrl': 'en.wikipedia.org',
            'cacheUrl': 'http://www.google.com/search?q=cache:TwrPfhd22hYJ:en.wikipedia.org',
            'title': '<b>Paris Hilton</b> - Wikipedia, the free encyclopedia',
            'titleNoFormatting': 'Paris Hilton - Wikipedia, the free encyclopedia',
            'content': '[1] In 2006, she released her debut album...'
          },
          {
            'GsearchResultClass': 'GwebSearch',
            'unescapedUrl': 'http://www.imdb.com/name/nm0385296/',
            'url': 'http://www.imdb.com/name/nm0385296/',
            'visibleUrl': 'www.imdb.com',
            'cacheUrl': 'http://www.google.com/search?q=cache:1i34KkqnsooJ:www.imdb.com',
            'title': '<b>Paris Hilton</b>',
            'titleNoFormatting': 'Paris Hilton',
            'content': 'Self: Zoolander. Socialite <b>Paris Hilton</b>...'
          }
        ],
        'cursor': {
          'pages': [
            {
              'start': '0',
              'label': 1
            },
            {
              'start': '4',
              'label': 2
            },
            {
              'start': '8',
              'label': 3
            },
            {
              'start': '12',
              'label': 4
            }
          ],
          'estimatedResultCount': '59600000',
          'currentPageIndex': 0,
          'moreResultsUrl': 'http://www.google.com/search?oe=utf8&ie=utf8...'
        }
      },
      'responseDetails': null,
      'responseStatus': 200
    }
    """;

var googleSearch = JObject.Parse(googleSearchText);

// get JSON result objects into a list
var results = googleSearch["responseData"]["results"].Children().ToList();

// serialize JSON results into .NET objects
var searchResults = new List<SearchResult>();
foreach (var result in results)
{
    // JToken.ToObject is a helper method that uses JsonSerializer internally
    var searchResult = result.ToObject<SearchResult>();
    searchResults.Add(searchResult);
}

// Title = <b>Paris Hilton</b> - Wikipedia, the free encyclopedia
// Content = [1] In 2006, she released her debut album...
// Url = http://en.wikipedia.org/wiki/Paris_Hilton

// Title = <b>Paris Hilton</b>
// Content = Self: Zoolander. Socialite <b>Paris Hilton</b>...
// Url = http://www.imdb.com/name/nm0385296/
```
<sup><a href='/src/Tests/Documentation/SerializationTests.cs#L974-L1052' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializingpartialjsonfragmentsexample' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * `Argon.JsonReader`
 * `Argon.JsonWriter`
 * `Argon.JTokenReader`
 * `Argon.JTokenWriter`
