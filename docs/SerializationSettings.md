<?xml version="1.0" encoding="utf-8"?>
<topic id="SerializationSettings" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">
JsonSerializer has a number of properties on it to customize how it serializes JSON. These can also be used
      with the methods on JsonConvert via the `Argon.JsonSerializerSettings` overloads.


<section address="DateFormatHandling">
 <title>DateFormatHandling</title>
 <content><para>`Argon.DateFormatHandling` controls how dates are serialized.
 
<table>
  <tableHeader>
    <row>
      <entry><para>Member</para></entry>
      <entry><para>Description</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>IsoDateFormat</legacyBold></para></entry>
    <entry><para>By default Json.NET writes dates in the ISO 8601 format, e.g. `"2012-03-21T05:40Z"`.</entry>
  </row>
  <row>
    <entry><para><legacyBold>MicrosoftDateFormat</legacyBold></para></entry>
    <entry><para>Dates are written in the Microsoft JSON format, e.g. `"\/Date(1198908717056)\/"`.</entry>
  </row>
</table>

</content>
</section>

<section address="MissingMemberHandling">
 <title>MissingMemberHandling</title>
 <content><para>`Argon.MissingMemberHandling` controls how missing members, e.g. JSON contains a property that isn't a member on the object, are handled during deserialization.
 
<table>
  <tableHeader>
    <row>
      <entry><para>Member</para></entry>
      <entry><para>Description</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>Ignore</legacyBold></para></entry>
    <entry><para>By default Json.NET ignores JSON if there is no field or property for its value to be set to during deserialization.</entry>
  </row>
  <row>
    <entry><para><legacyBold>Error</legacyBold></para></entry>
    <entry><para>Json.NET errors when there is a missing member during deserialization.</entry>
  </row>
</table>

</content>
</section>

<section address="ReferenceLoopHandling">
 <title>ReferenceLoopHandling</title>
 <content>
   <para>`Argon.ReferenceLoopHandling` controls how circular referencing objects,
   e.g. a Person object referencing itself via a Manager property, are serialized.
   <para>The `System.Object.Equals(System.Object)` method is used to test whether an object is in a circular reference.
   By default `Object.Equals(Object)` will test whether the references are equal for reference types and private and public values
   are equal for value types. Classes and structs can override this method. 
<table>
  <tableHeader>
    <row>
      <entry><para>Member</para></entry>
      <entry><para>Description</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>Error</legacyBold></para></entry>
    <entry><para>By default Json.NET will error if a reference loop is encountered (otherwise the serializer will get into an infinite loop).</entry>
  </row>
  <row>
    <entry><para><legacyBold>Ignore</legacyBold></para></entry>
    <entry><para>Json.NET will ignore objects in reference loops and not serialize them. The first time an object is encountered it will be serialized as usual but if the object is encountered as a child object of itself the serializer will skip serializing it.</entry>
  </row>
  <row>
    <entry><para><legacyBold>Serialize</legacyBold></para></entry>
    <entry><para>This option forces Json.NET to serialize objects in reference loops. This is useful if objects are nested but not indefinitely.</entry>
  </row>
</table>
<para>ReferenceLoopHandling can be used as an argument when calling the serializer, it can be set on an object's properties or
a collection's items using `Argon.JsonContainerAttribute.ItemReferenceLoopHandling`,
customized on a property with `Argon.JsonPropertyAttribute.ReferenceLoopHandling`
or a property's object properties or collection items using
`Argon.JsonPropertyAttribute.ItemReferenceLoopHandling`.
</content>
</section>

<section address="NullValueHandling">
 <title>NullValueHandling</title>
 <content><para>`Argon.NullValueHandling` controls how null values on .NET objects are handled during serialization and how null values in JSON are handled during deserialization.
 
