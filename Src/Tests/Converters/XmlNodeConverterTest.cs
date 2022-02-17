#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Globalization;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using Argon.Tests.Serialization;
using Argon.Tests.TestObjects;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Argon.Tests.XUnitAssert;
using System.IO;
using System.Xml;
using Argon.Converters;
using Argon.Linq;
using System.Xml.Linq;

namespace Argon.Tests.Converters
{
    [TestFixture]
    public class XmlNodeConverterTest : TestFixtureBase
    {
        private string SerializeXmlNode(XmlNode node)
        {
            var json = JsonConvert.SerializeXmlNode(node, Formatting.Indented);

            XmlReader reader = new XmlNodeReader(node);
            XObject xNode;
            if (node is XmlDocument)
            {
                xNode = XDocument.Load(reader);
            }
            else if (node is XmlAttribute)
            {
                var attribute = (XmlAttribute)node;
                xNode = new XAttribute(XName.Get(attribute.LocalName, attribute.NamespaceURI), attribute.Value);
            }
            else
            {
                reader.MoveToContent();
                xNode = XNode.ReadFrom(reader);
            }

            var linqJson = JsonConvert.SerializeXNode(xNode, Formatting.Indented);

            Assert.AreEqual(json, linqJson);

            return json;
        }

        private XmlNode DeserializeXmlNode(string json)
        {
            return DeserializeXmlNode(json, null);
        }

        private XmlNode DeserializeXmlNode(string json, string deserializeRootElementName)
        {
            JsonTextReader reader;

            reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            var converter = new XmlNodeConverter();
            if (deserializeRootElementName != null)
            {
                converter.DeserializeRootElementName = deserializeRootElementName;
            }

            var node = (XmlNode)converter.ReadJson(reader, typeof(XmlDocument), null, new JsonSerializer());

            var xmlText = node.OuterXml;

            reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            var d = (XDocument)converter.ReadJson(reader, typeof(XDocument), null, new JsonSerializer());

            var linqXmlText = d.ToString(SaveOptions.DisableFormatting);
            if (d.Declaration != null)
            {
                linqXmlText = d.Declaration + linqXmlText;
            }

            Assert.AreEqual(xmlText, linqXmlText);

            return node;
        }

        private string IndentXml(string xml)
        {
            var reader = XmlReader.Create(new StringReader(xml));

            var sw = new StringWriter();
            var writer = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true });

            while (reader.Read())
            {
                writer.WriteNode(reader, false);
            }

            writer.Flush();

            return sw.ToString();
        }

        [Fact]
        public void DeserializeXmlNode_DefaultDate()
        {
            var xmlNode = JsonConvert.DeserializeXmlNode("{Time: \"0001-01-01T00:00:00\"}");

            Assert.AreEqual("<Time>0001-01-01T00:00:00</Time>", xmlNode.OuterXml);
        }

        [Fact]
        public void XmlNode_Null()
        {
            var json = JsonConvert.SerializeXmlNode(null);

            Assert.AreEqual("null", json);
        }

        [Fact]
        public void XmlNode_Roundtrip_PropertyNameWithColon()
        {
            const string initialJson = @"{""Be:fore:After!"":""Value!""}";

            var xmlNode = JsonConvert.DeserializeXmlNode(initialJson, null, false, true);

            Assert.AreEqual("<Be_x003A_fore_x003A_After_x0021_>Value!</Be_x003A_fore_x003A_After_x0021_>", xmlNode.OuterXml);

            var json = JsonConvert.SerializeXmlNode(xmlNode);

            Assert.AreEqual(initialJson, json);
        }

        [Fact]
        public void XmlNode_Roundtrip_PropertyNameWithEscapedValue()
        {
            const string initialJson = @"{""BeforeAfter!"":""Value!""}";

            var xmlNode = JsonConvert.DeserializeXmlNode(initialJson);

            Assert.AreEqual("<BeforeAfter_x0021_>Value!</BeforeAfter_x0021_>", xmlNode.OuterXml);

            var json = JsonConvert.SerializeXmlNode(xmlNode);

            Assert.AreEqual(initialJson, json);
        }
