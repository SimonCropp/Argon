<?xml version="1.0" encoding="utf-8"?>
<topic id="SerializationCallbacks" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">

    <introduction>


      <para>Json.NET supports serialization callback methods. A callback can be used to manipulate an object before and after its serialization and deserialization by the JsonSerializer.

<list class="bullet">
  <listItem><para><legacyBold>OnSerializing</legacyBold></para></listItem>
  <listItem><para><legacyBold>OnSerialized</legacyBold></para></listItem>
  <listItem><para><legacyBold>OnDeserializing</legacyBold></para></listItem>
  <listItem><para><legacyBold>OnDeserialized</legacyBold></para></listItem>
</list>
<para>
To tell the serializer which methods should be called during the object's
serialization lifecycle, decorate a method with the appropriate attribute
(`System.Runtime.Serialization.OnSerializingAttribute`,
`System.Runtime.Serialization.OnSerializedAttribute`,
`System.Runtime.Serialization.OnDeserializingAttribute`,
`System.Runtime.Serialization.OnDeserializedAttribute`).
</para>

    

    <section>
      <title>Example</title>
      <content>

        <para>Example object with serialization callback methods:</para>

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="SerializationCallbacksObject" title="Serialization Callback Attributes" />

        <para>The example object being serialized and deserialized by Json.NET:</para>

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="SerializationCallbacksExample" title="Serialization Callback Example" />

      </content>
    </section>


## Related Topics
      `System.Runtime.Serialization.OnSerializingAttribute`
      `System.Runtime.Serialization.OnSerializedAttribute`
      `System.Runtime.Serialization.OnDeserializingAttribute`
      `System.Runtime.Serialization.OnDeserializedAttribute`