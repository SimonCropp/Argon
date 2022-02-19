<?xml version="1.0" encoding="utf-8"?>
<topic id="SerializationAttributes" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">
Attributes can be used to control how Json.NET serializes and deserializes .NET objects.

<list class="bullet">
  <listItem><para>`Argon.JsonObjectAttribute` - Placed on classes to control how they should be serialized as a JSON object.</listItem>
  <listItem><para>`Argon.JsonArrayAttribute` - Placed on collections to control how they should be serialized as a JSON array.</listItem>
  <listItem><para>`Argon.JsonDictionaryAttribute` - Placed on dictionaries to control how they should be serialized as a JSON object.</listItem>
  <listItem><para>`Argon.JsonPropertyAttribute` - Placed on fields and properties to control how they should be serialized as a property in a JSON object.</listItem>
  <listItem><para>`Argon.JsonConverterAttribute` - Placed on either classes or fields and properties to specify which JsonConverter should be used during serialization.</listItem>
  <listItem><para>`Argon.JsonExtensionDataAttribute` - Placed on a collection field or property to deserialize properties with no matching class member into the specified collection and write values during serialization.</listItem>
  <listItem><para>`Argon.JsonConstructorAttribute` - Placed on a constructor to specify that it should be used to create the class during deserialization.</listItem>
</list>


    
    <section>
      <title>Standard .NET Serialization Attributes</title>
      <content>
        <para>As well as using the built-in Json.NET attributes, Json.NET also looks for the `System.SerializableAttribute`
        (if IgnoreSerializableAttribute on DefaultContractResolver is set to false)
        `System.Runtime.Serialization.DataContractAttribute`,
        `System.Runtime.Serialization.DataMemberAttribute`,
        and `System.NonSerializedAttribute` and attributes when determining how JSON is to be serialized and deserialized.
        </para>

<alert class="note">
  <para>Json.NET attributes take precedence over standard .NET serialization attributes (e.g. if both JsonPropertyAttribute
  and DataMemberAttribute are present on a property and both customize the name,
  the name from JsonPropertyAttribute will be used).
