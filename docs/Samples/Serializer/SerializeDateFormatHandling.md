# DateTimeZoneHandling setting

This sample uses the `Argon.DateFormatHandling` setting to control how `System.DateTime` and `System.DateTimeOffset` are serialized.

<!-- snippet: SerializeDateFormatHandling -->
<a id='snippet-serializedateformathandling'></a>
```cs
var mayanEndOfTheWorld = new DateTime(2012, 12, 21);

var jsonIsoDate = JsonConvert.SerializeObject(mayanEndOfTheWorld);

Console.WriteLine(jsonIsoDate);
// "2012-12-21T00:00:00"

var jsonMsDate = JsonConvert.SerializeObject(mayanEndOfTheWorld, new JsonSerializerSettings
{
    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
});

Console.WriteLine(jsonMsDate);
// "\/Date(1356044400000+0100)\/"
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeDateFormatHandling.cs#L35-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializedateformathandling' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