/**
        [Fact]
        public void XmlNode_EncodeSpecialCharacters()
        {
            string initialJson = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""@href"": ""http://www.web2con.com/"",
      ""span"": [
        {
          ""@class"": ""summary"",
          ""#text"": ""Web 2.0 Conference"",
          ""#cdata-section"": ""my escaped text""
        },
        {
          ""@class"": ""location"",
          ""#text"": ""Argent Hotel, San Francisco, CA""
        }
      ],
      ""abbr"": [
        {
          ""@class"": ""dtstart"",
          ""@title"": ""2005-10-05"",
          ""#text"": ""October 5""
        },
        {
          ""@class"": ""dtend"",
          ""@title"": ""2005-10-08"",
          ""#text"": ""7""
        }
      ]
    }
  }
}";

            XmlDocument xmlNode = JsonConvert.DeserializeXmlNode(initialJson, "root", false, true);

            StringAssert.AreEqual(@"<root>
  <_x003F_xml>
    <_x0040_version>1.0</_x0040_version>
    <_x0040_standalone>no</_x0040_standalone>
  </_x003F_xml>
  <_x003F_xml-stylesheet>href=""classic.xsl"" type=""text/xml""</_x003F_xml-stylesheet>
  <span>
    <_x0040_class>vevent</_x0040_class>
    <a>
      <_x0040_class>url</_x0040_class>
      <_x0040_href>http://www.web2con.com/</_x0040_href>
      <span>
        <_x0040_class>summary</_x0040_class>
        <_x0023_text>Web 2.0 Conference</_x0023_text>
        <_x0023_cdata-section>my escaped text</_x0023_cdata-section>
      </span>
      <span>
        <_x0040_class>location</_x0040_class>
        <_x0023_text>Argent Hotel, San Francisco, CA</_x0023_text>
      </span>
      <abbr>
        <_x0040_class>dtstart</_x0040_class>
        <_x0040_title>2005-10-05</_x0040_title>
        <_x0023_text>October 5</_x0023_text>
      </abbr>
      <abbr>
        <_x0040_class>dtend</_x0040_class>
        <_x0040_title>2005-10-08</_x0040_title>
        <_x0023_text>7</_x0023_text>
      </abbr>
    </a>
  </span>
</root>", IndentXml(xmlNode.OuterXml));

            string json = JsonConvert.SerializeXmlNode(xmlNode, Formatting.Indented, true);

            Assert.AreEqual(initialJson, json);
        }
**/

        [Fact]
        public void XmlNode_UnescapeTextContent()
        {
            var xmlNode = new XmlDocument();
            xmlNode.LoadXml("<root>A &gt; B</root>");

            var json = JsonConvert.SerializeXmlNode(xmlNode);

            Assert.AreEqual(@"{""root"":""A > B""}", json);
        }

        [Fact]
        public void DeserializeXNode_DefaultDate()
        {
            var xmlNode = JsonConvert.DeserializeXNode("{Time: \"0001-01-01T00:00:00\"}");

            Assert.AreEqual("<Time>0001-01-01T00:00:00</Time>", xmlNode.ToString());
        }

        [Fact]
        public void WriteJsonNull()
        {
            var sw = new StringWriter();
            var jsonWriter = new JsonTextWriter(sw);

            var converter = new XmlNodeConverter();
            converter.WriteJson(jsonWriter, null, null);

            StringAssert.AreEqual(@"null", sw.ToString());
        }

        [Fact]
        public void XNode_Null()
        {
            var json = JsonConvert.SerializeXNode(null);

            Assert.AreEqual("null", json);
        }

        [Fact]
        public void XNode_UnescapeTextContent()
        {
            var xmlNode = XElement.Parse("<root>A &gt; B</root>");

            var json = JsonConvert.SerializeXNode(xmlNode);

            Assert.AreEqual(@"{""root"":""A > B""}", json);
        }

        [Fact]
        public void XNode_Roundtrip_PropertyNameWithColon()
        {
            const string initialJson = @"{""Be:fore:After!"":""Value!""}";

            var xmlNode = JsonConvert.DeserializeXNode(initialJson, null, false, true);

            Assert.AreEqual("<Be_x003A_fore_x003A_After_x0021_>Value!</Be_x003A_fore_x003A_After_x0021_>", xmlNode.ToString());

            var json = JsonConvert.SerializeXNode(xmlNode);

            Assert.AreEqual(initialJson, json);
        }

        [Fact]
        public void XNode_Roundtrip_PropertyNameWithEscapedValue()
        {
            const string initialJson = @"{""BeforeAfter!"":""Value!""}";

            var xmlNode = JsonConvert.DeserializeXNode(initialJson);

            Assert.AreEqual("<BeforeAfter_x0021_>Value!</BeforeAfter_x0021_>", xmlNode.ToString());

            var json = JsonConvert.SerializeXNode(xmlNode);

            Assert.AreEqual(initialJson, json);
        }

        //TODO: re enable
        /**
        [Fact]
        public void XNode_EncodeSpecialCharacters()
        {
            string initialJson = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""@href"": ""http://www.web2con.com/"",
      ""span"": [
        {
          ""@class"": ""summary"",
          ""#text"": ""Web 2.0 Conference"",
          ""#cdata-section"": ""my escaped text""
        },
        {
          ""@class"": ""location"",
          ""#text"": ""Argent Hotel, San Francisco, CA""
        }
      ],
      ""abbr"": [
        {
          ""@class"": ""dtstart"",
          ""@title"": ""2005-10-05"",
          ""#text"": ""October 5""
        },
        {
          ""@class"": ""dtend"",
          ""@title"": ""2005-10-08"",
          ""#text"": ""7""
        }
      ]
    }
  }
}";

            XDocument xmlNode = JsonConvert.DeserializeXNode(initialJson, "root", false, true);

            StringAssert.AreEqual(@"<root>
  <_x003F_xml>
    <_x0040_version>1.0</_x0040_version>
    <_x0040_standalone>no</_x0040_standalone>
  </_x003F_xml>
  <_x003F_xml-stylesheet>href=""classic.xsl"" type=""text/xml""</_x003F_xml-stylesheet>
  <span>
    <_x0040_class>vevent</_x0040_class>
    <a>
      <_x0040_class>url</_x0040_class>
      <_x0040_href>http://www.web2con.com/</_x0040_href>
      <span>
        <_x0040_class>summary</_x0040_class>
        <_x0023_text>Web 2.0 Conference</_x0023_text>
        <_x0023_cdata-section>my escaped text</_x0023_cdata-section>
      </span>
      <span>
        <_x0040_class>location</_x0040_class>
        <_x0023_text>Argent Hotel, San Francisco, CA</_x0023_text>
      </span>
      <abbr>
        <_x0040_class>dtstart</_x0040_class>
        <_x0040_title>2005-10-05</_x0040_title>
        <_x0023_text>October 5</_x0023_text>
      </abbr>
      <abbr>
        <_x0040_class>dtend</_x0040_class>
        <_x0040_title>2005-10-08</_x0040_title>
        <_x0023_text>7</_x0023_text>
      </abbr>
    </a>
  </span>
</root>", xmlNode.ToString());

            string json = JsonConvert.SerializeXNode(xmlNode, Formatting.Indented, true);

            Assert.AreEqual(initialJson, json);
        }

        [Fact]
        public void XNode_MetadataArray_EncodeSpecialCharacters()
        {
            string initialJson = @"{
  ""$id"": ""1"",
  ""$values"": [
    ""1"",
    ""2"",
    ""3"",
    ""4"",
    ""5""
  ]
}";

            XDocument xmlNode = JsonConvert.DeserializeXNode(initialJson, "root", false, true);

            StringAssert.AreEqual(@"<root>
  <_x0024_id>1</_x0024_id>
  <_x0024_values>1</_x0024_values>
  <_x0024_values>2</_x0024_values>
  <_x0024_values>3</_x0024_values>
  <_x0024_values>4</_x0024_values>
  <_x0024_values>5</_x0024_values>
</root>", xmlNode.ToString());

            string json = JsonConvert.SerializeXNode(xmlNode, Formatting.Indented, true);

            Assert.AreEqual(initialJson, json);
        }

**/
        [Fact]
        public void SerializeDollarProperty()
        {
            var json1 = @"{""$"":""test""}";

            var doc = JsonConvert.DeserializeXNode(json1);

            Assert.AreEqual(@"<_x0024_>test</_x0024_>", doc.ToString());

            var json2 = JsonConvert.SerializeXNode(doc);
            
            Assert.AreEqual(json1, json2);
        }

        [Fact]
        public void SerializeNonKnownDollarProperty()
        {
            var json1 = @"{""$JELLY"":""test""}";

            var doc = JsonConvert.DeserializeXNode(json1);

            Console.WriteLine(doc.ToString());

            Assert.AreEqual(@"<_x0024_JELLY>test</_x0024_JELLY>", doc.ToString());

            var json2 = JsonConvert.SerializeXNode(doc);

            Assert.AreEqual(json1, json2);
        }

        public class MyModel
        {
            public string MyProperty { get; set; }
        }

        [Fact]
        public void ConvertNullString()
        {
            var json = new JObject
            {
              ["Prop1"] = (string)null,
              ["Prop2"] = new MyModel().MyProperty
            };

            var xmlNodeConverter = new XmlNodeConverter { DeserializeRootElementName = "object" };
            var jsonSerializerSettings = new JsonSerializerSettings { Converters = new JsonConverter[] { xmlNodeConverter } };
            var jsonSerializer = JsonSerializer.CreateDefault(jsonSerializerSettings);
            var d = json.ToObject<XDocument>(jsonSerializer);

            StringAssert.Equals(@"<object>
  <Prop1 />
  <Prop2 />
</object>", d.ToString());
        }

        public class Foo
        {
            public XElement Bar { get; set; }
        }

        [Fact]
        public void SerializeAndDeserializeXElement()
        {
            var foo = new Foo { Bar = null };
            var json = JsonConvert.SerializeObject(foo);

            Assert.AreEqual(@"{""Bar"":null}", json);
            var foo2 = JsonConvert.DeserializeObject<Foo>(json);

            Assert.IsNull(foo2.Bar);
        }

        [Fact]
        public void MultipleNamespacesXDocument()
        {
            var xml = @"<result xp_0:end=""2014-08-15 13:12:11.9184"" xp_0:start=""2014-08-15 13:11:49.3140"" xp_0:time_diff=""22604.3836"" xmlns:xp_0=""Test1"" p2:end=""2014-08-15 13:13:49.5522"" p2:start=""2014-08-15 13:13:49.0268"" p2:time_diff=""525.4646"" xmlns:p2=""Test2"" />";

            var d = XDocument.Parse(xml);

            var json = JsonConvert.SerializeObject(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""result"": {
    ""@xp_0:end"": ""2014-08-15 13:12:11.9184"",
    ""@xp_0:start"": ""2014-08-15 13:11:49.3140"",
    ""@xp_0:time_diff"": ""22604.3836"",
    ""@xmlns:xp_0"": ""Test1"",
    ""@p2:end"": ""2014-08-15 13:13:49.5522"",
    ""@p2:start"": ""2014-08-15 13:13:49.0268"",
    ""@p2:time_diff"": ""525.4646"",
    ""@xmlns:p2"": ""Test2""
  }
}", json);

            var doc = JsonConvert.DeserializeObject<XDocument>(json);

            StringAssert.AreEqual(xml, doc.ToString());
        }

        [Fact]
        public void MultipleNamespacesXmlDocument()
        {
            var xml = @"<result xp_0:end=""2014-08-15 13:12:11.9184"" xp_0:start=""2014-08-15 13:11:49.3140"" xp_0:time_diff=""22604.3836"" xmlns:xp_0=""Test1"" p2:end=""2014-08-15 13:13:49.5522"" p2:start=""2014-08-15 13:13:49.0268"" p2:time_diff=""525.4646"" xmlns:p2=""Test2"" />";

            var d = new XmlDocument();
            d.LoadXml(xml);

            var json = JsonConvert.SerializeObject(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""result"": {
    ""@xp_0:end"": ""2014-08-15 13:12:11.9184"",
    ""@xp_0:start"": ""2014-08-15 13:11:49.3140"",
    ""@xp_0:time_diff"": ""22604.3836"",
    ""@xmlns:xp_0"": ""Test1"",
    ""@p2:end"": ""2014-08-15 13:13:49.5522"",
    ""@p2:start"": ""2014-08-15 13:13:49.0268"",
    ""@p2:time_diff"": ""525.4646"",
    ""@xmlns:p2"": ""Test2""
  }
}", json);

            var doc = JsonConvert.DeserializeObject<XmlDocument>(json);

            StringAssert.AreEqual(xml, doc.OuterXml);
        }

        [Fact]
        public void SerializeXmlElement()
        {
            var xml = @"<payload>
    <Country>6</Country>
    <FinancialTransactionApprovalRequestUID>79</FinancialTransactionApprovalRequestUID>
    <TransactionStatus>Approved</TransactionStatus>
    <StatusChangeComment></StatusChangeComment>
    <RequestedBy>Someone</RequestedBy>
</payload>";

            var xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(xml);

            var result = xmlDocument.FirstChild.ChildNodes.Cast<XmlNode>().ToArray();

            var json = JsonConvert.SerializeObject(result, Formatting.Indented); // <--- fails here with the cast message

            StringAssert.AreEqual(@"[
  {
    ""Country"": ""6""
  },
  {
    ""FinancialTransactionApprovalRequestUID"": ""79""
  },
  {
    ""TransactionStatus"": ""Approved""
  },
  {
    ""StatusChangeComment"": """"
  },
  {
    ""RequestedBy"": ""Someone""
  }
]", json);
        }

        [Fact]
        public void SerializeXElement()
        {
            var xml = @"<payload>
    <Country>6</Country>
    <FinancialTransactionApprovalRequestUID>79</FinancialTransactionApprovalRequestUID>
    <TransactionStatus>Approved</TransactionStatus>
    <StatusChangeComment></StatusChangeComment>
    <RequestedBy>Someone</RequestedBy>
</payload>";

            var xmlDocument = XDocument.Parse(xml);

            var result = xmlDocument.Root.Nodes().ToArray();

            var json = JsonConvert.SerializeObject(result, Formatting.Indented); // <--- fails here with the cast message

            StringAssert.AreEqual(@"[
  {
    ""Country"": ""6""
  },
  {
    ""FinancialTransactionApprovalRequestUID"": ""79""
  },
  {
    ""TransactionStatus"": ""Approved""
  },
  {
    ""StatusChangeComment"": """"
  },
  {
    ""RequestedBy"": ""Someone""
  }
]", json);
        }

        public class DecimalContainer
        {
            public decimal Number { get; set; }
        }

        [Fact]
        public void FloatParseHandlingDecimal()
        {
            var d = (decimal)Math.PI + 1000000000m;
            var x = new DecimalContainer { Number = d };

            var json = JsonConvert.SerializeObject(x, Formatting.Indented);

            var doc1 = JsonConvert.DeserializeObject<XDocument>(json, new JsonSerializerSettings
            {
                Converters = { new XmlNodeConverter() },
                FloatParseHandling = FloatParseHandling.Decimal
            });

            var xml = doc1.ToString();
            Assert.AreEqual("<Number>1000000003.14159265358979</Number>", xml);

            var json2 = JsonConvert.SerializeObject(doc1, Formatting.Indented);

            var x2 = JsonConvert.DeserializeObject<DecimalContainer>(json2);

            Assert.AreEqual(x.Number, x2.Number);
        }

        public class DateTimeOffsetContainer
        {
            public DateTimeOffset Date { get; set; }
        }

        [Fact]
        public void DateTimeParseHandlingOffset()
        {
            var d = new DateTimeOffset(2012, 12, 12, 12, 44, 1, TimeSpan.FromHours(12).Add(TimeSpan.FromMinutes(34)));
            var x = new DateTimeOffsetContainer { Date = d };

            var json = JsonConvert.SerializeObject(x, Formatting.Indented);

            var doc1 = JsonConvert.DeserializeObject<XDocument>(json, new JsonSerializerSettings
            {
                Converters = { new XmlNodeConverter() },
                DateParseHandling = DateParseHandling.DateTimeOffset
            });

            var xml = doc1.ToString();
            Assert.AreEqual("<Date>2012-12-12T12:44:01+12:34</Date>", xml);

            var json2 = JsonConvert.SerializeObject(doc1, Formatting.Indented);

            var x2 = JsonConvert.DeserializeObject<DateTimeOffsetContainer>(json2);

            Assert.AreEqual(x.Date, x2.Date);
        }

        [Fact]
        public void GroupElementsOfTheSameName()
        {
            var xml = "<root><p>Text1<span>Span1</span> <span>Span2</span> Text2</p></root>";

            var json = JsonConvert.SerializeXNode(XElement.Parse(xml));

            Assert.AreEqual(@"{""root"":{""p"":{""#text"":[""Text1"","" Text2""],""span"":[""Span1"",""Span2""]}}}", json);

            var doc = JsonConvert.DeserializeXNode(json);

            StringAssert.AreEqual(@"<root>
  <p>Text1 Text2<span>Span1</span><span>Span2</span></p>
</root>", doc.ToString());
        }

        [Fact]
        public void SerializeEmptyDocument()
        {
            var doc = new XmlDocument();
            doc.LoadXml("<root />");

            var json = JsonConvert.SerializeXmlNode(doc, Formatting.Indented, true);
            Assert.AreEqual("null", json);

            doc = new XmlDocument();
            doc.LoadXml("<root></root>");

            json = JsonConvert.SerializeXmlNode(doc, Formatting.Indented, true);
            Assert.AreEqual(@"""""", json);

            var doc1 = XDocument.Parse("<root />");

            json = JsonConvert.SerializeXNode(doc1, Formatting.Indented, true);
            Assert.AreEqual("null", json);

            doc1 = XDocument.Parse("<root></root>");

            json = JsonConvert.SerializeXNode(doc1, Formatting.Indented, true);
            Assert.AreEqual(@"""""", json);
        }

        [Fact]
        public void SerializeAndDeserializeXmlWithNamespaceInChildrenAndNoValueInChildren()
        {
            var xmlString = @"<root>
                              <b xmlns='http://www.example.com/ns'/>
                              <c>AAA</c>
                              <test>adad</test>
                              </root>";

            var xml = XElement.Parse(xmlString);

            var json1 = JsonConvert.SerializeXNode(xml);
            var xmlBack = JsonConvert.DeserializeObject<XElement>(json1);

            var equals = XElement.DeepEquals(xmlBack, xml);
            Assert.IsTrue(equals);
        }

        [Fact]
        public void DeserializeUndeclaredNamespacePrefix()
        {
            var doc = JsonConvert.DeserializeXmlNode("{ A: { '@xsi:nil': true } }");

            Assert.AreEqual(@"<A nil=""true"" />", doc.OuterXml);

            var xdoc = JsonConvert.DeserializeXNode("{ A: { '@xsi:nil': true } }");

            Assert.AreEqual(doc.OuterXml, xdoc.ToString());
        }

        [Fact]
        public void DeserializeMultipleRootElements()
        {
            var json = @"{
    ""Id"": 1,
     ""Email"": ""james@example.com"",
     ""Active"": true,
     ""CreatedDate"": ""2013-01-20T00:00:00Z"",
     ""Roles"": [
       ""User"",
       ""Admin""
     ],
    ""Team"": {
        ""Id"": 2,
        ""Name"": ""Software Developers"",
        ""Description"": ""Creators of fine software products and services.""
    }
}";
            ExceptionAssert.Throws<JsonSerializationException>(
                () => { JsonConvert.DeserializeXmlNode(json); },
                "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifying a DeserializeRootElementName. Path 'Email', line 3, position 13.");
        }

        [Fact]
        public void DocumentSerializeIndented()
        {
            var xml = @"<?xml version=""1.0"" standalone=""no""?>
<?xml-stylesheet href=""classic.xsl"" type=""text/xml""?>
<span class=""vevent"">
  <a class=""url"" href=""http://www.web2con.com/"">
    <span class=""summary"">Web 2.0 Conference<![CDATA[my escaped text]]></span>
    <abbr class=""dtstart"" title=""2005-10-05"">October 5</abbr>
    <abbr class=""dtend"" title=""2005-10-08"">7</abbr>
    <span class=""location"">Argent Hotel, San Francisco, CA</span>
  </a>
</span>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var jsonText = SerializeXmlNode(doc);
            var expected = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""@href"": ""http://www.web2con.com/"",
      ""span"": [
        {
          ""@class"": ""summary"",
          ""#text"": ""Web 2.0 Conference"",
          ""#cdata-section"": ""my escaped text""
        },
        {
          ""@class"": ""location"",
          ""#text"": ""Argent Hotel, San Francisco, CA""
        }
      ],
      ""abbr"": [
        {
          ""@class"": ""dtstart"",
          ""@title"": ""2005-10-05"",
          ""#text"": ""October 5""
        },
        {
          ""@class"": ""dtend"",
          ""@title"": ""2005-10-08"",
          ""#text"": ""7""
        }
      ]
    }
  }
}";

            StringAssert.AreEqual(expected, jsonText);
        }

        [Fact]
        public void SerializeNodeTypes()
        {
            var doc = new XmlDocument();
            string jsonText;

            var xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<xs:schema xs:id=""SomeID"" 
	xmlns="""" 
	xmlns:xs=""http://www.w3.org/2001/XMLSchema"" 
	xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
	<xs:element name=""MyDataSet"" msdata:IsDataSet=""true"">
	</xs:element>
</xs:schema>";

            var document = new XmlDocument();
            document.LoadXml(xml);

            // XmlAttribute
            var attribute = document.DocumentElement.ChildNodes[0].Attributes["IsDataSet", "urn:schemas-microsoft-com:xml-msdata"];
            attribute.Value = "true";

            jsonText = JsonConvert.SerializeXmlNode(attribute);

            Assert.AreEqual(@"{""@msdata:IsDataSet"":""true""}", jsonText);

            var d = XDocument.Parse(xml);
            var a = d.Root.Element("{http://www.w3.org/2001/XMLSchema}element").Attribute("{urn:schemas-microsoft-com:xml-msdata}IsDataSet");

            jsonText = JsonConvert.SerializeXNode(a);

            Assert.AreEqual(@"{""@msdata:IsDataSet"":""true""}", jsonText);

            // XmlProcessingInstruction
            var instruction = doc.CreateProcessingInstruction("xml-stylesheet", @"href=""classic.xsl"" type=""text/xml""");

            jsonText = JsonConvert.SerializeXmlNode(instruction);

            Assert.AreEqual(@"{""?xml-stylesheet"":""href=\""classic.xsl\"" type=\""text/xml\""""}", jsonText);

            // XmlProcessingInstruction
            var cDataSection = doc.CreateCDataSection("<Kiwi>true</Kiwi>");

            jsonText = JsonConvert.SerializeXmlNode(cDataSection);

            Assert.AreEqual(@"{""#cdata-section"":""<Kiwi>true</Kiwi>""}", jsonText);

            // XmlElement
            var element = doc.CreateElement("xs", "Choice", "http://www.w3.org/2001/XMLSchema");
            element.SetAttributeNode(doc.CreateAttribute("msdata", "IsDataSet", "urn:schemas-microsoft-com:xml-msdata"));

            var aa = doc.CreateAttribute(@"xmlns", "xs", "http://www.w3.org/2000/xmlns/");
            aa.Value = "http://www.w3.org/2001/XMLSchema";
            element.SetAttributeNode(aa);

            aa = doc.CreateAttribute(@"xmlns", "msdata", "http://www.w3.org/2000/xmlns/");
            aa.Value = "urn:schemas-microsoft-com:xml-msdata";
            element.SetAttributeNode(aa);

            element.AppendChild(instruction);
            element.AppendChild(cDataSection);

            doc.AppendChild(element);

            jsonText = JsonConvert.SerializeXmlNode(element, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""xs:Choice"": {
    ""@msdata:IsDataSet"": """",
    ""@xmlns:xs"": ""http://www.w3.org/2001/XMLSchema"",
    ""@xmlns:msdata"": ""urn:schemas-microsoft-com:xml-msdata"",
    ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
    ""#cdata-section"": ""<Kiwi>true</Kiwi>""
  }
}", jsonText);
        }

        [Fact]
        public void SerializeNodeTypes_Encoding()
        {
            var node = DeserializeXmlNode(@"{
  ""xs!:Choice!"": {
    ""@msdata:IsDataSet!"": """",
    ""@xmlns:xs!"": ""http://www.w3.org/2001/XMLSchema"",
    ""@xmlns:msdata"": ""urn:schemas-microsoft-com:xml-msdata"",
    ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
    ""#cdata-section"": ""<Kiwi>true</Kiwi>""
  }
}");

            Assert.AreEqual(@"<xs_x0021_:Choice_x0021_ msdata:IsDataSet_x0021_="""" xmlns:xs_x0021_=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata""><?xml-stylesheet href=""classic.xsl"" type=""text/xml""?><![CDATA[<Kiwi>true</Kiwi>]]></xs_x0021_:Choice_x0021_>", node.InnerXml);

            var json = SerializeXmlNode(node);

            StringAssert.AreEqual(@"{
  ""xs!:Choice!"": {
    ""@msdata:IsDataSet!"": """",
    ""@xmlns:xs!"": ""http://www.w3.org/2001/XMLSchema"",
    ""@xmlns:msdata"": ""urn:schemas-microsoft-com:xml-msdata"",
    ""?xml-stylesheet"": ""href=\""classic.xsl\"" type=\""text/xml\"""",
    ""#cdata-section"": ""<Kiwi>true</Kiwi>""
  }
}", json);
        }

        [Fact]
        public void DocumentFragmentSerialize()
        {
            var doc = new XmlDocument();

            var fragement = doc.CreateDocumentFragment();

            fragement.InnerXml = "<Item>widget</Item><Item>widget</Item>";

            var jsonText = JsonConvert.SerializeXmlNode(fragement);

            var expected = @"{""Item"":[""widget"",""widget""]}";

            Assert.AreEqual(expected, jsonText);
        }

        [Fact]
        public void XmlDocumentTypeSerialize()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?><!DOCTYPE STOCKQUOTE PUBLIC ""-//W3C//DTD StockQuote 1.5//EN"" ""http://www.idontexistnopenopewhatnope123.org/dtd/stockquote_1.5.dtd""><STOCKQUOTE ROWCOUNT=""2""><RESULT><ROW><ASK>0</ASK><BID>0</BID><CHANGE>-16.310</CHANGE><COMPANYNAME>Dow Jones</COMPANYNAME><DATETIME>2014-04-17 15:50:37</DATETIME><DIVIDEND>0</DIVIDEND><EPS>0</EPS><EXCHANGE></EXCHANGE><HIGH>16460.490</HIGH><LASTDATETIME>2014-04-17 15:50:37</LASTDATETIME><LASTPRICE>16408.540</LASTPRICE><LOW>16368.140</LOW><OPEN>16424.140</OPEN><PCHANGE>-0.099</PCHANGE><PE>0</PE><PREVIOUSCLOSE>16424.850</PREVIOUSCLOSE><SHARES>0</SHARES><TICKER>DJII</TICKER><TRADES>0</TRADES><VOLUME>136188700</VOLUME><YEARHIGH>11309.000</YEARHIGH><YEARLOW>9302.280</YEARLOW><YIELD>0</YIELD></ROW><ROW><ASK>0</ASK><BID>0</BID><CHANGE>9.290</CHANGE><COMPANYNAME>NASDAQ</COMPANYNAME><DATETIME>2014-04-17 15:40:01</DATETIME><DIVIDEND>0</DIVIDEND><EPS>0</EPS><EXCHANGE></EXCHANGE><HIGH>4110.460</HIGH><LASTDATETIME>2014-04-17 15:40:01</LASTDATETIME><LASTPRICE>4095.520</LASTPRICE><LOW>4064.700</LOW><OPEN>4080.300</OPEN><PCHANGE>0.227</PCHANGE><PE>0</PE><PREVIOUSCLOSE>4086.230</PREVIOUSCLOSE><SHARES>0</SHARES><TICKER>COMP</TICKER><TRADES>0</TRADES><VOLUME>1784210100</VOLUME><YEARHIGH>4371.710</YEARHIGH><YEARLOW>3154.960</YEARLOW><YIELD>0</YIELD></ROW></RESULT><STATUS>Couldn't find ticker: SPIC?</STATUS><STATUSCODE>2</STATUSCODE></STOCKQUOTE>";

            var expected = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@encoding"": ""utf-8""
  },
  ""!DOCTYPE"": {
    ""@name"": ""STOCKQUOTE"",
    ""@public"": ""-//W3C//DTD StockQuote 1.5//EN"",
    ""@system"": ""http://www.idontexistnopenopewhatnope123.org/dtd/stockquote_1.5.dtd""
  },
  ""STOCKQUOTE"": {
    ""@ROWCOUNT"": ""2"",
    ""RESULT"": {
      ""ROW"": [
        {
          ""ASK"": ""0"",
          ""BID"": ""0"",
          ""CHANGE"": ""-16.310"",
          ""COMPANYNAME"": ""Dow Jones"",
          ""DATETIME"": ""2014-04-17 15:50:37"",
          ""DIVIDEND"": ""0"",
          ""EPS"": ""0"",
          ""EXCHANGE"": """",
          ""HIGH"": ""16460.490"",
          ""LASTDATETIME"": ""2014-04-17 15:50:37"",
          ""LASTPRICE"": ""16408.540"",
          ""LOW"": ""16368.140"",
          ""OPEN"": ""16424.140"",
          ""PCHANGE"": ""-0.099"",
          ""PE"": ""0"",
          ""PREVIOUSCLOSE"": ""16424.850"",
          ""SHARES"": ""0"",
          ""TICKER"": ""DJII"",
          ""TRADES"": ""0"",
          ""VOLUME"": ""136188700"",
          ""YEARHIGH"": ""11309.000"",
          ""YEARLOW"": ""9302.280"",
          ""YIELD"": ""0""
        },
        {
          ""ASK"": ""0"",
          ""BID"": ""0"",
          ""CHANGE"": ""9.290"",
          ""COMPANYNAME"": ""NASDAQ"",
          ""DATETIME"": ""2014-04-17 15:40:01"",
          ""DIVIDEND"": ""0"",
          ""EPS"": ""0"",
          ""EXCHANGE"": """",
          ""HIGH"": ""4110.460"",
          ""LASTDATETIME"": ""2014-04-17 15:40:01"",
          ""LASTPRICE"": ""4095.520"",
          ""LOW"": ""4064.700"",
          ""OPEN"": ""4080.300"",
          ""PCHANGE"": ""0.227"",
          ""PE"": ""0"",
          ""PREVIOUSCLOSE"": ""4086.230"",
          ""SHARES"": ""0"",
          ""TICKER"": ""COMP"",
          ""TRADES"": ""0"",
          ""VOLUME"": ""1784210100"",
          ""YEARHIGH"": ""4371.710"",
          ""YEARLOW"": ""3154.960"",
          ""YIELD"": ""0""
        }
      ]
    },
    ""STATUS"": ""Couldn't find ticker: SPIC?"",
    ""STATUSCODE"": ""2""
  }
}";

            var doc1 = new XmlDocument
            {
              XmlResolver = null
            };
            doc1.LoadXml(xml);

            var json1 = JsonConvert.SerializeXmlNode(doc1, Formatting.Indented);

            StringAssert.AreEqual(expected, json1);

            var doc11 = JsonConvert.DeserializeXmlNode(json1);

            StringAssert.AreEqual(xml, ToStringWithDeclaration(doc11));

            var doc2 = XDocument.Parse(xml);

            var json2 = JsonConvert.SerializeXNode(doc2, Formatting.Indented);

            StringAssert.AreEqual(expected, json2);

            var doc22 = JsonConvert.DeserializeXNode(json2);

            StringAssert.AreEqual(xml, ToStringWithDeclaration(doc22));
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }

            public Utf8StringWriter(StringBuilder sb) : base(sb)
            {
            }
        }

        public static string ToStringWithDeclaration(XDocument doc, bool indent = false)
        {
            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(new Utf8StringWriter(builder), new XmlWriterSettings { Indent = indent }))
            {
                doc.Save(writer);
            }
            return builder.ToString();
        }

        public static string ToStringWithDeclaration(XmlDocument doc, bool indent = false)
        {
            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(new Utf8StringWriter(builder), new XmlWriterSettings { Indent = indent }))
            {
                doc.Save(writer);
            }
            return builder.ToString();
        }

        [Fact]
        public void NamespaceSerializeDeserialize()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<xs:schema xs:id=""SomeID"" 
	xmlns="""" 
	xmlns:xs=""http://www.w3.org/2001/XMLSchema"" 
	xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
	<xs:element name=""MyDataSet"" msdata:IsDataSet=""true"">
		<xs:complexType>
			<xs:choice maxOccurs=""unbounded"">
				<xs:element name=""customers"" >
					<xs:complexType >
						<xs:sequence>
							<xs:element name=""CustomerID"" type=""xs:integer"" 
										 minOccurs=""0"" />
							<xs:element name=""CompanyName"" type=""xs:string"" 
										 minOccurs=""0"" />
							<xs:element name=""Phone"" type=""xs:string"" />
						</xs:sequence>
					</xs:complexType>
				</xs:element>
			</xs:choice>
		</xs:complexType>
	</xs:element>
</xs:schema>";

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var jsonText = SerializeXmlNode(doc);

            var expected = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@encoding"": ""utf-8""
  },
  ""xs:schema"": {
    ""@xs:id"": ""SomeID"",
    ""@xmlns"": """",
    ""@xmlns:xs"": ""http://www.w3.org/2001/XMLSchema"",
    ""@xmlns:msdata"": ""urn:schemas-microsoft-com:xml-msdata"",
    ""xs:element"": {
      ""@name"": ""MyDataSet"",
      ""@msdata:IsDataSet"": ""true"",
      ""xs:complexType"": {
        ""xs:choice"": {
          ""@maxOccurs"": ""unbounded"",
          ""xs:element"": {
            ""@name"": ""customers"",
            ""xs:complexType"": {
              ""xs:sequence"": {
                ""xs:element"": [
                  {
                    ""@name"": ""CustomerID"",
                    ""@type"": ""xs:integer"",
                    ""@minOccurs"": ""0""
                  },
                  {
                    ""@name"": ""CompanyName"",
                    ""@type"": ""xs:string"",
                    ""@minOccurs"": ""0""
                  },
                  {
                    ""@name"": ""Phone"",
                    ""@type"": ""xs:string""
                  }
                ]
              }
            }
          }
        }
      }
    }
  }
}";

            StringAssert.AreEqual(expected, jsonText);

            var deserializedDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            Assert.AreEqual(doc.InnerXml, deserializedDoc.InnerXml);
        }

        [Fact]
        public void FailOnIncomplete()
        {
            var json = @"{'Row' : ";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeXmlNode(json, "ROOT"),
                "Unexpected end when reading JSON. Path 'Row', line 1, position 9.");
        }

        [Fact]
        public void DocumentDeserialize()
        {
            var jsonText = @"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""span"": {
        ""@class"": ""summary"",
        ""#text"": ""Web 2.0 Conference"",
        ""#cdata-section"": ""my escaped text""
      },
      ""@href"": ""http://www.web2con.com/""
    }
  }
}";

            var doc = (XmlDocument)DeserializeXmlNode(jsonText);

            var expected = @"<?xml version=""1.0"" standalone=""no""?>
