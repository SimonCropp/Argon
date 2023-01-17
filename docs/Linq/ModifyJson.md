# Modifying JSON

This sample loads JSON, modifies `Argon.JObject` and `Argon.JArray` instances and then writes the JSON back out again.

<!-- snippet: ModifyJson -->
<a id='snippet-modifyjson'></a>
```cs
var json = """
    {
      'channel': {
        'title': 'Star Wars',
        'link': 'http://www.starwars.com',
        'description': 'Star Wars blog.',
        'obsolete': 'Obsolete value',
        'item': []
      }
    }
    """;

var rss = JObject.Parse(json);

var channel = (JObject) rss["channel"];

channel["title"] = ((string) channel["title"]).ToUpper();
channel["description"] = ((string) channel["description"]).ToUpper();

channel.Property("obsolete").Remove();

channel.Property("description").AddAfterSelf(new JProperty("new", "New value"));

var item = (JArray) channel["item"];
item.Add("Item 1");
item.Add("Item 2");

Console.WriteLine(rss.ToString());
// {
//   "channel": {
//     "title": "STAR WARS",
//     "link": "http://www.starwars.com",
//     "description": "STAR WARS BLOG.",
//     "new": "New value",
//     "item": [
//       "Item 1",
//       "Item 2"
//     ]
//   }
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Linq/ModifyJson.cs#L12-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-modifyjson' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
