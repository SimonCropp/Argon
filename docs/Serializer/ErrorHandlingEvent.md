# ErrorHandling setting

This sample uses the `Argon.JsonSerializerSettings.Error` event to ignore the exceptions thrown from the invalid date strings.

<!-- snippet: ErrorHandlingEventUsage -->
<a id='snippet-ErrorHandlingEventUsage'></a>
```cs
var errors = new List<string>();

var c = JsonConvert.DeserializeObject<List<DateTime>>(
    """
    [
      '2009-09-09T00:00:00Z',
      'I am not a date and will error!',
      [
        1
      ],
      '1977-02-20T00:00:00Z',
      null,
      '2000-12-01T00:00:00Z'
    ]
    """,
    new JsonSerializerSettings
    {
        DeserializeError = (currentObject, originalObject, path, member, exception, markAsHandled) =>
        {
            errors.Add(exception.Message);
            markAsHandled();
        },
        Converters = {new IsoDateTimeConverter()}
    });

// 2009-09-09T00:00:00Z
// 1977-02-20T00:00:00Z
// 2000-12-01T00:00:00Z

// The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.
// Unexpected token parsing date. Expected String, got StartArray.
// Cannot convert null value to System.DateTime.
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Serializer/ErrorHandlingEvent.cs#L12-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-ErrorHandlingEventUsage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
