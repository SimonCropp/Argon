<?xml version="1.0" encoding="utf-8"?>
<topic id="SerializingJSON" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">
    <!--
    <summary>
      <para>Optional summary abstract</para>
    </summary>
    -->
    <introduction>
      <para>The quickest method of converting between JSON text and a .NET object is using the
      <codeEntityReference>T:Argon.JsonSerializer</codeEntityReference>.
      The JsonSerializer converts .NET objects into their JSON equivalent and back again by mapping the .NET object property names to the JSON property names
      and copies the values for you.</para>
      <autoOutline lead="none" excludeRelatedTopics="true" />
    </introduction>
    <!-- Optional procedures followed by optional code example but must have
         at least one procedure or code example -->
<section address="JsonConvert">
 <title>JsonConvert</title>
 <content><para>For simple scenarios where you want to convert to and from a JSON string, the
 <codeEntityReference>Overload:Argon.JsonConvert.SerializeObject</codeEntityReference> and
 <codeEntityReference>Overload:Argon.JsonConvert.DeserializeObject</codeEntityReference>
 methods on JsonConvert provide an easy-to-use wrapper over JsonSerializer.</para>
 <code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="SerializeObject" title="Serializing and Deserializing JSON with JsonConvert" />
<para>
	SerializeObject and DeserializeObject both have overloads that take a <codeEntityReference>T:Argon.JsonSerializerSettings</codeEntityReference> object.
	JsonSerializerSettings lets you use many of the JsonSerializer settings listed below while still using the simple serialization methods.
</para>
</content>
</section>

<section address="JsonSerializer">
 <title>JsonSerializer</title>
 <content><para>For more control over how an object is serialized, the <codeEntityReference>T:Argon.JsonSerializer</codeEntityReference> can be used directly.
The JsonSerializer is able to read and write JSON text directly to a stream via <codeEntityReference>T:Argon.JsonTextReader</codeEntityReference>
and <codeEntityReference>T:Argon.JsonTextWriter</codeEntityReference>.
Other kinds of JsonWriters can also be used, such as
<codeEntityReference>T:Argon.Linq.JTokenReader</codeEntityReference>/<codeEntityReference>T:Argon.Linq.JTokenWriter</codeEntityReference>,
to convert your object to and from LINQ to JSON objects, or
<codeEntityReference>T:Argon.Bson.BsonReader</codeEntityReference>/<codeEntityReference>T:Argon.Bson.BsonWriter</codeEntityReference>, to convert to and from BSON.</para>

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="JsonSerializerToStream" title="Serializing JSON to a Stream with JsonSerializer" />

<para>JsonSerializer has a number of properties on it to customize how it serializes JSON. These can also be used with the methods on JsonConvert via the JsonSerializerSettings overloads.</para>
<para>You can read more about the available JsonSerializer settings here: <link xlink:href="SerializationSettings" /></para>
</content>
</section>
    <relatedTopics>
      <link xlink:href="SerializationGuide" />
      <link xlink:href="SerializationSettings" />
      <link xlink:href="SerializationAttributes" />
      <link xlink:href="SerializingJSONFragments" />

      <codeEntityReference>T:Argon.JsonConvert</codeEntityReference>
      <codeEntityReference>T:Argon.JsonSerializer</codeEntityReference>
      <codeEntityReference>T:Argon.JsonSerializerSettings</codeEntityReference>
    </relatedTopics>
  </developerConceptualDocument>
</topic>