<table>
  <tableHeader>
    <row>
      <entry><para>Member</para></entry>
      <entry><para>Description</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>Include</legacyBold></para></entry>
    <entry><para>By default Json.NET writes null values to JSON when serializing and sets null values to fields/properties when deserializing.</entry>
  </row>
  <row>
    <entry><para><legacyBold>Ignore</legacyBold></para></entry>
    <entry><para>Json.NET will skip writing JSON properties if the .NET value is null when serializing and will skip setting fields/properties if the JSON property is null when deserializing.</entry>
  </row>
</table>
<para>NullValueHandling can also be customized on individual properties with JsonPropertyAttribute.

</content>
</section>

<section address="DefaultValueHandling">
 <title>DefaultValueHandling</title>
 <content><para>`Argon.DefaultValueHandling` controls how Json.NET uses default values set using the .NET `System.ComponentModel.DefaultValueAttribute` when serializing and deserializing.
 
<table>
  <tableHeader>
    <row>
      <entry><para>Member</para></entry>
      <entry><para>Description</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>Include</legacyBold></para></entry>
    <entry><para>By default Json.NET will write a field/property value to JSON when serializing if the value is the same as the field/property's default value.
    The Json.NET deserializer will continue setting a field/property if the JSON value is the same as the default value.</entry>
  </row>
  <row>
    <entry><para><legacyBold>Ignore</legacyBold></para></entry>
    <entry><para>Json.NET will skip writing a field/property value to JSON if the value is the same as the field/property's default value, or the custom
    value specified in `System.ComponentModel.DefaultValueAttribute` if the attribute is present. The Json.NET deserializer
    will skip setting a .NET object's field/property if the JSON value is the same as the default value.</entry>
  </row>
</table>
<para>DefaultValueHandling can also be customized on individual properties with JsonPropertyAttribute.

</content>
</section>

<section address="ObjectCreationHandling">
 <title>ObjectCreationHandling</title>
 <content><para>`Argon.ObjectCreationHandling` controls how objects are created and deserialized to during deserialization.
 
<table>
  <tableHeader>
    <row>
      <entry><para>Member</para></entry>
      <entry><para>Description</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>Auto</legacyBold></para></entry>
    <entry><para>By default Json.NET will attempt to set JSON values onto existing objects and add JSON values to existing collections during deserialization. </para></entry>
  </row>
  <row>
    <entry><para><legacyBold>Reuse</legacyBold></para></entry>
    <entry><para>Same behaviour as auto.</entry>
  </row>
  <row>
    <entry><para><legacyBold>Replace</legacyBold></para></entry>
    <entry><para>Json.NET will always recreate objects and collections before setting values to them during deserialization.</entry>
  </row>
</table>
<para>ObjectCreationHandling can also be customized on individual properties with JsonPropertyAttribute.

</content>
</section>

<section address="TypeNameHandling">
 <title>TypeNameHandling</title>
 <content>
 <alert class="caution">
  <para>
 `Argon.TypeNameHandling` should be used with caution when your application deserializes JSON from an external source.
  </para>
  <para>
    Incoming types should be validated with a custom `Argon.Serialization.ISerializationBinder` when deserializing with a value other than `TypeNameHandling.None`.
  </para>
 </alert>
 
 <para>
   `Argon.TypeNameHandling` controls whether Json.NET includes .NET type names
   during serialization with a `$type` property and reads .NET type names from that property to determine what type to create during
   deserialization.
 </para>
 <para>
   Metadata properties like `$type` must be located at the beginning of a JSON object to be successfully detected during deserialization. If you can't control
   the order of properties in your JSON object then `Argon.MetadataPropertyHandling` can be used to remove this restriction.
 </para>
 <para>
   The value of the `$type` property can be customized and validated by creating your own 
   `Argon.Serialization.ISerializationBinder`.
 </para>