<span class=""vevent"">
  <a class=""url"" href=""http://www.web2con.com/"">
    <span class=""summary"">Web 2.0 Conference<![CDATA[my escaped text]]></span>
  </a>
</span>";

            var formattedXml = GetIndentedInnerXml(doc);

            StringAssert.AreEqual(expected, formattedXml);
        }

        private string GetIndentedInnerXml(XmlNode node)
        {
            var settings = new XmlWriterSettings
            {
              Indent = true
            };

            var sw = new StringWriter();

            using (var writer = XmlWriter.Create(sw, settings))
            {
                node.WriteTo(writer);
            }

            return sw.ToString();
        }

        public class Foo2
        {
            public XmlElement Bar { get; set; }
        }

        [Fact]
        public void SerializeAndDeserializeXmlElement()
        {
            var foo = new Foo2 { Bar = null };
            var json = JsonConvert.SerializeObject(foo);

            Assert.AreEqual(@"{""Bar"":null}", json);
            var foo2 = JsonConvert.DeserializeObject<Foo2>(json);

            Assert.IsNull(foo2.Bar);
        }

        [Fact]
        public void SingleTextNode()
        {
            var xml = @"<?xml version=""1.0"" standalone=""no""?>
			<root>
			  <person id=""1"">
	  			<name>Alan</name>
		  		<url>http://www.google.com</url>
			  </person>
			  <person id=""2"">
			  	<name>Louis</name>
				  <url>http://www.yahoo.com</url>
			  </person>
			</root>";

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var jsonText = SerializeXmlNode(doc);

            var newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            Assert.AreEqual(doc.InnerXml, newDoc.InnerXml);
        }

        [Fact]
        public void EmptyNode()
        {
            var xml = @"<?xml version=""1.0"" standalone=""no""?>
			<root>
			  <person id=""1"">
				<name>Alan</name>
				<url />
			  </person>
			  <person id=""2"">
				<name>Louis</name>
				<url>http://www.yahoo.com</url>
			  </person>
			</root>";

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var jsonText = SerializeXmlNode(doc);

            StringAssert.AreEqual(@"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""root"": {
    ""person"": [
      {
        ""@id"": ""1"",
        ""name"": ""Alan"",
        ""url"": null
      },
      {
        ""@id"": ""2"",
        ""name"": ""Louis"",
        ""url"": ""http://www.yahoo.com""
      }
    ]
  }
}", jsonText);

            var newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            Assert.AreEqual(doc.InnerXml, newDoc.InnerXml);
        }

        [Fact]
        public void OtherElementDataTypes()
        {
            var jsonText = @"{""?xml"":{""@version"":""1.0"",""@standalone"":""no""},""root"":{""person"":[{""@id"":""1"",""Float"":2.5,""Integer"":99},{""Boolean"":true,""@id"":""2"",""date"":""\/Date(954374400000)\/""}]}}";

            var newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            var expected = @"<?xml version=""1.0"" standalone=""no""?><root><person id=""1""><Float>2.5</Float><Integer>99</Integer></person><person id=""2""><Boolean>true</Boolean><date>2000-03-30T00:00:00Z</date></person></root>";

            Assert.AreEqual(expected, newDoc.InnerXml);
        }

        [Fact]
        public void NoRootObject()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => { var newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(@"[1]"); }, "XmlNodeConverter can only convert JSON that begins with an object. Path '', line 1, position 1.");
        }

        [Fact]
        public void RootObjectMultipleProperties()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                () => { var newDoc = (XmlDocument)JsonConvert.DeserializeXmlNode(@"{Prop1:1,Prop2:2}"); },
                "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifying a DeserializeRootElementName. Path 'Prop2', line 1, position 15.");
        }

        [Fact]
        public void JavaScriptConstructor()
        {
            var jsonText = @"{root:{r:new Date(34343, 55)}}";

            var newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            var expected = @"<root><r><Date>34343</Date><Date>55</Date></r></root>";

            Assert.AreEqual(expected, newDoc.InnerXml);

            var json = SerializeXmlNode(newDoc);
            expected = @"{
  ""root"": {
    ""r"": {
      ""Date"": [
        ""34343"",
        ""55""
      ]
    }
  }
}";

            StringAssert.AreEqual(expected, json);
        }

        [Fact]
        public void ForceJsonArray()
        {
            var arrayXml = @"<root xmlns:json=""http://james.newtonking.com/projects/json"">
			  <person id=""1"">
				  <name>Alan</name>
				  <url>http://www.google.com</url>
				  <role json:Array=""true"">Admin</role>
			  </person>
			</root>";

            var arrayDoc = new XmlDocument();
            arrayDoc.LoadXml(arrayXml);

            var arrayJsonText = SerializeXmlNode(arrayDoc);
            var expected = @"{
  ""root"": {
    ""person"": {
      ""@id"": ""1"",
      ""name"": ""Alan"",
      ""url"": ""http://www.google.com"",
      ""role"": [
        ""Admin""
      ]
    }
  }
}";
            StringAssert.AreEqual(expected, arrayJsonText);

            arrayXml = @"<root xmlns:json=""http://james.newtonking.com/projects/json"">
			  <person id=""1"">
				  <name>Alan</name>
				  <url>http://www.google.com</url>
				  <role json:Array=""true"">Admin1</role>
				  <role json:Array=""true"">Admin2</role>
			  </person>
			</root>";

            arrayDoc = new XmlDocument();
            arrayDoc.LoadXml(arrayXml);

            arrayJsonText = SerializeXmlNode(arrayDoc);
            expected = @"{
  ""root"": {
    ""person"": {
      ""@id"": ""1"",
      ""name"": ""Alan"",
      ""url"": ""http://www.google.com"",
      ""role"": [
        ""Admin1"",
        ""Admin2""
      ]
    }
  }
}";
            StringAssert.AreEqual(expected, arrayJsonText);

            arrayXml = @"<root xmlns:json=""http://james.newtonking.com/projects/json"">
			  <person id=""1"">
				  <name>Alan</name>
				  <url>http://www.google.com</url>
				  <role json:Array=""false"">Admin1</role>
			  </person>
			</root>";

            arrayDoc = new XmlDocument();
            arrayDoc.LoadXml(arrayXml);

            arrayJsonText = SerializeXmlNode(arrayDoc);
            expected = @"{
  ""root"": {
    ""person"": {
      ""@id"": ""1"",
      ""name"": ""Alan"",
      ""url"": ""http://www.google.com"",
      ""role"": ""Admin1""
    }
  }
}";
            StringAssert.AreEqual(expected, arrayJsonText);

            arrayXml = @"<root>
			  <person id=""1"">
				  <name>Alan</name>
				  <url>http://www.google.com</url>
				  <role json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">Admin</role>
			  </person>
			</root>";

            arrayDoc = new XmlDocument();
            arrayDoc.LoadXml(arrayXml);

            arrayJsonText = SerializeXmlNode(arrayDoc);
            expected = @"{
  ""root"": {
    ""person"": {
      ""@id"": ""1"",
      ""name"": ""Alan"",
      ""url"": ""http://www.google.com"",
      ""role"": [
        ""Admin""
      ]
    }
  }
}";
            StringAssert.AreEqual(expected, arrayJsonText);
        }

        [Fact]
        public void MultipleRootPropertiesXmlDocument()
        {
            var json = @"{""count"": 773840,""photos"": null}";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => { JsonConvert.DeserializeXmlNode(json); },
                "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifying a DeserializeRootElementName. Path 'photos', line 1, position 26.");
        }

        [Fact]
        public void MultipleRootPropertiesXDocument()
        {
            var json = @"{""count"": 773840,""photos"": null}";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => { JsonConvert.DeserializeXNode(json); },
                "JSON root object has multiple properties. The root object must have a single property in order to create a valid XML document. Consider specifying a DeserializeRootElementName. Path 'photos', line 1, position 26.");
        }

        [Fact]
        public void MultipleRootPropertiesAddRootElement()
        {
            var json = @"{""count"": 773840,""photos"": 773840}";

            var newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot");

            Assert.AreEqual(@"<myRoot><count>773840</count><photos>773840</photos></myRoot>", newDoc.InnerXml);

            var newXDoc = JsonConvert.DeserializeXNode(json, "myRoot");

            Assert.AreEqual(@"<myRoot><count>773840</count><photos>773840</photos></myRoot>", newXDoc.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void NestedArrays()
        {
            var json = @"{
  ""available_sizes"": [
    [
      ""assets/images/resized/0001/1070/11070v1-max-150x150.jpg"",
      ""assets/images/resized/0001/1070/11070v1-max-150x150.jpg""
    ],
    [
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg"",
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg""
    ],
    [
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg""
    ]
  ]
}";

            var newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot");

            var xml = IndentXml(newDoc.InnerXml);

            StringAssert.AreEqual(@"<myRoot>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
  </available_sizes>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
</myRoot>", IndentXml(newDoc.InnerXml));

            var newXDoc = JsonConvert.DeserializeXNode(json, "myRoot");

            StringAssert.AreEqual(@"<myRoot>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
  </available_sizes>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
  <available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
</myRoot>", IndentXml(newXDoc.ToString(SaveOptions.DisableFormatting)));

            var newJson = JsonConvert.SerializeXmlNode(newDoc, Formatting.Indented);
            Console.WriteLine(newJson);
        }

        [Fact]
        public void RoundTripNestedArrays()
        {
            var json = @"{
  ""available_sizes"": [
    [
      ""assets/images/resized/0001/1070/11070v1-max-150x150.jpg"",
      ""assets/images/resized/0001/1070/11070v1-max-150x150.jpg""
    ],
    [
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg"",
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg""
    ],
    [
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg""
    ]
  ]
}";

            var newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot", true);

            StringAssert.AreEqual(@"<myRoot>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
  </available_sizes>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes json:Array=""true"">assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
</myRoot>", IndentXml(newDoc.InnerXml));

            var newXDoc = JsonConvert.DeserializeXNode(json, "myRoot", true);

            StringAssert.AreEqual(@"<myRoot>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes>
  </available_sizes>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
    <available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
  <available_sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <available_sizes json:Array=""true"">assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes>
  </available_sizes>
</myRoot>", IndentXml(newXDoc.ToString(SaveOptions.DisableFormatting)));

            var newJson = JsonConvert.SerializeXmlNode(newDoc, Formatting.Indented, true);
            StringAssert.AreEqual(json, newJson);
        }

        [Fact]
        public void MultipleNestedArraysToXml()
        {
            var json = @"{
  ""available_sizes"": [
    [
      [113, 150],
      ""assets/images/resized/0001/1070/11070v1-max-150x150.jpg""
    ],
    [
      [189, 250],
      ""assets/images/resized/0001/1070/11070v1-max-250x250.jpg""
    ],
    [
      [341, 450],
      ""assets/images/resized/0001/1070/11070v1-max-450x450.jpg""
    ]
  ]
}";

            var newDoc = JsonConvert.DeserializeXmlNode(json, "myRoot");

            Assert.AreEqual(@"<myRoot><available_sizes><available_sizes><available_sizes>113</available_sizes><available_sizes>150</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>189</available_sizes><available_sizes>250</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>341</available_sizes><available_sizes>450</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-450x450.jpg</available_sizes></available_sizes></myRoot>", newDoc.InnerXml);

            var newXDoc = JsonConvert.DeserializeXNode(json, "myRoot");

            Assert.AreEqual(@"<myRoot><available_sizes><available_sizes><available_sizes>113</available_sizes><available_sizes>150</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-150x150.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>189</available_sizes><available_sizes>250</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-250x250.jpg</available_sizes></available_sizes><available_sizes><available_sizes><available_sizes>341</available_sizes><available_sizes>450</available_sizes></available_sizes><available_sizes>assets/images/resized/0001/1070/11070v1-max-450x450.jpg</available_sizes></available_sizes></myRoot>", newXDoc.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void Encoding()
        {
            var doc = new XmlDocument();

            doc.LoadXml(@"<name>O""Connor</name>"); // i use "" so it will be easier to see the  problem

            var json = SerializeXmlNode(doc);
            StringAssert.AreEqual(@"{
  ""name"": ""O\""Connor""
}", json);
        }

        [Fact]
        public void SerializeComment()
        {
            var xml = @"<span class=""vevent"">
  <a class=""url"" href=""http://www.web2con.com/""><!-- Hi --><span>Text</span></a><!-- Hi! -->
</span>";
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var jsonText = SerializeXmlNode(doc);

            var expected = @"{
  ""span"": {
    ""@class"": ""vevent"",
    ""a"": {
      ""@class"": ""url"",
      ""@href"": ""http://www.web2con.com/""/* Hi */,
      ""span"": ""Text""
    }/* Hi! */
  }
}";

            StringAssert.AreEqual(expected, jsonText);

            var newDoc = (XmlDocument)DeserializeXmlNode(jsonText);
            Assert.AreEqual(@"<span class=""vevent""><a class=""url"" href=""http://www.web2con.com/""><!-- Hi --><span>Text</span></a><!-- Hi! --></span>", newDoc.InnerXml);
        }

        [Fact]
        public void SerializeExample()
        {
            var xml = @"<?xml version=""1.0"" standalone=""no""?>
			<root>
			  <person id=""1"">
				<name>Alan</name>
				<url>http://www.google.com</url>
			  </person>
			  <person id=""2"">
				<name>Louis</name>
				<url>http://www.yahoo.com</url>
			  </person>
			</root>";

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var jsonText = SerializeXmlNode(doc);
            // {
            //   "?xml": {
            //     "@version": "1.0",
            //     "@standalone": "no"
            //   },
            //   "root": {
            //     "person": [
            //       {
            //         "@id": "1",
            //         "name": "Alan",
            //         "url": "http://www.google.com"
            //       },
            //       {
            //         "@id": "2",
            //         "name": "Louis",
            //         "url": "http://www.yahoo.com"
            //       }
            //     ]
            //   }
            // }

            // format
            jsonText = JObject.Parse(jsonText).ToString();

            StringAssert.AreEqual(@"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@standalone"": ""no""
  },
  ""root"": {
    ""person"": [
      {
        ""@id"": ""1"",
        ""name"": ""Alan"",
        ""url"": ""http://www.google.com""
      },
      {
        ""@id"": ""2"",
        ""name"": ""Louis"",
        ""url"": ""http://www.yahoo.com""
      }
    ]
  }
}", jsonText);

            var newDoc = (XmlDocument)DeserializeXmlNode(jsonText);

            Assert.AreEqual(doc.InnerXml, newDoc.InnerXml);
        }

        [Fact]
        public void DeserializeExample()
        {
            var json = @"{
        ""?xml"": {
          ""@version"": ""1.0"",
          ""@standalone"": ""no""
        },
        ""root"": {
          ""person"": [
            {
              ""@id"": ""1"",
              ""name"": ""Alan"",
              ""url"": ""http://www.google.com""
            },
            {
              ""@id"": ""2"",
              ""name"": ""Louis"",
              ""url"": ""http://www.yahoo.com""
            }
          ]
        }
      }";

            var doc = (XmlDocument)DeserializeXmlNode(json);
            // <?xml version="1.0" standalone="no"?>
            // <root>
            //   <person id="1">
            //   <name>Alan</name>
            //   <url>http://www.google.com</url>
            //   </person>
            //   <person id="2">
            //   <name>Louis</name>
            //   <url>http://www.yahoo.com</url>
            //   </person>
            // </root>

            StringAssert.AreEqual(@"<?xml version=""1.0"" standalone=""no""?><root><person id=""1""><name>Alan</name><url>http://www.google.com</url></person><person id=""2""><name>Louis</name><url>http://www.yahoo.com</url></person></root>", doc.InnerXml);
        }

        [Fact]
        public void EscapingNames()
        {
            var json = @"{
              ""root!"": {
                ""person!"": [
                  {
                    ""@id!"": ""1"",
                    ""name!"": ""Alan"",
                    ""url!"": ""http://www.google.com""
                  },
                  {
                    ""@id!"": ""2"",
                    ""name!"": ""Louis"",
                    ""url!"": ""http://www.yahoo.com""
                  }
                ]
              }
            }";

            var doc = (XmlDocument)DeserializeXmlNode(json);

            Assert.AreEqual(@"<root_x0021_><person_x0021_ id_x0021_=""1""><name_x0021_>Alan</name_x0021_><url_x0021_>http://www.google.com</url_x0021_></person_x0021_><person_x0021_ id_x0021_=""2""><name_x0021_>Louis</name_x0021_><url_x0021_>http://www.yahoo.com</url_x0021_></person_x0021_></root_x0021_>", doc.InnerXml);

            var json2 = SerializeXmlNode(doc);

            StringAssert.AreEqual(@"{
  ""root!"": {
    ""person!"": [
      {
        ""@id!"": ""1"",
        ""name!"": ""Alan"",
        ""url!"": ""http://www.google.com""
      },
      {
        ""@id!"": ""2"",
        ""name!"": ""Louis"",
        ""url!"": ""http://www.yahoo.com""
      }
    ]
  }
}", json2);
        }

        [Fact]
        public void SerializeDeserializeMetadataProperties()
        {
            var circularDictionary = new PreserveReferencesHandlingTests.CircularDictionary();
            circularDictionary.Add("other", new PreserveReferencesHandlingTests.CircularDictionary { { "blah", null } });
            circularDictionary.Add("self", circularDictionary);

            var json = JsonConvert.SerializeObject(circularDictionary, Formatting.Indented,
                new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.All });

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""other"": {
    ""$id"": ""2"",
    ""blah"": null
  },
  ""self"": {
    ""$ref"": ""1""
  }
}", json);

            var node = DeserializeXmlNode(json, "root");
            var xml = GetIndentedInnerXml(node);
            var expected = @"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"" json:id=""1"">
  <other json:id=""2"">
    <blah />
  </other>
  <self json:ref=""1"" />
