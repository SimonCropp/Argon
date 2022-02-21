# MetadataPropertyHandling setting

This sample deserializes JSON with `Argon.MetadataPropertyHandling` set to ReadAhead so that metadata properties do not need to be at the start of an object.

<!-- snippet: DeserializeMetadataPropertyHandling -->
<a id='snippet-deserializemetadatapropertyhandling'></a>
```cs
var json = @"{
      'Name': 'James',
      'Password': 'Password1',
      '$type': 'MyNamespace.User, MyAssembly'
    }";

var o = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
{
    TypeNameHandling = TypeNameHandling.All,
    // $type no longer needs to be first
    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
});

var u = (User)o;

Console.WriteLine(u.Name);
// James
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/DeserializeMetadataPropertyHandling.cs#L37-L55' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializemetadatapropertyhandling' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
