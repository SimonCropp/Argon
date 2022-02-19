## Serialize DateFormatString

This sample uses the DateFormatString setting to control how `DateTime` and `DateTimeOffset` are deserialized.

<!-- snippet: DeserializeDateFormatString -->
<a id='snippet-deserializedateformatstring'></a>
```cs
var json = @"[
      '7 December, 2009',
      '1 January, 2010',
      '10 February, 2010'
    ]";

var dateList = JsonConvert.DeserializeObject<IList<DateTime>>(json, new JsonSerializerSettings
{
    DateFormatString = "d MMMM, yyyy"
});

foreach (var dateTime in dateList)
{
    Console.WriteLine(dateTime.ToLongDateString());
}
// Monday, 07 December 2009
// Friday, 01 January 2010
// Wednesday, 10 February 2010
```
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/DeserializeDateFormatString.cs#L35-L54' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializedateformatstring' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
