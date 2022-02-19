# DataContract and DataMember Attributes

This sample shows how .NET Framework attributes such as `DataContractAttribute`, `DataMemberAttribute` and `NonSerializedAttribute` can be used with Json.NET instead of Json.NET's own attributes.

<!-- snippet: DataContractAndDataMemberTypes -->
<a id='snippet-datacontractanddatamembertypes'></a>
```cs
[DataContract]
public class File
{
    // excluded from serialization
    // does not have DataMemberAttribute
    public Guid Id { get; set; }

    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public int Size { get; set; }
}
```
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/DataContractAndDataMember.cs#L32-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-datacontractanddatamembertypes' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

<!-- snippet: DataContractAndDataMemberUsage -->
<a id='snippet-datacontractanddatamemberusage'></a>
```cs
var file = new File
{
    Id = Guid.NewGuid(),
    Name = "ImportantLegalDocuments.docx",
    Size = 50 * 1024
};

var json = JsonConvert.SerializeObject(file, Formatting.Indented);

Console.WriteLine(json);
// {
//   "Name": "ImportantLegalDocuments.docx",
//   "Size": 51200
// }
```
<sup><a href='/Src/Tests/Documentation/Samples/Serializer/DataContractAndDataMember.cs#L51-L66' title='Snippet source file'>snippet source</a> | <a href='#snippet-datacontractanddatamemberusage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
