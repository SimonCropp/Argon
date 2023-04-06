# Querying JSON with complex JSON Path

This sample loads JSON and then queries values from it using `Argon.JToken.SelectToken`. An error is thrown when part of the JSON path is not found.

<!-- snippet: ErrorWhenNoMatchQuery -->
<a id='snippet-errorwhennomatchquery'></a>
```cs
var items = JArray.Parse(@"[
      {
        'Name': 'John Doe',
      },
      {
        'Name': 'Jane Doe',
      }
    ]");

// A true value for errorWhenNoMatch will result in an error if the queried value is missing
string result;
try
{
    result = (string) items.SelectToken(@"$.[3]['Name']", errorWhenNoMatch: true);
}
catch (JsonException)
{
    result = "Unable to find result in JSON.";
}
```
<sup><a href='/src/ArgonTests/Documentation/Samples/JsonPath/ErrorWhenNoMatchQuery.cs#L10-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-errorwhennomatchquery' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
