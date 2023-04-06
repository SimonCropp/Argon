# Convert JSON to XML

This sample converts JSON to XML.

<!-- snippet: ConvertJsonToXml -->
<a id='snippet-convertjsontoxml'></a>
```cs
var json = """
    {
      '@Id': 1,
      'Email': 'james@example.com',
      'Active': true,
      'CreatedDate': '2013-01-20T00:00:00Z',
      'Roles': [
        'User',
        'Admin'
      ],
      'Team': {
        '@Id': 2,
        'Name': 'Software Developers',
        'Description': 'Creators of fine software products and services.'
      }
    }
    """;

XNode node = JsonXmlConvert.DeserializeXNode(json, "Root");

Console.WriteLine(node.ToString());
// <Root Id="1">
//   <Email>james@example.com</Email>
//   <Active>true</Active>
//   <CreatedDate>2013-01-20T00:00:00Z</CreatedDate>
//   <Roles>User</Roles>
//   <Roles>Admin</Roles>
//   <Team Id="2">
//     <Name>Software Developers</Name>
//     <Description>Creators of fine software products and services.</Description>
//   </Team>
// </Root>
```
<sup><a href='/src/ArgonTests/Documentation/Samples/Xml/ConvertJsonToXml.cs#L12-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-convertjsontoxml' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