</root>";

            StringAssert.AreEqual(expected, xml);

            var xmlJson = SerializeXmlNode(node);
            var expectedXmlJson = @"{
  ""root"": {
    ""$id"": ""1"",
    ""other"": {
      ""$id"": ""2"",
      ""blah"": null
    },
    ""self"": {
      ""$ref"": ""1""
    }
  }
}";

            StringAssert.AreEqual(expectedXmlJson, xmlJson);
        }

        [Fact]
        public void SerializeDeserializeMetadataArray()
        {
            var json = @"{
  ""$id"": ""1"",
  ""$values"": [
    ""1"",
    ""2"",
    ""3"",
    ""4"",
    ""5""
  ]
}";

            XmlNode node = JsonConvert.DeserializeXmlNode(json, "root");
            var xml = GetIndentedInnerXml(node);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"" json:id=""1"">
  <values xmlns=""http://james.newtonking.com/projects/json"">1</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">2</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">3</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">4</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">5</values>
</root>", xml);

            var newJson = JsonConvert.SerializeXmlNode(node, Formatting.Indented, true);

            StringAssert.AreEqual(json, newJson);
        }

        [Fact]
        public void SerializeDeserializeMetadataArrayNoId()
        {
            var json = @"{
  ""$values"": [
    ""1"",
    ""2"",
    ""3"",
    ""4"",
    ""5""
  ]
}";

            XmlNode node = JsonConvert.DeserializeXmlNode(json, "root");
            var xml = GetIndentedInnerXml(node);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"">
  <values xmlns=""http://james.newtonking.com/projects/json"">1</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">2</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">3</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">4</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">5</values>
