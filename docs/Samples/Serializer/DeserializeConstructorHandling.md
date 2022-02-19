# ConstructorHandling setting

This sample uses the `Argon.ConstructorHandling` setting to successfully deserialize the class using its non-public constructor.

<!-- snippet: DeserializeConstructorHandlingTypes -->
<a id='snippet-deserializeconstructorhandlingtypes'></a>
```cs
public class Website
{
    public string Url { get; set; }

    Website()
    {
    }

    public Website(Website website)
    {
        if (website == null)
        {
            throw new ArgumentNullException(nameof(website));
        }

        Url = website.Url;
    }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeConstructorHandling.cs#L32-L51' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeconstructorhandlingtypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DeserializeConstructorHandlingUsage -->
<a id='snippet-deserializeconstructorhandlingusage'></a>
```cs
var json = @"{'Url':'http://www.google.com'}";

try
{
    JsonConvert.DeserializeObject<Website>(json);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    // Value cannot be null.
    // Parameter name: website
}

var website = JsonConvert.DeserializeObject<Website>(json, new JsonSerializerSettings
{
    ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
});

Console.WriteLine(website.Url);
// http://www.google.com
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeConstructorHandling.cs#L56-L77' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializeconstructorhandlingusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
