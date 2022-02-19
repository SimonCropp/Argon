<?xml version="1.0" encoding="utf-8"?>
<topic id="QueryingLINQtoJSON" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">LINQ to JSON provides methods for getting data from its objects. The index methods on JObject/JArray supports quickly get data by its property name
      on an object or index in a collection, while `Argon.Linq.JToken.Children` allows the retrieval of ranges
      of data as `IEnumerable<JToken>` to then query using LINQ.


    
    <section address="Index">
      <title>Getting values by Property Name or Collection Index</title>
      <content>

        <para>The simplest way to get a value from LINQ to JSON is to use the
        `Argon.Linq.JToken.Item(System.Object)` index on
        JObject/JArray and then cast the returned `Argon.Linq.JValue` to the type required.
        </para>

<code lang="cs" source="..\Src\Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonSimpleQuerying" title="Getting JSON Values" />

      </content>
    </section>
    <section address="QueryingLINQ">
      <title>Querying with LINQ</title>
      <content>
        <para>JObject/JArray can also be queried using LINQ. `Argon.Linq.JToken.Children`
        returns the children values of a JObject/JArray
		as an `IEnumerable<JToken>` that can then be queried with the standard Where/OrderBy/Select LINQ operators.
        
<alert class="note">
        <para>`Argon.Linq.JToken.Children` returns all the children of a token. If it is a
        JObject it will return a collection of properties to work with, and if
        it is a JArray a collection of the array's values will be returned.
</alert>

<code lang="cs" source="..\Src\Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonQuerying" title="Querying JSON" />

        <para>LINQ to JSON can also be used to manually convert JSON to a .NET object.

<code lang="cs" source="..\Src\Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonDeserializeObject" title="Deserializing Using LINQ Objects" />

        <para>Manually serializing and deserializing between .NET objects is useful when
        working with JSON that doesn't closely match the .NET objects.

<code lang="cs" source="..\Src\Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonDeserializeExample" title="Deserializing Using LINQ Example" />
      </content>
    </section>


## Related Topics
      <link xlink:href="LINQtoJSON" />

      `Argon.Linq.JToken.Item(System.Object)`
      `Argon.Linq.JToken.Children`