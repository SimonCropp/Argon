# ErrorHandling setting

This sample uses the `Argon.JsonSerializerSettings.Error` event to ignore the exceptions thrown from the invalid date strings.

<!-- snippet: ErrorHandlingEventUsage -->
<a id='snippet-errorhandlingeventusage'></a>
```cs
var errors = new List<string>();

var c = JsonConvert.DeserializeObject<List<DateTime>>(@"[
      '2009-09-09T00:00:00Z',
      'I am not a date and will error!',
      [
        1
      ],
      '1977-02-20T00:00:00Z',
      null,
      '2000-12-01T00:00:00Z'
    ]",
    new JsonSerializerSettings
    {
        Error = delegate(object _, ErrorEventArgs args)
        {
            errors.Add(args.ErrorContext.Error.Message);
            args.ErrorContext.Handled = true;
        },
        Converters = { new IsoDateTimeConverter() }
    });

// 2009-09-09T00:00:00Z
// 1977-02-20T00:00:00Z
// 2000-12-01T00:00:00Z

// The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.
// Unexpected token parsing date. Expected String, got StartArray.
// Cannot convert null value to System.DateTime.
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/ErrorHandlingEvent.cs#L36-L66' title='Snippet source file'>snippet source</a> | <a href='#snippet-errorhandlingeventusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
