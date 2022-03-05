# JsonIgnoreAttribute

This sample uses the `Argon.JsonIgnoreAttribute` to exclude a property from serialization.

<!-- snippet: PropertyJsonIgnoreTypes -->
<a id='snippet-propertyjsonignoretypes'></a>
```cs
public class Account
{
    public string FullName { get; set; }
    public string EmailAddress { get; set; }

    [JsonIgnore] public string PasswordHash { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/PropertyJsonIgnore.cs#L7-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-propertyjsonignoretypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: PropertyJsonIgnoreUsage -->
<a id='snippet-propertyjsonignoreusage'></a>
```cs
var account = new Account
{
    FullName = "Joe User",
    EmailAddress = "joe@example.com",
    PasswordHash = "VHdlZXQgJ1F1aWNrc2lsdmVyJyB0byBASmFtZXNOSw=="
};

var json = JsonConvert.SerializeObject(account);

Console.WriteLine(json);
// {"FullName":"Joe User","EmailAddress":"joe@example.com"}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/PropertyJsonIgnore.cs#L22-L36' title='Snippet source file'>snippet source</a> | <a href='#snippet-propertyjsonignoreusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
