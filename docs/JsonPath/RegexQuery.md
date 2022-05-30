# Load and query JSON with JToken.SelectToken

This sample loads JSON and then queries values from it using `Argon.JToken.SelectToken(System.String)` with a regex JSON Path.

<!-- snippet: RegexQuery -->
<a id='snippet-regexquery'></a>
```cs
var array = JArray.Parse(@"[
      {
        'PackageId': 'Argon',
        'Version': '11.0.1',
        'ReleaseDate': '2018-02-17T00:00:00'
      },
      {
        'PackageId': 'NUnit',
        'Version': '3.9.0',
        'ReleaseDate': '2017-11-10T00:00:00'
      }
    ]");

// Find packages
var packages = array.SelectTokens(@"$.[?(@.PackageId =~ /^Argon/)]").ToList();

foreach (var item in packages)
{
    Console.WriteLine((string) item["PackageId"]);
}

// Argon
```
<sup><a href='/src/Tests/Documentation/Samples/JsonPath/RegexQuery.cs#L10-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-regexquery' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
