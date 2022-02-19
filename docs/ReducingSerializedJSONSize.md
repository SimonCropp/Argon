<?xml version="1.0" encoding="utf-8"?>
<topic id="ReducingSerializedJSONSize" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">

    <introduction>


      <para>One of the common problems encountered when serializing .NET objects to
      JSON is that the JSON ends up containing a lot of unwanted properties and values.
      This can be especially significant when returning JSON to the client. More JSON
      means more bandwidth and a slower website.
      <para>To solve the issue of unwanted JSON, Json.NET has a range of built-in
      options to fine-tune what gets written from a serialized object.



    <section address="JsonIgnoreAttributeAndDataMemberAttribute">
      <title>JsonIgnoreAttribute and DataMemberAttribute</title>
      <content>

        <para>By default Json.NET will include all of a class's public properties and fields
        in the JSON it creates. Adding the
        `Argon.JsonIgnoreAttribute`
        to a property tells the serializer to always skip writing it to the JSON result.

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeOptOut" title="Opt-out Serialization Example" />
        
        <para>If a class has many properties and you only want to serialize a small subset
        of them, then adding JsonIgnore to all the others will be tedious and error prone.
        The way to tackle this scenario is to add the
        `System.Runtime.Serialization.DataContractAttribute`
        to the class and
        `System.Runtime.Serialization.DataMemberAttribute`
        to the properties to serialize. This is opt-in
        serialization - only the properties you mark up will be serialized, unlike
        opt-out serialization using JsonIgnoreAttribute.

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeOptIn" title="Opt-in Serialization Example" />
      </content>
    </section>
    <section address="Formatting">
      <title>Formatting</title>
      <content>
        <para>JSON written by the serializer with an option of
        `Argon.Formatting`
        set to Indented produces
        nicely formatted, easy-to-read JSON that is great for readability when you are
        developing. `Formatting.None` on the other hand keeps the JSON result small, skipping
        all unnecessary spaces and line breaks to produce the most compact and efficient
        JSON possible.
      </content>
    </section>
    <section address="NullValueHandling">
      <title>NullValueHandling</title>
      <content>
        <para>`Argon.NullValueHandling`
        is an option on the JsonSerializer and controls how the
        serializer handles properties with a null value. By setting a value of
        NullValueHandling.Ignore the JsonSerializer skips writing any properties that have
        a value of null.

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeNullValueHandlingObject" title="NullValueHandling Class" />
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeNullValueHandlingExample" title="NullValueHandling Ignore Example" />

        <para>NullValueHandling can also be customized on individual properties
        using the
        `Argon.JsonPropertyAttribute`.
        The JsonPropertyAttribute value of
        NullValueHandling will override the setting on the JsonSerializer for that
        property.
      </content>
    </section>
    <section address="DefaultValueHandling">
      <title>DefaultValueHandling</title>
      <content>
        <para>`Argon.DefaultValueHandling`
        is an option on the JsonSerializer and controls how the serializer handles
        properties with a default value. Setting a value of DefaultValueHandling.Ignore
        will make the JsonSerializer skip writing any properties that have a default
        value to the JSON result. For object references this will be null. For value
        types like int and DateTime the serializer will skip the default uninitialized
        value for that value type.

        <para>Json.NET also allows you to customize what the default value of an individual
        property is using the
        `System.ComponentModel.DefaultValueAttribute`.
        For example, if a string property called
        Department always returns an empty string in its default state and you don't want
        that empty string in your JSON, then placing the DefaultValueAttribute on Department
        with that value will mean Department is no longer written to JSON unless it has a
        value.
        
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeDefaultValueHandlingObject" title="DefaultValueHandling Class" />
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeDefaultValueHandlingExample" title="DefaultValueHandling Ignore Example" />
        
        <para>DefaultValueHandling can also be customized on individual properties using
        the `Argon.JsonPropertyAttribute`.
        The JsonPropertyAttribute value of DefaultValueHandling
        will override the setting on the JsonSerializer for that property.
      </content>
    </section>
    <section address="IContractResolver">
      <title>IContractResolver</title>
      <content>
        <para>For more flexibility, the
        `Argon.Serialization.IContractResolver`
        provides an interface to customize
        almost every aspect of how a .NET object gets serialized to JSON, including changing
        serialization behavior at runtime.

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeContractResolverObject" title="IContractResolver Class" />
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="ReducingSerializedJsonSizeContractResolverExample" title="IContractResolver Example" />
        
      </content>
    </section>


## Related Topics
      `Argon.Formatting`
      `Argon.JsonIgnoreAttribute`
      `Argon.DefaultValueHandling`
      `Argon.NullValueHandling`