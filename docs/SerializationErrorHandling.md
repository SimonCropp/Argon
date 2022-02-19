<?xml version="1.0" encoding="utf-8"?>
<topic id="SerializationErrorHandling" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">

    <introduction>


      <para>Json.NET supports error handling during serialization and
      deserialization. Error handling lets you catch an error and
      choose whether to handle it and continue with serialization or
      let the error bubble up and be thrown in your application.
      <para>Error handling is defined through two methods: the `Argon.JsonSerializer.Error` event on
      JsonSerializer and the `Argon.Serialization.OnErrorAttribute`.



    <section address="ErrorEvent">
      <title>Error Event</title>
      <content>

        <para>The `Argon.JsonSerializer.Error`
        event is an event handler found on `Argon.JsonSerializer`.
        The error event is raised whenever an exception is thrown while serializing
        or deserializing JSON. Like all settings found on JsonSerializer, it can also
        be set on `Argon.JsonSerializerSettings`
        and passed to the serialization methods on JsonConvert.
        
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="SerializationErrorHandling" title="Serialization Error Handling" />

        <para>In this example we are deserializing a JSON array to a collection
        of DateTimes. On the JsonSerializerSettings a handler has been assigned
        to the `Error` event which will log the error message and mark the error
        as handled.
        
        <para>The result of deserializing the JSON is three successfully deserialized
        dates and three error messages: one for the badly formatted string ("I am
        not a date and will error!"), one for the nested JSON array, and one for the
        null value since the list doesn't allow nullable DateTimes. The event
        handler has logged these messages and Json.NET has continued on deserializing
        the JSON because the errors were marked as handled.
        
        <para>One thing to note with error handling in Json.NET is that an
        unhandled error will bubble up and raise the event on each of its
        parents. For example an unhandled error when serializing a collection of objects
        will be raised twice, once against the object and then again on the collection.
        This will let you handle an error either where it occurred or on one of its
        parents.
        
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="SerializationErrorHandlingWithParent" title="Parent Error Handling" />      

        <para>If you aren't immediately handling an error and only want to
        perform an action against it once, then you can check to see whether the
        `Argon.Serialization.ErrorEventArgs`'s
        CurrentObject is equal to the OriginalObject.
        OriginalObject is the object that threw the error and CurrentObject is
        the object that the event is being raised against. They will only equal
        the first time the event is raised against the OriginalObject.
      </content>
    </section>
    
    <section address="OnErrorAttribute">
      <title>OnErrorAttribute</title>
      <content>

        <para>The `Argon.Serialization.OnErrorAttribute`
        works much like the other <link xlink:href="SerializationAttributes">.NET serialization attributes</link>
        that Json.NET supports. To use it you simply place the
        attribute on a method that takes the correct parameters: a
        StreamingContext and an ErrorContext. The name of the method doesn't
        matter.
        
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="SerializationErrorHandlingAttributeObject" title="Serialization Error Handling Attribute" />
        
        <para>In this example accessing the Roles property will throw an
        exception when no roles have been set. The HandleError method will set
        the error when serializing Roles as handled and allow Json.NET to
        continue serializing the class.

<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="SerializationErrorHandlingAttributeExample" title="Serialization Error Handling Example" />

      </content>
    </section>


## Related Topics
      <link xlink:href="SerializationAttributes" />
      `Argon.JsonSerializer.Error`
      `Argon.Serialization.OnErrorAttribute`