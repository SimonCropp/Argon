<?xml version="1.0" encoding="utf-8"?>
<topic id="SelectToken" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">
      `Argon.Linq.JToken.SelectToken`
      provides a method to query LINQ to JSON using a single string path to a desired
      `Argon.Linq.JToken`.
      SelectToken makes dynamic queries easy because the entire query is defined in a string.


    <section address="SelectToken">
      <title>SelectToken</title>
      <content>

        <para>SelectToken is a method on JToken and takes a string path to a child token.
        SelectToken returns the child token or a null reference if a token couldn't be
        found at the path's location.
        <para>The path is made up of property names and array indexes separated by periods,
        e.g. `Manufacturers[0].Name`.

<code lang="cs" source="..\Src\Tests\Documentation\LinqToJsonTests.cs" region="SelectTokenComplex" title="SelectToken Example" />
      </content>
    </section>
    <section address="SelectTokenJSONPath">
      <title>SelectToken with JSONPath</title>
      <content>
        <para>SelectToken supports JSONPath queries. Find out more about JSONPath <externalLink>
<linkText>here</linkText>
<linkUri>https://goessner.net/articles/JsonPath/</linkUri>
</externalLink>.

        <code lang="cs" source="..\Src\Tests\Documentation\Samples\JsonPath\QueryJsonSelectTokenJsonPath.cs" region="Usage" title="SelectToken With JSONPath" />
      </content>
    </section>
    <section address="SelectTokenLINQ">
      <title>SelectToken with LINQ</title>
      <content>

        <para>SelectToken can be used in combination with standard LINQ methods.
<code lang="cs" source="..\Src\Tests\Documentation\LinqToJsonTests.cs" region="SelectTokenLinq" title="SelectToken With LINQ Example" />
      </content>
    </section>


## Related Topics
      <link xlink:href="LINQtoJSON" />

      `Argon.Linq.JToken.SelectToken`