</root>", xml);

            var newJson = JsonConvert.SerializeXmlNode(node, Formatting.Indented, true);

            Console.WriteLine(newJson);

            StringAssert.AreEqual(json, newJson);
        }

        [Fact]
        public void SerializeDeserializeMetadataArrayWithIdLast()
        {
            var json = @"{
  ""$values"": [
    ""1"",
    ""2"",
    ""3"",
    ""4"",
    ""5""
  ],
  ""$id"": ""1""
}";

            XmlNode node = JsonConvert.DeserializeXmlNode(json, "root");
            var xml = GetIndentedInnerXml(node);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"" json:id=""1"">
  <values xmlns=""http://james.newtonking.com/projects/json"">1</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">2</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">3</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">4</values>
  <values xmlns=""http://james.newtonking.com/projects/json"">5</values>
</root>", xml);

            var newJson = JsonConvert.SerializeXmlNode(node, Formatting.Indented, true);

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""$values"": [
    ""1"",
    ""2"",
    ""3"",
    ""4"",
    ""5""
  ]
}", newJson);
        }

        [Fact]
        public void SerializeMetadataPropertyWithBadValue()
        {
            var json = @"{
  ""$id"": []
}";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => { JsonConvert.DeserializeXmlNode(json, "root"); },
                "Unexpected JsonToken: StartArray. Path '$id', line 2, position 10.");
        }

        [Fact]
        public void SerializeDeserializeMetadataWithNullValue()
        {
            var json = @"{
  ""$id"": null
}";

            XmlNode node = JsonConvert.DeserializeXmlNode(json, "root");
            var xml = GetIndentedInnerXml(node);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"" json:id="""" />", xml);

            var newJson = JsonConvert.SerializeXmlNode(node, Formatting.Indented, true);

            StringAssert.AreEqual(@"{
  ""$id"": """"
}", newJson);
        }

        [Fact]
        public void SerializeDeserializeMetadataArrayNull()
        {
            var json = @"{
  ""$id"": ""1"",
  ""$values"": null
}";

            XmlNode node = JsonConvert.DeserializeXmlNode(json, "root");
            var xml = GetIndentedInnerXml(node);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?>
