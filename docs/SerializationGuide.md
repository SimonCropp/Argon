<?xml version="1.0" encoding="utf-8"?>
<topic id="SerializationGuide" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">
The Json.NET serializer can serialize a wide variety of .NET objects. This guide looks at how it works, first at a high level and then in more detail.



    <section address="Summary">
      <title>Summary</title>
      <content>

        <para>At a high level, the Json.NET serializer will convert primitive .NET values into primitive JSON values,
        will convert .NET arrays and collections to JSON arrays, and will convert everything else to JSON objects.
        <para>Json.NET will throw an error if it encounters incorrect JSON when deserializing a value. For example, if
        the serializer encounters a JSON property with an array of values and the type of matching .NET property is not
        a collection, then an error will be thrown, and vice-versa.
      </content>
    </section>
    <section address="ComplexTypes">
      <title>Complex Types</title>
<content>
<table>
  <tableHeader>
    <row>
      <entry><para>.NET</para></entry>
      <entry><para>JSON</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>IList, IEnumerable, IList&lt;T&gt;, Array</legacyBold></para></entry>
    <entry><para>Array (properties on the collection will not be serialized)</para></entry>
  </row>
  <row>
    <entry><para><legacyBold>IDictionary, IDictionary&lt;TKey, TValue&gt;</legacyBold></para></entry>
    <entry><para>Object (dictionary name/values only, properties on the dictionary will not be serialized)</para></entry>
  </row>
  <row>
    <entry><para><legacyBold>Object (more detail below)</legacyBold></para></entry>
    <entry><para>Object</para></entry>
  </row>
</table>
</content>
    </section>
    <section address="PrimitiveTypes">
      <title>Primitive Types</title>
<content>
<table>
  <tableHeader>
    <row>
      <entry><para>.NET</para></entry>
      <entry><para>JSON</para></entry>
    </row>
  </tableHeader>
  <row>
    <entry><para><legacyBold>String</legacyBold></para></entry>
    <entry><para>String</para></entry>
  </row>
  <row>
    <entry><para><legacyBold>Byte</legacyBold></para>
<para><legacyBold>SByte</legacyBold></para>
<para><legacyBold>UInt16</legacyBold></para>
<para><legacyBold>Int16</legacyBold></para>
<para><legacyBold>UInt32</legacyBold></para>
<para><legacyBold>Int32</legacyBold></para>
<para><legacyBold>UInt64</legacyBold></para>
<para><legacyBold>Int64</legacyBold></para></entry>
    <entry><para>Integer</para></entry>
  </row>
  <row>
    <entry>
    <para><legacyBold>Float</legacyBold></para>
    <para><legacyBold>Double</legacyBold></para>
    <para><legacyBold>Decimal</legacyBold></para>
    </entry>
    <entry><para>Float</para></entry>
  </row>
  <row>
    <entry>
    <para><legacyBold>Enum</legacyBold></para>
    </entry>
    <entry><para>Integer (can be the enum value name with `Argon.Converters.StringEnumConverter`)</para></entry>
  </row>
  <row>
    <entry>
    <para><legacyBold>DateTime</legacyBold></para>
    </entry>
    <entry><para>String (<link xlink:href="DatesInJSON" />)</para></entry>
  </row>
  <row>
    <entry>
    <para><legacyBold>Byte[]</legacyBold></para>
    </entry>
    <entry><para>String (base 64 encoded)</para></entry>
  </row>
  <row>
    <entry>
    <para><legacyBold>Type</legacyBold></para>
    </entry>
    <entry><para>String (type name)</para></entry>
  </row>
  <row>
    <entry>
    <para><legacyBold>Guid</legacyBold></para>
    </entry>
    <entry><para>String</para></entry>
  </row>
  <row>
    <entry>
    <para><legacyBold>`System.ComponentModel.TypeConverter` (convertible to String)</legacyBold></para>
    </entry>
    <entry><para>String</para></entry>
  </row>
