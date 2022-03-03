# Converting between JSON and XML

Json.NET supports converting JSON to XML and vice versa using the `Argon.Converters.XmlNodeConverter`.

Elements, attributes, text, comments, character data, processing instructions, namespaces, and the XML declaration are all preserved when converting between the two. The only caveat is that it is possible to lose the order of differently named nodes at the same level when they are grouped together into an array.


## Conversion Rules

 * Elements remain unchanged.
 * Attributes are prefixed with an @ and should be at the start of the object.
 * Single child text nodes are a value directly against an element, otherwise they are accessed via #text.
 * The XML declaration and processing instructions are prefixed with ?.
 * Character data, comments, whitespace and significant whitespace nodes are accessed via
  #cdata-section, #comment, #whitespace and #significant-whitespace respectively.
 * Multiple nodes with the same name at the same level are grouped together into an array.
 * Empty elements are null.

If the XML created from JSON doesn't match, then convert it manually. The best approach to do this is to load the JSON into a LINQ to JSON object like JObject or JArray and then use LINQ to create an XDocument. The opposite process, using LINQ with an XDocument to create a JObject or JArray, also works. More about using LINQ to JSON with LINQ [QueryingLINQtoJSON].

The version of Json.NET being used in the application will change what XML conversion methods are available. SerializeXmlNode/DeserializeXmlNode are available when the framework supports XmlDocument; SerializeXNode/DeserializeXNode are available when the framework supports XDocument.


## SerializeXmlNode

The JsonConvert has two helper methods for converting between JSON and XML. The first is `Argon.JsonConvert.SerializeXmlNode`. This method takes an XmlNode and serializes it to JSON text.

<!-- snippet: SerializeXmlNode -->
<a id='snippet-serializexmlnode'></a>
```cs
var xml = @"<?xml version='1.0' standalone='no'?>
        <root>
          <person id='1'>
            <name>Alan</name>
            <url>http://www.google.com</url>
          </person>
          <person id='2'>
            <name>Louis</name>
            <url>http://www.yahoo.com</url>
          </person>
        </root>";

var doc = new XmlDocument();
doc.LoadXml(xml);

var jsonText = JsonXmlConvert.SerializeXmlNode(doc);
//{
//  "?xml": {
//    "@version": "1.0",
//    "@standalone": "no"
//  },
//  "root": {
//    "person": [
//      {
//        "@id": "1",
//        "name": "Alan",
//        "url": "http://www.google.com"
//      },
//      {
//        "@id": "2",
//        "name": "Louis",
//        "url": "http://www.yahoo.com"
//      }
//    ]
//  }
//}
```
<sup><a href='/src/Tests/Documentation/ConvertingJsonAndXmlTests.cs#L14-L51' title='Snippet source file'>snippet source</a> | <a href='#snippet-serializexmlnode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Because multiple nodes with the same name at the same level are grouped together into an array, the conversion process can produce different JSON depending on the number of nodes. For example, if some XML for a user has a single `<Role>` node, then that role will be text against a JSON `"Role"` property, but if the user has multiple `<Role>` nodes, then the role values will be placed in a JSON array.

To fix this situation a custom XML attribute can be added to force a JSON array to be created.

<!-- snippet: ForceJsonArray -->
<a id='snippet-forcejsonarray'></a>
```cs
var xml = @"<person id='1'>
			  <name>Alan</name>
			  <url>http://www.google.com</url>
			  <role>Admin1</role>
			</person>";

var doc = new XmlDocument();
doc.LoadXml(xml);

var json = JsonXmlConvert.SerializeXmlNode(doc);
//{
//  "person": {
//    "@id": "1",
//    "name": "Alan",
//    "url": "http://www.google.com",
//    "role": "Admin1"
//  }
//}

xml = @"<person xmlns:json='http://james.newtonking.com/projects/json' id='1'>
			  <name>Alan</name>
			  <url>http://www.google.com</url>
			  <role json:Array='true'>Admin</role>
			</person>";

doc = new();
doc.LoadXml(xml);

json = JsonXmlConvert.SerializeXmlNode(doc);
//{
//  "person": {
//    "@id": "1",
//    "name": "Alan",
//    "url": "http://www.google.com",
//    "role": [
//      "Admin"
//    ]
//  }
//}
```
<sup><a href='/src/Tests/Documentation/ConvertingJsonAndXmlTests.cs#L97-L137' title='Snippet source file'>snippet source</a> | <a href='#snippet-forcejsonarray' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## DeserializeXmlNode

The second helper method on JsonConvert is `Argon.JsonConvert.DeserializeXmlNode`. This method takes JSON text and deserializes it into an XmlNode.

Because valid XML must have one root element, the JSON passed to DeserializeXmlNode should have one property in the root JSON object. If the root JSON object has multiple properties, then the overload that also takes an element name should be used. A root element with that name will be inserted into the deserialized XmlNode.

<!-- snippet: DeserializeXmlNode -->
<a id='snippet-deserializexmlnode'></a>
```cs
var json = @"{
          '?xml': {
            '@version': '1.0',
            '@standalone': 'no'
          },
          'root': {
            'person': [
              {
                '@id': '1',
                'name': 'Alan',
                'url': 'http://www.google.com'
              },
              {
                '@id': '2',
                'name': 'Louis',
                'url': 'http://www.yahoo.com'
              }
            ]
          }
        }";

var doc = JsonXmlConvert.DeserializeXmlNode(json);
// <?xml version="1.0" standalone="no"?>
// <root>
//   <person id="1">
//     <name>Alan</name>
//     <url>http://www.google.com</url>
//   </person>
//   <person id="2">
//     <name>Louis</name>
//     <url>http://www.yahoo.com</url>
//   </person>
// </root>
```
<sup><a href='/src/Tests/Documentation/ConvertingJsonAndXmlTests.cs#L57-L91' title='Snippet source file'>snippet source</a> | <a href='#snippet-deserializexmlnode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Related Topics

 * `Argon.Converters.XmlNodeConverter`
 * `Argon.JsonConvert`
