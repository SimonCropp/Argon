# Serialize Raw JSON value

This sample uses `Argon.Linq.JRaw` properties to serialize JSON with raw content.

<!-- snippet: SerializeRawJsonTypes -->
<a id='snippet-serializerawjsontypes'></a>
```cs
public class JavaScriptSettings
{
    public JRaw OnLoadFunction { get; set; }
    public JRaw OnUnloadFunction { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeRawJson.cs#L7-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializerawjsontypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: SerializeRawJsonUsage -->
<a id='snippet-serializerawjsonusage'></a>
```cs
var settings = new JavaScriptSettings
{
    OnLoadFunction = new JRaw("OnLoad"),
    OnUnloadFunction = new JRaw("function(e) { alert(e); }")
};

var json = JsonConvert.SerializeObject(settings, Formatting.Indented);

Console.WriteLine(json);
// {
//   "OnLoadFunction": OnLoad,
//   "OnUnloadFunction": function(e) { alert(e); }
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/SerializeRawJson.cs#L18-L32' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializerawjsonusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
