# DateTimeZoneHandling setting

This sample uses the DateFormatString setting to control how `System.DateTime` and `System.DateTimeOffset` are serialized.

<!-- snippet: SerializeDateFormatString -->
<a id='snippet-serializedateformatstring'></a>
```cs
IList<DateTime> dateList = new List<DateTime>
{
    new(2009, 12, 7, 23, 10, 0, DateTimeKind.Utc),
    new(2010, 1, 1, 9, 0, 0, DateTimeKind.Utc),
    new(2010, 2, 10, 10, 0, 0, DateTimeKind.Utc)
};

var json = JsonConvert.SerializeObject(dateList, new JsonSerializerSettings
{
    DateFormatString = "d MMMM, yyyy",
    Formatting = Formatting.Indented
});

Console.WriteLine(json);
// [
//   "7 December, 2009",
//   "1 January, 2010",
//   "10 February, 2010"
// ]
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeDateFormatString.cs#L35-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializedateformatstring' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
