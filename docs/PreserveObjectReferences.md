<?xml version="1.0" encoding="utf-8"?>
<topic id="PreserveObjectReferences" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">
By default Json.NET will serialize all objects it encounters by value.
      If a list contains two Person references and both references point to the
      same object, then the JsonSerializer will write out all the names and values
      for each reference.
      
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="PreservingObjectReferencesOff" title="Preserve Object References Off" />
      
      <para>In most cases this is the desired result, but in certain scenarios
      writing the second item in the list as a reference to the first is a better
      solution. If the above JSON was deserialized now, then the returned list would
      contain two completely separate Person objects with the same values. Writing
      references by value will also cause problems on objects where a circular
      reference occurs.



    <section address="PreserveReferencesHandling">
      <title>PreserveReferencesHandling</title>
      <content>
        <para>Setting `Argon.PreserveReferencesHandling` will track object references
        when serializing and deserializing JSON.
        
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="PreservingObjectReferencesOn" title="Preserve Object References On" />       
        
        <para>The first Person in the list is serialized with the addition of an
        object ID. The second Person in JSON is now only a reference to the first.
        <para>With PreserveReferencesHandling on, now only one Person object is created
        on deserialization and the list contains two references to it, mirroring
        what was started with.
 <para>
   Metadata properties like `$id` must be located at the beginning of a JSON object to be successfully detected during deserialization. If it is not possible control
   the order of properties in the JSON object then `Argon.MetadataPropertyHandling` can be used to remove this restriction.
 </para>
<alert class="note">
  <para>References cannot be preserved when a value is set via a non-default constructor.
        With a non-default constructor, child values must be created before the parent value so they can be passed into
        the constructor, making tracking reference impossible.
        `System.Runtime.Serialization.ISerializable` types are an example
        of a class whose values are populated with a non-default constructor and won't work with PreserveReferencesHandling.
</alert>
      </content>
    </section>
    
    <section address="IsReference">
      <title>IsReference</title>
      <content>
        <para>The PreserveReferencesHandling setting on the JsonSerializer will
        change how all objects are serialized and deserialized. For fine grain
        control over which objects and members should be serialized as a
        reference there is the IsReference property on the JsonObjectAttribute,
        JsonArrayAttribute and JsonPropertyAttribute.
        
        <para>Setting IsReference on JsonObjectAttribute or JsonArrayAttribute to
        true will mean the JsonSerializer will always serialize the type the
        attribute is against as a reference. Setting IsReference on the
        JsonPropertyAttribute to true will serialize only that property as a
        reference.

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="PreservingObjectReferencesAttribute" title="IsReference" />
      </content>
    </section>
    
    <section address="IReferenceResolver">
      <title>IReferenceResolver</title>
      <content>
        <para>To customize how references are generated and resolved the
        `Argon.Serialization.IReferenceResolver`
        interface is available to inherit from and use with
        the JsonSerializer.
      </content>
    </section>


## Related Topics
      `Argon.PreserveReferencesHandling`