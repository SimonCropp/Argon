using System.Xml;

class XmlDocumentWrapper : XmlNodeWrapper, IXmlDocument
{
    readonly XmlDocument document;

    public XmlDocumentWrapper(XmlDocument document)
        : base(document)
    {
        this.document = document;
    }

    public IXmlNode CreateComment(string? data)
    {
        return new XmlNodeWrapper(document.CreateComment(data));
    }

    public IXmlNode CreateTextNode(string? text)
    {
        return new XmlNodeWrapper(document.CreateTextNode(text));
    }

    public IXmlNode CreateCDataSection(string? data)
    {
        return new XmlNodeWrapper(document.CreateCDataSection(data));
    }

    public IXmlNode CreateWhitespace(string? text)
    {
        return new XmlNodeWrapper(document.CreateWhitespace(text));
    }

    public IXmlNode CreateSignificantWhitespace(string? text)
    {
        return new XmlNodeWrapper(document.CreateSignificantWhitespace(text));
    }

    public IXmlNode CreateXmlDeclaration(string? version, string? encoding, string? standalone)
    {
        return new XmlDeclarationWrapper(document.CreateXmlDeclaration(version, encoding, standalone));
    }

    public IXmlNode CreateXmlDocumentType(string? name, string? publicId, string? systemId, string? internalSubset)
    {
        return new XmlDocumentTypeWrapper(document.CreateDocumentType(name, publicId, systemId, null));
    }

    public IXmlNode CreateProcessingInstruction(string target, string? data)
    {
        return new XmlNodeWrapper(document.CreateProcessingInstruction(target, data));
    }

    public IXmlElement CreateElement(string elementName)
    {
        return new XmlElementWrapper(document.CreateElement(elementName));
    }

    public IXmlElement CreateElement(string qualifiedName, string namespaceUri)
    {
        return new XmlElementWrapper(document.CreateElement(qualifiedName, namespaceUri));
    }

    public IXmlNode CreateAttribute(string name, string? value)
    {
        var attribute = new XmlNodeWrapper(document.CreateAttribute(name))
        {
            Value = value
        };

        return attribute;
    }

    public IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string? value)
    {
        var attribute = new XmlNodeWrapper(document.CreateAttribute(qualifiedName, namespaceUri))
        {
            Value = value
        };

        return attribute;
    }

    public IXmlElement? DocumentElement
    {
        get
        {
            if (document.DocumentElement == null)
            {
                return null;
            }

            return new XmlElementWrapper(document.DocumentElement);
        }
    }
}