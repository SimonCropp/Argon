using System.Xml;
using System.Xml.Linq;

class XDocumentWrapper : XContainerWrapper, IXmlDocument
{
    XDocument Document => (XDocument)WrappedNode!;

    public XDocumentWrapper(XDocument document)
        : base(document)
    {
    }

    public override List<IXmlNode> ChildNodes
    {
        get
        {
            var childNodes = base.ChildNodes;
            if (Document.Declaration != null && (childNodes.Count == 0 || childNodes[0].NodeType != XmlNodeType.XmlDeclaration))
            {
                childNodes.Insert(0, new XDeclarationWrapper(Document.Declaration));
            }

            return childNodes;
        }
    }

    protected override bool HasChildNodes
    {
        get
        {
            if (base.HasChildNodes)
            {
                return true;
            }

            return Document.Declaration != null;
        }
    }

    public IXmlNode CreateComment(string? text)
    {
        return new XObjectWrapper(new XComment(text));
    }

    public IXmlNode CreateTextNode(string? text)
    {
        return new XObjectWrapper(new XText(text));
    }

    public IXmlNode CreateCDataSection(string? data)
    {
        return new XObjectWrapper(new XCData(data));
    }

    public IXmlNode CreateWhitespace(string? text)
    {
        return new XObjectWrapper(new XText(text));
    }

    public IXmlNode CreateSignificantWhitespace(string? text)
    {
        return new XObjectWrapper(new XText(text));
    }

    public IXmlNode CreateXmlDeclaration(string? version, string? encoding, string? standalone)
    {
        return new XDeclarationWrapper(new XDeclaration(version, encoding, standalone));
    }

    public IXmlNode CreateXmlDocumentType(string? name, string? publicId, string? systemId, string? internalSubset)
    {
        return new XDocumentTypeWrapper(new XDocumentType(name, publicId, systemId, internalSubset));
    }

    public IXmlNode CreateProcessingInstruction(string target, string? data)
    {
        return new XProcessingInstructionWrapper(new XProcessingInstruction(target, data));
    }

    public IXmlElement CreateElement(string elementName)
    {
        return new XElementWrapper(new XElement(elementName));
    }

    public IXmlElement CreateElement(string qualifiedName, string namespaceUri)
    {
        var localName = MiscellaneousUtils.GetLocalName(qualifiedName);
        return new XElementWrapper(new XElement(XName.Get(localName, namespaceUri)));
    }

    public IXmlNode CreateAttribute(string name, string? value)
    {
        return new XAttributeWrapper(new XAttribute(name, value));
    }

    public IXmlNode CreateAttribute(string qualifiedName, string namespaceUri, string? value)
    {
        var localName = MiscellaneousUtils.GetLocalName(qualifiedName);
        return new XAttributeWrapper(new XAttribute(XName.Get(localName, namespaceUri), value));
    }

    public IXmlElement? DocumentElement
    {
        get
        {
            if (Document.Root == null)
            {
                return null;
            }

            return new XElementWrapper(Document.Root);
        }
    }

    public override IXmlNode AppendChild(IXmlNode newChild)
    {
        if (newChild is XDeclarationWrapper declarationWrapper)
        {
            Document.Declaration = declarationWrapper.Declaration;
            return declarationWrapper;
        }

        return base.AppendChild(newChild);
    }
}