<table>
  <tableHeader>
    <row>
      <entry><para>Member</para></entry>
      <entry><para>Description</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>None</legacyBold></para></entry>
    <entry><para>By default Json.NET does not read or write type names during deserialization.</entry>
  </row>
  <row>
    <entry><para><legacyBold>Objects</legacyBold></para></entry>
    <entry><para>Json.NET will write and use type names for objects but not collections.</entry>
  </row>
  <row>
    <entry><para><legacyBold>Arrays</legacyBold></para></entry>
    <entry><para>Json.NET will write and use type names for collections but not objects.</entry>
  </row>
  <row>
    <entry><para><legacyBold>Auto</legacyBold></para></entry>
    <entry><para>Json.NET will check whether an object/collection matches its declared property and writes the type name if they do not match, e.g. a property with a type of Mammal has a derived instance of Dog assigned. Auto will ensure that type information isn't lost when serializing/deserializing automatically without having to write type names for every object.</entry>
  </row>
  <row>
    <entry><para><legacyBold>All</legacyBold></para></entry>
    <entry><para>Json.NET will write and use type names for objects and collections.</entry>
  </row>
</table>
<para>TypeNameHandling can be used as an argument when calling the serializer, it can be set on an object's properties or
a collection's items using `Argon.JsonContainerAttribute.ItemTypeNameHandling`,
customized on a property with `Argon.JsonPropertyAttribute.TypeNameHandling`
or a property's object properties or collection items using
`Argon.JsonPropertyAttribute.ItemTypeNameHandling`.
</content>
</section>

<section address="TypeNameAssemblyFormat">
 <title>TypeNameAssemblyFormat</title>
 <content><para>`System.Runtime.Serialization.Formatters.FormatterAssemblyStyle` controls how type names are written during serialization.
 
<table>
  <tableHeader>
    <row>
      <entry><para>Member</para></entry>
      <entry><para>Description</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>Simple</legacyBold></para></entry>
    <entry><para>By default Json.NET writes the partial assembly name with the type, e.g. System.Data.DataSet, System.Data. Note that Silverlight and Windows Phone are not able to use this format.</entry>
  </row>
  <row>
    <entry><para><legacyBold>Full</legacyBold></para></entry>
    <entry><para>Json.NET will write the full assembly name, including version number, culture and public key token.</entry>
  </row>
</table>
<para>Read more about the valid values at `System.Runtime.Serialization.Formatters.FormatterAssemblyStyle`.

</content>
</section>

<section address="SerializationBinder">
 <title>SerializationBinder</title>
 <content><para>The `Argon.Serialization.ISerializationBinder` is used to resolve .NET types to type names during serialization and type names to .NET types during deserialization.
 <para>
 If TypeNameHandling is enabled then it is strongly recommended that a custom `Argon.Serialization.ISerializationBinder` is used to validate incoming type names for security reasons.
</content>
</section>

<section address="MetadataPropertyHandling">
 <title>MetadataPropertyHandling</title>
 <content><para>`Argon.MetadataPropertyHandling` controls how metadata properties like `$type` and `$id` are read during deserialization.
 <para>
  For performance reasons by default the JsonSerializer assumes that any metadata properties are located at the beginning of a JSON object.
  If you are unable to guarantee the order of properties in JSON you are deserializing then `MetadataPropertyHandling.ReadAhead` removes this
  restriction at the cost of some performance.
 </para>
 
<table>
  <tableHeader>
    <row>
      <entry><para>Member</para></entry>
      <entry><para>Description</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>Default</legacyBold></para></entry>
    <entry><para>By default Json.NET will only read metadata properties that are at the beginning of a JSON object.</entry>
  </row>
  <row>
    <entry><para><legacyBold>ReadAhead</legacyBold></para></entry>
    <entry><para>Json.NET will look for metadata properties located anywhere in a JSON object.</entry>
  </row>
  <row>
    <entry><para><legacyBold>Ignore</legacyBold></para></entry>
    <entry><para>Json.NET will ignore metadata properties.</entry>
  </row>
</table>

</content>
</section>

<section address="ConstructorHandling">
 <title>ConstructorHandling</title>
 <content><para>`Argon.ConstructorHandling` controls how constructors are used when initializing objects during deserialization.
 
