using System.Xml;

class XmlNodeWrapper : IXmlNode
{
    readonly XmlNode _node;
    List<IXmlNode>? _childNodes;
    List<IXmlNode>? _attributes;

    public XmlNodeWrapper(XmlNode node)
    {
        _node = node;
    }

    public object? WrappedNode => _node;

    public XmlNodeType NodeType => _node.NodeType;

    public virtual string? LocalName => _node.LocalName;

    public List<IXmlNode> ChildNodes
    {
        get
        {
            // childnodes is read multiple times
            // cache results to prevent multiple reads which kills perf in large documents
            if (_childNodes == null)
            {
                if (_node.HasChildNodes)
                {
                    _childNodes = new List<IXmlNode>(_node.ChildNodes.Count);
                    foreach (XmlNode childNode in _node.ChildNodes)
                    {
                        _childNodes.Add(WrapNode(childNode));
                    }
                }
                else
                {
                    _childNodes = XmlNodeConverter.EmptyChildNodes;
                }
            }

            return _childNodes;
        }
    }

    protected virtual bool HasChildNodes => _node.HasChildNodes;

    internal static IXmlNode WrapNode(XmlNode node)
    {
        switch (node.NodeType)
        {
            case XmlNodeType.Element:
                return new XmlElementWrapper((XmlElement)node);
            case XmlNodeType.XmlDeclaration:
                return new XmlDeclarationWrapper((XmlDeclaration)node);
            case XmlNodeType.DocumentType:
                return new XmlDocumentTypeWrapper((XmlDocumentType)node);
            default:
                return new XmlNodeWrapper(node);
        }
    }

    public List<IXmlNode> Attributes
    {
        get
        {
            // attributes is read multiple times
            // cache results to prevent multiple reads which kills perf in large documents
            if (_attributes == null)
            {
                if (HasAttributes)
                {
                    _attributes = new List<IXmlNode>(_node.Attributes.Count);
                    foreach (XmlAttribute attribute in _node.Attributes)
                    {
                        _attributes.Add(WrapNode(attribute));
                    }
                }
                else
                {
                    _attributes = XmlNodeConverter.EmptyChildNodes;
                }
            }

            return _attributes;
        }
    }

    bool HasAttributes
    {
        get
        {
            if (_node is XmlElement element)
            {
                return element.HasAttributes;
            }

            return _node.Attributes?.Count > 0;
        }
    }

    public IXmlNode? ParentNode
    {
        get
        {
            var node = _node is XmlAttribute attribute ? attribute.OwnerElement : _node.ParentNode;

            if (node == null)
            {
                return null;
            }

            return WrapNode(node);
        }
    }

    public string? Value
    {
        get => _node.Value;
        set => _node.Value = value;
    }

    public IXmlNode AppendChild(IXmlNode newChild)
    {
        var xmlNodeWrapper = (XmlNodeWrapper)newChild;
        _node.AppendChild(xmlNodeWrapper._node);
        _childNodes = null;
        _attributes = null;

        return newChild;
    }

    public string? NamespaceUri => _node.NamespaceURI;
}