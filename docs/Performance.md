<?xml version="1.0" encoding="utf-8"?>
<topic id="Performance" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">Out of the box Json.NET is faster than DataContractJsonSerializer and JavaScriptSerializer.
      Here are some tips to make it go even faster.


    <section address="ReuseContractResolver">
      <title>Reuse Contract Resolver</title>
      <content>
        <para>The `Argon.Serialization.IContractResolver` resolves .NET types to contracts that are used during serialization inside JsonSerializer.
        Creating a contract involves inspecting a type with slow reflection, so contracts are typically
        cached by implementations of IContractResolver like `Argon.Serialization.DefaultContractResolver`.
        <para>To avoid the overhead of recreating contracts every time a JsonSerializer is used create the contract resolver once and reuse it.
        Note that if not using a contract resolver then a shared internal instance is automatically used when serializing and deserializing.
        </para>

<code lang="cs" source="..\Src\Tests\Documentation\PerformanceTests.cs" region="ReuseContractResolver" title="Reuse ContractResolver" />
        
      </content>
    </section>
    <section address="MemoryUsage">
      <title>Optimize Memory Usage</title>
      <content>
        <para>To keep an application consistently fast, it is important to minimize the
        amount of time the .NET framework spends performing <externalLink>
<linkText>garbage collection</linkText>
<linkUri>http://msdn.microsoft.com/en-us/library/ms973837.aspx</linkUri>
</externalLink>.
        Allocating too many objects or allocating very large objects can slow down or even
        halt an application while garbage collection is in progress.
        </para>
        <para>To minimize memory usage and the number of objects allocated, Json.NET supports
        serializing and deserializing directly to a stream. Reading or writing JSON a piece at a time, instead of having
        the entire JSON string loaded into memory, is especially important when working with JSON
        documents greater than 85kb in size to avoid the JSON string ending up in the <externalLink>
<linkText>large object heap</linkText>
<linkUri>http://msdn.microsoft.com/en-us/magazine/cc534993.aspx</linkUri>
</externalLink>.

<code lang="cs" source="..\Src\Tests\Documentation\PerformanceTests.cs" region="DeserializeString" title="Deserialize String" />
<code lang="cs" source="..\Src\Tests\Documentation\PerformanceTests.cs" region="DeserializeStream" title="Deserialize Stream" />
        
      </content>
    </section>
    <section address="JsonConverters">
      <title>JsonConverters</title>
      <content>
        <para>Passing a `Argon.JsonConverter` to SerializeObject or DeserializeObject provides a way to completely
        change how an object is serialized. There is, however, a small amount of overhead; the CanConvert method is called for every
        value to check whether serialization should be handled by that JsonConverter.
        <para>There are a couple of ways to continue to use JsonConverters without any overhead. The simplest way
        is to specify the JsonConverter using the `Argon.JsonConverterAttribute`. This attribute tells the serializer
        to always use that converter when serializing and deserializing the type, without the check.

<code lang="cs" source="..\Src\Tests\Documentation\PerformanceTests.cs" region="JsonConverterAttribute" title="Use JsonConverter with JsonConverterAttribute" />

        <para>If the class to convert isn't owned and is it nor possible to use an attribute, a JsonConverter can still be used by
        creating a `Argon.Serialization.IContractResolver`.
        
<code lang="cs" source="..\Src\Tests\Documentation\PerformanceTests.cs" region="JsonConverterContractResolver" title="Use JsonConverter with IContractResolver" />

        <para>The IContractResolver in the example above will set all DateTimes to use the JavaScriptDateConverter.        
      </content>
    </section>
    <section address="ManuallySerialize">
      <title>Manually Serialize</title>
      <content>
        <para>The absolute fastest way to read and write JSON is to use JsonTextReader/JsonTextWriter directly to manually serialize types.
        Using a reader or writer directly skips any of the overhead from a serializer, such as reflection.
        
<code lang="cs" source="..\Src\Tests\Documentation\PerformanceTests.cs" region="ReaderWriter" title="Manually serialize using JsonTextWriter" />

        <para>
          If performance is important, then this is the best choice. More about using JsonReader/JsonWriter here: <link xlink:href="ReadingWritingJSON" />
        </para>
      </content>
    </section>
    <section address="Benchmarks">
      <title>Benchmarks</title>
      <content>
        
	<mediaLink>
      <image class="image" xlink:href="performance" mimeType="image/png" width="643" height="345" />
      <summary>Json.NET Performance</summary>
	</mediaLink>
        
      </content>
    </section>


## Related Topics
      `Argon.JsonSerializer`
      `Argon.JsonConverter`
      `Argon.JsonConverterAttribute`
      `Argon.JsonTextWriter`
      `Argon.JsonTextReader`