# Querying JSON with complex JSON Path

This sample loads JSON and then queries values from it using `Argon.Linq.JToken.SelectToken(System.String)` with a strict equals JSON Path.

<!-- snippet: StrictEqualsQueryUsage -->
<a id='snippet-strictequalsqueryusage'></a>
```cs
var items = JArray.Parse(@"[
      {
        'Name': 'Valid JSON',
        'Valid': true
      },
      {
        'Name': 'Invalid JSON',
        'Valid': 'true'
      }
    ]");

// Use === operator. Compared types must be the same to be valid
var strictResults = items.SelectTokens(@"$.[?(@.Valid === true)]").ToList();

foreach (var item in strictResults)
{
    Console.WriteLine((string)item["Name"]);
}
// Valid JSON
```
<sup><a href='/src/Tests/Documentation/Samples/JsonPath/StrictEqualsQuery.cs#L35-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-strictequalsqueryusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