</alert>        

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="SerializationAttributes" title="Serialization Attributes Example" />
        
      </content>
    </section>
    
    <section>
      <title>Json.NET Serialization Attributes</title>
      <content>
        <autoOutline />
    </content>
        <sections>
    <section address="JsonObjectAttribute">
      <title>JsonObjectAttribute</title>
      <content>
        <para>The MemberSerialization flag on this attribute specifies whether member serialization is opt-in
        (a member must have the JsonProperty or DataMember attribute to be serialized), opt-out (everything is
        serialized by default but can be ignored with the JsonIgnoreAttribute, Json.NET's default behavior) or
        fields (all public and private fields are serialized and properties are ignored).
        <para>Placing the the `System.Runtime.Serialization.DataContractAttribute`
        on a type is another way to default member serialization to opt-in.
        <para>The NamingStrategy setting on this attributes can be set to a `Argon.Serialization.NamingStrategy`
        type that specifies how property names are serialized.
        <para>Json.NET serializes .NET classes that implement IEnumerable as a JSON array populated with the
        IEnumerable values. Placing the `Argon.JsonObjectAttribute`
        overrides this behavior and forces the serializer to serialize the class's fields and properties.
      </content>
    </section>
        
    <section address="JsonArrayAttributeJsonDictionaryAttribute">
      <title>JsonArrayAttribute/JsonDictionaryAttribute</title>
      <content>
        <para>The `Argon.JsonArrayAttribute` and
        `Argon.JsonDictionaryAttribute` are used to specify
        whether a class is serialized as that collection type.
        <para>The collection attributes have options to customize the JsonConverter, type name handling, and reference handling that are applied to collection items.
      </content>
    </section>
        
    <section address="JsonPropertyAttribute">
      <title>JsonPropertyAttribute</title>
      <content>
        <para>JsonPropertyAttribute has a number of uses:</para>
        
<list class="bullet">
  <listItem><para>By default, the JSON property will have the same name as the .NET property. This attribute allows the name to be customized.</listItem>
  <listItem><para>JsonPropertyAttribute indicates that a property should be serialized when member serialization is set to opt-in.</listItem>
  <listItem><para>It includes non-public properties in serialization and deserialization.</listItem>
  <listItem><para>It can be used to customize type name, reference, null, and default value handling for the property value.</listItem>
  <listItem><para>It can be used to customize the `Argon.Serialization.NamingStrategy` of the serialized property name.</listItem>
  <listItem><para>It can be used to customize the property's collection items JsonConverter, type name handling, and reference handling.</listItem>
</list>
        
        <para> The DataMemberAttribute can be used as a substitute for JsonPropertyAttribute.
        
      </content>
    </section>
        
    <section address="JsonIgnoreAttribute">
      <title>JsonIgnoreAttribute</title>
      <content>
        <para>Excludes a field or property from serialization.
        <para>The `System.NonSerializedAttribute` can be used as a substitute for JsonIgnoreAttribute.
      </content>
    </section>
        
    <section address="JsonConverterAttribute">
      <title>JsonConverterAttribute</title>
      <content>
        <para>The `Argon.JsonConverterAttribute` specifies which
        `Argon.JsonConverter` is used to convert an object.
        <para>The attribute can be placed on a class or a member. When placed on a class, the JsonConverter
        specified by the attribute will be the default way of serializing that class. When the attribute is
        on a field or property, then the specified JsonConverter will always be used to serialize that value.
        <para>The priority of which JsonConverter is used is member attribute, then class attribute, and finally
        any converters passed to the JsonSerializer.

<code JsonConverterAttributeProperty.cs" region="Types" title="JsonConverterAttribute Property Example" />
       
        <para>This example shows the JsonConverterAttribute being applied to a property.
                
        <para>To apply a JsonConverter to the items in a collection, use either `Argon.JsonArrayAttribute`,
        `Argon.JsonDictionaryAttribute` or
        `Argon.JsonPropertyAttribute`
        and set the ItemConverterType property to the converter type you want to use.
      </content>
    </section>
        
    <section address="JsonExtensionDataAttribute">
      <title>JsonExtensionDataAttribute</title>
      <content>
        <para>The `Argon.JsonExtensionDataAttribute` instructs the
        `Argon.JsonSerializer` to deserialize properties with no matching field or property
        on the type into the specified collection. During serialization the values in this collection are written back to the instance's JSON object.

<alert class="note">
  <para>All extension data values will be written during serialization, even if a property the same name has already been written.
</alert>        
        <para>This example shows the JsonExtensionDataAttribute being applied to a field, unmatched JSON properties being added to the field's collection during deserialization.
        <code DeserializeExtensionData.cs" region="Types" title="Types" />
        <code DeserializeExtensionData.cs" region="Usage" title="Usage" />
      </content>
    </section>
        
    <section address="JsonConstructorAttribute">
      <title>JsonConstructorAttribute</title>
      <content>
        <para>The `Argon.JsonConstructorAttribute` instructs the
        `Argon.JsonSerializer` to use a specific constructor when deserializing a class. It can be used to create a class using a parameterized constructor instead of the default constructor, or to pick which specific parameterized constructor to use if there are multiple.
        <code JsonConstructorAttribute.cs" region="Types" title="Types" />
        <code JsonConstructorAttribute.cs" region="Usage" title="Usage" />
      </content>
    </section>
    
    </sections>
    </section>


## Related Topics
      `Argon.JsonObjectAttribute`
      `Argon.JsonArrayAttribute`
      `Argon.JsonDictionaryAttribute`
      `Argon.JsonPropertyAttribute`
      `Argon.JsonConverterAttribute`
      `Argon.JsonExtensionDataAttribute`
      `Argon.JsonConstructorAttribute`