<table>
  <tableHeader>
    <row>
      <entry><para>Member</para></entry>
      <entry><para>Description</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>Default</legacyBold></para></entry>
    <entry><para>By default Json.NET will first look for a constructor marked with the JsonConstructorAttribute, then look for a public default constructor
    (a constructor that doesn't take any arguments), then check if the class has a single public constructor with arguments and finally check for a
    non-public default constructor. If the class has multiple public constructors with arguments an error will be thrown. This can be fixed by
    marking one of the constructors with the JsonConstructorAttribute.</entry>
  </row>
  <row>
    <entry><para><legacyBold>AllowNonPublicDefaultConstructor</legacyBold></para></entry>
    <entry><para>Json.NET will use a classes private default constructor before constructors with arguments if available. </para></entry>
  </row>
</table>

</content>
</section>

<section address="Converters">
 <title>Converters</title>
 <content><para>This is the collection of JsonConverters that will be used during serialization and deserialization.
 <para>A `Argon.JsonConverter` allows JSON to be manually written during serialization and read during deserialization.
 This is useful for particularly complex JSON structures or for when you want to change how a type is serialized.
 <para>When a JsonConverter has been added to a JsonSerializer it will be checked for every value that is being serialized/deserialized
 using its CanConvert to see if it should be used. If CanConvert returns true then the JsonConverter will be used to read or write
 the JSON for that value. Note that while a JsonConverter gives you complete control over that values JSON, many Json.NET serialization
 features are no longer available like type name and reference handling.

<para>JsonConverters can be used as an argument when calling the serializer, it can be set on an object or property
using `Argon.JsonConverterAttribute`, it be set on an object's properties or
a collection's items using `Argon.JsonContainerAttribute.ItemConverterType`,
or a property's object properties or collection items using
`Argon.JsonPropertyAttribute.ItemConverterType`.

 <para>To create your own custom converter inherit from the JsonConverter class. Read more about the built-in JsonConverters below:</para>
<list class="bullet">
  <listItem><para><link xlink:href="DatesInJSON" /></para></listItem>
  <listItem><para><link xlink:href="ConvertingJSONandXML" /></para></listItem>
  <listItem><para><link xlink:href="CustomCreationConverter" /></para></listItem>
  <listItem><para>`Argon.Converters.StringEnumConverter`</para></listItem>
</list>


</content>
</section>

<section address="ContractResolver">
 <title>ContractResolver</title>
 <content><para>Internally for every .NET type the JsonSerializer will create a contract of how the type should be serialized and deserialized,
 based on type metadata and attributes applied to the class. Specifying a custom `Argon.Serialization.IContractResolver`
 allows the creation of contracts to be customized.
 <para>Read more about Contract Resolvers here: <link xlink:href="ContractResolver" /></para>
</content>
</section>

<section address="TraceWriter">
 <title>TraceWriter</title>
 <content><para>The Json.NET serializer supports logging and debugging using the
      `Argon.Serialization.ITraceWriter` interface.
      By assigning a trace writer you can debug what happens inside the Json.NET serializer when serializing and deserializing JSON.
 <para>Read more about TraceWriters here: <link xlink:href="SerializationTracing" /></para>
</content>
</section>

<section address="Error">
 <title>Error</title>
 <content><para>The `Argon.JsonSerializer.Error` event can catch errors during serialization and either handle the event and continue with
 serialization or let the error bubble up and be thrown to the application.
 <para>Read more about error handling here: <link xlink:href="SerializationErrorHandling" /></para>
</content>
</section>


## Related Topics
      <link xlink:href="SerializationGuide" />
      <link xlink:href="SerializationAttributes" />
      <link xlink:href="DatesInJSON" />
      
      `Argon.JsonSerializer`
      `Argon.JsonSerializerSettings`
      `Argon.JsonConverter`
      `System.Runtime.Serialization.SerializationBinder`