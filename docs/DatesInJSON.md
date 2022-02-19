<?xml version="1.0" encoding="utf-8"?>
<topic id="DatesInJSON" revisionNumber="1">
  <developerConceptualDocument xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5" xmlns:xlink="http://www.w3.org/1999/xlink">
DateTimes in JSON are hard.
      <para>The problem comes from the <externalLink>
<linkText>JSON spec</linkText>
<linkUri>http://www.ietf.org/rfc/rfc4627.txt</linkUri>
</externalLink> itself: there is no literal
      syntax for dates in JSON. The spec has objects, arrays, strings, integers,
      and floats, but it defines no standard for what a date looks like.

    <section address="DatesAndJsonNET">
      <title>Dates and Json.NET</title>
      <content>
        <para>The default format used by Json.NET is the <externalLink>
<linkText>ISO 8601 standard</linkText>
<linkUri>http://en.wikipedia.org/wiki/ISO_8601</linkUri>
</externalLink>: `"2012-03-19T07:22Z"`.
        <para>Before Json.NET 4.5 dates were written using the Microsoft
        format: `"\/Date(1198908717056)\/"`. To use this format, or
        to maintain compatibility with Microsoft JSON serializers or
        older versions of Json.NET, then change the
        `Argon.DateFormatHandling`
        setting to MicrosoftDateFormat.
        <para>The `Argon.DateTimeZoneHandling` setting can be
        used to convert a DateTime's `System.DateTimeKind` when serializing. For example set
        DateTimeZoneHandling to Utc to serialize all DateTimes as UTC dates. Note that this setting does not effect DateTimeOffsets.
        <para>If dates don't follow the ISO 8601 standard, then the DateFormatString setting can be used to customize the format of
        	date strings that are read and written using .NET's <externalLink>
<linkText>custom date and time format syntax</linkText>
<linkUri>https://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx</linkUri>
</externalLink>.
      </content>
    </section>
    <section address="DateTimeJsonConverters">
      <title>DateTime JsonConverters</title>
      <content>

        <para>With no standard for dates in JSON, the number of possible
        different formats when interoping with other systems is endless.
        Fortunately Json.NET has a solution to deal with reading and writing
        custom dates: JsonConverters. A JsonConverter is used to override how a
        type is serialized.
        
<code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="SerializingDatesInJson" title="DateTime JsonConverters Example" />        
        
        <para>Pass the JsonConverter to use to the Json.NET
        serializer.
      </content>
    </section>
    <section address="JavaScriptDateTimeConverter">
      <title>JavaScriptDateTimeConverter</title>
      <content>
        <para>The JavaScriptDateTimeConverter class is one of the two DateTime
        JsonConverters that come with Json.NET. This converter serializes a
        DateTime as a <externalLink>
<linkText>JavaScript Date object</linkText>
<linkUri>http://msdn.microsoft.com/en-us/library/cd9w2te4.aspx</linkUri>
</externalLink>: `new Date(1234656000000)`</para>
        <para>Technically this is invalid JSON according to the spec, but all
        browsers and some JSON frameworks, including Json.NET, support it.
      </content>
    </section>
    <section address="IsoDateTimeConverter">
      <title>IsoDateTimeConverter</title>
      <content>
      <alert class="note">
  <para>From Json.NET 4.5 and onwards dates are written using the ISO 8601
        format by default, and using this converter is unnecessary.
</alert>
        <para>IsoDateTimeConverter serializes a DateTime to an <externalLink>
<linkText>ISO 8601</linkText>
<linkUri>http://en.wikipedia.org/wiki/ISO_8601</linkUri>
</externalLink> formatted
        string: `"2009-02-15T00:00:00Z"`</para>        
        <para>The IsoDateTimeConverter class has a property, DateTimeFormat, to
        further customize the formatted string.
      </content>
    </section>


## Related Topics
      `Argon.DateFormatHandling`
      `Argon.DateTimeZoneHandling`
      `Argon.Converters.JavaScriptDateTimeConverter`
      `Argon.Converters.IsoDateTimeConverter`