<root xmlns:json=""http://james.newtonking.com/projects/json"" json:id=""1"">
  <values xmlns=""http://james.newtonking.com/projects/json"" />
</root>", xml);

            var newJson = JsonConvert.SerializeXmlNode(node, Formatting.Indented, true);

            StringAssert.AreEqual(json, newJson);
        }

        [Fact]
        public void EmptyPropertyName()
        {
            var json = @"{
  ""8452309520V2"": {
    """": {
      ""CLIENT"": {
        ""ID_EXPIRATION_1"": {
          ""VALUE"": ""12/12/2000"",
          ""DATATYPE"": ""D"",
          ""MSG"": ""Missing Identification Exp. Date 1""
        },
        ""ID_ISSUEDATE_1"": {
          ""VALUE"": """",
          ""DATATYPE"": ""D"",
          ""MSG"": ""Missing Identification Issue Date 1""
        }
      }
    },
    ""457463534534"": {
      ""ACCOUNT"": {
        ""FUNDING_SOURCE"": {
          ""VALUE"": ""FS0"",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Source of Funds""
        }
      }
    }
  }
}{
  ""34534634535345"": {
    """": {
      ""CLIENT"": {
        ""ID_NUMBER_1"": {
          ""VALUE"": """",
          ""DATATYPE"": ""S"",
          ""MSG"": ""Missing Picture ID""
        },
        ""ID_EXPIRATION_1"": {
          ""VALUE"": ""12/12/2000"",
          ""DATATYPE"": ""D"",
          ""MSG"": ""Missing Picture ID""
        },
        ""WALK_IN"": {
          ""VALUE"": """",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Walk in""
        },
        ""PERSONAL_MEETING"": {
          ""VALUE"": ""PM1"",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Met Client in Person""
        },
        ""ID_ISSUEDATE_1"": {
          ""VALUE"": """",
          ""DATATYPE"": ""D"",
          ""MSG"": ""Missing Picture ID""
        },
        ""PHOTO_ID"": {
          ""VALUE"": """",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Picture ID""
        },
        ""ID_TYPE_1"": {
          ""VALUE"": """",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Picture ID""
        }
      }
    },
    ""45635624523"": {
      ""ACCOUNT"": {
        ""FUNDING_SOURCE"": {
          ""VALUE"": ""FS1"",
          ""DATATYPE"": ""L"",
          ""MSG"": ""Missing Source of Funds""
        }
      }
    }
  }
}";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => { DeserializeXmlNode(json); },
                "XmlNodeConverter cannot convert JSON with an empty property name to XML. Path '8452309520V2.', line 3, position 9.");
        }

        [Fact]
        public void SingleItemArrayPropertySerialization()
        {
            var product = new Product
            {
              Name = "Apple",
              ExpiryDate = new DateTime(2008, 12, 28, 0, 0, 0, DateTimeKind.Utc),
              Price = 3.99M,
              Sizes = new string[] { "Small" }
            };

            var output = JsonConvert.SerializeObject(product, new IsoDateTimeConverter());

            var xmlProduct = JsonConvert.DeserializeXmlNode(output, "product", true);

            StringAssert.AreEqual(@"<product>
  <Name>Apple</Name>
  <ExpiryDate>2008-12-28T00:00:00Z</ExpiryDate>
  <Price>3.99</Price>
  <Sizes json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">Small</Sizes>
</product>", IndentXml(xmlProduct.InnerXml));

            var output2 = JsonConvert.SerializeXmlNode(xmlProduct.DocumentElement, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""product"": {
    ""Name"": ""Apple"",
    ""ExpiryDate"": ""2008-12-28T00:00:00Z"",
    ""Price"": ""3.99"",
    ""Sizes"": [
      ""Small""
    ]
  }
}", output2);
        }

        public class TestComplexArrayClass
        {
            public string Name { get; set; }
            public IList<Product> Products { get; set; }
        }

        [Fact]
        public void ComplexSingleItemArrayPropertySerialization()
        {
            var o = new TestComplexArrayClass
            {
                Name = "Hi",
                Products = new List<Product>
                {
                    new() { Name = "First" }
                }
            };

            var output = JsonConvert.SerializeObject(o, new IsoDateTimeConverter());

            var xmlProduct = JsonConvert.DeserializeXmlNode(output, "test", true);

            StringAssert.AreEqual(@"<test>
  <Name>Hi</Name>
  <Products json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <Name>First</Name>
    <ExpiryDate>2000-01-01T00:00:00Z</ExpiryDate>
    <Price>0</Price>
    <Sizes />
  </Products>
</test>", IndentXml(xmlProduct.InnerXml));

            var output2 = JsonConvert.SerializeXmlNode(xmlProduct.DocumentElement, Formatting.Indented, true);

            StringAssert.AreEqual(@"{
  ""Name"": ""Hi"",
  ""Products"": [
    {
      ""Name"": ""First"",
      ""ExpiryDate"": ""2000-01-01T00:00:00Z"",
      ""Price"": ""0"",
      ""Sizes"": null
    }
  ]
}", output2);
        }

        [Fact]
        public void OmitRootObject()
        {
            var xml = @"<test>
  <Name>Hi</Name>
  <Name>Hi</Name>
  <Products json:Array=""true"" xmlns:json=""http://james.newtonking.com/projects/json"">
    <Name>First</Name>
    <ExpiryDate>2000-01-01T00:00:00Z</ExpiryDate>
    <Price>0</Price>
    <Sizes />
  </Products>
</test>";

            var d = new XmlDocument();
            d.LoadXml(xml);

            var output = JsonConvert.SerializeXmlNode(d, Formatting.Indented, true);

            StringAssert.AreEqual(@"{
  ""Name"": [
    ""Hi"",
    ""Hi""
  ],
  ""Products"": [
    {
      ""Name"": ""First"",
      ""ExpiryDate"": ""2000-01-01T00:00:00Z"",
      ""Price"": ""0"",
      ""Sizes"": null
    }
  ]
}", output);
        }

        [Fact]
        public void EmtpyElementWithArrayAttributeShouldWriteAttributes()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<root xmlns:json=""http://james.newtonking.com/projects/json"">
<A>
<B name=""sample"" json:Array=""true""/>
<C></C>
<C></C>
</A>
</root>";

            var d = new XmlDocument();
            d.LoadXml(xml);

            var json = JsonConvert.SerializeXmlNode(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""?xml"": {
    ""@version"": ""1.0"",
    ""@encoding"": ""utf-8""
  },
  ""root"": {
    ""A"": {
      ""B"": [
        {
          ""@name"": ""sample""
        }
      ],
      ""C"": [
        """",
        """"
      ]
    }
  }
}", json);

            var d2 = JsonConvert.DeserializeXmlNode(json);

            StringAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <A>
    <B name=""sample"" />
    <C></C>
    <C></C>
  </A>
</root>", ToStringWithDeclaration(d2, true));
        }

        [Fact]
        public void EmtpyElementWithArrayAttributeShouldWriteElement()
        {
            var xml = @"<root>
<Reports d1p1:Array=""true"" xmlns:d1p1=""http://james.newtonking.com/projects/json"" />
</root>";

            var d = new XmlDocument();
            d.LoadXml(xml);

            var json = JsonConvert.SerializeXmlNode(d, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""root"": {
    ""Reports"": [
      {}
    ]
  }
}", json);
        }

        [Fact]
        public void DeserializeNonInt64IntegerValues()
        {
            var dict = new Dictionary<string, object> { { "Int16", (short)1 }, { "Float", 2f }, { "Int32", 3 } };
            var obj = JObject.FromObject(dict);
            var serializer = JsonSerializer.Create(new JsonSerializerSettings { Converters = { new XmlNodeConverter { DeserializeRootElementName = "root" } } });
            using (var reader = obj.CreateReader())
            {
                var value = (XmlDocument)serializer.Deserialize(reader, typeof(XmlDocument));

                Assert.AreEqual(@"<root><Int16>1</Int16><Float>2</Float><Int32>3</Int32></root>", value.InnerXml);
            }
        }

        [Fact]
        public void DeserializingBooleanValues()
        {
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(@"{root:{""@booleanType"":true}}"));
            var xml = new MemoryStream();

            JsonBodyToSoapXml(ms, xml);

            var xmlString = System.Text.Encoding.UTF8.GetString(xml.ToArray());

            Assert.AreEqual(@"﻿<?xml version=""1.0"" encoding=""utf-8""?><root booleanType=""true"" />", xmlString);
        }

        [Fact]
        public void IgnoreCultureForTypedAttributes()
        {
            var originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;

            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");

                // in russian culture value 12.27 will be written as 12,27

                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    Converters = { new XmlNodeConverter() },
                });

                var json = new StringBuilder(@"{
                    ""metrics"": {
                        ""type"": ""CPULOAD"",
                        ""@value"": 12.27
                    }
                }");

                using (var stringReader = new StringReader(json.ToString()))
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    var document = (XmlDocument)serializer.Deserialize(jsonReader, typeof(XmlDocument));
                    StringAssert.AreEqual(@"<metrics value=""12.27""><type>CPULOAD</type></metrics>", document.OuterXml);
                }
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void NullAttributeValue()
        {
            var node = JsonConvert.DeserializeXmlNode(@"{
                    ""metrics"": {
                        ""type"": ""CPULOAD"",
                        ""@value"": null
                    }
                }");

            StringAssert.AreEqual(@"<metrics value=""""><type>CPULOAD</type></metrics>", node.OuterXml);
        }

        [Fact]
        public void NonStandardAttributeValues()
        {
            var o = new JObject
            {
                new JProperty("root", new JObject
                {
                    new JProperty("@uri", new JValue(new Uri("http://localhost/"))),
                    new JProperty("@time_span", new JValue(TimeSpan.FromMinutes(1))),
                    new JProperty("@bytes", new JValue(System.Text.Encoding.UTF8.GetBytes("Hello world")))
                })
            };

            using (var jsonReader = o.CreateReader())
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    Converters = { new XmlNodeConverter() },
                });

                var document = (XmlDocument)serializer.Deserialize(jsonReader, typeof(XmlDocument));

                StringAssert.AreEqual(@"<root uri=""http://localhost/"" time_span=""00:01:00"" bytes=""SGVsbG8gd29ybGQ="" />", document.OuterXml);
            }
        }

        [Fact]
        public void NonStandardElementsValues()
        {
            var o = new JObject
            {
                new JProperty("root", new JObject
                {
                    new JProperty("uri", new JValue(new Uri("http://localhost/"))),
                    new JProperty("time_span", new JValue(TimeSpan.FromMinutes(1))),
                    new JProperty("bytes", new JValue(System.Text.Encoding.UTF8.GetBytes("Hello world")))
                })
            };

            using (var jsonReader = o.CreateReader())
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    Converters = { new XmlNodeConverter() },
                });

                var document = (XmlDocument)serializer.Deserialize(jsonReader, typeof(XmlDocument));

                StringAssert.AreEqual(@"<root><uri>http://localhost/</uri><time_span>00:01:00</time_span><bytes>SGVsbG8gd29ybGQ=</bytes></root>", document.OuterXml);
            }
        }

        private static void JsonBodyToSoapXml(Stream json, Stream xml)
        {
            var settings = new Argon.JsonSerializerSettings();
            settings.Converters.Add(new Argon.Converters.XmlNodeConverter());
            var serializer = Argon.JsonSerializer.Create(settings);
            using (var reader = new Argon.JsonTextReader(new System.IO.StreamReader(json)))
            {
                var doc = (XmlDocument)serializer.Deserialize(reader, typeof(XmlDocument));
                if (reader.Read() && reader.TokenType != JsonToken.Comment)
                {
                    throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
                }
                using (var writer = XmlWriter.Create(xml))
                {
                    doc.Save(writer);
                }
            }
        }

        [Fact]
        public void DeserializeXNodeDefaultNamespace()
        {
            var xaml = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:toolkit=""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"" Style=""{StaticResource trimFormGrid}"" x:Name=""TrimObjectForm"">
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width=""63*"" />
    <ColumnDefinition Width=""320*"" />
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions xmlns="""">
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
  </Grid.RowDefinitions>
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordTypedTitle"" Grid.Column=""1"" Grid.Row=""0"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordExternalReference"" Grid.Column=""1"" Grid.Row=""1"" xmlns="""" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateCreated"" Grid.Column=""1"" Grid.Row=""2"" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateDue"" Grid.Column=""1"" Grid.Row=""3"" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Author, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAuthor"" Grid.Column=""1"" Grid.Row=""4"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Container, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordContainer"" Grid.Column=""1"" Grid.Row=""5"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordIsEnclosed"" Grid.Column=""1"" Grid.Row=""6"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAssignee"" Grid.Column=""1"" Grid.Row=""7"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Title (Free Text Part)"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""0"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""External ID"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""1"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Created"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""2"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Due"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""3"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Author"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""4"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Container"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""5"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Enclosed?"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""6"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Assignee"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""7"" xmlns="""" />
</Grid>";

            var json = JsonConvert.SerializeXNode(XDocument.Parse(xaml), Formatting.Indented);

            var expectedJson = @"{
  ""Grid"": {
    ""@xmlns"": ""http://schemas.microsoft.com/winfx/2006/xaml/presentation"",
    ""@xmlns:x"": ""http://schemas.microsoft.com/winfx/2006/xaml"",
    ""@xmlns:toolkit"": ""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"",
    ""@Style"": ""{StaticResource trimFormGrid}"",
    ""@x:Name"": ""TrimObjectForm"",
    ""Grid.ColumnDefinitions"": {
      ""ColumnDefinition"": [
        {
          ""@Width"": ""63*""
        },
        {
          ""@Width"": ""320*""
        }
      ]
    },
    ""Grid.RowDefinitions"": {
      ""@xmlns"": """",
      ""RowDefinition"": [
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null
      ]
    },
    ""TextBox"": [
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordTypedTitle"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""0"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordExternalReference"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""1"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Author, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordAuthor"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""4"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Container, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordContainer"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""5"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordIsEnclosed"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""6"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordAssignee"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""7"",
        ""@xmlns"": """"
      }
    ],
    ""toolkit:DatePicker"": [
      {
        ""@Style"": ""{StaticResource trimFormGrid_DP}"",
        ""@Value"": ""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordDateCreated"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""2""
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_DP}"",
        ""@Value"": ""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordDateDue"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""3""
      }
    ],
    ""TextBlock"": [
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Title (Free Text Part)"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""0"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""External ID"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""1"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Date Created"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""2"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Date Due"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""3"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Author"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""4"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Container"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""5"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Enclosed?"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""6"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Assignee"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""7"",
        ""@xmlns"": """"
      }
    ]
  }
}";

            StringAssert.AreEqual(expectedJson, json);

            XNode node = JsonConvert.DeserializeXNode(json);

            var xaml2 = node.ToString();

            var expectedXaml = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:toolkit=""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"" Style=""{StaticResource trimFormGrid}"" x:Name=""TrimObjectForm"">
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width=""63*"" />
    <ColumnDefinition Width=""320*"" />
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions xmlns="""">
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
  </Grid.RowDefinitions>
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordTypedTitle"" Grid.Column=""1"" Grid.Row=""0"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordExternalReference"" Grid.Column=""1"" Grid.Row=""1"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Author, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAuthor"" Grid.Column=""1"" Grid.Row=""4"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Container, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordContainer"" Grid.Column=""1"" Grid.Row=""5"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordIsEnclosed"" Grid.Column=""1"" Grid.Row=""6"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAssignee"" Grid.Column=""1"" Grid.Row=""7"" xmlns="""" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateCreated"" Grid.Column=""1"" Grid.Row=""2"" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateDue"" Grid.Column=""1"" Grid.Row=""3"" />
  <TextBlock Grid.Column=""0"" Text=""Title (Free Text Part)"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""0"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""External ID"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""1"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Created"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""2"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Due"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""3"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Author"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""4"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Container"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""5"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Enclosed?"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""6"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Assignee"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""7"" xmlns="""" />
</Grid>";

            StringAssert.AreEqual(expectedXaml, xaml2);
        }

        [Fact]
        public void DeserializeXmlNodeDefaultNamespace()
        {
            var xaml = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:toolkit=""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"" Style=""{StaticResource trimFormGrid}"" x:Name=""TrimObjectForm"">
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width=""63*"" />
    <ColumnDefinition Width=""320*"" />
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions xmlns="""">
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
  </Grid.RowDefinitions>
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordTypedTitle"" Grid.Column=""1"" Grid.Row=""0"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordExternalReference"" Grid.Column=""1"" Grid.Row=""1"" xmlns="""" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateCreated"" Grid.Column=""1"" Grid.Row=""2"" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateDue"" Grid.Column=""1"" Grid.Row=""3"" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Author, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAuthor"" Grid.Column=""1"" Grid.Row=""4"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Container, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordContainer"" Grid.Column=""1"" Grid.Row=""5"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordIsEnclosed"" Grid.Column=""1"" Grid.Row=""6"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAssignee"" Grid.Column=""1"" Grid.Row=""7"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Title (Free Text Part)"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""0"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""External ID"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""1"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Created"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""2"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Due"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""3"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Author"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""4"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Container"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""5"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Enclosed?"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""6"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Assignee"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""7"" xmlns="""" />
</Grid>";

            var document = new XmlDocument();
            document.LoadXml(xaml);

            var json = JsonConvert.SerializeXmlNode(document, Formatting.Indented);

            var expectedJson = @"{
  ""Grid"": {
    ""@xmlns"": ""http://schemas.microsoft.com/winfx/2006/xaml/presentation"",
    ""@xmlns:x"": ""http://schemas.microsoft.com/winfx/2006/xaml"",
    ""@xmlns:toolkit"": ""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"",
    ""@Style"": ""{StaticResource trimFormGrid}"",
    ""@x:Name"": ""TrimObjectForm"",
    ""Grid.ColumnDefinitions"": {
      ""ColumnDefinition"": [
        {
          ""@Width"": ""63*""
        },
        {
          ""@Width"": ""320*""
        }
      ]
    },
    ""Grid.RowDefinitions"": {
      ""@xmlns"": """",
      ""RowDefinition"": [
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null
      ]
    },
    ""TextBox"": [
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordTypedTitle"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""0"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordExternalReference"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""1"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Author, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordAuthor"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""4"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Container, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordContainer"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""5"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordIsEnclosed"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""6"",
        ""@xmlns"": """"
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_TB}"",
        ""@Text"": ""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordAssignee"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""7"",
        ""@xmlns"": """"
      }
    ],
    ""toolkit:DatePicker"": [
      {
        ""@Style"": ""{StaticResource trimFormGrid_DP}"",
        ""@Value"": ""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordDateCreated"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""2""
      },
      {
        ""@Style"": ""{StaticResource trimFormGrid_DP}"",
        ""@Value"": ""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"",
        ""@Name"": ""RecordDateDue"",
        ""@Grid.Column"": ""1"",
        ""@Grid.Row"": ""3""
      }
    ],
    ""TextBlock"": [
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Title (Free Text Part)"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""0"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""External ID"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""1"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Date Created"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""2"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Date Due"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""3"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Author"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""4"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Container"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""5"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Enclosed?"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""6"",
        ""@xmlns"": """"
      },
      {
        ""@Grid.Column"": ""0"",
        ""@Text"": ""Assignee"",
        ""@Style"": ""{StaticResource trimFormGrid_LBL}"",
        ""@Grid.Row"": ""7"",
        ""@xmlns"": """"
      }
    ]
  }
}";

            StringAssert.AreEqual(expectedJson, json);

            XmlNode node = JsonConvert.DeserializeXmlNode(json);

            var sw = new StringWriter();
            var writer = XmlWriter.Create(sw, new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            });
            node.WriteTo(writer);
            writer.Flush();

            var xaml2 = sw.ToString();

            var expectedXaml = @"<Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:toolkit=""clr-namespace:Microsoft.Phone.Controls;assembly=Microsoft.Phone.Controls.Toolkit"" Style=""{StaticResource trimFormGrid}"" x:Name=""TrimObjectForm"">
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width=""63*"" />
    <ColumnDefinition Width=""320*"" />
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions xmlns="""">
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
    <RowDefinition />
  </Grid.RowDefinitions>
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding TypedTitle, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordTypedTitle"" Grid.Column=""1"" Grid.Row=""0"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding ExternalReference, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordExternalReference"" Grid.Column=""1"" Grid.Row=""1"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Author, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAuthor"" Grid.Column=""1"" Grid.Row=""4"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Container, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordContainer"" Grid.Column=""1"" Grid.Row=""5"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding IsEnclosed, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordIsEnclosed"" Grid.Column=""1"" Grid.Row=""6"" xmlns="""" />
  <TextBox Style=""{StaticResource trimFormGrid_TB}"" Text=""{Binding Assignee, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordAssignee"" Grid.Column=""1"" Grid.Row=""7"" xmlns="""" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateCreated, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateCreated"" Grid.Column=""1"" Grid.Row=""2"" />
  <toolkit:DatePicker Style=""{StaticResource trimFormGrid_DP}"" Value=""{Binding DateDue, Converter={StaticResource trimPropertyConverter}}"" Name=""RecordDateDue"" Grid.Column=""1"" Grid.Row=""3"" />
  <TextBlock Grid.Column=""0"" Text=""Title (Free Text Part)"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""0"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""External ID"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""1"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Created"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""2"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Date Due"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""3"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Author"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""4"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Container"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""5"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Enclosed?"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""6"" xmlns="""" />
  <TextBlock Grid.Column=""0"" Text=""Assignee"" Style=""{StaticResource trimFormGrid_LBL}"" Grid.Row=""7"" xmlns="""" />
</Grid>";

            StringAssert.AreEqual(expectedXaml, xaml2);
        }

        [Fact]
        public void DeserializeAttributePropertyNotAtStart()
        {
            var json = @"{""item"": {""@action"": ""update"", ""@itemid"": ""1"", ""elements"": [{""@action"": ""none"", ""@id"": ""2""},{""@action"": ""none"", ""@id"": ""3""}],""@description"": ""temp""}}";

            var xmldoc = JsonConvert.DeserializeXmlNode(json);

            Assert.AreEqual(@"<item action=""update"" itemid=""1"" description=""temp""><elements action=""none"" id=""2"" /><elements action=""none"" id=""3"" /></item>", xmldoc.InnerXml);
        }

        [Fact]
        public void SerializingXmlNamespaceScope()
        {
            var xmlString = @"<root xmlns=""http://www.example.com/ns"">
  <a/>
  <bns:b xmlns:bns=""http://www.example.com/ns""/>
  <c/>
</root>";

            var xml = XElement.Parse(xmlString);

            var json1 = JsonConvert.SerializeObject(xml);

            Assert.AreEqual(@"{""root"":{""@xmlns"":""http://www.example.com/ns"",""a"":null,""bns:b"":{""@xmlns:bns"":""http://www.example.com/ns""},""c"":null}}", json1);
            var xml1 = new XmlDocument();
            xml1.LoadXml(xmlString);

            var json2 = JsonConvert.SerializeObject(xml1);

            Assert.AreEqual(@"{""root"":{""@xmlns"":""http://www.example.com/ns"",""a"":null,""bns:b"":{""@xmlns:bns"":""http://www.example.com/ns""},""c"":null}}", json2);
        }

        public class NullableXml
        {
            public string Name;
            public XElement notNull;
            public XElement isNull;
        }

        [Fact]
        public void SerializeAndDeserializeNullableXml()
        {
            var xml = new NullableXml { Name = "test", notNull = XElement.Parse("<root>test</root>") };
            var json = JsonConvert.SerializeObject(xml);

            var w2 = JsonConvert.DeserializeObject<NullableXml>(json);
            Assert.AreEqual(xml.Name, w2.Name);
            Assert.AreEqual(xml.isNull, w2.isNull);
            Assert.AreEqual(xml.notNull.ToString(), w2.notNull.ToString());
        }

        [Fact]
        public void SerializeAndDeserializeXElementWithNamespaceInChildrenRootDontHaveNameSpace()
        {
            var xmlString = @"<root>
                              <b xmlns='http://www.example.com/ns'>Asd</b>
                              <c>AAA</c>
                              <test>adad</test>
                              </root>";

            var xml = XElement.Parse(xmlString);

            var json1 = JsonConvert.SerializeXNode(xml);
            var xmlBack = JsonConvert.DeserializeObject<XElement>(json1);

            var equals = XElement.DeepEquals(xmlBack, xml);
            Assert.IsTrue(equals);
        }

        [Fact]
        public void SerializeAndDeserializeXmlElementWithNamespaceInChildrenRootDontHaveNameSpace()
        {
            var xmlString = @"<root>
                              <b xmlns='http://www.example.com/ns'>Asd</b>
                              <c>AAA</c>
                              <test>adad</test>
                              </root>";

            var xml = new XmlDocument();
            xml.LoadXml(xmlString);

            var json1 = JsonConvert.SerializeXmlNode(xml);
            var xmlBack = JsonConvert.DeserializeObject<XmlDocument>(json1);

            Assert.AreEqual(@"<root><b xmlns=""http://www.example.com/ns"">Asd</b><c>AAA</c><test>adad</test></root>", xmlBack.OuterXml);
        }

        [Fact]
        public void DeserializeBigInteger()
        {
            var json = "{\"DocumentId\":13779965364495889899 }";

            var node = JsonConvert.DeserializeXmlNode(json);

            Assert.AreEqual("<DocumentId>13779965364495889899</DocumentId>", node.OuterXml);

            var json2 = JsonConvert.SerializeXmlNode(node);

            Assert.AreEqual(@"{""DocumentId"":""13779965364495889899""}", json2);
        }

        [Fact]
        public void DeserializeXmlIncompatibleCharsInPropertyName()
        {
            var json = "{\"%name\":\"value\"}";

            var node = JsonConvert.DeserializeXmlNode(json);

            Assert.AreEqual("<_x0025_name>value</_x0025_name>", node.OuterXml);

            var json2 = JsonConvert.SerializeXmlNode(node);

            Assert.AreEqual(json, json2);
        }

        [Fact]
        public void RootPropertyError()
        {
            var json = @"{
  ""$id"": ""1"",
  ""AOSLocaleName"": ""en-US"",
  ""AXLanguage"": ""EN-AU"",
  ""Company"": ""AURE"",
  ""CompanyTimeZone"": 8,
  ""CurrencyInfo"": {
    ""$id"": ""2"",
    ""CurrencyCode"": ""AUD"",
    ""Description"": ""Australian Dollar"",
    ""ExchangeRate"": 100.0,
    ""ISOCurrencyCode"": ""AUD"",
    ""Prefix"": """",
    ""Suffix"": """"
  },
  ""IsSysAdmin"": true,
  ""UserId"": ""lamar.miller"",
  ""UserPreferredCalendar"": 0,
  ""UserPreferredTimeZone"": 8
}";

            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeXmlNode(json),
                "JSON root object has property '$id' that will be converted to an attribute. A root object cannot have any attribute properties. Consider specifying a DeserializeRootElementName. Path '$id', line 2, position 12.");
        }

        [Fact]
        public void SerializeEmptyNodeAndOmitRoot()
        {
            var xmlString = @"<myemptynode />";

            var xml = new XmlDocument();
            xml.LoadXml(xmlString);

            var json = JsonConvert.SerializeXmlNode(xml, Formatting.Indented, true);

            Assert.AreEqual("null", json);
        }

        [Fact]
        public void Serialize_XDocument_NoRoot()
        {
            var d = new XDocument();

            var json = JsonConvert.SerializeXNode(d);

            Assert.AreEqual(@"{}", json);
        }

        [Fact]
        public void Deserialize_XDocument_NoRoot()
        {
            var d = JsonConvert.DeserializeXNode(@"{}");

            Assert.AreEqual(null, d.Root);
            Assert.AreEqual(null, d.Declaration);
        }

        [Fact]
        public void Serialize_XDocument_NoRootWithDeclaration()
        {
            var d = new XDocument
            {
              Declaration = new XDeclaration("Version!", "Encoding!", "Standalone!")
            };

            var json = JsonConvert.SerializeXNode(d);

            Assert.AreEqual(@"{""?xml"":{""@version"":""Version!"",""@encoding"":""Encoding!"",""@standalone"":""Standalone!""}}", json);
        }

        [Fact]
        public void Deserialize_XDocument_NoRootWithDeclaration()
        {
            var d = JsonConvert.DeserializeXNode(@"{""?xml"":{""@version"":""Version!"",""@encoding"":""Encoding!"",""@standalone"":""Standalone!""}}");

            Assert.AreEqual(null, d.Root);
            Assert.AreEqual("Version!", d.Declaration.Version);
            Assert.AreEqual("Encoding!", d.Declaration.Encoding);
            Assert.AreEqual("Standalone!", d.Declaration.Standalone);
        }

        [Fact]
        public void DateTimeToXml_Unspecified()
        {
            var json = @"{""CreatedDate"": ""2014-01-23T00:00:00""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual("2014-01-23T00:00:00", dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Unspecified: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Fact]
        public void DateTimeToXml_Utc()
        {
            var json = @"{""CreatedDate"": ""2014-01-23T00:00:00Z""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual("2014-01-23T00:00:00Z", dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Utc: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Fact]
        public void DateTimeToXml_Local()
        {
            var dt = DateTime.Parse("2014-01-23T00:00:00+01:00");

            var json = @"{""CreatedDate"": ""2014-01-23T00:00:00+01:00""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual(dt.ToString("yyyy-MM-ddTHH:mm:sszzzzzzz", CultureInfo.InvariantCulture), dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Local: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Fact]
        public void DateTimeToXml_Unspecified_Precision()
        {
            var json = @"{""CreatedDate"": ""2014-01-23T00:00:00.1234567""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual("2014-01-23T00:00:00.1234567", dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Unspecified: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Fact]
        public void DateTimeToXml_Utc_Precision()
        {
            var json = @"{""CreatedDate"": ""2014-01-23T00:00:00.1234567Z""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual("2014-01-23T00:00:00.1234567Z", dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Utc: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Fact]
        public void DateTimeToXml_Local_Precision()
        {
            var dt = DateTime.Parse("2014-01-23T00:00:00.1234567+01:00");

            var json = @"{""CreatedDate"": ""2014-01-23T00:00:00.1234567+01:00""}";
            var dxml = JsonConvert.DeserializeXNode(json, "root");
            Assert.AreEqual(dt.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture), dxml.Root.Element("CreatedDate").Value);

            Console.WriteLine("DateTimeToXml_Local: " + dxml.Root.Element("CreatedDate").Value);
        }

        [Fact]
        public void SerializeEmptyNodeAndOmitRoot_XElement()
        {
            var xmlString = @"<myemptynode />";

            var xml = XElement.Parse(xmlString);

            var json = JsonConvert.SerializeXNode(xml, Formatting.Indented, true);

            Assert.AreEqual("null", json);
        }

        [Fact]
        public void SerializeElementExplicitAttributeNamespace()
        {
            var original = XElement.Parse("<MyElement xmlns=\"http://example.com\" />");
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", original.ToString());

            var json = JsonConvert.SerializeObject(original);
            Assert.AreEqual(@"{""MyElement"":{""@xmlns"":""http://example.com""}}", json);

            var deserialized = JsonConvert.DeserializeObject<XElement>(json);
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", deserialized.ToString());
        }

        [Fact]
        public void SerializeElementImplicitAttributeNamespace()
        {
            var original = new XElement("{http://example.com}MyElement");
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", original.ToString());

            var json = JsonConvert.SerializeObject(original);
            Assert.AreEqual(@"{""MyElement"":{""@xmlns"":""http://example.com""}}", json);

            var deserialized = JsonConvert.DeserializeObject<XElement>(json);
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", deserialized.ToString());
        }

        [Fact]
        public void SerializeDocumentExplicitAttributeNamespace()
        {
            var original = XDocument.Parse("<MyElement xmlns=\"http://example.com\" />");
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", original.ToString());

            var json = JsonConvert.SerializeObject(original);
            Assert.AreEqual(@"{""MyElement"":{""@xmlns"":""http://example.com""}}", json);

            var deserialized = JsonConvert.DeserializeObject<XDocument>(json);
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", deserialized.ToString());
        }

        [Fact]
        public void SerializeDocumentImplicitAttributeNamespace()
        {
            var original = new XDocument(new XElement("{http://example.com}MyElement"));
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", original.ToString());

            var json = JsonConvert.SerializeObject(original);
            Assert.AreEqual(@"{""MyElement"":{""@xmlns"":""http://example.com""}}", json);

            var deserialized = JsonConvert.DeserializeObject<XDocument>(json);
            Assert.AreEqual(@"<MyElement xmlns=""http://example.com"" />", deserialized.ToString());
        }

        public class Model
        {
            public XElement Document { get; set; }
        }

        [Fact]
        public void DeserializeDateInElementText()
        {
            var model = new Model
            {
              Document = new XElement("Value", new XAttribute("foo", "bar"))
              {
                Value = "2001-01-01T11:11:11"
              }
            };

            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>(new[] { new XmlNodeConverter() })
            });

            var json = new StringBuilder(1024);

            using (var stringWriter = new StringWriter(json, CultureInfo.InvariantCulture))
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.Formatting = Formatting.None;
                serializer.Serialize(jsonWriter, model);

                Assert.AreEqual(@"{""Document"":{""Value"":{""@foo"":""bar"",""#text"":""2001-01-01T11:11:11""}}}", json.ToString());
            }

            using (var stringReader = new StringReader(json.ToString()))
            using (var jsonReader = new JsonTextReader(stringReader))
            {
                var document = (XDocument)serializer.Deserialize(jsonReader, typeof(XDocument));

                StringAssert.AreEqual(@"<Document>
  <Value foo=""bar"">2001-01-01T11:11:11</Value>
</Document>", document.ToString());
            }
        }
    }
}