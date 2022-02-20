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

using System.Xml;
using System.Xml.Linq;

namespace Argon;

/// <summary>
/// Provides methods for converting between .NET types and JSON types.
/// </summary>
/// <example>
///   <code lang="cs" source="..\Src\Tests\Documentation\SerializationTests.cs" region="SerializeObject" title="Serializing and Deserializing JSON with JsonConvert" />
/// </example>
public static class JsonXmlConvert
{
    /// <summary>
    /// Serializes the <see cref="XmlNode"/> to a JSON string.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <returns>A JSON string of the <see cref="XmlNode"/>.</returns>
    public static string SerializeXmlNode(XmlNode? node)
    {
        return SerializeXmlNode(node, Formatting.None);
    }

    /// <summary>
    /// Serializes the <see cref="XmlNode"/> to a JSON string using formatting.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <returns>A JSON string of the <see cref="XmlNode"/>.</returns>
    public static string SerializeXmlNode(XmlNode? node, Formatting formatting)
    {
        var converter = new XmlNodeConverter();

        return JsonConvert.SerializeObject(node, formatting, converter);
    }

    /// <summary>
    /// Serializes the <see cref="XmlNode"/> to a JSON string using formatting and omits the root object if <paramref name="omitRootObject"/> is <c>true</c>.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <param name="omitRootObject">Omits writing the root object.</param>
    /// <returns>A JSON string of the <see cref="XmlNode"/>.</returns>
    public static string SerializeXmlNode(XmlNode? node, Formatting formatting, bool omitRootObject)
    {
        var converter = new XmlNodeConverter { OmitRootObject = omitRootObject };

        return JsonConvert.SerializeObject(node, formatting, converter);
    }

    /// <summary>
    /// Deserializes the <see cref="XmlNode"/> from a JSON string.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <returns>The deserialized <see cref="XmlNode"/>.</returns>
    public static XmlDocument? DeserializeXmlNode(string value)
    {
        return DeserializeXmlNode(value, null);
    }

    /// <summary>
    /// Deserializes the <see cref="XmlNode"/> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized <see cref="XmlNode"/>.</returns>
    public static XmlDocument? DeserializeXmlNode(string value, string? deserializeRootElementName)
    {
        return DeserializeXmlNode(value, deserializeRootElementName, false);
    }

    /// <summary>
    /// Deserializes the <see cref="XmlNode"/> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>
    /// and writes a Json.NET array attribute for collections.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A value to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <returns>The deserialized <see cref="XmlNode"/>.</returns>
    public static XmlDocument? DeserializeXmlNode(string value, string? deserializeRootElementName, bool writeArrayAttribute)
    {
        return DeserializeXmlNode(value, deserializeRootElementName, writeArrayAttribute, false);
    }

    /// <summary>
    /// Deserializes the <see cref="XmlNode"/> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>,
    /// writes a Json.NET array attribute for collections, and encodes special characters.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A value to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <param name="encodeSpecialCharacters">
    /// A value to indicate whether to encode special characters when converting JSON to XML.
    /// If <c>true</c>, special characters like ':', '@', '?', '#' and '$' in JSON property names aren't used to specify
    /// XML namespaces, attributes or processing directives. Instead special characters are encoded and written
    /// as part of the XML element name.
    /// </param>
    /// <returns>The deserialized <see cref="XmlNode"/>.</returns>
    public static XmlDocument? DeserializeXmlNode(string value, string? deserializeRootElementName, bool writeArrayAttribute, bool encodeSpecialCharacters)
    {
        var converter = new XmlNodeConverter
        {
            DeserializeRootElementName = deserializeRootElementName,
            WriteArrayAttribute = writeArrayAttribute,
            EncodeSpecialCharacters = encodeSpecialCharacters
        };

        return (XmlDocument?)JsonConvert.DeserializeObject(value, typeof(XmlDocument), converter);
    }

    /// <summary>
    /// Serializes the <see cref="XNode"/> to a JSON string.
    /// </summary>
    /// <param name="node">The node to convert to JSON.</param>
    /// <returns>A JSON string of the <see cref="XNode"/>.</returns>
    public static string SerializeXNode(XObject? node)
    {
        return SerializeXNode(node, Formatting.None);
    }

    /// <summary>
    /// Serializes the <see cref="XNode"/> to a JSON string using formatting.
    /// </summary>
    /// <param name="node">The node to convert to JSON.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <returns>A JSON string of the <see cref="XNode"/>.</returns>
    public static string SerializeXNode(XObject? node, Formatting formatting)
    {
        return SerializeXNode(node, formatting, false);
    }

    /// <summary>
    /// Serializes the <see cref="XNode"/> to a JSON string using formatting and omits the root object if <paramref name="omitRootObject"/> is <c>true</c>.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output should be formatted.</param>
    /// <param name="omitRootObject">Omits writing the root object.</param>
    /// <returns>A JSON string of the <see cref="XNode"/>.</returns>
    public static string SerializeXNode(XObject? node, Formatting formatting, bool omitRootObject)
    {
        var converter = new XmlNodeConverter { OmitRootObject = omitRootObject };

        return JsonConvert.SerializeObject(node, formatting, converter);
    }

    /// <summary>
    /// Deserializes the <see cref="XNode"/> from a JSON string.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <returns>The deserialized <see cref="XNode"/>.</returns>
    public static XDocument? DeserializeXNode(string value)
    {
        return DeserializeXNode(value, null);
    }

    /// <summary>
    /// Deserializes the <see cref="XNode"/> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized <see cref="XNode"/>.</returns>
    public static XDocument? DeserializeXNode(string value, string? deserializeRootElementName)
    {
        return DeserializeXNode(value, deserializeRootElementName, false);
    }

    /// <summary>
    /// Deserializes the <see cref="XNode"/> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>
    /// and writes a Json.NET array attribute for collections.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A value to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <returns>The deserialized <see cref="XNode"/>.</returns>
    public static XDocument? DeserializeXNode(string value, string? deserializeRootElementName, bool writeArrayAttribute)
    {
        return DeserializeXNode(value, deserializeRootElementName, writeArrayAttribute, false);
    }

    /// <summary>
    /// Deserializes the <see cref="XNode"/> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>,
    /// writes a Json.NET array attribute for collections, and encodes special characters.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A value to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <param name="encodeSpecialCharacters">
    /// A value to indicate whether to encode special characters when converting JSON to XML.
    /// If <c>true</c>, special characters like ':', '@', '?', '#' and '$' in JSON property names aren't used to specify
    /// XML namespaces, attributes or processing directives. Instead special characters are encoded and written
    /// as part of the XML element name.
    /// </param>
    /// <returns>The deserialized <see cref="XNode"/>.</returns>
    public static XDocument? DeserializeXNode(string value, string? deserializeRootElementName, bool writeArrayAttribute, bool encodeSpecialCharacters)
    {
        var converter = new XmlNodeConverter
        {
            DeserializeRootElementName = deserializeRootElementName,
            WriteArrayAttribute = writeArrayAttribute,
            EncodeSpecialCharacters = encodeSpecialCharacters
        };

        return (XDocument?)JsonConvert.DeserializeObject(value, typeof(XDocument), converter);
    }
}