</table>
</content>
    </section>

    <section address="Breakdown">
      <title>Breakdown of Type Serialization</title>
      <content>
        <autoOutline />
		</content>
		<sections>
    <section address="Objects">
      <title>Objects</title>
      <content>

        <para>.NET types that don't fall into any other category listed below
        (i.e. aren't lists, dictionaries, dynamic, implement ISerializable, etc.)
        are serialized as JSON objects. You can also force a type to be serialized as a JSON object by placing the
        JsonObjectAttribute on the type.
        <para>By default a type's properties are serialized in opt-out mode. What that means is that all public fields and properties with
        getters are automatically serialized to JSON, and fields and properties that shouldn't be serialized are opted-out by placing
        JsonIgnoreAttribute on them. To serialize private members, the JsonPropertyAttribute can be placed on private fields and
        properties.
        <para>Types can also be serialized using opt-in mode. Only properties and fields that have a JsonPropertyAttribute
        or DataMemberAttribute on them will be serialized. Opt-in mode for an object is specified by placing the JsonObjectAttribute
        or DataContractAttribute on the type.
        <para>Finally, types can be serialized using a fields mode. All fields, both public and private, are serialized
        and properties are ignored. This can be specified by setting MemberSerialization.Fields on a type with the JsonObjectAttribute
        or by using the .NET `System.SerializableAttribute`
        and setting IgnoreSerializableAttribute on DefaultContractResolver to false.
      </content>
    </section>   

    <section address="Lists">
      <title>IEnumerable, Lists, and Arrays</title>
      <content>

        <para> .NET lists (types that inherit from IEnumerable) and .NET arrays are converted to JSON arrays. Because JSON
        arrays only support a range of values and not properties, any additional properties and fields declared on .NET
        collections are not serialized. In situations where a type implements IEnumerable but a JSON array is not wanted, then
        the JsonObjectAttribute can be placed on the type to force it to be serialized as a JSON object instead.
        <para>JsonArrayAttribute has options on it to customize the JsonConverter, type name handling, and reference handling
        that are applied to collection items.
        <para>Note that if TypeNameHandling or PreserveReferencesHandling has been enabled for JSON arrays on the serializer,
        then JSON arrays are wrapped in a containing object. The object will have the type name/reference properties and a
        $values property, which will have the collection data.
        <para>When deserializing, if a member is typed as the interface IList&lt;T&gt;, then it will be deserialized as a
        List&lt;T&gt;.
        <para>You can read more about serializing collections here: <link xlink:href="SerializingCollections" /></para>
      </content>
    </section>   

    <section address="Dictionarys">
      <title>Dictionaries and Hashtables</title>
      <content>

        <para>.NET dictionaries (types that inherit from IDictionary) are converted to JSON objects. Note that only the
        dictionary name/values will be written to the JSON object when serializing, and properties on the JSON object will
        be added to the dictionary's name/values when deserializing. Additional members on the .NET dictionary are ignored
        during serialization.
        <para>When serializing a dictionary, the keys of the dictionary are converted to strings and used as the
        JSON object property names. The string written for a key can be customized by either overriding
        `System.Object.ToString` for the key type or by implementing a
        `System.ComponentModel.TypeConverter`. A TypeConverter will also
        support converting a custom string back again when deserializing a dictionary.
        <para>JsonDictionaryAttribute has options on it to customize the JsonConverter, type name handling, and reference
        handling that are applied to collection items.
        <para>When deserializing, if a member is typed as the interface IDictionary&lt;TKey, TValue&gt; then it will be
        deserialized as a Dictionary&lt;TKey, TValue&gt;.
        <para>You can read more about serializing collections here: <link xlink:href="SerializingCollections" /></para>
      </content>
    </section>

    <section address="Untyped_Objects">
      <title>Untyped Objects</title>
      <content>

        <para>.NET properties on a class that don't specify a type (i.e. they are just `object`) are serialized as usual. When
        untyped properties are deserialized, the serializer has no way of knowing what type to create (unless type name handling
        is enabled and the JSON contains the type names).
        <para>For these untyped properties, the Json.NET serializer will read the JSON into LINQ to JSON objects and set them
        to the property. JObject will be created for JSON objects; JArray will be created for JSON arrays, and JValue will be
        created for primitive JSON values.
      </content>
    </section>

    <section address="Dynamic">
      <title>Dynamic</title>
      <content>

        <para>There are two different usages of `dynamic` (introduced in .NET 4) in .NET. The first are .NET properties with a
        type of dynamic. Dynamic properties behave like properties declared as object: any value can be assigned to it, but
        the difference being that properties and methods can be called on a dynamic property without casting. In Json.NET,
        dynamic properties are serialized and deserialized exactly the same as untyped objects: because dynamic isn't an actual
        type, Json.NET falls back to deserializing the JSON as LINQ to JSON objects.
        <para>The second usage of dynamic in .NET are by the types that implement
        `System.Dynamic.IDynamicMetaObjectProvider`. This interface lets
        the implementer create dynamic objects that intercept the property and method calls on an object and use them.
        `System.Dynamic.ExpandoObject`
        is a good example of a dynamic object.
        <para>Dynamic objects are serialized as JSON objects. A property is written for every member name returned by
        <codeEntityReference qualifyHint="true">M:System.Dynamic.DynamicMetaObject.GetDynamicMemberNames`.
        A dynamic object's normal properties aren't serialized by default but can be included by placing the
        JsonPropertyAttribute on them.
        </para>
        <para>When deserializing dynamic objects, the serializer first attempts to set JSON property values on a normal .NET
        member with the matching name. If no .NET member is found with the property name, then the serializer will call
        SetMember on the dynamic object. Because there is no type information for dynamic members on a dynamic object, the
        values assigned to them will be LINQ to JSON objects.
      </content>
    </section>

    <section address="ISerializable">
      <title>ISerializable</title>
      <content>

        <para>Types that implement `System.Runtime.Serialization.ISerializable` and
        are marked with `System.SerializableAttribute` are serialized as JSON objects. When serializing, only the values returned from
        ISerializable.GetObjectData are used; members on the type are ignored. When deserializing, the constructor with a
        SerializationInfo and StreamingContext is called, passing the JSON object's values.
        <para>In situations where this behavior is not wanted, the JsonObjectAttribute can be placed on a .NET type that
        implements ISerializable to force it to be serialized as a normal JSON object.
      </content>
    </section>

    <section address="LINQ">
      <title>LINQ to JSON</title>
      <content>

        <para>LINQ to JSON types (e.g. JObject and JArray) are automatically serialized and deserialized to their equivalent JSON
        when encountered by the Json.NET serializer.
      </content>
    </section>   

    <section address="JsonConverter">
      <title>JsonConverter</title>
      <content>

        <para>Serialization of values that are convertible by a `Argon.JsonConverter`
        (i.e. CanConvert returns true for its type) is
        completely overridden by the JsonConverter. The test to see whether a value can be converted by the JsonSerializer takes
        precedence over all other tests.
        <para>JsonConverters can be defined and specified in a number of places: in an attribute on a member, in an attribute
        on a class, and added to the JsonSerializer's converters collection. The priority of which JsonConverter is used is the
        JsonConverter defined by attribute on a member, then the JsonConverter defined by an attribute on a class, and finally
        any converters passed to the JsonSerializer.
        
<alert class="note">
  <para>Because a JsonConverter creates a new value, a converter will not work with readonly properties because there is no way to assign the new
  value to the property. Either change the property to have a public setter or place a JsonPropertyAttribute or DataMemberAttribute on the property.
</alert>

      </content>
    </section>
    
      </sections>
    </section>


## Related Topics
      <link xlink:href="SerializationSettings" />
      <link xlink:href="SerializationAttributes" />
      <link xlink:href="DatesInJSON" />

      `Argon.JsonSerializer`
      `Argon.JsonSerializerSettings`