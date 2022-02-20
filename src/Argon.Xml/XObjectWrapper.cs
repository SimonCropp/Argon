using System.Xml;
using System.Xml.Linq;

class XObjectWrapper : IXmlNode
{
    readonly XObject? _xmlObject;

    public XObjectWrapper(XObject? xmlObject)
    {
        _xmlObject = xmlObject;
    }

    public object? WrappedNode => _xmlObject;

    public virtual XmlNodeType NodeType => _xmlObject?.NodeType ?? XmlNodeType.None;

    public virtual string? LocalName => null;

    public virtual List<IXmlNode> ChildNodes => XmlNodeConverter.EmptyChildNodes;

    public virtual List<IXmlNode> Attributes => XmlNodeConverter.EmptyChildNodes;

    public virtual IXmlNode? ParentNode => null;

    public virtual string? Value
    {
        get => null;
        set => throw new InvalidOperationException();
    }

    public virtual IXmlNode AppendChild(IXmlNode newChild)
    {
        throw new InvalidOperationException();
    }

    public virtual string? NamespaceUri => null;
}