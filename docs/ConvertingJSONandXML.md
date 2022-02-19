<?xml version="1.0" encoding="utf-8"?>
<topic id="ConvertingJSONandXML" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">Json.NET supports converting JSON to XML and vice versa using the
      `Argon.Converters.XmlNodeConverter`.
      <para>Elements, attributes, text, comments, character data, processing instructions,
      namespaces, and the XML declaration are all preserved when converting between the two. The
      only caveat is that it is possible to lose the order of differently named nodes at the
      same level when they are grouped together into an array.

    <section>
      <title>Conversion Rules</title>
      <content>
   
<list class="bullet">
  <listItem><para>Elements remain unchanged.</listItem>
  <listItem><para>Attributes are prefixed with an @ and should be at the start of the object.</listItem>
  <listItem><para>Single child text nodes are a value directly against an element, otherwise they are accessed via #text.</listItem>
  <listItem><para>The XML declaration and processing instructions are prefixed with ?.</listItem>
  <listItem><para>Character data, comments, whitespace and significant whitespace nodes are accessed via
  #cdata-section, #comment, #whitespace and #significant-whitespace respectively.</listItem>
  <listItem><para>Multiple nodes with the same name at the same level are grouped together into an array.</listItem>
  <listItem><para>Empty elements are null.</listItem>
</list>

<para>If the XML created from JSON doesn't match, then convert it manually.
The best approach to do this is to load the JSON into a LINQ to JSON object like JObject or JArray and then use LINQ to create
an XDocument. The opposite process, using LINQ with an XDocument to create a JObject or JArray, also works.
More about using LINQ to JSON with LINQ <link xlink:href="QueryingLINQtoJSON">here</link>.

<alert class="note">
  <para>The version of Json.NET being used in the application will change what XML conversion methods are available.
  SerializeXmlNode/DeserializeXmlNode are available when the framework supports XmlDocument;
  SerializeXNode/DeserializeXNode are available when the framework supports XDocument.
</alert>

      </content>
    </section>
    <section>
      <title>SerializeXmlNode</title>
      <content>

The JsonConvert has two helper methods for converting between JSON and XML. The first is `Argon.JsonConvert.SerializeXmlNode`. This method takes an XmlNode and serializes it to JSON text.

<code lang="cs" source="..\Src\Tests\Documentation\ConvertingJsonAndXmlTests.cs" region="SerializeXmlNode" title="Converting XML to JSON with SerializeXmlNode" />

Because multiple nodes with the same name at the same level are grouped together into an array, the conversion process can produce different JSON depending on the number of nodes. For example, if some XML for a user has a single `<Role>` node, then that role will be text against a JSON `"Role"` property, but if the user has multiple `<Role>` nodes, then the role values will be placed in a JSON array.
       
       <para>To fix this situation a custom XML attribute can be added to force a JSON array to be created.

<code lang="cs" source="..\Src\Tests\Documentation\ConvertingJsonAndXmlTests.cs" region="ForceJsonArray" title="Attribute to Force a JSON Array" />
    </content>
    </section>
    <section>
      <title>DeserializeXmlNode</title>
      <content>
   
       <para>The second helper method on JsonConvert is
       `Argon.JsonConvert.DeserializeXmlNode`.
       This method takes JSON text and deserializes it into an XmlNode.
   
       <para>Because valid XML must have one root element, the JSON passed to DeserializeXmlNode should
       have one property in the root JSON object. If the root JSON object has multiple properties, then
       the overload that also takes an element name should be used. A root element with that name will
       be inserted into the deserialized XmlNode.

<code lang="cs" source="..\Src\Tests\Documentation\ConvertingJsonAndXmlTests.cs" region="DeserializeXmlNode" title="Converting JSON to XML with DeserializeXmlNode" />

      </content>
    </section>


## Related Topics
      `Argon.Converters.XmlNodeConverter`
      `Argon.JsonConvert`