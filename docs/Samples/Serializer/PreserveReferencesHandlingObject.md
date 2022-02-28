# PreserveReferencesHandling setting

This sample shows how the `Argon.PreserveReferencesHandling` setting can be used to serialize values by reference instead of by value.

<!-- snippet: PreserveReferencesHandlingObjectTypes -->
<a id='snippet-preservereferenceshandlingobjecttypes'></a>
```cs
public class Directory
{
    public string Name { get; set; }
    public Directory Parent { get; set; }
    public IList<File> Files { get; set; }
}

public class File
{
    public string Name { get; set; }
    public Directory Parent { get; set; }
}
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/PreserveReferencesHandlingObject.cs#L7-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-preservereferenceshandlingobjecttypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: PreserveReferencesHandlingObjectUsage -->
<a id='snippet-preservereferenceshandlingobjectusage'></a>
```cs
var root = new Directory { Name = "Root" };
var documents = new Directory { Name = "My Documents", Parent = root };

var file = new File { Name = "ImportantLegalDocument.docx", Parent = documents };

documents.Files = new List<File> { file };

try
{
    JsonConvert.SerializeObject(documents, Formatting.Indented);
}
catch (JsonSerializationException)
{
    // Self referencing loop detected for property 'Parent' with type
    // 'Argon.Tests.Documentation.Examples.ReferenceLoopHandlingObject+Directory'. Path 'Files[0]'.
}

var preserveReferenacesAll = JsonConvert.SerializeObject(documents, Formatting.Indented, new JsonSerializerSettings
{
    PreserveReferencesHandling = PreserveReferencesHandling.All
});

Console.WriteLine(preserveReferenacesAll);
// {
//   "$id": "1",
//   "Name": "My Documents",
//   "Parent": {
//     "$id": "2",
//     "Name": "Root",
//     "Parent": null,
//     "Files": null
//   },
//   "Files": {
//     "$id": "3",
//     "$values": [
//       {
//         "$id": "4",
//         "Name": "ImportantLegalDocument.docx",
//         "Parent": {
//           "$ref": "1"
//         }
//       }
//     ]
//   }
// }

var preserveReferenacesObjects = JsonConvert.SerializeObject(documents, Formatting.Indented, new JsonSerializerSettings
{
    PreserveReferencesHandling = PreserveReferencesHandling.Objects
});

Console.WriteLine(preserveReferenacesObjects);
// {
//   "$id": "1",
//   "Name": "My Documents",
//   "Parent": {
//     "$id": "2",
//     "Name": "Root",
//     "Parent": null,
//     "Files": null
//   },
//   "Files": [
//     {
//       "$id": "3",
//       "Name": "ImportantLegalDocument.docx",
//       "Parent": {
//         "$ref": "1"
//       }
//     }
//   ]
// }
```
<sup><a href='/src/Tests/Documentation/Samples/Serializer/PreserveReferencesHandlingObject.cs#L25-L97' title='Snippet source file'>snippet source</a> | <a href='#snippet-preservereferenceshandlingobjectusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
