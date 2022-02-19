<?xml version="1.0" encoding="utf-8"?>
<topic id="CustomCreationConverter" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">
The `Argon.Converters.CustomCreationConverter`1`
      is a JsonConverter that provides a way
      to customize how an object is created during JSON deserialization. Once
      the object has been created it will then have values populated onto it by
      the serializer.

    <section>
      <title>Example</title>
      <content>
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="CustomCreationConverterObject" title="CustomCreationConverter" />
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="CustomCreationConverterExample" title="CustomCreationConverter Example" />
      </content>
    </section>


## Related Topics

 * `Argon.Converters.CustomCreationConverter`1`