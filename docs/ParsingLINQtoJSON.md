<?xml version="1.0" encoding="utf-8"?>
<topic id="ParsingLINQtoJSON" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">LINQ to JSON has methods available for parsing JSON from a string or loading JSON directly from a file.


## Parsing JSON text
      <content>
        <para>JSON values can be read from a string using 
        `Argon.Linq.JToken.Parse(System.String)`.

<code lang="cs" source="..\Src\Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParse" title="Parsing a JSON Object from text" />
<code lang="cs" source="..\Src\Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParseArray" title="Parsing a JSON Array from text" />
      </content>
    </section>
    
    <section address="LoadingJSON">
      <title>Loading JSON from a file</title>
      <content>
        <para>JSON can also be loaded directly from a file using `Argon.Linq.JToken.ReadFrom(Argon.JsonReader)`.
<code lang="cs" source="..\Src\Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonReadObject" title="Reading JSON from a file" />
      </content>
    </section>


## Related Topics
      <link xlink:href="LINQtoJSON" />
      
      `Argon.Linq.JToken.Parse(System.String)`
      `Argon.Linq.JToken.ReadFrom(Argon.JsonReader)`